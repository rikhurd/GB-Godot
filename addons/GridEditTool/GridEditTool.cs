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
        SetInputEventForwardingAlwaysEnabled();
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveControlFromDocks(_dock);
		_dock.Free();
	}

    public override int _Forward3DGuiInput(Camera3D viewportCamera, InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent &&
            mouseEvent.Pressed &&
            mouseEvent.ButtonIndex == MouseButton.Left)
        {
            Vector2 mousePos = mouseEvent.Position;

            // Raycast from camera, only check collision layer 2
            var hitPosAndChunk = RaycastChunk(viewportCamera, mousePos, 1000);
            if (hitPosAndChunk == null) return (int)AfterGuiInput.Pass;

            Vector3 hitPos = hitPosAndChunk.Value.hitPosition;
            GridChunk chunk = hitPosAndChunk.Value.chunk;

            int tileX = Mathf.FloorToInt(hitPos.X / chunk.TileSize);
            int tileY = Mathf.FloorToInt(hitPos.Z / chunk.TileSize);

            TileData clickedTile = chunk.GetLocalTile(tileX,tileY);

            GD.Print($"Clicked Tile at {tileX}{tileY} IsWalkable={clickedTile.IsWalkable}, Occupied={clickedTile.IsOccupied}");

            return (int)AfterGuiInput.Stop; // consume the click
        }

        return (int)AfterGuiInput.Pass;
    }
    private (Vector3 hitPosition, GridChunk chunk)? RaycastChunk(Camera3D viewportCamera, Vector2 screenPos, float length)
    {
        var worldspace = viewportCamera.GetWorld3D().DirectSpaceState;

        Vector3 rayOrigin = viewportCamera.ProjectRayOrigin(screenPos);
        Vector3 rayEnd = viewportCamera.ProjectPosition(screenPos, length);

        var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
        query.CollisionMask = 1 << 1; // Only colliding with collision layer 2 which is set in the inspector.
        query.CollideWithBodies = true;
        query.CollideWithAreas = false;

        var hitResult = worldspace.IntersectRay(query);

        if (!hitResult.ContainsKey("collider")) return null;

        Node collider = (Node3D)hitResult["collider"];
        if (collider == null) return null;

        GridChunk chunk = collider.Owner as GridChunk;

        if (chunk == null) return null;

        Vector3 hitPosition = chunk.ToLocal((Vector3)hitResult["position"]);

        return (hitPosition, chunk);
    }
    
}
#endif
