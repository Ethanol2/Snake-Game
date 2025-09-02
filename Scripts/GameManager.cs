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
    [Export] private float _difficultyAdjust = 0.03f;
    [Export] private bool _allowEdgeWrap = true;
    [Export] private float _deathWaitTime = 0.5f;

    [ExportCategory("UI")]
    [Export] private PauseMenu _pauseMenu;
    [ExportSubgroup("GUI")]
    [Export] private CanvasLayer _guiCanvas;
    [Export] private Control _GUIControl;
    [Export] private Label _scoreLabel;
    [Export] private Label _gameOverLabel;
    [Export] private Button _clickToContinueButton;
    [Export] private Leaderboard _leaderboard;

    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private int _score = 0;
    private bool _gameOver = false;

    // Lifecycle
    public async override void _Ready()
    {
        this.AssertNotNull(_player);
        this.AssertNotNull(_grid);
        this.AssertNotNull(_GUIControl);
        this.AssertNotNull(_pauseMenu);

        Position = GetViewportRect().Size / 2f;

        rng.Randomize();

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

        _allowEdgeWrap = MainScene.Rules.EdgeWrap;

        if (MainScene.Instance)
            _player.Debug = MainScene.ForceDebug;

        _player.OnTargetAquired += OnPlayerGetTarget;
        _player.OnTailCollide += OnPlayerDeath;
        _player.OnEdgeWrapped += OnPlayerEdgeWrap;

        _guiCanvas.Layer = -1;

        _pauseMenu.Visible = false;
        _pauseMenu.ContinueButton.Pressed += OnPausedContinue;
        _pauseMenu.ExitButton.Pressed += OnPauseQuit;

        while (!_player.IsNodeReady())
            await Task.Delay((int)(GetProcessDeltaTime() * 1000d));

        _minSpeed = BaseSpeedForGrid(_grid.GridWidth);
        _player.Speed = _minSpeed;
        SpawnTarget(_target);
    }
    public async void OnPlayerDeath(Vector2 deathPos)
    {
        _guiCanvas.Layer = 1;

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

            if (_score > 0 || _player.Debug)
                _leaderboard.Init(_score, MainScene.Rules);
            else
                _leaderboard.InitDisplayOnly(MainScene.Rules);
        }
    }

    // Callbacks
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("Pause"))
        {
            if (_pauseMenu.Visible)
                OnPausedContinue();
            else
                PauseGame();

            GetViewport().SetInputAsHandled();
        }
    }
    public void OnPlayerGetTarget(Node2D target)
    {
        SpawnTarget(target);
        _score++;
        UpdateScore(_score);

        float speed = CalculateSpeed(_minSpeed, _score, _grid.GridWidth, _difficultyAdjust, MainScene.Rules.SpeedMultipliyer);

        _player.Speed = speed;
    }
    private void OnPlayerEdgeWrap(Vector2 position)
    {
        if (!_allowEdgeWrap)
        {
            OnPlayerDeath(position);
        }
    }
    private void PauseGame()
    {
        _scoreLabel.Visible = false;
        _pauseMenu.Visible = true;

        GetTree().Paused = true;
    }
    private void OnPausedContinue()
    {
        _scoreLabel.Visible = true;
        _pauseMenu.Visible = false;

        GetTree().Paused = false;
    }
    private void OnPauseQuit()
    {
        _pauseMenu.Visible = false;
        OnPlayerDeath(Vector2.Zero);
    }

    // Utility
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
    public void ReturnToMenu()
    {
        if (_leaderboard != null)
        {
            _leaderboard.SaveScore(MainScene.Rules);
        }
        GetTree().Paused = false;
        MainScene.ReturnToMenu();
    }

    // ChatGPT function
    private float CalculateSpeed(float baseSpeed, int score, int gridSize, float mod, float multiplier = 1f)
    {
        float maxSpeed = MaxSpeedForGrid(gridSize);

        return baseSpeed + (maxSpeed - baseSpeed) * (1f - Mathf.Exp(-mod * score * multiplier));
    }

    // ChatGPT function
    private float MaxSpeedForGrid(int gridSize)
    {
        float minCap = 7f;   // practical ceiling for small grids
        float maxCap = 25f;  // ceiling for massive grids

        // Normalize log scale between grid=10 → 0 and grid=1000 → 1
        float t = Mathf.Log(gridSize) / Mathf.Log(1000f);
        return Mathf.Lerp(minCap, maxCap, t);
    }

    // ChatGPT function
    private float BaseSpeedForGrid(int gridSize)
    {
        float minBase = 2.5f;  // comfortable on 10×10
        float maxBase = 8f;    // baseline for 1000×1000

        float t = Mathf.Log(gridSize) / Mathf.Log(1000f); // normalize 0–1
        return Mathf.Lerp(minBase, maxBase, t);
    }


}
