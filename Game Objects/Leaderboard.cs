using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Leaderboard : Panel
{
	[Export] private PackedScene _scoreBox;
	[Export] private Control _scoreBoxParent;

	private List<ScoreBox> _scoreBoxes = new List<ScoreBox>();
	private ScoreBox _activeBox;

	public void Init(int newScore)
	{
		List<int> sortedScores = new List<int>(newScore);
		if (ScoreKeeper.Initialized)
		{
			sortedScores.AddRange(ScoreKeeper.GetScores());
			sortedScores.Sort();
			sortedScores.Reverse();
			sortedScores = sortedScores.Slice(0, 10 > sortedScores.Count ? sortedScores.Count : 10);

			Dictionary<string, List<int>> scores = ScoreKeeper.GetScoresAndNames();
			foreach (string name in scores.Keys)
			{
				foreach (int score in scores[name])
				{
					if (_scoreBoxes.Count >= 9)
						break;
					if (!sortedScores.Contains(score))
						continue;

					ScoreBox newBox = _scoreBox.Instantiate() as ScoreBox;
					newBox.Init(false, sortedScores.IndexOf(score) + 1, score, name);
					_scoreBoxes.Add(newBox);
				}

			}
		}

		ScoreBox activeBox = _scoreBox.Instantiate() as ScoreBox;

		int index = 1;
		if (sortedScores.Count > 1)
		{
			sortedScores.IndexOf(newScore);
			index = index < 0 ? 10 : index + 1;
		}

		activeBox.Init(true, index, newScore, ScoreKeeper.LatestName);
		activeBox.OnNameSet += OnActiveNameSet;
		_scoreBoxes.Add(activeBox);

		_activeBox = activeBox;

		_scoreBoxes = _scoreBoxes.OrderBy(x => x.Score).Reverse().ToList();
		foreach (ScoreBox child in _scoreBoxes)
		{
			_scoreBoxParent.AddChild(child);
		}
	}
	private void OnActiveNameSet(string newName)
	{
		if (ScoreKeeper.Initialized)
		{
			ScoreKeeper.LatestName = newName;
		}
	}
	public void SaveScore()
	{
		ScoreKeeper.AddScore(_activeBox.PlayerName, _activeBox.Score);
		ScoreKeeper.WriteToDisk();
	}
}
