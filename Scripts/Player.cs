using Godot;
using System;
using System.Collections.Generic;

public partial class Player : Node2D
{
    [ExportCategory("Parameters")]
    [Export] private int _speed = 10;
    [Export] private Vector2 _direction = Vector2.Up;
    [Export] private bool _edgeWrap = true;

    [ExportGroup("References")]
    [Export] private Grid _grid;
    [Export] private GridObject _base;

    [ExportGroup("Debug")]
    [Export] private bool _debug = false;
    [Export] private Vector2 _realPosition = Vector2.Zero;
    [Export] private int _growthQueue = 0;

    [ExportGroup("Debug/Snake Body")]
    [Export] private Vector2[] _positions { get => _snakeBody == null ? null : _snakeBody.Positions.ToArray(); set {}}

    private _SnakeBody _snakeBody;

    // Events
    public event System.Action<Vector2> OnPositionChanged;

    // Lifecycle
    public override void _Ready()
    {
        if (_grid == null)
        {
            GD.PrintErr("Missing grid reference", this);
            return;
        }
        if (_base == null)
        {
            GD.PrintErr("Missing Grid_Object reference");
            return;
        }

        _base.Scale = _grid.GetSpriteScale(_base.Sprite) * 0.95f;

        _base.Position = _realPosition = _grid.ConvertPosition(_realPosition);
        _snakeBody = new _SnakeBody(_base);

        _base.AreaEntered += OnCollisionEnter;

        _snakeBody = new _SnakeBody(_base);

        ZIndex = 100;
    }
    public override void _Process(double delta)
    {
        _realPosition += _direction * (float)delta * (_grid.SquareSize * _speed);
        if (_edgeWrap)
            _realPosition = _grid.WrapEdge(_realPosition, _base.Position);

        Vector2 newPos = _grid.ConvertPosition(_realPosition);

        if (_base.Position != newPos)
        {

            if (_snakeBody.CheckExists(_base.Position))
            {
                this.Log("Dead");
            }

            if (_growthQueue > 0)
            {
                _snakeBody.AddPosition(_base.Position, this);
                _growthQueue--;
            }
            else
                _snakeBody.PushPosition(_base.Position);

            _base.Position = newPos;
            OnPositionChanged?.Invoke(_base.Position);
        }        

        if (_debug)
        {
            QueueRedraw();

            if (Input.IsPhysicalKeyPressed(Key.F) && _growthQueue == 0)
            {
                _growthQueue++;
                GetViewport().SetInputAsHandled();
            }
        }
    }

    // Callbacks
    public override void _Draw()
    {
        if (_debug)
        {
            DrawCircle(_realPosition, _grid.SquareSize.X / 5f, Colors.Red);
            DrawLine(
                _realPosition,
                _realPosition + (_grid.SquareSize.X / 5f * _direction), Colors.Blue
            );
        }
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (Mathf.Abs(_direction.Y) < 1f)
        {
            if (@event.IsActionPressed("Up"))
            {
                _realPosition = SwapPositionOffset(_realPosition, _base.Position, _direction, Vector2.Up);
                _direction = Vector2.Up;
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("Down"))
            {
                _realPosition = SwapPositionOffset(_realPosition, _base.Position, _direction, Vector2.Down);
                _direction = Vector2.Down;
                GetViewport().SetInputAsHandled();
            }
        }
        else
        {
            if (@event.IsActionPressed("Left"))
            {
                _realPosition = SwapPositionOffset(_realPosition, _base.Position, _direction, Vector2.Left);
                _direction = Vector2.Left;
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("Right"))
            {
                _realPosition = SwapPositionOffset(_realPosition, _base.Position, _direction, Vector2.Right);
                _direction = Vector2.Right;
                GetViewport().SetInputAsHandled();
            }
        }
    }
    private void OnCollisionEnter(Area2D @other)
    {

    }

    // Utility
    private Vector2 SwapPositionOffset(Vector2 realPosition, Vector2 position, Vector2 oldDirection, Vector2 newDirection)
    {
        Vector2 newPosition;
        Vector2 diff = realPosition - position;

        // Account for square size
        diff /= _grid.SquareSize;
        diff.X *= _grid.SquareSize.Y;
        diff.Y *= _grid.SquareSize.X;

        if (Mathf.Abs(oldDirection.X) > 0f)
            newPosition = new Vector2(
                position.X,
                Mathf.IsZeroApprox(oldDirection.X + newDirection.Y) ? position.Y - diff.X : position.Y + diff.X
            );
        else
            newPosition = new Vector2(
                Mathf.IsZeroApprox(oldDirection.Y + newDirection.X) ? position.X - diff.Y : position.X + diff.Y,
                position.Y
            );

        return newPosition;
    }
    private void GrowSnake()
    {

    }

    // Support Objects
    private class _SnakeBody
    {
        public int Length;
        public List<Vector2> Positions;
        public List<Area2D> Nodes;
        public Area2D BaseNode;

        public _SnakeBody(Area2D baseNode)
        {
            Length = 0;
            Positions = new List<Vector2>();
            Nodes = new List<Area2D>();
            BaseNode = baseNode;
        }
        public bool CheckExists(Vector2 position) => Positions.Contains(position);
        public void PushPosition(Vector2 position)
        {
            Positions.Add(position);
            if (Positions.Count > Length)
                Positions.RemoveAt(0);

            for (int i = 0; i < Positions.Count; i++)
            {
                Nodes[i].Position = Positions[i];
            }
        }
        public void AddPosition(Vector2 position, Player parent)
        {
            Length += 1;
            Area2D newNode = BaseNode.Duplicate() as Area2D;
            parent.AddChild(newNode);
            Nodes.Add(newNode);
            PushPosition(position);
        }
    }
}
