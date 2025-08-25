using Godot;
using System;
using System.Diagnostics;

[StackTraceHidden]
public static class DebugLog
{
    public static void Log(string name, object message, bool stacktrace = false, int stacktraceLines = 3) =>
        GD.Print($"[{name}] {message}" + (stacktrace ? $"\n\t{SliceStacktrace(System.Environment.StackTrace, stacktraceLines)}" : ""));

    public static void Log(this Node node, object message, bool stacktrace = false, int stacktraceLines = 3) =>
        GD.Print($"[{node.Name}] {message}" + (stacktrace ? $"\n\t{SliceStacktrace(System.Environment.StackTrace, stacktraceLines)}" : ""));

    public static void LogError(this Node node, object message, bool stacktrace = false, int stacktraceLines = 3) =>
        GD.PrintErr($"[{node.Name}] {message}" + (stacktrace ? $"\n\t{SliceStacktrace(System.Environment.StackTrace, stacktraceLines)}" : ""));

    public static void LogError(string name, object message, bool stacktrace = false, int stacktraceLines = 3) =>
        GD.PrintErr($"[{name}] {message}" + (stacktrace ? $"\n\t{SliceStacktrace(System.Environment.StackTrace, stacktraceLines)}" : ""));

#nullable enable
    [StackTraceHidden]
    public static void AssertNotNull(this Node node, object @object, string? message = null)
    {
        if (@object == null)
        {
            GD.PrintErr($"[{node.Name}] NullReferenceException" + (message == null ? $" ->\n" + SliceStacktrace(System.Environment.StackTrace, 1) : " -> " + message));

            throw new NullReferenceException(message);
        }
    }
#nullable disable

    private static string SliceStacktrace(string stacktrace, int lineCount)
    {
        string[] lines = stacktrace.Split('\n');
        string output = "";
        for (int i = 1; i < lineCount + 1 && i < lines.Length; i++)
        {
            output += lines[i] + "\n";
        }
        return output;
    }
}
