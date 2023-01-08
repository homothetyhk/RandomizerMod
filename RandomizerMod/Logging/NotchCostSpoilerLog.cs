using Newtonsoft.Json;

namespace RandomizerMod.Logging
{
    class NotchCostSpoilerLog : RandoLogger
    {
        static string[] _charmNames = new string[]
        {
          "Gathering_Swarm",
          "Wayward_Compass",
          "Grubsong",
          "Stalwart_Shell",
          "Baldur_Shell",
          "Fury_of_the_Fallen",
          "Quick_Focus",
          "Lifeblood_Heart",
          "Lifeblood_Core",
          "Defender's_Crest",
          "Flukenest",
          "Thorns_of_Agony",
          "Mark_of_Pride",
          "Steady_Body",
          "Heavy_Blow",
          "Sharp_Shadow",
          "Spore_Shroom",
          "Longnail",
          "Shaman_Stone",
          "Soul_Catcher",
          "Soul_Eater",
          "Glowing_Womb",
          "Fragile_Heart",
          "Fragile_Greed",
          "Fragile_Strength",
          "Nailmaster's_Glory",
          "Joni's_Blessing",
          "Shape_of_Unn",
          "Hiveblood",
          "Dream_Wielder",
          "Dashmaster",
          "Quick_Slash",
          "Spell_Twister",
          "Deep_Focus",
          "Grubberfly's_Elegy",
          "Kingsoul",
          "Sprintmaster",
          "Dreamshield",
          "Weaversong",
          "Grimmchild",
        };

        public override void Log(LogArguments args)
        {
            List<int> notchCosts = args.ctx.notchCosts;
            if (notchCosts is null || notchCosts.Count < 40) return;

            Dictionary<string, int> costLookup = new(40);
            for (int i = 0; i < 40; i++)
            {
                costLookup[_charmNames[i]] = notchCosts[i];
            }

            JsonSerializer js = new()
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented,
            };
            LogManager.Write((tw) => js.Serialize(tw, costLookup), "NotchCostSpoilerLog.json");
        }
    }
}
