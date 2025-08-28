using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    [Export] private float _deathWaitTime = 0.5f;

    [ExportCategory("UI")]
    [Export] private Control _GUIControl;
    [Export] private Label _scoreLabel;
    [Export] private Label _gameOverLabel;
    [Export] private Button _clickToContinueButton;
    [Export] private Leaderboard _leaderboard;

    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private int _score = 0;
    private bool _gameOver = false;

    public override void _Ready()
    {
        this.AssertNotNull(_player);
        this.AssertNotNull(_grid);
        this.AssertNotNull(_difficultyCurve);
        this.AssertNotNull(_GUIControl);

        Position = GetViewportRect().Size / 2f;

        rng.Randomize();
        SpawnTarget(_target);

        UpdateScore(0);
        if (_gameOverLabel != null)
            _GUIControl.RemoveChild(_gameOverLabel);
        if (_clickToContinueButton != null)
        {
            _clickToContinueButton.ProcessMode = ProcessModeEnum.Always;
            _GUIControl.RemoveChild(_clickToContinueButton);
        }
        if (_leaderboard != null)
            _GUIControl.RemoveChild(_leaderboard);

        _player.Speed = _minSpeed;

        _player.Debug = MainScene.ForceDebug;

        _player.OnTargetAquired += OnPlayerGetTarget;
        _player.OnTailCollide += OnPlayerDeath;
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
    public async void OnPlayerDeath(Vector2 deathPos)
    {
        GetTree().Paused = true;
        if (_gameOverLabel != null)
        {
            _GUIControl.RemoveChild(_scoreLabel);
            _GUIControl.AddChild(_gameOverLabel);

            await Task.Delay((int)(_deathWaitTime * 1000));

            await AnimateLabelToLabel(_scoreLabel, _gameOverLabel);
        }

        _gameOver = true;

        if (_clickToContinueButton != null)
        {
            _GUIControl.AddChild(_clickToContinueButton);
            _clickToContinueButton.Pressed += ReturnToMenu;
        }
        if (_leaderboard != null)
        {
            _GUIControl.AddChild(_leaderboard);
            _leaderboard.Init(_score);
        }
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
    private async Task AnimateLabelToLabel(Label target, Label label, float duration = 1f)
    {
        label.ProcessMode = ProcessModeEnum.Always;
        label.SetAnchorsPreset(Control.LayoutPreset.TopWide, false);

        float scale = target.LabelSettings.FontSize / (float)label.LabelSettings.FontSize;
        this.Log(scale);

        Tween tween = label.CreateTween();
        tween.SetParallel();
        tween.TweenProperty(label, "scale", new Vector2(scale, scale), duration);
        tween.TweenProperty(label, "position", target.Position, duration);

        await Task.Delay((int)(duration * 1000f));
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("Pause"))
        {
            OnPlayerDeath(Vector2.Zero);
            GetViewport().SetInputAsHandled();
        }
    }
    public void ReturnToMenu()
    {
        if (_leaderboard != null)
        {
            _leaderboard.SaveScore();
        }
        GetTree().Paused = false;
        MainScene.ReturnToMenu();
    }
}
