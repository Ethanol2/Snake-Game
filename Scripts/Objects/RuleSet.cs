using Godot;
using System;
using System.Data;

public partial class RuleSet : Resource
{
	// Edge Wrap
	[Export] private bool _edgeWrap = true;
	public bool EdgeWrap { get => _edgeWrap; set { _edgeWrap = value; OnChanged?.Invoke(this); } }

	// Speed
	[Export] private SPEEDMODE _speedDifficulty = SPEEDMODE.NORMAL;
	public SPEEDMODE SpeedDifficulty
	{
		get => _speedDifficulty;
		set { _speedDifficulty = value; OnChanged?.Invoke(this); } }
	public float SpeedMultipliyer => SPEED_MODES[(int)_speedDifficulty];

	// Grid Size
	[Export] private int _gridSizeIndex = 2;
	public int GridSize
	{
		get => GRID_SIZES[_gridSizeIndex];
	}
	public static readonly int[] GRID_SIZES = { 10, 30, 50, 70, 90, 110, 150, 200, 300, 1000 };

	// Events
	public event Action<RuleSet> OnChanged;

	// Support Objects
	public enum SPEEDMODE { SLOW, SLOWER, NORMAL, FASTER, FAST, STUPID }
	public static readonly float[] SPEED_MODES = { 0.25f, 0.5f, 1f, 1.5f, 2f, 5f };

	// Methods
	public int GetIntValue()
	{
		int value = 0;

		// Buffer
		value = AddValue(value, 1);

		// Rules
		value = AddValue(value, _edgeWrap ? 1 : 0);
		value = AddValue(value, (int)_speedDifficulty);
		value = AddValue(value, _gridSizeIndex);

		return value;
	}
	private int AddValue(int value, int property)
	{
		value *= 10;
		return value + property;
	}
	public static RuleSet SetIntValue(int value)
	{
		RuleSet rules = new RuleSet();

		if (value < 0)
		{
			return rules;
		}

		string convert = value.ToString();
		int[] digits = new int[convert.Length];
		for (int i = 0; i < convert.Length; i++)
		{
			if (int.TryParse(convert[i].ToString(), out int parsed))
				digits[i] = parsed;
			else
				digits[i] = 0;
		}

		int rulesLength = 3;
		if (digits.Length < rulesLength + 1)
			throw new Exception($"Rule format is invalid for parsing. Expected length: {rulesLength + 1}. First character is discarded");

		rules._edgeWrap = digits[1] == 0 ? false : true;
		rules._speedDifficulty = (SPEEDMODE)digits[2];
		rules._gridSizeIndex = digits[3];

		rules.OnChanged?.Invoke(rules);

		return rules;
	}
	public void SetGridSizeIndex(int index)  {_gridSizeIndex = index; OnChanged?.Invoke(this); }
	public int GetGridSizeIndex() => _gridSizeIndex;


	public static implicit operator int(RuleSet instance) => instance.GetIntValue();
	public static implicit operator RuleSet(int value) => SetIntValue(value);
}
