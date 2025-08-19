using Godot;
using System;
using System.Diagnostics;

public static class DebugLog
{
    public static void Log(this Node node, object message, bool stacktrace = false)
    {
        GD.Print($"[{node.Name}] {message}" + (stacktrace ? "\n\t{System.Environment.StackTrace}" : ""));
    }

    public static void LogError(this Node node, object message, bool stacktrace = false)
    {
        GD.PrintErr($"[{node.Name}] {message}" + (stacktrace ? "\n\t{System.Environment.StackTrace}" : ""));
    }

#nullable enable
    [StackTraceHidden]
    public static void AssertNotNull(this Node node, object @object, string? message = null)
    {
        if (@object == null)
        {
            GD.PrintErr($"[{node.Name}] NullReferenceException" + (message == null ? "" : " -> " + message));

            throw new NullReferenceException(message);
        }
    }
#nullable disable
}
