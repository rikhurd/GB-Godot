using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export]
	public float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	[ExportGroup("Character Visuals")]
	[Export]
	public Node3D CharacterVisualBody { get; set; }
	[Export]
	public float LerpWeight { get; set; } = 0.2f;
	private PhysicsDirectSpaceState3D _spaceState;

	public override void _Ready()
	{
	}
	/*
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
	}*/
	/*
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

		float camRotationY = CameraPivot.Rotation.Y;
		if (Input.IsActionPressed(RotateLeft))
			camRotationY -= Mathf.DegToRad(CameraRotateSpeed) * (float)delta;
		if (Input.IsActionPressed(RotateRight))
			camRotationY += Mathf.DegToRad(CameraRotateSpeed) * (float)delta;

		CameraPivot.Rotation = new Vector3(
			CameraPivot.Rotation.X,
			camRotationY,
			CameraPivot.Rotation.Z
		);

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

			/*
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

	*/
}
