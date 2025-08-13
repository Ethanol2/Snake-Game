using Godot;
using System;
using System.Collections.Generic;

public partial class Player : Area2D
{
    [ExportCategory("Parameters")]
    [Export] private int _speed = 10;
    [Export] private Vector2 _direction = Vector2.Up;

    [ExportGroup("References")]
    [Export] private Grid _grid;
    [Export] private Sprite2D _sprite;

    [ExportGroup("Debug")]
    [Export] private Vector2 _realPosition;

    // Lifecycle
    public override void _Ready()
    {
        if (_grid == null)
        {
            GD.PrintErr("Missing grid reference", this);
            return;
        }
        if (_sprite == null)
        {
            GD.PrintErr("Missing sprite reference");
            return;
        }

        Vector2 spriteSize = _sprite.Texture.GetSize();
        _sprite.Scale = _grid.SquareSize / spriteSize;

        _realPosition = _grid.GetPosition(Position);
    }
    public override void _Process(double delta)
    {
        _realPosition += _direction * _speed * (float)delta;
        Position = _grid.GetPosition(_realPosition);
    }

    // Callbacks
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("Up") && _direction != Vector2.Down)
        {
            _direction = Vector2.Up;
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed("Down") && _direction != Vector2.Up)
        {
            _direction = Vector2.Down;
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed("Left") && _direction != Vector2.Right)
        {
            _direction = Vector2.Left;
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed("Right") && _direction != Vector2.Left)
        {
            _direction = Vector2.Right;
            GetViewport().SetInputAsHandled();
        }
    }

    private struct _SnakeBody
    {
        public int length;
        public List<Vector2> positions;

        public _SnakeBody(Vector2 start)
        {
            length = 1;
            positions = new List<Vector2>() { start };
        }
        public bool AddPosition(Vector2 position)
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
