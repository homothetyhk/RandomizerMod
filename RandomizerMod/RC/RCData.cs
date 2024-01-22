using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerMod.Settings;

namespace RandomizerMod.RC
{
    public static class RCData
    {
        private static readonly (LogicFileType type, string fileName)[] files = new[]
        {
            (LogicFileType.Terms, "terms"),
            (LogicFileType.Macros, "macros"),
            (LogicFileType.Waypoints, "waypoints"),
            (LogicFileType.Transitions, "transitions"),
            (LogicFileType.Locations, "locations"),
            (LogicFileType.ItemStrings, "items"),
            (LogicFileType.StateData, "state"),
        };

        /// <summary>
        /// Creates a new LogicManager, allowing edits from local files and runtime hooks.
        /// </summary>
        public static LogicManager GetNewLogicManager(GenerationSettings gs)
        {
            LogicManagerBuilder lmb = new() { VariableResolver = new RandoVariableResolver() };
            ILogicFormat fmt = new JsonLogicFormat();

            foreach ((LogicFileType type, string fileName) in files)
            {
                lmb.DeserializeFile(type, fmt, RandomizerMod.Assembly.GetManifestResourceStream($"RandomizerMod.Resources.Logic.{fileName}.json"));
            }

            foreach (var a in _runtimeLogicOverrideOwner.GetSubscribers())
            {
                try
                {
                    a?.Invoke(gs, lmb);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Error invoking logic override event", e);
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
