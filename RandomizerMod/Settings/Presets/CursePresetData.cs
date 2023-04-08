namespace RandomizerMod.Settings.Presets
{
    public static class CursePresetData
    {
        public static CursedSettings None;
        public static CursedSettings Classic;
        public static CursedSettings Modern;
        public static CursedSettings UltraCursed;

        public static Dictionary<string, CursedSettings> CursedPresets;

        static CursePresetData()
        {
            None = new CursedSettings
            {
                ReplaceJunkWithOneGeo = false,
                RemoveSpellUpgrades = false,
                LongerProgressionChains = false,
                Deranged = false,
                CursedMasks = 0,
                CursedNotches = 0,
                RandomizeMimics = false,
                MaximumGrubsReplacedByMimics = 0,
            };
            Classic = new CursedSettings
            {
                ReplaceJunkWithOneGeo = true,
                RemoveSpellUpgrades = true,
                LongerProgressionChains = true,
                Deranged = false,
                CursedMasks = 0,
                CursedNotches = 0,
                RandomizeMimics = false,
                MaximumGrubsReplacedByMimics = 0,
            };
            Modern = new CursedSettings
            {
                ReplaceJunkWithOneGeo = false,
                RemoveSpellUpgrades = false,
                LongerProgressionChains = false,
                Deranged = true,
                CursedMasks = 4,
                CursedNotches = 2,
                RandomizeMimics = true,
                MaximumGrubsReplacedByMimics = 10,
            };
            UltraCursed = new CursedSettings
            {
                ReplaceJunkWithOneGeo = true,
                RemoveSpellUpgrades = true,
                LongerProgressionChains = true,
                Deranged = true,
                CursedMasks = 4,
                CursedNotches = 2,
                RandomizeMimics = true,
                MaximumGrubsReplacedByMimics = 10,
            };

            CursedPresets = new Dictionary<string, CursedSettings>
            {
                { "None", None },
                { "Classic", Classic },
                { "Modern", Modern },
                { "Ultra Cursed", UltraCursed },
            };
        }
    }
}
