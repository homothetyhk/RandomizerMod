using Newtonsoft.Json;
using RandomizerCore;

namespace RandomizerMod.Logging
{
    class ItemSpoilerLog : RandoLogger
    {
        public readonly struct SpoilerEntry
        {
            public readonly string item;
            public readonly string location;
            public readonly string[] costs;

            public SpoilerEntry(ItemPlacement p)
            {
                item = p.item.Name;
                location = p.location.Name;
                costs = p.location.costs?.Select(c => c.ToString())?.ToArray();
            }
        }

        public override void Log(LogArguments args)
        {
            JsonSerializer js = new()
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented,
            };
            using StringWriter sw = new();
            js.Serialize(sw, args.ctx.itemPlacements?.Select(p => new SpoilerEntry(p))?.ToList() ?? new());
            LogManager.Write(sw.ToString(), "ItemSpoilerLog.json");
        }
    }
}
