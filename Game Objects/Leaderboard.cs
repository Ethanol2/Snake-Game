using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Leaderboard : Panel
{
	[Export] private PackedScene _scoreBox;
	[Export] private Control _scoreBoxParent;
	[Export] private Label _noScoresLabel;

	private List<ScoreBox> _scoreBoxes;
	private ScoreBox _activeBox;

    public override void _Ready()
    {
		RemoveChild(_noScoresLabel);
    }
	public void InitDisplayOnly(int rules)
	{
		List<int> sortedScores = new List<int>();

		FreeScoreBoxes();
		_scoreBoxes = HandleSaveData(ref sortedScores, rules);

		_scoreBoxes = _scoreBoxes.OrderBy(x => x.Score).Reverse().ToList();
		foreach (ScoreBox child in _scoreBoxes)
		{
			_scoreBoxParent.AddChild(child);
		}

		if (_scoreBoxes.Count == 0)
		{
			if (!_noScoresLabel.IsInsideTree())
				AddChild(_noScoresLabel);
		}
		else
		{
			if (_noScoresLabel.IsInsideTree())
				RemoveChild(_noScoresLabel);
		}
	}
	public void Init(int newScore, int rules)
	{
		List<int> sortedScores = new List<int>(newScore);

		FreeScoreBoxes();
		_scoreBoxes = HandleSaveData(ref sortedScores, rules);

		ScoreBox activeBox = _scoreBox.Instantiate() as ScoreBox;

		int index = 1;
		if (sortedScores.Count > 1)
		{
			sortedScores.IndexOf(newScore);
			index = index < 0 ? 10 : index + 1;
		}

		activeBox.Init(true, index, newScore, DataKeeper.LatestName);
		activeBox.OnNameSet += OnActiveNameSet;
		_scoreBoxes.Add(activeBox);

		_activeBox = activeBox;

		_scoreBoxes = _scoreBoxes.OrderBy(x => x.Score).Reverse().ToList();
		foreach (ScoreBox child in _scoreBoxes)
		{
			_scoreBoxParent.AddChild(child);
		}
	}
	private List<ScoreBox> HandleSaveData(ref List<int> sortedScores, int rules)
	{
		List<ScoreBox> scoreBoxes = new List<ScoreBox>();

		if (DataKeeper.Initialized)
		{
			sortedScores.AddRange(DataKeeper.GetScores(rules));
			sortedScores.Sort();
			sortedScores.Reverse();
			sortedScores = sortedScores.Slice(0, 10 > sortedScores.Count ? sortedScores.Count : 10);

			Dictionary<string, List<int>> scores = DataKeeper.GetScoresAndNames(MainScene.Rules);
			foreach (string name in scores.Keys)
			{
				foreach (int score in scores[name])
				{
					if (scoreBoxes.Count >= 9)
						break;
					if (!sortedScores.Contains(score))
						continue;

					ScoreBox newBox = _scoreBox.Instantiate() as ScoreBox;
					newBox.Init(false, sortedScores.IndexOf(score) + 1, score, name);
					scoreBoxes.Add(newBox);
				}

			}
		}

		return scoreBoxes;
	}
	private void FreeScoreBoxes()
	{
		if (_scoreBoxes != null)
		{
			foreach (ScoreBox box in _scoreBoxes)
			{
				if (box.IsInsideTree())
				{
					_scoreBoxParent.RemoveChild(box);
				}
				box.QueueFree();
			}
		}
	}
	private void OnActiveNameSet(string newName)
	{
		if (DataKeeper.Initialized)
		{
			DataKeeper.LatestName = newName;
		}
	}
	public void SaveScore(int rules)
	{
		if (_activeBox != null)
		{
			DataKeeper.AddScore(rules, _activeBox.PlayerName, _activeBox.Score);
			DataKeeper.WriteToDisk();
		}
	}
}
