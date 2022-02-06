using RandomizerCore;
using RandomizerMod.RC;

namespace RandomizerMod.Logging
{
    class TransitionSpoilerLog : RandoLogger
    {
        public readonly struct SpoilerEntry
        {
            public readonly string source;
            public readonly string target;

            public SpoilerEntry(TransitionPlacement p)
            {
                source = p.Source.Name;
                target = p.Target.Name;
            }
        }

        public override void Log(LogArguments args)
        {
            string contents = RandomizerData.JsonUtil.Serialize(args.ctx.transitionPlacements?.Select(p => new SpoilerEntry(p))?.ToList() ?? new());
            LogManager.Write(contents, "TransitionSpoilerLog.json");
        }
    }
}
