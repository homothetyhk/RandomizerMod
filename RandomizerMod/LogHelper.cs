// ReSharper disable file UnusedMember.Global


using System;

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

        public static void LogDebug(string message)
        {
            //RandomizerMod.Instance.LogDebug(message);
        }

        public static void LogDebug(object message)
        {
            //RandomizerMod.Instance.LogDebug(message);
        }

        public static void LogError(string message)
        {
            //RandomizerMod.Instance.LogError(message);
        }

        public static void LogError(object message)
        {
            //RandomizerMod.Instance.LogError(message);
        }

        public static void LogFine(string message)
        {
            //RandomizerMod.Instance.LogFine(message);
        }

        public static void LogFine(object message)
        {
            //RandomizerMod.Instance.LogFine(message);
        }

        public static void LogWarn(string message)
        {
            //RandomizerMod.Instance.LogWarn(message);
        }

        public static void LogWarn(object message)
        {
            //RandomizerMod.Instance.LogWarn(message);
        }
    }
}
