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

        private static readonly (LogicManagerBuilder.JsonType type, string fileName)[] files = new[]
        {
            (LogicManagerBuilder.JsonType.Terms, "terms"),
            (LogicManagerBuilder.JsonType.Macros, "macros"),
            (LogicManagerBuilder.JsonType.Waypoints, "waypoints"),
            (LogicManagerBuilder.JsonType.Transitions, "transitions"),
            (LogicManagerBuilder.JsonType.Locations, "locations"),
            (LogicManagerBuilder.JsonType.Items, "items"),
        };

        /// <summary>
        /// Creates a new LogicManager, allowing edits from local files and runtime hooks.
        /// </summary>
        public static LogicManager GetNewLogicManager(GenerationSettings gs)
        {
            LogicManagerBuilder lmb = new() { VariableResolver = new RandoVariableResolver() };

            foreach ((LogicManagerBuilder.JsonType type, string fileName) in files)
            {
                lmb.DeserializeJson(type, RandomizerMod.Assembly.GetManifestResourceStream($"RandomizerMod.Resources.Logic.{fileName}.json"));
            }

            foreach (var a in _runtimeLogicOverrideOwner.GetSubscribers())
            {
                try
                {
                    a?.Invoke(gs, lmb);
                }
                catch (Exception e)
                {
                    LogError("Error invoking logic override event:\n" + e);
                }
            }

            return new(lmb);
        }

        /// <summary>
        /// Event invoked when building the LogicManager for randomization.
        /// <br/>A subscriber with a nonpositive key is invoked before local logic edits.
        /// <br/>A subscriber with a positive key is invoked after local logic edits.
        /// </summary>
        public static readonly PriorityEvent<Action<GenerationSettings, LogicManagerBuilder>> RuntimeLogicOverride = new(out _runtimeLogicOverrideOwner);
        private static readonly PriorityEvent<Action<GenerationSettings, LogicManagerBuilder>>.IPriorityEventOwner _runtimeLogicOverrideOwner;
    }
}
