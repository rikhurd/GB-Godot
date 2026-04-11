using Godot;

[GlobalClass]
public partial class LevelData : Resource
{
    public Godot.Collections.Array<TileData> ChunkTileData;

    public Godot.Collections.Array<TileData> PlayerSpawnTiles;

}