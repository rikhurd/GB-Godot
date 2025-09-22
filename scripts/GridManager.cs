using Godot;
using System;

public partial class GridManager : Node3D
{
	public static GridManager GridManagerInstance;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GridManagerInstance = this;
		GD.Print("GridManager loaded");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
