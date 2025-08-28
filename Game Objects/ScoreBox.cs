using Godot;
using System;

public partial class ScoreBox : Panel
{
	[Export] private Control _uiParent;
	[Export] private Label _placementLabel;
	[Export] private LineEdit _nameInput;
	[Export] private Label _nameLabel;
	[Export] private Label _scoreLabel;

	private bool _isActive = false;
	private int _score;
	private string _name;
	private double _blinkT = 0.5d;
	private bool _blinkState = true;
	private int _placement;

	[ExportCategory("Values")]
	[Export]
	public int Score
	{
		get => _score;
		set
		{
			_scoreLabel.Text = value.ToString();
			_score = value;
		}
	}
	[Export]
	public string PlayerName
	{
		get => _name;
		set
		{
			_name = value;
			_nameLabel.Text = value;
			_nameInput.PlaceholderText = value;
		}
	}
	[Export]
	public bool IsActive { get => _isActive; set => _isActive = value; }
	public int Placement => _placement;

	public Action<string> OnNameSet;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.AssertNotNull(null, _placementLabel, _nameLabel, _nameInput, _scoreLabel);
	}
	public override void _Process(double delta)
	{
		if (_isActive)
		{
			_blinkT += delta / _nameInput.CaretBlinkInterval;
			if (_blinkT >= 1d)
			{
				_blinkT = 0d;
				_blinkState = !_blinkState;
				_scoreLabel.Modulate = ToggleAlpha(_scoreLabel.Modulate, _blinkState);
				_placementLabel.Modulate = ToggleAlpha(_placementLabel.Modulate, _blinkState);
			}
		}
	}
	private Color ToggleAlpha(Color colour, bool state)
	{
		colour.A = state ? 1f : 0f;
		return colour;
	}

	public void Init(bool isActive, int placement, int score, string name)
	{
		Name = name;
		PlayerName = name;
		_isActive = isActive;
		Score = score;
		_placementLabel.Text = $"{placement}.";
		_placement = placement;

		if (_isActive)
		{
			_uiParent.RemoveChild(_nameLabel);
			_nameInput.TextSubmitted += OnInputTextSubmitted;
			_nameInput.TextChanged += OnInputTextChanged;
			_nameInput.Visible = true;
			CallDeferred(nameof(SetFocus));
		}
		else
		{
			_uiParent.RemoveChild(_nameInput);
			_nameLabel.Text = name;
			_nameLabel.Visible = true;
		}
	}
	private void OnInputTextChanged(string name)
	{
		if (name == string.Empty)
			_name = _nameInput.PlaceholderText;
		else
			_name = name;

		_isActive = true;
	}
	public void OnInputTextSubmitted(string name)
	{
		if (name != string.Empty)
			_name = name;

		_isActive = false;
		_scoreLabel.Modulate = ToggleAlpha(_scoreLabel.Modulate, true);
		_placementLabel.Modulate = ToggleAlpha(_placementLabel.Modulate, true);

		OnNameSet?.Invoke(_name);
	}
	private void SetFocus()
	{
		_nameInput.GrabFocus();
		_blinkT = 0f;
	}
}
