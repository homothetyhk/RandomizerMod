using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings.Presets
{
    public static class CursePresetData
    {
        public static CursedSettings None;
        public static CursedSettings Random;
        public static CursedSettings Classic;
        public static CursedSettings SplitClaw;
        public static CursedSettings SplitEverything; // we demand split CH
        public static CursedSettings UltraCursed;

        public static Dictionary<string, CursedSettings> CursedPresets;

        static CursePresetData()
        {
            None = new CursedSettings
            {
                RandomCurses = false,
                RandomizeFocus = false,
                ReplaceJunkWithOneGeo = false,
                RemoveSpellUpgrades = false,
                LongerProgressionChains = false,
                SplitClaw = false,
                SplitCloak = false,
                RandomizeNail = false
            };
            Random = new CursedSettings
            {
                RandomCurses = true,
                RandomizeFocus = false,
                ReplaceJunkWithOneGeo = false,
                RemoveSpellUpgrades = false,
                LongerProgressionChains = false,
                SplitClaw = false,
                SplitCloak = false,
                RandomizeNail = false
            };
            Classic = new CursedSettings
            {
                RandomCurses = false,
                RandomizeFocus = true,
                ReplaceJunkWithOneGeo = true,
                RemoveSpellUpgrades = true,
                LongerProgressionChains = true,
                SplitClaw = false,
                SplitCloak = false,
                RandomizeNail = false
            };
            SplitClaw = new CursedSettings
            {
                RandomCurses = false,
                RandomizeFocus = false,
                ReplaceJunkWithOneGeo = false,
                RemoveSpellUpgrades = false,
                LongerProgressionChains = false,
                SplitClaw = true,
                SplitCloak = false,
                RandomizeNail = false
            };
            SplitEverything = new CursedSettings
            {
                RandomCurses = false,
                RandomizeFocus = false,
                ReplaceJunkWithOneGeo = false,
                RemoveSpellUpgrades = false,
                LongerProgressionChains = false,
                SplitClaw = true,
                SplitCloak = true,
                RandomizeNail = true
            };
            UltraCursed = new CursedSettings
            {
                RandomCurses = false,
                RandomizeFocus = true,
                ReplaceJunkWithOneGeo = true,
                RemoveSpellUpgrades = true,
                LongerProgressionChains = true,
                SplitClaw = true,
                SplitCloak = true,
                RandomizeNail = true
            };

            CursedPresets = new Dictionary<string, CursedSettings>
            {
                { "None", None },
                { "Random", Random },
                { "Classic", Classic },
                { "Split Claw", SplitClaw },
                { "Split Everything", SplitEverything },
                { "Ultra Cursed", UltraCursed },
            };
        }


    }
}
