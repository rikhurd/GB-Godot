using Godot;
using System;

[Tool]
public partial class GridChunk : Node3D
{
	[ExportGroup("Chunk Nodes")]
	[Export]
	private MeshInstance3D ChunkMeshInstance;
	[Export]
	public ShaderMaterial ChunkMaterial;
	[Export]
	public CollisionShape3D ChunkCollision;
	private BoxShape3D ChunkCollisionShape;
	private ArrayMesh ArrayMesh = new ArrayMesh();
	private int _chunkSize = 32;
	private int _chunkHeight = 1;
	private int _tileSize = 1;

	[ExportGroup("Chunk Variables")]
	[Export]
	public Vector2I ChunkID { get; private set; }

	[Export]
	public int ChunkSize
	{
		get => _chunkSize;
		set
		{
			if (_chunkSize != value)
			{
				_chunkSize = value;
				ScheduleMeshBuild();
				UpdateChunkCollision();
				UpdateChunkShader();
			}
		}
	}

	[Export]
	public int ChunkHeight
	{
		get => _chunkHeight;
		set
		{
			if (_chunkHeight != value)
			{
				_chunkHeight = value;
				ScheduleMeshBuild();
				UpdateChunkCollision();
				UpdateChunkShader();
			}
		}
	}

	[Export]
	public int TileSize
	{
		get => _tileSize;
		set
		{
			if (_tileSize != value)
			{
				_tileSize = value;
				ScheduleMeshBuild();
				UpdateChunkCollision();
				UpdateChunkShader();
			}
		}
	}
	// Create 2D array to hold tile data
	private TileData[,] ChunkTileData;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void InitializeChunk(Vector2I chunkID, int chunkSize, int tileSize, int chunkHeight, TileData[,] tileData)
	{
		ChunkID = chunkID;
		ChunkSize = chunkSize;
		TileSize = tileSize;
		ChunkHeight = chunkHeight;

		// Create the tile data array
		ChunkTileData = tileData;
    }

	private void ScheduleMeshBuild()
	{
		CallDeferred(nameof(BuildMesh));
	}
	private void BuildMesh()
	{
		// --- Vertices & UVs ---
		Vector3[] vertices = new Vector3[(ChunkSize + 1) * (ChunkSize + 1)];
		Vector2[] uvs = new Vector2[vertices.Length];
		Vector3[] normals = new Vector3[vertices.Length];

		for (int y = 0; y <= ChunkSize; y++)
		{
			for (int x = 0; x <= ChunkSize; x++)
			{
				int i = y * (ChunkSize + 1) + x;

				float vx = x * TileSize;
				float vz = y * TileSize;

				vertices[i] = new Vector3(vx, 0, vz);
				uvs[i] = new Vector2((float)x / ChunkSize, (float)y / ChunkSize);
				normals[i] = Vector3.Up; // flat plane facing up
			}
		}

		// --- Indices (two triangles per quad) ---
		int[] indices = new int[ChunkSize * ChunkSize * 6];
		int index = 0;

		for (int y = 0; y < ChunkSize; y++)
		{
			for (int x = 0; x < ChunkSize; x++)
			{
				int i0 = y * (ChunkSize + 1) + x;
				int i1 = i0 + 1;
				int i2 = i0 + (ChunkSize + 1);
				int i3 = i2 + 1;

				// triangle 1
				indices[index++] = i0;
				indices[index++] = i1;
				indices[index++] = i3;

				// triangle 2
				indices[index++] = i0;
				indices[index++] = i3;
				indices[index++] = i2;
			}
		}

		// --- ArrayMesh setup ---
		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Mesh.ArrayType.Max);

		arrays[(int)Mesh.ArrayType.Vertex] = vertices;
		arrays[(int)Mesh.ArrayType.Normal] = normals;
		arrays[(int)Mesh.ArrayType.TexUV] = uvs;
		arrays[(int)Mesh.ArrayType.Index] = indices;

		ArrayMesh.ClearSurfaces();
		ArrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

		ChunkMeshInstance.Mesh = ArrayMesh;
	}
	private void UpdateChunkShader()
	{
		if (ChunkMaterial == null)
		{
			return;
		}

		// The shader expects a Vector2 for grid divisions
		int GridDivisions = ChunkSize;

		// The shader expects a float for cell size
		// float CellSize = TileSize;

		ChunkMaterial.SetShaderParameter("grid_size", ChunkSize);
		// ChunkMaterial.SetShaderParameter("cell_size", CellSize);
	}
	private void UpdateChunkCollision()
{
    if (ChunkCollision == null) return;

    if (ChunkCollision.Shape == null || ChunkCollision.Shape is not BoxShape3D)
    {
        ChunkCollisionShape = new BoxShape3D();
        ChunkCollision.Shape = ChunkCollisionShape;
    }
    else
    {
        ChunkCollisionShape = (BoxShape3D)ChunkCollision.Shape;
    }

    ChunkCollisionShape.Size = new Vector3(ChunkSize * TileSize, ChunkHeight, ChunkSize * TileSize);

    ChunkCollision.Position = new Vector3(
        (ChunkSize * TileSize) * 0.5f,
        ChunkHeight * 0.5f,
        (ChunkSize * TileSize) * 0.5f
    );
}

	public TileData GetLocalTile(int x, int y)
	{
		if (x < 0 || x >= ChunkSize || y < 0 || y >= ChunkSize)
			return default;
		return ChunkTileData[x, y];
	}

	public void SetLocalTile(int x, int y, TileData tileData)
	{
		if (x < 0 || x >= ChunkSize || y < 0 || y >= ChunkSize)
			return;
		ChunkTileData[x, y] = tileData;
	}

	public void ModifyTile(TileData tileData)
	{
		int x = tileData.TileIndex.X;
		int y = tileData.TileIndex.Y;

		// For now just toggle the solid
		tileData.Solid = !tileData.Solid;

		ChunkTileData[x, y] = tileData;
		GD.Print($"Tile: {tileData.TileIndex} Solid: {tileData.Solid}");
		
		ScheduleMeshBuild();
    }
}
