using Godot;
using System;

public static class DebugLog
{
    public static void Log(this Node node, object message)
    {
        GD.Print($"[{node.Name}] {message}\n\t{System.Environment.StackTrace}");
    }
    public static void Log(this Node node, object message, params object[] targets)
    {
        GD.Print($"[{node.Name}] {message}", targets);
    }

    public static void LogError(this Node node, object message)
    {
        GD.PrintErr($"[{node.Name}] {message}\n\t{System.Environment.StackTrace}");
    }
    public static void LogError(this Node node, object message, params object[] targets)
    {
        GD.PrintErr($"[{node.Name}] {message}\n\t{System.Environment.StackTrace}", targets);
    }
}
