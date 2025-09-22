using Godot;
using System;

public partial class PlayerMovement : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	[ExportGroup("Input Actions")]
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
	public float MouseSensitivity = 0.01f;
	[Export]
	public float TiltLimit { get; set; } = Mathf.DegToRad(75.0f);
    private Node3D CameraPivot;
	private Camera3D Camera;

	[ExportGroup("Character Visuals")]
	[Export] 
    public Node3D CharacterVisualBody { get; set; }
	[Export]
	public float LerpWeight { get; set; } = 0.2f;

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		CameraPivot = GetNode<Node3D>("CameraPivot");
	}

    public override void _Input(InputEvent @event)
    {
		if (@event is InputEventMouseMotion mouseMotion)
		{
			float NewCameraPitch = CameraPivot.Rotation.X - mouseMotion.Relative.Y * MouseSensitivity;
			NewCameraPitch = Mathf.Clamp(NewCameraPitch, -TiltLimit, TiltLimit);

			// Apply yaw (Y rotation)
			float NewCameraYaw = CameraPivot.Rotation.Y - mouseMotion.Relative.X * MouseSensitivity;

			CameraPivot.Rotation = new Vector3(NewCameraPitch, NewCameraYaw, 0);
        }
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
