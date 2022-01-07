namespace RandomizerMod.Settings.Presets
{
    public static class SplitGroupPresetData
    {
        public static SplitGroupSettings Disabled;
        public static SplitGroupSettings A;
        public static SplitGroupSettings B;
        public static SplitGroupSettings C;

        public static Dictionary<string, SplitGroupSettings> Presets;
        static SplitGroupPresetData()
        {
            Disabled = new SplitGroupSettings
            {
                Dreamers = -1,
                Skills = -1,
                Charms = -1,
                Keys = -1,
                MaskShards = -1,
                VesselFragments = -1,
                CharmNotches = -1,
                PaleOre = -1,
                GeoChests = -1,
                RancidEggs = -1,
                Relics = -1,
                WhisperingRoots = -1,
                BossEssence = -1,
                Grubs = -1,
                Mimics = -1,
                Maps = -1,
                Stags = -1,
                LifebloodCocoons = -1,
                GrimmkinFlames = -1,
                JournalEntries = -1,
                GeoRocks = -1,
                BossGeo = -1,
                SoulTotems = -1,
                LoreTablets = -1,
            };

            A = new SplitGroupSettings
            {
                Dreamers = 0,
                Skills = 0,
                Charms = 0,
                Keys = 0,
                MaskShards = 1,
                VesselFragments = 1,
                CharmNotches = 0,
                PaleOre = 0,
                GeoChests = 0,
                RancidEggs = 1,
                Relics = 1,
                WhisperingRoots = 1,
                BossEssence = 1,
                Grubs = 1,
                Mimics = 1,
                Maps = 1,
                Stags = 1,
                LifebloodCocoons = 1,
                GrimmkinFlames = 1,
                JournalEntries = 1,
                GeoRocks = 1,
                BossGeo = 1,
                SoulTotems = 1,
                LoreTablets = 1,
            };

            B = new SplitGroupSettings
            {
                Dreamers = 0,
                Skills = 0,
                Charms = 0,
                Keys = 0,
                MaskShards = 0,
                VesselFragments = 0,
                CharmNotches = 0,
                PaleOre = 0,
                GeoChests = 0,
                RancidEggs = 1,
                Relics = 1,
                WhisperingRoots = 1,
                BossEssence = 0,
                Grubs = 1,
                Mimics = 1,
                Maps = 1,
                Stags = 1,
                LifebloodCocoons = 1,
                GrimmkinFlames = 0,
                JournalEntries = 1,
                GeoRocks = 1,
                BossGeo = 0,
                SoulTotems = 1,
                LoreTablets = 1,
            };

            C = new SplitGroupSettings
            {
                Dreamers = 0,
                Skills = 0,
                Charms = 1,
                Keys = 1,
                MaskShards = 1,
                VesselFragments = 1,
                CharmNotches = 1,
                PaleOre = 1,
                GeoChests = 0,
                RancidEggs = 2,
                Relics = 2,
                WhisperingRoots = 2,
                BossEssence = 0,
                Grubs = 2,
                Mimics = 2,
                Maps = 2,
                Stags = 1,
                LifebloodCocoons = 1,
                GrimmkinFlames = 1,
                JournalEntries = 1,
                GeoRocks = 2,
                BossGeo = 0,
                SoulTotems = 2,
                LoreTablets = 2,
            };

            Presets = new Dictionary<string, SplitGroupSettings>
            {
                { "Disabled", Disabled },
                { "A", A },
                { "B", B },
                { "C", C },
            };
        }
    }
}
