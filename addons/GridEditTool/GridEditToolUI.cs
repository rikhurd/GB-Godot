using Godot;
using System;

[Tool]
public partial class GridEditToolUI : Control
{
	[Export] private ItemList GridEditMethods;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GridEditMethods.ItemSelected += OnItemSelected;
	}

	private void OnItemSelected(long index)
	{
		string itemName = GridEditMethods.GetItemText((int)index);

		switch (index)
		{
			case 0:
				GD.Print($"Item0 selected — Name: {itemName}");
				break;

			case 1:
				GD.Print($"Item1 selected — Name: {itemName}");
				break;

			case 2:
				GD.Print($"Item2 selected — Name: {itemName}");
				break;

			default:
				GD.Print("Unknown item selected.");
				break;
		}
	}
}
