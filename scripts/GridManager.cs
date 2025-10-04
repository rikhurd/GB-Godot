using Godot;
using System;
using System.Collections.Generic;

[Tool]
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

    [Export]
    public PackedScene ChunkScene;
    /// <summary>
    /// This sets the amount of chunks inside a level. Since first chunk's ID is 0,0 this checks for the one dimension.
    /// For example LevelSize = 32 is qual to negative -32 and positive 32 so the level is 64x64 chunks.
    /// There needs to be done testing if in procedural maps there can be no limits, but that is going to be done later when there is a lot more going on inside the level,
    /// which limits the efficiency on endless maps. Also there is no need for limitless maps in current game loop design. 
    /// </summary>
    [Export]
    public int LevelSize = 16;

    public Dictionary<Vector2I, GridChunk> GridChunks = new Dictionary<Vector2I, GridChunk>();

    public override void _Ready()
    {
        Instance = this;

        InitializeGridManager();
    }

    // InitalizeGridManager or InitalizeLevel?
    public void InitializeGridManager()
    {
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

    public void SpawnChunk(Vector2I chunkID)
    {
        if (GridChunks.ContainsKey(chunkID))
            return;

        if (chunkID.X < -LevelSize || chunkID.X > LevelSize ||
            chunkID.Y < -LevelSize || chunkID.Y > LevelSize)
        {
    GD.Print("Out of bounds chunkID: ", chunkID);
                return;
        }
        // Instantiate chunk
        GridChunk chunk = ChunkScene.Instantiate<GridChunk>();
        AddChild(chunk);

        // Position it in world space
        chunk.Position = new Vector3(
            chunkID.X * ChunkSize * TileSize,
            0,
            chunkID.Y * ChunkSize * TileSize
        );

        TileData[,] tileData = new TileData[ChunkSize, ChunkSize];
        chunk.InitializeChunk(chunkID, ChunkSize, TileSize, ChunkHeight, tileData);
        GridChunks[chunkID] = chunk;

        return;
    }

    /// <summary>
    /// Gets the TileData at the given global tile position in the world grid.
    /// Converts global position to chunk ID and local tile coordinates.
    /// Example: ChunkSize = 32, globalTilePos = (50,70) -> chunkID = (1,2), local = (18,6).
    /// globalTilePos can come from player input, mouse clicks, or player movement.
    /// </summary>
    /// <param name="globalTilePos">The coordinates of the tile in the entire world grid.</param>
    /// <returns>The TileData structure at the specified global position.</returns>
    public TileData GetGlobalTile(Vector2I globalTilePos)
    {
        Vector2I chunkID = new(
            DivFloor(globalTilePos.X, ChunkSize),
            DivFloor(globalTilePos.Y, ChunkSize)
        );

        GD.Print("DivFloor chunkID: ", chunkID);

        if (!GridChunks.TryGetValue(chunkID, out GridChunk chunk))
        {
            // Lazy-load: spawn chunk if missing
            // chunk = SpawnChunk(chunkID);
        }

        Vector2I local = new(
            Mod(globalTilePos.X, ChunkSize),
            Mod(globalTilePos.Y, ChunkSize)
        );

        return chunk.GetLocalTile(local.X, local.Y);
    }

    public void SetGlobalTile(Vector2I globalTilePos, TileData tile)
    {
        Vector2I chunkID = new(
            DivFloor(globalTilePos.X, ChunkSize),
            DivFloor(globalTilePos.Y, ChunkSize)
        );

        if (!GridChunks.TryGetValue(chunkID, out GridChunk chunk))
        {
            // chunk = SpawnChunk(chunkID);
        }

        Vector2I local = new(
            Mod(globalTilePos.X, ChunkSize),
            Mod(globalTilePos.Y, ChunkSize)
        );

        chunk.SetLocalTile(local.X, local.Y, tile);
    }

    // Integer division that always rounds DOWN
    // This ensures that negative coordinates map correctly to chunk IDs
    // Example: DivFloor(50, 32) = 1, DivFloor(-1, 32) = -1
    private int DivFloor(int a, int b) => (int)Math.Floor((double)a / b);
    // Proper modulo operation that always returns a positive remainder
    // Example: Mod(50, 32) = 18, Mod(-1, 32) = 31
    private int Mod(int a, int b) => ((a % b) + b) % b;
}
