using Godot;

[Tool]
[GlobalClass]
public partial class GridChunkData : Resource
{
    [Export] public int ChunkSize;

    [Export] public Godot.Collections.Array<TileData> ChunkTileData = new();
    [Export] public Godot.Collections.Array<Vector2I> PlayerSpawnPoints = new();
    [Export] public Godot.Collections.Array<Vector2I> EnemySpawnPoints = new();

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
    public void SetPlayerSpawn(int x, int y)
    {
        var pos = new Vector2I(x, y);
        EnemySpawnPoints.Remove(pos);
        if (!PlayerSpawnPoints.Contains(pos))
            PlayerSpawnPoints.Add(pos);

        var tile = GetLocalTile(x, y);
        if (tile != null)
        {
            tile.IsBlocked = false;
            tile.IsOccupied = false;
        }
    }

    public void SetEnemySpawn(int x, int y)
    {
        var pos = new Vector2I(x, y);
        PlayerSpawnPoints.Remove(pos);
        if (!EnemySpawnPoints.Contains(pos))
            EnemySpawnPoints.Add(pos);

        var tile = GetLocalTile(x, y);
        if (tile != null)
        {
            tile.IsBlocked = false;
            tile.IsOccupied = false;
        }
    }

    public void ClearSpawn(int x, int y)
    {
        var pos = new Vector2I(x, y);
        PlayerSpawnPoints.Remove(pos);
        EnemySpawnPoints.Remove(pos);
    }
}