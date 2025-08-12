using Godot;
using System;
using System.Collections.Generic;

public partial class Player : Area2D
{
    [Export] private int _speed = 10;
    [Export] private Vector2 _direction = Vector2.Up;

    private Vector2 _screenSize;

    // Lifecycle
    public override void _Ready()
    {
        _screenSize = GetViewportRect().Size;
    }
    public override void _Process(double delta)
    {

    }

    // Callbacks
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("Up"))
        {

        }
    }

    private struct _SnakeBody
    {
        public int length;
        public List<Vector3> positions;

        public _SnakeBody(Vector3 start)
        {
            length = 1;
            positions = new List<Vector3>() { start };
        }
        public bool AddPosition(Vector3 position)
        {
            if (positions.Contains(position))
            {
                return true;
            }
            
            positions.Add(position);
            if (positions.Count > length)
                positions.RemoveAt(0);

            return false;
        }
    }
}
