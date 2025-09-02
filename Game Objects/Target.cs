using Godot;
using System;
using System.Threading.Tasks;

public partial class Target : Node2D
{
	[Export] private GridObject _base;
	[Export] private Grid _grid;

	// Called when the node enters the scene tree for the first time.
	public async override void _Ready()
	{
		this.AssertNotNull(_grid);
		this.AssertNotNull(_base);

		while (!_grid.IsNodeReady())
			await Task.Delay((int)(GetProcessDeltaTime() * 1000d));
			
		_base.Scale = _grid.GetSpriteScale(_base.Sprite);
	}
}
