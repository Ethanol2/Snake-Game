using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public partial class Player : Node2D
{
    [ExportCategory("Parameters")]
    [Export] private float _speed = 2f;
    [Export] private Vector2 _direction = Vector2.Up;
    [Export] private float _bodySizeRelativeToGrid = 0.85f;

    [ExportGroup("References")]
    [Export] private Grid _grid;
    [Export] private GridObject _base;

    [ExportGroup("Debug")]
    [Export] private bool _debug = false;
    [Export] private Vector2 _realPosition = Vector2.Zero;
    [Export] private int _growthQueue = 0;

    [ExportGroup("Debug/Snake Body")]
    [Export] private Vector2 _headPosition { get => _base.Position; set {}}
    [Export] private Vector2[] _positions { get => _snakeBody == null ? null : _snakeBody.Positions.ToArray(); set {}}

    private _SnakeBody _snakeBody;
    private Vector2 _previous_dir;
    private Vector2 _next_dir;

    // Properties
    public int Length => _snakeBody == null ? 0 : _snakeBody.Positions.Count + 1;
    public float Speed { get => _speed; set { _speed = value; this.Log("Speed: " + _speed); } }
    public bool Debug { get => _debug; set => _debug = value; }
    public Vector2 RealPosition => _realPosition;

    // Events
    public event Action<Vector2> OnPositionChanged;
    public event Action<Node2D> OnTargetAquired;
    public event Action<Vector2> OnTailCollide;
    public event Action<Vector2> OnEdgeWrapped;

    // Lifecycle
    public async override void _Ready()
    {
        _snakeBody = new _SnakeBody(_base);
        _snakeBody.OnBodyColision += OnCollisionEnter;

        ProcessMode = ProcessModeEnum.Disabled;
        while (!_grid.IsNodeReady())
        {
            await Task.Delay((int)(GetProcessDeltaTime() * 1000d));
        }

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

        _base.Scale = _grid.GetSpriteScale(_base.Sprite) * _bodySizeRelativeToGrid;

        _base.Position = _realPosition = _grid.ConvertPosition(_realPosition);
        _base.AreaEntered += OnCollisionEnter;

        _previous_dir = Vector2.Zero;
        _next_dir = _direction;
        
        ProcessMode = ProcessModeEnum.Pausable;
    }
    public override void _Process(double delta)
    {
        _realPosition += (float)delta * (_grid.SquareSize * _speed) * _direction;

        Vector2 newPos = _grid.ConvertPosition(_realPosition);

        if (!_base.Position.IsEqualApprox(newPos))
        {
            // To account for small errors in the math, and floating point bullshit, the diff is multiplied by 1.01 so that it always rounds >=1
            int diff = Mathf.RoundToInt(((_base.Position - newPos) * _direction / _grid.SquareSize).Length() * 1.01f);

            for (int i = 0; i < diff; i++)
            {
                _base.Position = newPos - (_direction * _grid.SquareSize * (diff - i));
                _base.Position = _grid.WrapEdge(_base.Position);

                if (_growthQueue > 0)
                {
                    _snakeBody.AddPosition(_base.Position, this);
                    _growthQueue--;
                }
                else
                    _snakeBody.PushPosition(_base.Position);
                    
            }

            if (_snakeBody.CheckForOverlap(diff - 1, out Vector2 overlap, newPos))
            {
                this.Log("Collided with Tail @" + overlap);
                OnTailCollide?.Invoke(overlap);
            }

            _realPosition = _grid.WrapEdge(_realPosition, out bool wrapped);

            if (wrapped)
                OnEdgeWrapped?.Invoke(newPos);

            newPos = _grid.WrapEdge(newPos);

            _base.Position = newPos;
            _previous_dir = _direction;
            
            if (!_direction.IsEqualApprox(_next_dir))
            {
                _realPosition = SwapPositionOffset(_realPosition, _base.Position, _direction, _next_dir);
                _direction = _next_dir;
            }

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
            DrawCircle(_realPosition, _grid.SquareSize.X * 0.2f, Colors.Green);
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
                if (!_previous_dir.IsEqualApprox(Vector2.Down))
                {
                    _realPosition = SwapPositionOffset(_realPosition, _base.Position, _direction, Vector2.Up);
                    _direction = Vector2.Up;
                    _next_dir = _direction;
                }
                else
                {
                    _next_dir = Vector2.Up;
                }
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("Down"))
            {
                if (!_previous_dir.IsEqualApprox(Vector2.Up))
                {
                    _realPosition = SwapPositionOffset(_realPosition, _base.Position, _direction, Vector2.Down);
                    _direction = Vector2.Down;
                    _next_dir = _direction;
                }
                else
                {
                    _next_dir = Vector2.Down;
                }
                GetViewport().SetInputAsHandled();
            }
        }
        else
        {
            if (@event.IsActionPressed("Left"))
            {
                if (!_previous_dir.IsEqualApprox(Vector2.Right))
                {
                    _realPosition = SwapPositionOffset(_realPosition, _base.Position, _direction, Vector2.Left);
                    _direction = Vector2.Left;
                    _next_dir = _direction;
                }
                else
                {
                    _next_dir = Vector2.Left;
                }
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("Right"))
            {
                if (!_previous_dir.IsEqualApprox(Vector2.Left))
                {
                    _realPosition = SwapPositionOffset(_realPosition, _base.Position, _direction, Vector2.Right);
                    _direction = Vector2.Right;
                    _next_dir = _direction;
                }
                else
                {
                    _next_dir = Vector2.Right;
                }
                GetViewport().SetInputAsHandled();
            }
        }
    }
    private void OnCollisionEnter(Area2D @other)
    {
        Node parent = @other.GetParent();
        if (parent == null)
            return;

        if (parent.Name == "Target")
        {
            _growthQueue++;
            OnTargetAquired?.Invoke(parent as Node2D);
        }
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
    public List<Vector2> GetPositions()
    {
        List<Vector2> positions = new List<Vector2>(_snakeBody.Positions);
        positions.Add(_base.Position);
        return positions;
    }

    // Support Objects
    private class _SnakeBody
    {
        public int Length;
        public List<Vector2> Positions;
        public List<Area2D> Nodes;
        public Area2D BaseNode;

        public event Action<Area2D> OnBodyColision;

        public _SnakeBody(Area2D baseNode)
        {
            Length = 0;
            Positions = new List<Vector2>();
            Nodes = new List<Area2D>();
            BaseNode = baseNode;
        }
        public bool CheckExists(Vector2 position) => Positions.Contains(position);
        public bool CheckForOverlap(int sliceCount, out Vector2 overlapPos, params Vector2[] extras)
        {
            overlapPos = Vector2.Inf;

            if (Positions.Count == 0)
                return false;

            for (int i = 0; i < Positions.Count - sliceCount; i++)
            {
                for (int k = Positions.Count - sliceCount; k < Positions.Count; k++)
                {
                    if (Positions[i].IsEqualApprox(Positions[k]))
                    {
                        overlapPos = Positions[i];
                        return true;
                    }
                }
                foreach (Vector2 extra in extras)
                {
                    if (Positions[i].IsEqualApprox(extra))
                    {
                        overlapPos = extra;
                        return true;
                    }
                }
            }
            return false;
        }
        public void PushPosition(Vector2 position)
        {
            Positions.Add(position);
            if (Positions.Count > Length)
                Positions.RemoveAt(0);

            for (int i = 0; i < Nodes.Count; i++)
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
            newNode.AreaEntered += PassColision;
            PushPosition(position);
        }
        public void PassColision(Area2D area2D) => OnBodyColision?.Invoke(area2D);
    }
}
