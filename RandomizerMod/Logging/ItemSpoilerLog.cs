using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomizerCore;
using System.IO;

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

        public override void Log(string directory, LogArguments args)
        {
            RandomizerData.JsonUtil.Serialize(args.ctx.itemPlacements?.Select(p => new SpoilerEntry(p))?.ToList() ?? new(), Path.Combine(directory, "ItemSpoilerLog.json"));
        }
    }
}
