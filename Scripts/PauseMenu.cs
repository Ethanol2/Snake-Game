using Godot;
using System;

public partial class PauseMenu : CanvasLayer
{
	[Export] private Button _continueButton;
	[Export] private Button _exitButton;

	public Button ContinueButton => _continueButton;
	public Button ExitButton => _exitButton;
}
