using Godot;
using System;

public partial class PlayerController : Node3D
{
	[Export] public float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	[Export] public Player TargetPlayer;
	[Export] public bool AttachedToPlayer = true;

	[ExportGroup("Input Actions")]
	[Export] public string PlayerChoose { get; set; } = "PlayerChoose";
	[Export] public string MoveLeft { get; set; } = "ui_left";
	[Export] public string MoveRight { get; set; } = "ui_Right";
	[Export] public string MoveUp { get; set; } = "ui_up";
	[Export] public string MoveDown { get; set; } = "ui_down";
	[Export] public string RotateLeft { get; set; } = "RotateLeft";
	[Export] public string RotateRight { get; set; } = "RotateRight";

	[ExportGroup("Camera Variables")]
	[Export] public Camera3D CameraNode;
	[Export] public float CameraRotateSpeed = 90f;
	[Export] public float RayLength = 1000f;
	[Export] public float MouseSensitivity = 0.01f;
	[Export] public float TiltLimit { get; set; } = Mathf.DegToRad(75.0f);
	[Export] public float FollowLerpSpeed = 5.0f;

	private Vector3 cameraVelocity = Vector3.Zero;

	public override void _Input(InputEvent @event)
	{
		// HandleMouseLook(@event);
		// HandleMouseToggle(@event);

		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
		{
			if (Input.IsActionJustPressed(PlayerChoose))
			{
				GD.Print("Mouse click position: ", mouseButton.Position);
				CheckTileClick(mouseButton.Position);
			}
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
		TileData clickedTile = GridManager.Instance.GetGlobalTile(globalTilePos);

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

		Node collider = (Node3D)hitResult["collider"];
		if (collider == null) return null;

		GridChunk chunk = collider.Owner as GridChunk;

		if (chunk == null) return null;

		Vector3 hitPosition = (Vector3)hitResult["position"];
		return (hitPosition, chunk);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 newVelocity = Vector3.Zero;

		// --- Handle camera rotation ---
		float camRotationY = Rotation.Y;
		if (Input.IsActionPressed(RotateLeft))
		{
			camRotationY -= Mathf.DegToRad(CameraRotateSpeed) * (float)delta;
		}
		if (Input.IsActionPressed(RotateRight))
		{
			camRotationY += Mathf.DegToRad(CameraRotateSpeed) * (float)delta;
		}

		Rotation = new Vector3(
			Rotation.X,
			camRotationY,
			Rotation.Z
		);

		// --- Get input direction ---
		Vector2 inputDir = Input.GetVector(MoveLeft, MoveRight, MoveUp, MoveDown);
		Vector3 direction = Vector3.Zero;

		if (inputDir != Vector2.Zero)
		{
			Transform3D camTransform = GlobalTransform;

			Vector3 camForward = camTransform.Basis.Z;
			Vector3 camRight = camTransform.Basis.X;

			camForward.Y = 0;
			camRight.Y = 0;
			camForward = camForward.Normalized();
			camRight = camRight.Normalized();

			direction = (camRight * inputDir.X + camForward * inputDir.Y).Normalized();
		}

		if (AttachedToPlayer && TargetPlayer != null)
		{
			// Smooth camera follow when decoupled
			Vector3 targetPosition = TargetPlayer.GlobalTransform.Origin;
			GlobalPosition = GlobalPosition.Lerp(
				targetPosition, FollowLerpSpeed * (float)delta
			);

			// --- Player movement ---
			newVelocity = TargetPlayer.Velocity;

			// Apply horizontal input
			newVelocity.X = direction.X * Speed;
			newVelocity.Z = direction.Z * Speed;

			// Gravity
			if (!TargetPlayer.IsOnFloor())
				newVelocity += TargetPlayer.GetGravity() * (float)delta;

			// Jump
			if (Input.IsActionJustPressed("ui_accept") && TargetPlayer.IsOnFloor())
				newVelocity.Y = JumpVelocity;

			// Move the player
			TargetPlayer.Velocity = newVelocity;
			TargetPlayer.MoveAndSlide();
		}
		else
		{
			// --- Free camera movement ---
			cameraVelocity.X = direction.X * Speed;
			cameraVelocity.Z = direction.Z * Speed;

			// Apply movement
			GlobalTranslate(cameraVelocity * (float)delta);
		}
	}
	
	public void EnableGridEdit()
    {
        GD.Print("Grid editing enabled!");
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
