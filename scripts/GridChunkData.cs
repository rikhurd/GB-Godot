using Godot;

[Tool]
[GlobalClass]
public partial class GridChunkData : Resource
{
    [Export] public int ChunkSize;

    [Export] public Godot.Collections.Array<TileData> ChunkTileData = new();

    [Export] public Godot.Collections.Array<TileData> PlayerSpawnTiles = new();

    public TileData GetLocalTile(int x, int y)
    {
        if (x < 0 || x >= ChunkSize || y < 0 || y >= ChunkSize) return null;
        return ChunkTileData[y * ChunkSize + x];
    }

    public void SetLocalTile(int x, int y, TileData tileData)
    {
        if (x < 0 || x >= ChunkSize || y < 0 || y >= ChunkSize) return;
        ChunkTileData[y * ChunkSize + x] = tileData;
    }

    public void ModifyTile(TileData tileData)
    {
        int x = tileData.TileIndex.X;
        int y = tileData.TileIndex.Y;

        SetLocalTile(x, y, tileData);
    }

    public static GridChunkData InitializeChunkTileData(int newChunkSize)
    {
        GridChunkData newData = new GridChunkData
        {
            ChunkSize = newChunkSize,
            ChunkTileData = new Godot.Collections.Array<TileData>()
        };

        for (int y = 0; y < newChunkSize; y++)
        {
            for (int x = 0; x < newChunkSize; x++)
            {
                newData.ChunkTileData.Add(new TileData
                {
                    TileIndex = new Vector2I(x, y),
                    IsBlocked = false,
                    IsOccupied = false,
                    Height = 0
                });
            }
        }

        GD.Print($"Initialized {newChunkSize * newChunkSize} tiles for chunk.");
        return newData;
    }
}