using Godot;
using System;

public struct TileData
{
	public Vector2I TileIndex;
    public bool IsWalkable;
    public bool IsOccupied;
    public float Height;
	public TileData(Vector2I tileIndex, bool walkable, bool occupied, float height)
    {
		TileIndex = tileIndex;
        IsWalkable = walkable;
        IsOccupied = occupied;
        Height = height;
    }
}