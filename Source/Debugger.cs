using System;

namespace Universum;

public static class Debugger {
    public enum Severity {
        Info = 0,
        Warning = 1,
        Error = 2,
    }
    
    public const string TAB = "    ";
    
    public static void Log(
        string key,
        Verse.NamedArgument[] args = null,
        string prefix = "",
        Severity severity = Severity.Info
    ) {
        bool noKey = key == null;
        if (noKey) return;
        
        bool noArgs = args == null;
        string message = noArgs
            ? Verse.TranslatorFormattedStringExtensions.Translate(key)
            : Verse.TranslatorFormattedStringExtensions.Translate(key, args);

        bool noPrefix = prefix == null;
        if (!noPrefix) message = String.Concat(prefix, message);
        
        Log(message, severity);
    }

    public static void Log(string message, Severity severity = Severity.Info) {
        bool noMessage = message == null;
        if (noMessage) return;

        switch (severity) {
            case Severity.Info:
                Verse.Log.Message(message);
                return;
            case Severity.Warning:
                Verse.Log.Warning(message);
                return;
            case Severity.Error:
                Verse.Log.Error(message);
                return;
            default:
                Verse.Log.Message(message);
                return;
        }
    }
}
