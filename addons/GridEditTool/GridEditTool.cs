#if TOOLS
using Godot;
using System;

[Tool]
public partial class GridEditTool : EditorPlugin
{
	private Control _dock;
	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
		_dock = GD.Load<PackedScene>("res://addons/GridEditTool/GridEditToolDock.tscn").Instantiate<Control>();
		AddControlToDock(DockSlot.LeftUl, _dock);
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveControlFromDocks(_dock);
		_dock.Free();
	}
}
#endif
