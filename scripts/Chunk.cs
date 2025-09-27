using Godot;
using System;

[Tool]
public partial class Chunk : Node3D
{
	[ExportGroup("Chunk Nodes")]
	[Export]
	private MeshInstance3D ChunkMeshInstance;
	[Export]
	public ShaderMaterial ChunkMaterial;
	[Export]
	public CollisionShape3D ChunkCollision;
	private BoxShape3D ChunkCollisionShape;
	private Vector2I _chunkSize = new Vector2I(3, 3);
	private int _chunkHeight = 1;
	private int _tileSize = 1;

	[ExportGroup("Chunk Variables")]
	[Export]
	public Vector2I ChunkSize
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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BuildMesh();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void ScheduleMeshBuild()
	{
		CallDeferred(nameof(BuildMesh));
	}
	private void BuildMesh()
	{
		int ChunkX = ChunkSize.X * TileSize;
		int ChunkY = ChunkSize.Y * TileSize;

		float halfX = ChunkX * 0.5f;
		float halfY = ChunkY * 0.5f;

		Vector3[] vertices = new Vector3[]
		{
			new Vector3(-halfX, 0, -halfY),
			new Vector3(halfX, 0, -halfY),
			new Vector3(halfX, 0, halfY),
			new Vector3(-halfX, 0, halfY)
		};

		Vector2[] uvs = new Vector2[]
		{
			new Vector2(0,0),
			new Vector2(1,0),
			new Vector2(1,1),
			new Vector2(0,1)
		};

		int[] indices = new int[] { 0, 1, 2, 2, 3, 0 };

		// MANUAL NORMALS: every vertex points up
		Vector3[] normals = new Vector3[vertices.Length];
		for (int i = 0; i < normals.Length; i++)
			normals[i] = Vector3.Up;

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Mesh.ArrayType.Max);

		arrays[(int)Mesh.ArrayType.Vertex] = vertices;
		arrays[(int)Mesh.ArrayType.Normal] = normals;
		arrays[(int)Mesh.ArrayType.TexUV] = uvs;
		arrays[(int)Mesh.ArrayType.Index] = indices;

		var arrayMesh = new ArrayMesh();
		arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

		ChunkMeshInstance.Mesh = arrayMesh;
	}



	private void UpdateChunkShader()
	{
		if (ChunkMaterial == null)
		{
			return;
		}

		// The shader expects a Vector2 for grid divisions
		Vector2 GridDivisions = ChunkSize;

		// The shader expects a float for cell size
		// float CellSize = TileSize;

		ChunkMaterial.SetShaderParameter("grid_size_xy", GridDivisions);
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

		ChunkCollisionShape.Size = new Vector3(ChunkSize.X * TileSize, ChunkHeight, ChunkSize.Y * TileSize);
		ChunkCollision.Position = new Vector3(0, ChunkHeight * 0.5f, 0);
	}
}
