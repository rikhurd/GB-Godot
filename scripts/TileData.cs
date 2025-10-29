using Godot;
using System;

public struct TileData
{
	public Vector2I TileIndex;
    public bool Solid;
    public bool Occupied;
    public float Height;
	public TileData(Vector2I tileIndex, bool solid, bool occupied, float height)
    {
		TileIndex = tileIndex;
        Solid = solid;
        Occupied = occupied;
        Height = height;
    }
}