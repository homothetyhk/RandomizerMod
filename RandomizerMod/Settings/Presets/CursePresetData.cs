using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                CursedMasks = false,
                CursedNotches = false,
                RandomizeMimics = false,
                MaximumGrubsReplacedByMimics = 0,
            };
            Classic = new CursedSettings
            {
                ReplaceJunkWithOneGeo = true,
                RemoveSpellUpgrades = true,
                LongerProgressionChains = true,
                CursedMasks = false,
                CursedNotches = false,
                RandomizeMimics = false,
                MaximumGrubsReplacedByMimics = 0,
            };
            Modern = new CursedSettings
            {
                ReplaceJunkWithOneGeo = false,
                RemoveSpellUpgrades = false,
                LongerProgressionChains = false,
                CursedMasks = true,
                CursedNotches = true,
                RandomizeMimics = true,
                MaximumGrubsReplacedByMimics = 10,
            };
            UltraCursed = new CursedSettings
            {
                ReplaceJunkWithOneGeo = true,
                RemoveSpellUpgrades = true,
                LongerProgressionChains = true,
                CursedMasks = true,
                CursedNotches = true,
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
