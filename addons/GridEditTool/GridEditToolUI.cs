using Godot;
using System;

[Tool]
public partial class GridEditToolUI : Control
{
	[Export] public ItemList GridEditMethods;
	[Export] public Button SelectGridButton;
	[Export] public Button DeselectGridButton;
	[Export] public Label SelectedGridName;
	[Export] public Label GridDataLabel;
	[Export] public BoxContainer GridEditContainer;
	[Export] public BoxContainer GridDataContainer;
	[Export] public Button CreateGridData;
	[Export] public Button DeleteGridData;
	[Export] public Tree GridDataTree;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public void DisplayEditContainer()
	{
		GridEditContainer.Visible = true;
	}
	public void HideDisplayEditContainer()
    {
		GridEditContainer.Visible = false;
    }
}
