using RandomizerCore;

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
                source = p.source.Name;
                target = p.target.Name;
            }
        }

        public override void Log(LogArguments args)
        {
            string contents = RandomizerData.JsonUtil.Serialize(args.ctx.transitionPlacements?.Select(p => new SpoilerEntry(p))?.ToList() ?? new());
            LogManager.Write(contents, "TransitionSpoilerLog.json");
        }
    }
}
