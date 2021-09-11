// ReSharper disable file UnusedMember.Global


using System;
using System.Diagnostics;

namespace RandomizerMod
{
    public static class LogHelper
    {
        public static event Action<string> OnLog;

        public static void Log(string message = "")
        {
            OnLog?.Invoke(message);
        }

        public static void Log(object message)
        {
            Log(message.ToString());
        }

        [Conditional("DEBUG")]
        public static void LogDebug(string message)
        {
            OnLog?.Invoke(message);
        }

        public static void LogError(string message)
        {
            OnLog?.Invoke(message);
        }

        public static void LogWarn(string message)
        {
            OnLog?.Invoke(message);
        }
    }
}
