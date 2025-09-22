using Godot;
using System;

[Tool]
public partial class Chunk : Node3D
{
    private MeshInstance3D ChunkMeshInstance;

	private Vector2I _chunkSize = new Vector2I(3, 3);
    private int _tileSize = 1;

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
            }
        }
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (ChunkMeshInstance == null || !IsInstanceValid(ChunkMeshInstance))
		{
			// Try to get it first
			ChunkMeshInstance = GetNodeOrNull<MeshInstance3D>("ChunkMeshInstance");

			// If it doesn't exist, create and add it
			if (ChunkMeshInstance == null)
			{
				ChunkMeshInstance = new MeshInstance3D();
				ChunkMeshInstance.Name = "ChunkMeshInstance";
				AddChild(ChunkMeshInstance);

				if (Engine.IsEditorHint() && IsInsideTree())
				{
					ChunkMeshInstance.Owner = GetTree().EditedSceneRoot;
				}
			}
		}
		BuildMesh();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void ScheduleMeshBuild()
	{
		if (IsInsideTree())
			CallDeferred(nameof(BuildMesh));
	}
	private void BuildMesh()
	{
		if (ChunkMeshInstance == null)
        return; // safety check

		SurfaceTool st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);

		int ChunkX = ChunkSize.X * TileSize;
		int ChunkY = ChunkSize.Y * TileSize;

		// Define the vertices for the quad
		Vector3[] vertices = new Vector3[]
		{
			new Vector3(0, 0, 0),
			new Vector3(ChunkX, 0, 0),
			new Vector3(ChunkX, 0, ChunkY),
			new Vector3(0, 0, ChunkY)
		};

		// Define UVs (optional)
		Vector2[] uvs = new Vector2[]
		{
			new Vector2(0,0),
			new Vector2(1,0),
			new Vector2(1,1),
			new Vector2(0,1)
		};

		// Define triangles
		int[] indices = new int[]
		{
			0,1,2,
			2,3,0
		};

		// Add vertices to the SurfaceTool
		for (int i = 0; i < indices.Length; i++)
		{
			st.SetUV(uvs[indices[i]]);
			st.AddVertex(vertices[indices[i]]);
		}

		st.GenerateNormals();
		st.GenerateTangents();

		Mesh NewMesh = st.Commit();
		ChunkMeshInstance.Mesh = NewMesh;
	}
}
