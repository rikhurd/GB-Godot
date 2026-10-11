#if TOOLS
using Godot;
using System;

[Tool]
public partial class GridEditTool : EditorPlugin
{
    private EditorDock _dock;

    private GridEditToolUI DockContent;
    private GridChunk SelectedGrid;
    private bool RegisterEditMethods = false;
    private ArrayMesh ChunkArrayMesh = new ArrayMesh();
    private ImageTexture DebugTileStateTexture;

    private enum EditMode
    {
        ToggleBlocked,
        ToggleOccupied,
        SetPlayerSpawn,
        SetEnemySpawn,
        ClearSpawn,
    }

    private int _chunkSize = 32;
	private int _chunkHeight = 1;
	private int _tileSize = 4;

    [Export] public int ChunkSize;

	[Export] public int ChunkHeight;

	[Export] public int TileSize;

    private EditMode CurrentEditMode = EditMode.ToggleBlocked;
    public override void _EnterTree()
    {
        // Initialization of the plugin goes here.
        _dock = new EditorDock();
        _dock.Title = "Grid Editor";
        _dock.DefaultSlot = EditorDock.DockSlot.LeftUl;

        DockContent = GD.Load<PackedScene>("res://addons/GridEditTool/GridEditToolDock.tscn").Instantiate<GridEditToolUI>();
        _dock.AddChild(DockContent);

        AddDock(_dock);
        SetInputEventForwardingAlwaysEnabled();

        DockContent.SelectGridButton.ButtonDown += OnGridSelection;
        DockContent.DeselectGridButton.ButtonDown += DeselectGrid;
        DockContent.GridEditMethods.ItemSelected += OnItemSelected;
        DockContent.CreateGridData.ButtonDown += CreateGridDataForSelected;
        DockContent.ToggleTileStateOverlay.Toggled += OnToggleTileStateOverlay;
        DockContent.SaveTileData.ButtonDown += SaveTileData;
        DockContent.ChunkSizeXSpinBox.ValueChanged += OnGridSettingsChanged;
        DockContent.TileSizeSpinBox.ValueChanged += OnGridSettingsChanged;
    }
    private void OnGridSettingsChanged(double value)
    {
        ChunkSize = (int)DockContent.ChunkSizeXSpinBox.Value;
        TileSize = (int)DockContent.TileSizeSpinBox.Value;

        RefreshChunk();
    }
    private void RefreshChunk()
    {
        ScheduleMeshBuild();
        UpdateChunkCollision();
        UpdateChunkShader();
        UpdateTileStateTexture();
    }

    public override void _ExitTree()
    {
        // Clean-up of the plugin goes here.
        DeselectGrid();
        RemoveDock(_dock);
        _dock.Free();
    }

    public override int _Forward3DGuiInput(Camera3D viewportCamera, InputEvent @event)
    {
        if (RegisterEditMethods == false)
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

            int tileX = Mathf.FloorToInt(hitPos.X / TileSize);
            int tileY = Mathf.FloorToInt(hitPos.Z / TileSize);

            //TileData clickedTile = GetChunkLocalTile(chunk, tileX, tileY);
            TileData clickedTile = SelectedGrid.GridChunkData.GetLocalTile(tileX, tileY);

            if (clickedTile == null)
            {
                //This is caused by clicking on the very edge of the grid node
                GD.Print($"No tile at [{tileX},{tileY}] - out of range");
                return (int)AfterGuiInput.Pass;
            }

            switch (CurrentEditMode)
            {
                case EditMode.ToggleBlocked:
                    clickedTile.IsBlocked = !clickedTile.IsBlocked;
                    break;
                case EditMode.ToggleOccupied:
                    clickedTile.IsOccupied = !clickedTile.IsOccupied;
                    break;
                case EditMode.SetPlayerSpawn:
                    SelectedGrid.GridChunkData.SetPlayerSpawn(tileX, tileY);
                    break;
                case EditMode.SetEnemySpawn:
                    SelectedGrid.GridChunkData.SetEnemySpawn(tileX, tileY);
                    break;
                case EditMode.ClearSpawn:
                    SelectedGrid.GridChunkData.ClearSpawn(tileX, tileY);
                    break;
            }

            SelectedGrid.GridChunkData.SetLocalTile(tileX, tileY, clickedTile);
            SaveTileData();
            UpdateTileStateTexture();

            GD.Print($"Edited Tile at [{tileX},{tileY}] IsBlocked={clickedTile.IsBlocked}, Occupied={clickedTile.IsOccupied}");

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
        if (SelectedGrid == null) return;

        if (SelectedGrid?.ChunkMeshInstance != null)
        {
            SelectedGrid.ChunkMeshInstance.MaterialOverride = null;
        }
        SelectedGrid = null;
        RegisterEditMethods = false;
        DockContent.SelectedGridName.Text = "None";
        DockContent.HideDisplayEditContainer();

        GD.Print($"GridChunk node deselected");
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
                GD.Print($"GridChunk.Data path: {gridChunk.GridChunkData?.ResourcePath}");

                SelectedGrid = gridChunk;
                RegisterEditMethods = true;
                DockContent.SelectedGridName.Text = gridChunk.Name;
                DockContent.DisplayEditContainer();

                if (gridChunk.GridChunkData.ChunkTileData == null || gridChunk.GridChunkData.ChunkTileData.Count == 0)
                {
                    DockContent.GridDataLabel.Visible = true;
                    DockContent.GridDataLabel.Text = "TileData is empty!";
                    DockContent.GridDataLabel.Modulate = new Color(1, 0.2f, 0.2f);
                    DockContent.GridDataContainer.Visible = true;
                } else
                {
                    DockContent.GridDataLabel.Visible = false;
                }
                UpdateTileStateTexture();
            }
        }
    }

    private void OnItemSelected(long index)
	{
		string itemName = DockContent.GridEditMethods.GetItemText((int)index);

		switch (index)
		{
			case 0:
                CurrentEditMode = EditMode.ToggleBlocked;
				GD.Print($"Item0 selected — Name: {itemName}");
				break;

			case 1:
				GD.Print($"NOT IMPLEMENTED YET - Item1 selected — Name: {itemName}");
				break;

			case 2:
                CurrentEditMode = EditMode.SetPlayerSpawn;
				GD.Print($"Item2 selected — Name: {itemName}");
                break;

            case 3:
                CurrentEditMode = EditMode.SetEnemySpawn;
                GD.Print($"Item3 selected — Name: {itemName}");
                break;
                
            case 4:
                CurrentEditMode = EditMode.ClearSpawn;
                GD.Print($"Item3 selected — Name: {itemName}");
                break;

			default:
				GD.Print("Unknown item selected.");
				break;
		}
	}

    private void CreateGridDataForSelected()
    {
        if (SelectedGrid?.GridChunkData == null)
        {
            GD.PrintErr("Assign a GridChunkData resource in the Inspector first.");
            return;
        }

        SelectedGrid.SyncDataChunkSize();
        SelectedGrid.GridChunkData.ChunkSize = ChunkSize;

        // if (SelectedGrid.GridChunkData.ChunkTileData == null || SelectedGrid.GridChunkData.ChunkTileData.Count == 0)
        
        GridChunkData filled = new GridChunkData().InitializeChunkTileData(ChunkSize, TileSize);
        SelectedGrid.GridChunkData.ChunkTileData = filled.ChunkTileData;

        Error err = ResourceSaver.Save(SelectedGrid.GridChunkData);
        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to save grid data: {err}");
            return;
        }

        GD.Print($"Populated and saved tile data to {SelectedGrid.GridChunkData.ResourcePath}");
    
        EditorInterface.Singleton.MarkSceneAsUnsaved();
    }

    private void SaveTileData()
    {
        if (SelectedGrid?.GridChunkData == null) return;

        Error err = ResourceSaver.Save(SelectedGrid.GridChunkData);
        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to save grid data: {err}");
        }
        UpdateTileStateTexture();
    }
    private void OnToggleTileStateOverlay(bool enabled)
    {
        UpdateTileStateTexture();
        if (SelectedGrid == null) return;

        MeshInstance3D mesh = SelectedGrid.ChunkMeshInstance;

        if (enabled)
        {
            mesh.MaterialOverride = SelectedGrid.DebugOverlayMaterial;
        }
        else
        {
            mesh.MaterialOverride = null; // falls back to the mesh's normal ChunkMaterial
        }
    }

    private void ScheduleMeshBuild()
	{
		CallDeferred(nameof(BuildMesh));
	}
    public void UpdateTileStateTexture()
	{
		if (SelectedGrid?.GridChunkData == null || SelectedGrid?.GridChunkData.ChunkTileData == null ||
            SelectedGrid?.GridChunkData.ChunkTileData.Count == 0 || SelectedGrid?.DebugOverlayMaterial == null)
			return;

		Image image = Image.CreateEmpty(ChunkSize, ChunkSize, false, Image.Format.Rgf); // R = Blocked, G = occupied

		for (int y = 0; y < ChunkSize; y++)
		{
			for (int x = 0; x < ChunkSize; x++)
			{
				TileData tile = SelectedGrid.GridChunkData.GetLocalTile(x, y);
				if (tile == null) continue;

				float r = tile.IsBlocked ? 1.0f : 0.0f;
				float g = tile.IsOccupied ? 1.0f : 0.0f;
				image.SetPixel(x, y, new Color(r, g, 0f));
			}
		}

		if (DebugTileStateTexture == null  ||
            DebugTileStateTexture.GetWidth() != ChunkSize ||
            DebugTileStateTexture.GetHeight() != ChunkSize)

			DebugTileStateTexture = ImageTexture.CreateFromImage(image);
		else
			DebugTileStateTexture.Update(image);
            
		SelectedGrid.DebugOverlayMaterial.SetShaderParameter("tile_state_texture", DebugTileStateTexture);
	}

    private void UpdateChunkShader()
	{
		if (SelectedGrid.ChunkMaterial == null)
		{
			return;
		}

		// The shader expects a Vector2 for grid divisions
		int GridDivisions = ChunkSize;

		// The shader expects a float for cell size
		// float CellSize = TileSize;

		SelectedGrid.ChunkMaterial.SetShaderParameter("grid_size", ChunkSize);

		#if TOOLS
		SelectedGrid.DebugOverlayMaterial.SetShaderParameter("grid_size", ChunkSize);
		#endif
	
	}
	private void UpdateChunkCollision()
	{
		if (SelectedGrid.ChunkCollision == null) return;

		if (SelectedGrid.ChunkCollision.Shape == null || SelectedGrid.ChunkCollision.Shape is not BoxShape3D)
		{
			SelectedGrid.ChunkCollisionShape = new BoxShape3D();
			SelectedGrid.ChunkCollision.Shape = SelectedGrid.ChunkCollisionShape;
		}
		else
		{
			SelectedGrid.ChunkCollisionShape = (BoxShape3D)SelectedGrid.ChunkCollision.Shape;
		}

		SelectedGrid.ChunkCollisionShape.Size = new Vector3(ChunkSize * TileSize, ChunkHeight, ChunkSize * TileSize);

		SelectedGrid.ChunkCollision.Position = new Vector3(
			(ChunkSize * TileSize) * 0.5f,
			ChunkHeight * 0.5f - 0.5f,
			(ChunkSize * TileSize) * 0.5f
		);
	}

    private void BuildMesh()
	{
		// --- Vertices & UVs ---
		Vector3[] vertices = new Vector3[(ChunkSize + 1) * (ChunkSize + 1)];
		Vector2[] uvs = new Vector2[vertices.Length];
		Vector3[] normals = new Vector3[vertices.Length];

		for (int y = 0; y <= ChunkSize; y++)
		{
			for (int x = 0; x <= ChunkSize; x++)
			{
				int i = y * (ChunkSize + 1) + x;

				float vx = x * TileSize;
				float vz = y * TileSize;

				vertices[i] = new Vector3(vx, 0, vz);
				uvs[i] = new Vector2((float)x / ChunkSize, (float)y / ChunkSize);
				normals[i] = Vector3.Up; // flat plane facing up
			}
		}

		// --- Indices (two triangles per quad) ---
		int[] indices = new int[ChunkSize * ChunkSize * 6];
		int index = 0;

		for (int y = 0; y < ChunkSize; y++)
		{
			for (int x = 0; x < ChunkSize; x++)
			{
				int i0 = y * (ChunkSize + 1) + x;
				int i1 = i0 + 1;
				int i2 = i0 + (ChunkSize + 1);
				int i3 = i2 + 1;

				// triangle 1
				indices[index++] = i0;
				indices[index++] = i1;
				indices[index++] = i3;

				// triangle 2
				indices[index++] = i0;
				indices[index++] = i3;
				indices[index++] = i2;
			}
		}

		// --- ArrayMesh setup ---
		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Mesh.ArrayType.Max);

		arrays[(int)Mesh.ArrayType.Vertex] = vertices;
		arrays[(int)Mesh.ArrayType.Normal] = normals;
		arrays[(int)Mesh.ArrayType.TexUV] = uvs;
		arrays[(int)Mesh.ArrayType.Index] = indices;

		ChunkArrayMesh.ClearSurfaces();
		ChunkArrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

		SelectedGrid.ChunkMeshInstance.Mesh = ChunkArrayMesh;
	}
}
#endif
