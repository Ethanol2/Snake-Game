using Godot;
using System;

public partial class MainScene : Node
{
	private static MainScene _Instance;

	[Export] private PackedScene _gameScene;
	[Export] private Control _mainMenu;
	[Export] private Leaderboard _leaderboard;
	[Export] private string _scoresSaveFile = "user://scores.json";

	[ExportCategory("Settings Controls")]
	[Export] private Label _speedControlLabel;
	[Export] private HSlider _speedControl;
	[Export] private Label _gridControlLabel;
	[Export] private HSlider _gridControl;
	[Export] private CheckButton _edgeWrapControl;
	[Export] private CheckButton _musicControl;

	[ExportCategory("Debug")]
	[Export] private bool _forceDebugEditor = false;
	[Export] private bool _forceDebugBuild = false;
	[Export] private Node2D _activeGame;
	[Export] private RuleSet _activeRules;

	// Properties
	public static MainScene Instance => _Instance;
	public static bool ForceDebug
	{
		get
		{
			if (_Instance)
			{
				if (Engine.IsEditorHint())
					return _Instance._forceDebugEditor;
				else
					return _Instance._forceDebugBuild;
			}

			return false;
		}
	}
	public static RuleSet Rules => _Instance ? _Instance._activeRules : new RuleSet();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.AssertNotNull(null, _gameScene, _mainMenu, _leaderboard);

		if (_Instance && _Instance != this)
		{
			this.Log("An instance already exists. Destroying");
			QueueFree();
			return;
		}

		_Instance = this;

		DataKeeper.Init(_scoresSaveFile);

		_activeRules = DataKeeper.ActiveRuleSet;
		_activeRules.OnChanged += OnRulesChanged;

		InverseSetRules();
		RulesControlsSubscribe();

		_leaderboard.InitDisplayOnly(_activeRules);
	}
	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.Key1))
		{
			DataKeeper.TestDataWrite();
		}
    }

	public void _StartGame()
	{
		if (_activeGame == null)
		{
			_activeGame = _gameScene.Instantiate() as Node2D;
			AddChild(_activeGame);
			RemoveChild(_mainMenu);

			DataKeeper.ActiveRuleSet = _activeRules;
			DataKeeper.WriteToDisk();

			AudioManager.PlayGameStart();
		}
	}
	public void _ReturnToMenu()
	{
		if (_activeGame != null)
		{
			RemoveChild(_activeGame);
			_activeGame.QueueFree();
			_activeGame = null;
		}
		AddChild(_mainMenu);
		_leaderboard.InitDisplayOnly(_activeRules);
	}
	public void _ExitGame()
	{
		GetTree().Quit();
	}
	public void _SetEdgeWrap(bool value) => _activeRules.EdgeWrap = value;
	public void _SetSpeed(double speed) => _activeRules.SpeedDifficulty = (RuleSet.SPEEDMODE)Mathf.RoundToInt(speed);
	public void _SetSpeed(RuleSet.SPEEDMODE speed) => _activeRules.SpeedDifficulty = speed;
	public void _SetGridSize(double index) => _activeRules.SetGridSizeIndex(Mathf.RoundToInt(index));
	public void _SetGridSize(int index) => _activeRules.SetGridSizeIndex(index);
	public void _SetMusicEnabled(bool value) => DataKeeper.MusicEnabled = value;
	public void _SetRules()
	{
		if (DebugLog.CheckNull(_edgeWrapControl, _speedControl, _speedControlLabel, _gridControl, _gridControlLabel))
			return;

		_activeRules.EdgeWrap = !_edgeWrapControl.ButtonPressed;

		_activeRules.SpeedDifficulty = (RuleSet.SPEEDMODE)Mathf.RoundToInt(_speedControl.Value);
		_speedControlLabel.Text = "Speed: " + _activeRules.SpeedDifficulty;

		_activeRules.SetGridSizeIndex(Mathf.RoundToInt(_gridControl.Value));
		_gridControlLabel.Text = "Grid Size: " + _activeRules.GridSize;
	}
	private void RulesControlsSubscribe()
	{
		if (DebugLog.CheckNull(_edgeWrapControl, _speedControl, _speedControlLabel, _gridControl, _gridControlLabel, _musicControl))
			return;

		_edgeWrapControl.Toggled += _SetEdgeWrap;
		_speedControl.ValueChanged += _SetSpeed;
		_gridControl.ValueChanged += _SetGridSize;
		_musicControl.Toggled += _SetMusicEnabled;
	}
	private void InverseSetRules()
	{
		if (DebugLog.CheckNull(_edgeWrapControl, _speedControl, _speedControlLabel, _gridControl, _gridControlLabel, _musicControl))
			return;

		_edgeWrapControl.ButtonPressed = _activeRules.EdgeWrap;

		_speedControlLabel.Text = "Speed: " + _activeRules.SpeedDifficulty;
		_speedControl.Value = (double)_activeRules.SpeedDifficulty;

		_gridControlLabel.Text = "Grid Size: " + _activeRules.GridSize;
		_gridControl.Value = _activeRules.GetGridSizeIndex();

		_musicControl.ButtonPressed = DataKeeper.MusicEnabled;
	}
	private void OnRulesChanged(RuleSet ruleSet)
	{
		this.Log($"Rules changed to: " + ruleSet.GetIntValue());
		_leaderboard.InitDisplayOnly(ruleSet);
		_speedControlLabel.Text = "Speed: " + ruleSet.SpeedDifficulty;
		_gridControlLabel.Text = "Grid Size: " + ruleSet.GridSize;

	}
	public void _ClearSaveFile()
	{
		DataKeeper.ClearSaveFile();
		DataKeeper.ActiveRuleSet = _activeRules;
		DataKeeper.WriteToDisk();
	}

	// Static Methods
	public static void StartGame() { if (_Instance) _Instance._StartGame(); }
	public static void ReturnToMenu() { if (_Instance) _Instance._ReturnToMenu(); }
	public static void ExitGame() { if (_Instance) _Instance._ExitGame(); }

	public static implicit operator bool(MainScene instance) => instance != null;	
}
