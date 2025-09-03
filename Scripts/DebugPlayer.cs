using Godot;
using System;

public partial class DebugPlayer : Node2D
{
	[Export] private Grid _grid;
	[Export] private float _speed = 10f;
	[Export] private Vector2 _position1 = Vector2.Inf;
	[Export] private Vector2 _position2 = Vector2.Inf;
	[Export] private Vector2 _direction;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			_position1 = _grid.ConvertPosition(GetLocalMousePosition());

			if (_position2 != Vector2.Inf)
			{
				Vector2 posX = _position2;
				posX.X = _position1.X;

				Vector2 posY = _position2;
				posY.Y = _position1.Y;

				if (_position2.DistanceSquaredTo(posX) > _position2.DistanceSquaredTo(posY))
					_position2 = posY;
				else
					_position2 = posX;

				_direction = (_position2 - _position1).Normalized();
			}

			QueueRedraw();
		}
		if (Input.IsMouseButtonPressed(MouseButton.Right))
		{
			_position2 = _grid.ConvertPosition(GetLocalMousePosition());

			if (_position1 != Vector2.Inf)
			{
				Vector2 posX = _position2;
				posX.X = _position1.X;

				Vector2 posY = _position2;
				posY.Y = _position1.Y;

				if (_position2.DistanceSquaredTo(posX) > _position2.DistanceSquaredTo(posY))
					_position2 = posY;
				else
					_position2 = posX;

				_direction = (_position2 - _position1).Normalized();
			}

			QueueRedraw();
		}
	}

	public override void _Draw()
	{
		if (_position1 == Vector2.Inf || _position2 == Vector2.Inf)
			return;

		DrawCircle(_position1, _grid.SquareSize.X / 5f, Colors.Green);
		DrawCircle(_position2, _grid.SquareSize.X / 5f, Colors.Red);

		int diff = Mathf.RoundToInt(((_position1 - _position2) * _direction / _grid.SquareSize).Length() * 1.01f);

		for (int i = 1; i < diff; i++)
		{
			Vector2 middlePos = _position2 - (_direction * _grid.SquareSize * (diff - i));
			DrawCircle(middlePos, _grid.SquareSize.X / 5f, Colors.Purple.Lerp(Colors.Yellow, (float)i/diff));				
		}
    }
}
