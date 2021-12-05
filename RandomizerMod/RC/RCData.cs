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

        private static LogicManagerBuilder _itemLMB;
        private static LogicManagerBuilder _areaLMB;
        private static LogicManagerBuilder _roomLMB;

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

            _itemLMB = new();
            foreach ((LogicManagerBuilder.JsonType type, string fileName) in files)
            {
                _itemLMB.DeserializeJson(type, RandomizerMod.Assembly.GetManifestResourceStream($"RandomizerMod.Resources.Item.{fileName}.json"));
            }

            _areaLMB = new();
            foreach ((LogicManagerBuilder.JsonType type, string fileName) in files)
            {
                _areaLMB.DeserializeJson(type, RandomizerMod.Assembly.GetManifestResourceStream($"RandomizerMod.Resources.Area.{fileName}.json"));
            }

            _roomLMB = new();
            foreach ((LogicManagerBuilder.JsonType type, string fileName) in files)
            {
                _roomLMB.DeserializeJson(type, RandomizerMod.Assembly.GetManifestResourceStream($"RandomizerMod.Resources.Room.{fileName}.json"));
            }
        }

        /// <summary>
        /// Clones and returns the builder for the LogicManager of the requested mode.
        /// </summary>
        public static LogicManagerBuilder GetNewBuilder(LogicMode mode)
        {
            if (!Loaded) Load();
            return mode switch
            {
                LogicMode.Room => new(_roomLMB),
                LogicMode.Area => new(_areaLMB),
                _ => new(_itemLMB),
            };
        }

        public static void ApplyLocalLogicChanges(LogicMode mode, LogicManagerBuilder lmb)
        {
            string directory = Path.Combine(RandomizerMod.Folder, mode switch
            {
                LogicMode.Room => "Room",
                LogicMode.Area => "Area",
                _ => "Item",
            });
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
        /// Creates a new LogicManager for the requested mode, allowing edits from local files and runtime hooks.
        /// </summary>
        public static LogicManager GetNewLogicManager(GenerationSettings gs)
        {
            LogicMode mode = gs.TransitionSettings.GetLogicMode();
            LogicManagerBuilder lmb = GetNewBuilder(mode);
            ApplyLocalLogicChanges(mode, lmb);
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
