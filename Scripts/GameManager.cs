using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node2D
{
    [ExportCategory("References")]
    [Export] private Target _target;
    [Export] private Player _player;
    [Export] private Grid _grid;

    private RandomNumberGenerator rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        Position = GetViewportRect().Size / 2f;

        rng.Randomize();
        SpawnTarget(_target);

        _player.OnTargetAquired += OnPlayerGetTarget;
    }
    public void OnPlayerGetTarget(Node2D target)
    {
        SpawnTarget(target);
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
}
