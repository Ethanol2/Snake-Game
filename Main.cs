using Godot;
using System;

public partial class Main : Node2D
{
    private Vector2 _screenSize;

    public override void _Ready()
    {
        _screenSize = GetViewportRect().Size;
        Position = _screenSize / 2f;
    }
}
