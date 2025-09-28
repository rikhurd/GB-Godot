using Godot;
using System;
using System.Collections.Generic;

public partial class GridManager : Node
{
    public static GridManager Instance;
	[ExportGroup("Chunk Variables")]
	[Export]
    public int ChunkSize = 32;
	[Export]
	public int TileSize = 3;
	[Export]
	public int ChunkHeight = 1;

    [Export] public PackedScene ChunkScene;

    public Dictionary<Vector2I, GridChunk> GridChunks = new Dictionary<Vector2I, GridChunk>();

    public override void _Ready()
    {
        Instance = this;

        // Add initial chunks for debug
        SpawnChunk(new Vector2I(0, 0));
        SpawnChunk(new Vector2I(0, 1));
        SpawnChunk(new Vector2I(1, 0));
        SpawnChunk(new Vector2I(1, 1));

        SpawnChunk(new Vector2I(0, -1));
        SpawnChunk(new Vector2I(-1, 0));
        SpawnChunk(new Vector2I(-1, -1));
        SpawnChunk(new Vector2I(1, -1));
        SpawnChunk(new Vector2I(-1, 1));
    }

    public GridChunk SpawnChunk(Vector2I chunkID)
    {
        if (GridChunks.ContainsKey(chunkID))
            return GridChunks[chunkID];

        // Instantiate chunk
        GridChunk chunk = ChunkScene.Instantiate<GridChunk>();
        AddChild(chunk);        

        // Position it in world space
        chunk.Position = new Vector3(
            chunkID.X * ChunkSize * TileSize,
            0,
            chunkID.Y * ChunkSize * TileSize
        );

        chunk.InitializeChunk(chunkID, ChunkSize, TileSize, ChunkHeight);
		GridChunks[chunkID] = chunk;

        return chunk;
    }

    /// <summary>
    /// Gets the TileData at the given global tile position in the world grid.
    /// Converts global position to chunk ID and local tile coordinates.
    /// Example: ChunkSize = 32, globalTilePos = (50,70) -> chunkID = (1,2), local = (18,6).
    /// globalTilePos can come from player input, mouse clicks, or player movement.
    /// </summary>
    /// <param name="globalTilePos">The coordinates of the tile in the entire world grid.</param>
    /// <returns>The TileData structure at the specified global position.</returns>
    public TileData GetTile(Vector2I globalTilePos)
	{
		Vector2I chunkID = new(
			DivFloor(globalTilePos.X, ChunkSize),
			DivFloor(globalTilePos.Y, ChunkSize)
		);

		if (!GridChunks.TryGetValue(chunkID, out GridChunk chunk))
		{
			// Lazy-load: spawn chunk if missing
			chunk = SpawnChunk(chunkID);
		}

		Vector2I local = new(
			Mod(globalTilePos.X, ChunkSize),
			Mod(globalTilePos.Y, ChunkSize)
		);

		return chunk.GetTile(local.X, local.Y);
	}

    public void SetTile(Vector2I globalTilePos, TileData tile)
    {
        Vector2I chunkID = new(
            DivFloor(globalTilePos.X, ChunkSize),
            DivFloor(globalTilePos.Y, ChunkSize)
        );

        if (!GridChunks.TryGetValue(chunkID, out GridChunk chunk))
        {
            chunk = SpawnChunk(chunkID);
        }

        Vector2I local = new(
            Mod(globalTilePos.X, ChunkSize),
            Mod(globalTilePos.Y, ChunkSize)
        );

        chunk.SetTile(local.X, local.Y, tile);
    }

	// Integer division that always rounds DOWN
	// This ensures that negative coordinates map correctly to chunk IDs
	// Example: DivFloor(50, 32) = 1, DivFloor(-1, 32) = -1
    private int DivFloor(int a, int b) => (int)Math.Floor((double)a / b);
	// Proper modulo operation that always returns a positive remainder
	// Example: Mod(50, 32) = 18, Mod(-1, 32) = 31
    private int Mod(int a, int b) => ((a % b) + b) % b;
}
