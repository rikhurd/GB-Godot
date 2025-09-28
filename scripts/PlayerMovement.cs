using Godot;
using System;

public partial class PlayerMovement : CharacterBody3D
{
	[Export]
	public float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	[ExportGroup("Input Actions")]
	[Export]
	public string PlayerChoose { get; set; } = "PlayerChoose";
	[Export]
	public string MoveLeft { get; set; } = "ui_left";
	[Export]
	public string MoveRight { get; set; } = "ui_Right";
	[Export]
	public string MoveUp { get; set; } = "ui_up";
	[Export]
	public string MoveDown { get; set; } = "ui_down";

	[ExportGroup("Camera Variables")]
	[Export]
	public Camera3D CameraNode;
	[Export]
	private Node3D CameraPivot;
	[Export]
	public float RayLength = 1000f;
	[Export]
	public float MouseSensitivity = 0.01f;
	[Export]
	public float TiltLimit { get; set; } = Mathf.DegToRad(75.0f);

	[ExportGroup("Character Visuals")]
	[Export] 
    public Node3D CharacterVisualBody { get; set; }
	[Export]
	public float LerpWeight { get; set; } = 0.2f;
	private PhysicsDirectSpaceState3D _spaceState;

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		HandleMouseLook(@event);
		HandleMouseToggle(@event);

		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
		{
			if (Input.IsActionJustPressed(PlayerChoose))
			{
				GD.Print(mouseButton.Position);
				CheckTileClick(mouseButton.Position);
			}
		}
	}
	private void HandleMouseLook(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			float newCameraPitch = CameraPivot.Rotation.X - mouseMotion.Relative.Y * MouseSensitivity;
			newCameraPitch = Mathf.Clamp(newCameraPitch, -TiltLimit, TiltLimit);

			float newCameraYaw = CameraPivot.Rotation.Y - mouseMotion.Relative.X * MouseSensitivity;

			CameraPivot.Rotation = new Vector3(newCameraPitch, newCameraYaw, 0);
		}
	}

	private void HandleMouseToggle(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
		{
			Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
				? Input.MouseModeEnum.Visible
				: Input.MouseModeEnum.Captured;
		}
	}

	private void CheckTileClick(Vector2 mousePos)
	{
		if (CameraNode == null) return;

		// Raycast from camera, only check collision layer 2
		var hitPosAndChunk = RaycastChunk(mousePos, RayLength);
		if (hitPosAndChunk == null) return;

		Vector3 hitPos = hitPosAndChunk.Value.hitPosition;
		GridChunk chunk = hitPosAndChunk.Value.chunk;

		int tileX = Mathf.FloorToInt(hitPos.X / GridManager.Instance.TileSize);
		int tileY = Mathf.FloorToInt(hitPos.Z / GridManager.Instance.TileSize);

		Vector2I globalTilePos = new Vector2I(tileX, tileY);
		TileData clickedTile = GridManager.Instance.GetTile(globalTilePos);

		GD.Print($"Clicked Tile at {globalTilePos} in Chunk {chunk.ChunkID}: Solid={clickedTile.Solid}, Occupied={clickedTile.Occupied}");
	}

	private (Vector3 hitPosition, GridChunk chunk)? RaycastChunk(Vector2 screenPos, float length)
	{
		var worldspace = CameraNode.GetWorld3D().DirectSpaceState;

		Vector3 rayOrigin = CameraNode.ProjectRayOrigin(screenPos);
		Vector3 rayEnd = CameraNode.ProjectPosition(screenPos, length);

		var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
		query.CollisionMask = 1 << 1; // Only colliding with collision layer 2 which is set in the inspector.
		query.CollideWithBodies = true;
		query.CollideWithAreas = false;

		var hitResult = worldspace.IntersectRay(query);

		if (!hitResult.ContainsKey("collider")) return null;

		Node collider = (Node3D) hitResult["collider"];
		if (collider == null) return null;

		GridChunk chunk = collider.Owner as GridChunk;

		if (chunk == null) return null;

		Vector3 hitPosition = (Vector3)hitResult["position"];
		return (hitPosition, chunk);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 NewVelocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			NewVelocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			NewVelocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector(MoveLeft, MoveRight, MoveUp, MoveDown);
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			Transform3D CamTransform = CameraPivot.GlobalTransform;

			Vector3 CamForward = CamTransform.Basis.Z;
			Vector3 CamRight = CamTransform.Basis.X;

			// Ignore vertical tilt (so Forward is always "flat")
			CamForward.Y = CamRight.Y = 0;

			CamForward = CamForward.Normalized();
			CamRight = CamRight.Normalized();

			direction = (CamRight * inputDir.X + CamForward * inputDir.Y).Normalized();

			NewVelocity.X = direction.X * Speed;
			NewVelocity.Z = direction.Z * Speed;

			float TargetYaw = CameraPivot.Rotation.Y;
			float CurrentYaw = CharacterVisualBody.Rotation.Y;

			float NewPlayerYaw = Mathf.LerpAngle(CurrentYaw, TargetYaw, LerpWeight);

			CharacterVisualBody.Rotation = new Vector3(
				CharacterVisualBody.Rotation.X,
				NewPlayerYaw,
				CharacterVisualBody.Rotation.Z
			);
		}
		else
		{
			NewVelocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			NewVelocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = NewVelocity;
		MoveAndSlide();
	}
}
