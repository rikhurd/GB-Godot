using Godot;
using System;

public partial class GameUI : Control
{
    [Export] private Button EditGridButton;
    public override void _Ready()
    {
        EditGridButton.Toggled += OnEditButtonToggled;
    }
    // Button click is signaled in the editor Node properties
    private void OnEditButtonToggled(bool active)
    {   
        GameManager.Instance.SetEditMode(active);
    }
}