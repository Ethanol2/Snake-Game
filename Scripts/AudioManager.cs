using Godot;
using System;
using System.Threading.Tasks;

public partial class AudioManager : Node
{
	private static AudioManager _Instance;

	[Export] private AudioStreamPlayer[] _audioStreamPlayers;

	[ExportCategory("UI")]
	[Export] private AudioStream[] _uiSelect;
	[Export] private AudioStream[] _uiClick;
	[Export] private AudioStream _toggleOn;
	[Export] private AudioStream _toggleOff;
	[Export] private AudioStream[] _gameStart;

	[ExportCategory("Game")]
	[Export] private AudioStream[] _success;
	[Export] private AudioStream[] _death;
	[Export] private AudioStream[] _celebration;

	private PlayerHandler[] _players;
	private RandomNumberGenerator _rand;

	public static AudioManager Instance => _Instance;

	// Called when the node enters the scene tree for the first time.
	public async override void _Ready()
	{
		if (_audioStreamPlayers.Length < 0)
		{
			this.LogError("There needs be at least one player assigned");
			return;
		}

		if (_Instance && _Instance != this)
		{
			this.LogError("An instance already exists. Destroying");
			Free();
			return;
		}
		_Instance = this;

		_rand = new RandomNumberGenerator();

		// Delay to allow stuff to initialize without sfx
		await Task.Delay(1000);

		_players = new PlayerHandler[_audioStreamPlayers.Length];
		for (int i = 0; i < _players.Length; i++)
		{
			_players[i] = new PlayerHandler(_audioStreamPlayers[i]);
		}

		GetTree().NodeAdded += OnNodeAddedToTree;
		//GetTree().NodeRemoved += OnNodeRemovedFromTree;
		GetAllNodesRecursive(GetParent());
	}
	private void GetAllNodesRecursive(Node parent)
	{
		foreach (Node node in parent.GetChildren())
		{
			OnNodeAddedToTree(node);
			GetAllNodesRecursive(node);
		}
	}
	private void OnNodeAddedToTree(Node node)
	{
		if (node is not Control) return;

		Control control = node as Control;

		if (node is Button button)
		{
			this.Log($"Adding \"{node.Name}\" to sound subscription");

			//control.MouseEntered -= OnControlFocused;
			control.MouseEntered += OnControlFocused;
			if (button is CheckButton toggle)
			{
				//toggle.Toggled -= OnToggleChanged;
				toggle.Toggled += OnToggleChanged;
			}
			else
			{
				//button.Pressed -= OnButtonPressed;
				button.Pressed += OnButtonPressed;
			}
		}
		else if (node is Slider slider)
		{
			this.Log($"Adding \"{node.Name}\" to sound subscription");

			//control.MouseEntered += OnControlFocused;
			//slider.ValueChanged -= OnSliderChanged;
			slider.ValueChanged += OnSliderChanged;
		}
	}
	private void OnNodeRemovedFromTree(Node node)
	{
		if (node is not Control) return;

		Control control = node as Control;

		if (node is Button button)
		{
			this.Log($"Removing \"{node.Name}\" from sound subscription");

			control.FocusEntered -= OnControlFocused;

			if (button is CheckButton toggle)
				toggle.Toggled -= OnToggleChanged;
			else
				button.Pressed -= OnButtonPressed;
		}
		else if (node is Slider slider)
		{
			this.Log($"Removing \"{node.Name}\" from sound subscription");

			control.FocusEntered -= OnControlFocused;
			slider.ValueChanged -= OnSliderChanged;
		}
	}
	private void OnControlFocused() => _PlayRandom(_uiSelect);
	private void OnSliderChanged(double value) => OnButtonPressed();
	private void OnButtonPressed() => _PlayRandom(_uiClick);
	private void OnToggleChanged(bool value)
	{
		if (value)
			_PlayOnce(_toggleOn);
		else
			_PlayOnce(_toggleOff);
	}

	private void _PlayOnce(AudioStream audioClip)
	{
		//this.Log($"Playing \"{audioClip.ResourcePath}\"", true);

		foreach (PlayerHandler player in _players)
		{
			if (player.Ready)
			{
				player.PlayOnce(audioClip);
				return;
			}
		}

		this.Log("PlayOnce was called without any players available. Consider adding more players");
	}
	private void _PlayRandom(AudioStream[] audioClips)
	{
		if (audioClips.Length == 0) return;

		int index = _rand.RandiRange(0, audioClips.Length - 1);
		_PlayOnce(audioClips[index]);
	}

	public static void PlayOnce(AudioStream audioClip)
	{
		if (!Instance) return;
		_Instance._PlayOnce(audioClip);
	}
	public static void PlayRandom(AudioStream[] audioClips)
	{
		if (!Instance) return;
		_Instance._PlayRandom(audioClips);
	}
	public static void PlayGameSuccess()
	{
		if (!_Instance) return;
		_Instance._PlayRandom(_Instance._success);
	}
	public static void PlayGameDeath()
	{
		if (!_Instance) return;
		_Instance._PlayRandom(_Instance._death);
	}
	public static void PlayGameCelebration()
	{
		if (!_Instance) return;
		_Instance._PlayRandom(_Instance._celebration);
	}
	public static void PlayGameStart()
	{
		if (!_Instance) return;
		_Instance._PlayRandom(_Instance._gameStart);
	}

	public static implicit operator bool(AudioManager instance) => instance != null;

	public class PlayerHandler
	{
		public AudioStreamPlayer Player;

		public bool Ready { get; private set; } = true;

		public PlayerHandler(AudioStreamPlayer player)
		{
			Player = player;
		}
		public async void PlayOnce(AudioStream audioClip)
		{
			if (!Ready) return;

			int duration = Mathf.RoundToInt(audioClip.GetLength() * 1000d);
			Player.Stream = audioClip;
			Player.Play();

			Ready = false;
			await Task.Delay(duration);

			Player.Stop();
			Player.Stream = null;

			Ready = true;
		} 
	}
}
