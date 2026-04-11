#if TOOLS
using Godot;
using System;

[Tool]
public partial class GridEditTool : EditorPlugin
{
    private GridEditToolUI _dock;
    private EditorSettings EditorSettings;
    private GridChunk SelectedGrid;
    private bool RegisterEditMethods = false;
    public override void _EnterTree()
    {
        // Initialization of the plugin goes here.
        _dock = GD.Load<PackedScene>("res://addons/GridEditTool/GridEditToolDock.tscn").Instantiate<GridEditToolUI>();
        AddControlToDock(DockSlot.LeftUr, _dock);
        SetInputEventForwardingAlwaysEnabled();

        EditorSettings = EditorInterface.Singleton.GetEditorSettings();
        //EditorInterface.Singleton.PopupNodeSelector();

        _dock.SelectGridButton.ButtonDown += OnGridSelection;
        _dock.DeselectGridButton.ButtonDown += DeselectGrid;
        _dock.GridEditMethods.ItemSelected += OnItemSelected;
        _dock.CreateGridData.ButtonDown += InitializeTileData;
    }

    public override void _ExitTree()
    {
        // Clean-up of the plugin goes here.
        RemoveControlFromDocks(_dock);
        _dock.Free();
    }

    public override int _Forward3DGuiInput(Camera3D viewportCamera, InputEvent @event)
    {
        if (!RegisterEditMethods)
            return (int)AfterGuiInput.Pass;

        if (@event is InputEventMouseButton mouseEvent &&
            mouseEvent.Pressed &&
            mouseEvent.ButtonIndex == MouseButton.Left)
        {
            Vector2 mousePos = mouseEvent.Position;

            // Raycast from camera, only check collision layer 2
            var hitPosAndChunk = RaycastChunk(viewportCamera, mousePos, 1000);
            if (hitPosAndChunk == null) return (int)AfterGuiInput.Pass;

            Vector3 hitPos = hitPosAndChunk.Value.hitPosition;

            int tileX = Mathf.FloorToInt(hitPos.X / SelectedGrid.TileSize);
            int tileY = Mathf.FloorToInt(hitPos.Z / SelectedGrid.TileSize);

            //TileData clickedTile = GetChunkLocalTile(chunk, tileX, tileY);
            TileData clickedTile = SelectedGrid.GetLocalTile(tileX, tileY);

            GD.Print($"Clicked Tile at [{tileX},{tileY}] IsWalkable={clickedTile.IsWalkable}, Occupied={clickedTile.IsOccupied}");

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

    private void OnGridSelection()
    {
        Callable callback = Callable.From<NodePath>(OnNodeSelected);
        EditorInterface.Singleton.PopupNodeSelector(callback);
    }
    private void DeselectGrid()
    {
        SelectedGrid = null;
        RegisterEditMethods = false;
        _dock.SelectedGridName.Text = "None";
        _dock.HideDisplayEditContainer();
    }
    private void OnNodeSelected(NodePath nodePath)
    {
        if (nodePath.IsEmpty)
        {
            GD.Print("Node selection canceled");
            return;
        }

        Node selectedNode = EditorInterface.Singleton.GetEditedSceneRoot().GetNodeOrNull(nodePath);

        if (selectedNode != null)
        {
            if (selectedNode is GridChunk gridChunk)
            {
                GD.Print($"Succesfully selected GridChunk node: {selectedNode.Name} at path {nodePath}");

                SelectedGrid = gridChunk;
                RegisterEditMethods = true;
                _dock.SelectedGridName.Text = gridChunk.Name;
                _dock.DisplayEditContainer();
                if (gridChunk.LevelData == null || gridChunk.LevelData.ChunkTileData.Count == 0)
                {
                    _dock.GridDataLabel.Text = "TileData is empty!";
                    _dock.GridDataLabel.Modulate = new Color(1, 0.2f, 0.2f);
                    _dock.GridDataContainer.Visible = true;
                }
            }
        }
    }

    private void InitializeTileData()
	{
        if (SelectedGrid == null)
            GD.Print("No Grid selected!");
        
        int ChunkSizeX = SelectedGrid.ChunkSize;
        int ChunkSizeY = SelectedGrid.ChunkSize;

        SelectedGrid.LevelData.ChunkTileData.Resize(ChunkSizeX * ChunkSizeY);

        for (int y = 0; y < ChunkSizeY; y++)
            for (int x = 0; x < ChunkSizeX; x++)
            {
                int index = y * ChunkSizeX * ChunkSizeY + x;
                SelectedGrid.LevelData.ChunkTileData[index] = new TileData
                {
                    TileIndex = new Vector2I(y, x),  // Note: Vector2I is x,y—adjust if needed
                    IsWalkable = false,
                    IsOccupied = false,
                    Height = 0
                };
        }
    }

    private void OnItemSelected(long index)
	{
		string itemName = _dock.GridEditMethods.GetItemText((int)index);

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

            case 3:
                GD.Print($"Item3 selected — Name: {itemName}");
                break;

			default:
				GD.Print("Unknown item selected.");
				break;
		}
	}
}
#endif
