using Godot;
using System;

public partial class Target : Node2D
{
	[Export] private GridObject _base;
	[Export] private Grid _grid;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.AssertNotNull(_grid);
		this.AssertNotNull(_base);

		_base.Scale = _grid.GetSpriteScale(_base.Sprite);
	}
}
