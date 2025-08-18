using Godot;
using System;
using System.Xml.Linq;

public partial class Grid : Node2D
{
	[ExportGroup("Grid Settings")]
	[Export] private int _gridSize = 50;
	[Export] private bool _drawGrid = false;
	[Export] private Color _gridColour = Colors.White;

	[ExportCategory("Debug")]
	[Export] private Vector2 _squareSize;
	[Export] private int _verticalGridSize;

	// Privates
	private Vector2 _viewportSize;

	// Properties
	public int GridSize => _gridSize;
	public Vector2 SquareSize => _squareSize;

	// Lifecycle
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_viewportSize = GetViewportRect().Size;
		float aspectRatio = _viewportSize.Y / _viewportSize.X;
		_verticalGridSize = (int)Mathf.Round(_gridSize * aspectRatio);
		_squareSize = new Vector2(_viewportSize.X / _gridSize, _viewportSize.Y / _verticalGridSize);
	}

	// Callbacks
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

	// Utility

	public Vector2 ConvertPosition(Vector2 position)
	{
		Vector2 newPosition = position + _viewportSize;

		newPosition.X = SnapToGrid(newPosition.X, _gridSize % 2 == 0 ? SquareSize.X / 2f : 0f, SquareSize.X);
		newPosition.Y = SnapToGrid(newPosition.Y, _verticalGridSize % 2 == 0 ? SquareSize.Y / 2f : 0f,  SquareSize.Y);

		return newPosition - _viewportSize;
	}
	private float SnapToGrid(float pos, float offset, float squareSize)
	{
		float diff = (pos + offset) % squareSize;
		if (diff > squareSize / 2f)
		{
			pos += squareSize - diff;
		}
		else
		{
			pos -= diff;
		}
		return pos;
	}
	public Vector2 WrapEdge(Vector2 realPosition, Vector2 gridPosition)
	{
		Vector2 newPosition = realPosition;
		Vector2 limits = _viewportSize / 2f;

		if (realPosition.X > limits.X)
			newPosition.X = -limits.X;
		else if (realPosition.X < -limits.X)
			newPosition.X = limits.X;

		if (realPosition.Y > limits.Y)
			newPosition.Y = -limits.Y;
		else if (realPosition.Y < -limits.Y)
			newPosition.Y = limits.Y;

		return newPosition;
	}

}
