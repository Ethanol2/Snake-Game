using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node2D
{
    [ExportCategory("Game References")]
    [Export] private Target _target;
    [Export] private Player _player;
    [Export] private Grid _grid;

    [ExportCategory("Game Settings")]
    [Export] private float _minSpeed = 3f;
    [Export] private float _maxSpeed = 35f;
    [Export] private int _maxSpeedAtPoints = 50;
    [Export] private Curve _difficultyCurve;
    [Export] private bool _allowEdgeWrap = true;

    [ExportCategory("UI")]
    [Export] private Label _scoreLabel;

    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private int _score = 0;

    public override void _Ready()
    {
        this.AssertNotNull(_player);
        this.AssertNotNull(_grid);
        this.AssertNotNull(_difficultyCurve);

        Position = GetViewportRect().Size / 2f;

        rng.Randomize();
        SpawnTarget(_target);

        UpdateScore(0);
        _player.Speed = _minSpeed;

        _player.OnTargetAquired += OnPlayerGetTarget;
    }
    public void OnPlayerGetTarget(Node2D target)
    {
        SpawnTarget(target);
        _score++;
        UpdateScore(_score);

        float speed = _difficultyCurve.Sample(_score / (float)_maxSpeedAtPoints) * (_maxSpeed - _minSpeed);
        speed += _minSpeed;

        _player.Speed = speed;
    }
    private void SpawnTarget(Node2D target)
    {

        if (_player.Length >= _grid.GridSquareCount)
        {
            GD.Print("Game Won!");

            this.RemoveChild(target);
            return;
        }
        List<Vector2I> badCoords = _grid.ConvertPositionsToCoordinates(_player.GetPositions());

        List<Vector2I> goodCoords = new List<Vector2I>();

        Vector2I pos = Vector2I.Zero;
        for (int x = 0; x < _grid.GridWidth; x++)
        {
            pos.X = x;
            for (int y = 0; y < _grid.GridHeight; y++)
            {
                pos.Y = y;

                if (badCoords.Contains(pos))
                    badCoords.Add(pos);
                else
                    goodCoords.Add(pos);
            }
        }

        Vector2I spawn = goodCoords[rng.RandiRange(0, goodCoords.Count - 1)];
        this.Log("Spawning target at " + spawn);

        target.Position = _grid.ConvertCoordinateToPosition(spawn);
    }
    private void UpdateScore(int score) { if (_scoreLabel != null) _scoreLabel.Text = $"Score: {score}"; }
}
