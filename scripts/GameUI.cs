using Godot;
using System;

public partial class GameUI : Control
{
    public override void _Ready()
    {
    }

    private void OnEditButtonPressed()
    {
        GD.Print("Button clicked!");
    }
}