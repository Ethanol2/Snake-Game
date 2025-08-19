using Godot;
using System;

[Tool]
public partial class GridObject : Area2D
{
	[ExportCategory("Resources")]
	[Export]
	private Texture2D _spriteTexture
	{
		get => _sprite?.Texture;
		set
		{
			if (_sprite == null)
				return;

			_sprite.Texture = value;
		}
	}

	[ExportGroup("Objects")]
	[Export] private Sprite2D _sprite;
	[Export] private CollisionShape2D _collider;

	// Properties
	public Sprite2D Sprite => _sprite;
	public CollisionShape2D Collider => _collider;
	public Variant ParentScript => GetParent().GetScript();
}
