using Godot;

[Tool]
[GlobalClass]
public partial class TileData : Resource
{
    [Export] public Vector2I TileIndex;
    [Export] public bool IsBlocked;
    [Export] public bool IsOccupied;
    [Export] public float Height;
}