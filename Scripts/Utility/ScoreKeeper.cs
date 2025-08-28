using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public class ScoreKeeper
{
	private static ScoreKeeper _instance;

	private string _scoreFilePath = "";

	private ScoreData _data;

	public static ScoreKeeper Instance => _instance;

	public static bool Initialized => _instance;
	public static string LatestName
	{
		get => _instance ? _instance._data.LatestName : System.Environment.UserName;
		set { if (_instance) _instance._data.LatestName = value; }
	}

	public static void Init(string filePath)
	{
		if (_instance != null)
		{
			_instance.LogError("An instance already exists", true, 2);
		}
		_instance = new ScoreKeeper();
		_instance._Init(filePath);
	}
	private void _Init(string filePath)
	{
		_scoreFilePath = filePath;

		if (FileAccess.FileExists(filePath))
		{
			string fileContents;
			using (FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read))
			{
				if (file == null)
				{
					this.LogError($"There was an issue opening \"{filePath}\" ->\n" + FileAccess.GetOpenError());
					return;
				}
				fileContents = file.GetAsText();
				file.Close();
			}

			if (fileContents != string.Empty)
			{
				try
				{
					_data = System.Text.Json.JsonSerializer.Deserialize<ScoreData>(fileContents);
					return;
				}
				catch (Exception e)
				{
					this.LogError("Something went wrong parsing the score file ->\n" + e);
				}
			}
		}

		_data = new ScoreData();
		_data.LatestName = System.Environment.UserName;
	}
	public static List<int> GetScores()
	{
		if (!IsInitialized())
			return null;

		List<int> scores = new List<int>();
		foreach (string name in _instance._data.Scores.Keys)
		{
			scores.AddRange(_instance._data.Scores[name]);
		}
		return scores;
	}
	public static Dictionary<string, List<int>> GetScoresAndNames()
	{
		if (IsInitialized())
			return _instance._data.Scores;
		return null;
	}
	public static void AddScore(string name, int score)
	{
		if (IsInitialized())
		{
			_instance._data.LatestName = name;

			if (_instance._data.Scores.ContainsKey(name))
				_instance._data.Scores[name].Add(score);
			else
				_instance._data.Scores.Add(name, new List<int>() {score});
		}
	}
	public static void WriteToDisk()
	{
		if (IsInitialized())
			_instance._WriteToDisk();
	}
	private void _WriteToDisk()
	{
		string jsonText;
		try
		{
			jsonText = System.Text.Json.JsonSerializer.Serialize<ScoreData>(
				_data,
				new System.Text.Json.JsonSerializerOptions() {WriteIndented=true});
		}
		catch (Exception e)
		{
			this.LogError("Something went wrong writing to disk ->\n" + e);
			return;
		}

		using (FileAccess file = FileAccess.Open(_scoreFilePath, FileAccess.ModeFlags.Write))
		{
			if (file == null)
			{
				this.LogError($"There was an issue opening \"{_scoreFilePath}\" ->\n" + FileAccess.GetOpenError());
				return;
			}

			file.StoreString(jsonText);
			file.Close();
		}
	}
	public static void TestDataWrite()
	{
		if (!IsInitialized()) return;

		_instance._data.Scores = new Dictionary<string, List<int>>
		{
			{ "Bob", new List<int>() {10, 15} },
			{ "Jim", new List<int>() {5, 30} },
			{ "Jane", new List<int>() {6, 1}},
			{ "Craig", new List<int>() {1, 1}}
		};
		_instance._data.LatestName = "Craig";

		WriteToDisk();
	}

	private static bool IsInitialized()
	{
		if (_instance) return true;

		DebugLog.Log("ScoreKeeper", "Not initialized");
		return false;
	}
	
	public class ScoreData
	{
		public string LatestName { get; set; } = "";
		public Dictionary<string, List<int>> Scores { get; set; } = new Dictionary<string, List<int>>();
	}
	
	public static implicit operator bool(ScoreKeeper instance) => instance != null;
}
