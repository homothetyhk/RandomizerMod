using Newtonsoft.Json;
using RandomizerCore;
using RandomizerMod.RC;

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
                item = p.Item.Name;
                location = p.Location.Name;
                costs = p.Location.costs?.Select(c => c.ToString())?.ToArray();
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
            js.Serialize(sw, args.ctx.itemPlacements.Select(p => new SpoilerEntry(p)).ToList());
            LogManager.Write(sw.ToString(), "ItemSpoilerLog.json");
        }
    }
}
