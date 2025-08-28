using Godot;
using System;

public partial class MainScene : Node
{
	private static MainScene _Instance;

	[Export] private PackedScene _gameScene;
	[Export] private Control _mainMenu;
	[Export] private string _scoresSaveFile = "user://scores.json";

	[ExportCategory("Debug")]
	[Export] private bool _forceDebugEditor = false;
	[Export] private bool _forceDebugBuild = false;
	[Export] private Node2D _activeGame;

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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.AssertNotNull(_gameScene);
		this.AssertNotNull(_mainMenu);

		if (_Instance && _Instance != this)
		{
			this.Log("An instance already exists. Destroying");
			QueueFree();
			return;
		}

		_Instance = this;
		ScoreKeeper.Init(_scoresSaveFile);
	}
	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.Key1))
		{
			ScoreKeeper.TestDataWrite();
		}
    }

	public void _StartGame()
	{
		if (_activeGame == null)
		{
			_activeGame = _gameScene.Instantiate() as Node2D;
			AddChild(_activeGame);
			RemoveChild(_mainMenu);
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
	}
	public void _ExitGame()
	{
		GetTree().Quit();
	}

	// Static Methods
	public static void StartGame() { if (_Instance) _Instance._StartGame(); }
	public static void ReturnToMenu() { if (_Instance) _Instance._ReturnToMenu(); }
	public static void ExitGame() { if (_Instance) _Instance._ExitGame(); }

	public static implicit operator bool(MainScene instance) => instance != null;	
}
