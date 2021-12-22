using System;
using System.IO;
using Newtonsoft.Json;
using RandomizerCore;
using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerMod.Settings;
using static RandomizerMod.LogHelper;

namespace RandomizerMod.RC
{
    public static class RCData
    {
        public static bool Loaded = false;
        private static LogicManagerBuilder _combinedLMB;

        private static readonly (LogicManagerBuilder.JsonType type, string fileName)[] files = new[]
        {
            (LogicManagerBuilder.JsonType.Terms, "terms"),
            (LogicManagerBuilder.JsonType.Macros, "macros"),
            (LogicManagerBuilder.JsonType.Waypoints, "waypoints"),
            (LogicManagerBuilder.JsonType.Transitions, "transitions"),
            (LogicManagerBuilder.JsonType.Locations, "locations"),
            (LogicManagerBuilder.JsonType.Items, "items"),
        };

        public static void Load()
        {
            if (Loaded) return;
            Loaded = true;

            _combinedLMB = new();
            foreach ((LogicManagerBuilder.JsonType type, string fileName) in files)
            {
                _combinedLMB.DeserializeJson(type, RandomizerMod.Assembly.GetManifestResourceStream($"RandomizerMod.Resources.Logic.{fileName}.json"));
            }
        }

        /// <summary>
        /// Clones and returns the builder for the LogicManager.
        /// </summary>
        public static LogicManagerBuilder GetNewBuilder()
        {
            if (!Loaded) Load();
            return new(_combinedLMB);
        }

        public static void ApplyLocalLogicChanges(LogicManagerBuilder lmb)
        {
            string directory = Path.Combine(RandomizerMod.Folder, "Logic");
            try
            {
                DirectoryInfo di = new(directory);
                if (di.Exists)
                {
                    List<FileInfo> macros = new();
                    List<FileInfo> logic = new();

                    foreach (FileInfo fi in di.EnumerateFiles())
                    {
                        if (!fi.Extension.ToLower().EndsWith("json")) continue;
                        else if (fi.Name.ToLower().StartsWith("macro")) macros.Add(fi);
                        else logic.Add(fi);
                    }
                    foreach (FileInfo fi in macros)
                    {
                        using FileStream fs = fi.OpenRead();
                        lmb.DeserializeJson(LogicManagerBuilder.JsonType.MacroEdit, fs);
                    }
                    foreach (FileInfo fi in logic)
                    {
                        using FileStream fs = fi.OpenRead();
                        lmb.DeserializeJson(LogicManagerBuilder.JsonType.LogicEdit, fs);
                    }
                }
            }
            catch (Exception e)
            {
                LogError("Error fetching local logic changes:\n" + e);
            }
        }

        /// <summary>
        /// Creates a new LogicManager, allowing edits from local files and runtime hooks.
        /// </summary>
        public static LogicManager GetNewLogicManager(GenerationSettings gs)
        {
            LogicManagerBuilder lmb = GetNewBuilder();
            ApplyLocalLogicChanges(lmb);
            foreach (var a in _runtimeLogicOverrideOwner.GetSubscriberList())
            {
                try
                {
                    a?.Invoke(gs, lmb);
                }
                catch (Exception e)
                {
                    Log("Error invoking logic override event:\n" + e);
                }
            }
            
            return new(lmb);
        }

        public static readonly PriorityEvent<Action<GenerationSettings, LogicManagerBuilder>> RuntimeLogicOverride = new(out _runtimeLogicOverrideOwner);
        private static readonly PriorityEvent<Action<GenerationSettings, LogicManagerBuilder>>.IPriorityEventOwner _runtimeLogicOverrideOwner;
    }
}
