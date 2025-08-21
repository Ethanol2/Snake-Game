using Godot;
using System;
using System.Collections.Generic;
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
	[Export] private bool _drawGridCenters = false;

	// Privates
	private Vector2 _viewportSize;

	// Properties
	public int GridWidth => _gridSize;
	public int GridHeight => _verticalGridSize;
	public int GridSquareCount => _gridSize * _verticalGridSize;
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

		while (currentPos.X <= (_viewportSize.X / 2f) + 1)
		{
			DrawLine(
				new Vector2(currentPos.X, currentPos.Y),
				new Vector2(currentPos.X, -currentPos.Y),
				_gridColour,
				2f,
				true
				);

			currentPos.X += _squareSize.X;
		}

		currentPos = new Vector2(_viewportSize.X / 2f, _viewportSize.Y / -2f);
		currentPos.Y -= _squareSize.Y;

		while (currentPos.Y <= (_viewportSize.Y / 2f) + 1)
		{
			DrawLine(
				new Vector2(currentPos.X, currentPos.Y),
				new Vector2(-currentPos.X, currentPos.Y),
				_gridColour,
				2f,
				true
				);

			currentPos.Y += _squareSize.Y;
		}

		if (!_drawGridCenters)
			return;

		for (int x = 0; x < GridWidth; x++)
		{
			for (int y = 0; y < GridHeight; y++)
			{
				Vector2 labelPos = ConvertCoordinateToPosition(new Vector2I(x, y));

				Vector2 size = Vector2.One * 0.2f;

				DrawLine(
					labelPos + SquareSize * size,
					labelPos - SquareSize * size,
					Colors.Blue.Lerp(Colors.Red, (float)x / GridWidth)
				);

				size.X *= -1f;

				DrawLine(
					labelPos + SquareSize * size,
					labelPos - SquareSize * size,
					Colors.Blue.Lerp(Colors.Red, (float)y / GridHeight)
				);
			}
		}
	}

	// Utility

	public Vector2 ConvertPosition(Vector2 position)
	{
		Vector2 newPosition = position + _viewportSize;

		newPosition.X = SnapToGrid(newPosition.X, _gridSize % 2 == 0 ? SquareSize.X / 2f : 0f, SquareSize.X);
		newPosition.Y = SnapToGrid(newPosition.Y, _verticalGridSize % 2 == 0 ? SquareSize.Y / 2f : 0f, SquareSize.Y);

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
	public Vector2 WrapEdge(Vector2 position) => WrapEdge(position, out bool temp);
	public Vector2 WrapEdge(Vector2 position, out bool wrapped)
	{
		Vector2 newPosition = position;
		Vector2 limits = _viewportSize / 2f;
		wrapped = false;

		if (position.X > limits.X)
		{
			newPosition.X += -_viewportSize.X;
			wrapped = true;
		}
		else if (position.X < -limits.X)
		{
			newPosition.X += _viewportSize.X;
			wrapped = true;
		}

		if (position.Y > limits.Y)
		{
			newPosition.Y += -_viewportSize.Y;
			wrapped = true;
		}
		else if (position.Y < -limits.Y)
		{
			newPosition.Y += _viewportSize.Y;
			wrapped = true;
		}

		return newPosition;
	}
	public Vector2 GetSpriteScale(Sprite2D sprite) => SquareSize / sprite.Texture.GetSize();
	public Vector2I ConvertPositionToCoordinates(Vector2 position)
	{
		position += _viewportSize;
		position /= SquareSize;
		return new Vector2I(Mathf.RoundToInt(position.Y), Mathf.RoundToInt(position.Y));
	}
	public List<Vector2I> ConvertPositionsToCoordinates(List<Vector2> positions)
	{
		List<Vector2I> coordinates = new List<Vector2I>();

		foreach (Vector2 pos in positions)
			coordinates.Add(ConvertPositionToCoordinates(pos));

		return coordinates;
	}
	public Vector2 ConvertCoordinateToPosition(Vector2I coordinate)
	{
		Vector2 pos = coordinate;
		pos *= SquareSize;
		pos -= _viewportSize / 2f;
		pos += SquareSize / 2f;

		return pos;
	}

}
