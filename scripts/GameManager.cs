using Godot;
using System;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
	public static PlayerController PlayerInstance { get; private set; }

    public bool EditModeActive { get; private set; } = false;

    [Signal] public delegate void EditModeToggledEventHandler(bool active);

    // Call this when the player spawns
    public void RegisterPlayer(PlayerController player)
    {
        PlayerInstance = player;
    }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
    }
    
    public void SetEditMode(bool active)
    {
        EditModeActive = active;
        EmitSignal(SignalName.EditModeToggled, active);
    }
}
