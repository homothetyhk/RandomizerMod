using RandomizerCore;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace RandomizerMod.Logging
{
    public class LogArguments
    {
        public object randomizer { get; init; }
        public GenerationSettings gs { get; init; }
        public RandoModContext ctx { get; init; }
        public Dictionary<string, object> properties { get; init; } = new();
    }
}
