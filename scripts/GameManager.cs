using Godot;
using System;

public partial class GameManager : Node
{
	public static Player PlayerInstance { get; private set; }

    // Call this when the player spawns
    public static void RegisterPlayer(Player player)
    {
        PlayerInstance = player;
    }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}
}
