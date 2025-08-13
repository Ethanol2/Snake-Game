using Godot;
using System;

public partial class Grid : Node2D
{
	[Export] private int _gridSize = 50;
	[Export] private bool _drawGrid = false;
	[Export] private Color _gridColour = Colors.White;

	// Privates
	private Vector2 _squareSize;
	private Vector2 _viewportSize;

	// Properties
	public int GridSize => _gridSize;
	public Vector2 SquareSize => _squareSize;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_viewportSize = GetViewportRect().Size;
		float aspectRatio = _viewportSize.Y / _viewportSize.X;
		_squareSize = new Vector2(_viewportSize.X / _gridSize, _viewportSize.Y / Mathf.Round(_gridSize * aspectRatio));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public override void _Draw()
	{
		if (!_drawGrid)
			return;

		Vector2 currentPos = new Vector2(_viewportSize.X / -2f, _viewportSize.Y / 2f);
		currentPos.X -= _squareSize.X;

		while (currentPos.X <= _viewportSize.X / 2f)
		{
			DrawLine(
				new Vector2(currentPos.X, currentPos.Y),
				new Vector2(currentPos.X, -currentPos.Y),
				_gridColour,
				-1f,
				true
				);

			currentPos.X += _squareSize.X;
		}

		currentPos = new Vector2(_viewportSize.X / 2f, _viewportSize.Y / -2f);
		currentPos.Y -= _squareSize.Y;

		while (currentPos.Y <= _viewportSize.Y / 2f)
		{
			DrawLine(
				new Vector2(currentPos.X, currentPos.Y),
				new Vector2(-currentPos.X, currentPos.Y),
				_gridColour,
				-1f,
				true
				);

			currentPos.Y += _squareSize.Y;
		}
	}

	public Vector2 GetPosition(Vector2 realPosition)
	{
		GD.PrintErr("This function needs to be completed");
		return realPosition;
	}
}
