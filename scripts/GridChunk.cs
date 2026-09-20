using Godot;
using System;

[Tool]
public partial class GridChunk : Node3D
{
	[ExportGroup("Chunk Nodes")]
	[Export] public MeshInstance3D ChunkMeshInstance;
	[Export] public ShaderMaterial ChunkMaterial;
	[Export] public CollisionShape3D ChunkCollision;
	private ImageTexture TileStateTexture;
	public BoxShape3D ChunkCollisionShape;

	#if TOOLS
	[ExportGroup("Editor Debug")]
	[Export] public ShaderMaterial DebugOverlayMaterial;
	#endif

	[ExportGroup("Chunk Variables")]
	public Vector2I ChunkID { get; private set; }

	
	[ExportGroup("Chunk Data Variables")]
	[Export] public GridChunkData GridChunkData;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/*
		UpdateChunkCollision();
		UpdateChunkShader();
		ScheduleMeshBuild();
		*/
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

/*
	public void InitializeChunk(Vector2I chunkID, int chunkSize, int tileSize, int chunkHeight, TileData[,] tileData)
	{
		ChunkID = chunkID;
		ChunkSize = chunkSize;
		TileSize = tileSize;
		ChunkHeight = chunkHeight;

		// Create the tile data array
		ChunkTileData = tileData;
    }
	*/
	// Initializes Chunk Tile Data.

	

	
	public void SyncDataChunkSize()
	{
    if (GridChunkData != null)
		{
			// GridChunkData.ChunkSize = ChunkSize;
		}
	}
	/*
	public void UpdateTileStateTexture()
	{
		if (GridChunkData == null || GridChunkData.ChunkTileData == null || GridChunkData.ChunkTileData.Count == 0 || ChunkMaterial == null)
			return;

		Image image = Image.CreateEmpty(ChunkSize, ChunkSize, false, Image.Format.Rgf); // R = Blocked, G = occupied

		for (int y = 0; y < ChunkSize; y++)
		{
			for (int x = 0; x < ChunkSize; x++)
			{
				TileData tile = GridChunkData.GetLocalTile(x, y);
				if (tile == null) continue;

				float r = tile.IsBlocked ? 1.0f : 0.0f;
				float g = tile.IsOccupied ? 1.0f : 0.0f;
				image.SetPixel(x, y, new Color(r, g, 0f));
			}
		}

		if (TileStateTexture == null)
			TileStateTexture = ImageTexture.CreateFromImage(image);
		else
			TileStateTexture.Update(image);

		ChunkMaterial.SetShaderParameter("tile_state_texture", TileStateTexture);
	}*/
}
