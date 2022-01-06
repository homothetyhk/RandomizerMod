using RandomizerCore;
using RandomizerMod.Settings;

namespace RandomizerMod.Logging
{
    public class LogArguments
    {
        public object randomizer { get; init; }
        public GenerationSettings gs { get; init; }
        public RandoContext ctx { get; init; }
    }
}
