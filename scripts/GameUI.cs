using Godot;
using System;

public partial class GameUI : Control
{
    [Export] private Button EditGridButton;

    // Signal to broadcast button click
    [Signal] public delegate void EditGridButtonPressedEventHandler();
    public override void _Ready()
    {
    }

    private void OnEditButtonPressed()
    {
        GD.Print("Button clicked!");
        EmitSignal("EditGridButtonPressedEventHandler");
    }
}