using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomizerMod.Settings.Presets
{
    public static class NoveltyPresetData
    {
        public static Dictionary<string, NoveltySettings> NoveltyPresets;
        public static NoveltySettings None;
        public static NoveltySettings Basic;
        public static NoveltySettings Clawful;
        public static NoveltySettings SplitStuff;
        public static NoveltySettings Everything;


        static NoveltyPresetData()
        {
            None = new()
            {
                RandomizeSwim = false,
                RandomizeElevatorPass = false,
                RandomizeNail = false,
                RandomizeFocus = false,
                SplitClaw = false,
                SplitCloak = false,
                EggShop = false,
            };

            Basic = new()
            {
                RandomizeSwim = true,
                RandomizeElevatorPass = true,
                RandomizeNail = false,
                RandomizeFocus = false,
                SplitClaw = false,
                SplitCloak = false,
                EggShop = true,
            };

            Clawful = new()
            {
                RandomizeSwim = true,
                RandomizeElevatorPass = true,
                RandomizeNail = false,
                RandomizeFocus = false,
                SplitClaw = true,
                SplitCloak = false,
                EggShop = true,
            };

            SplitStuff = new()
            {
                RandomizeSwim = true,
                RandomizeElevatorPass = true,
                RandomizeNail = false,
                RandomizeFocus = false,
                SplitClaw = true,
                SplitCloak = true,
                EggShop = true,
            };

            Everything = new()
            {
                RandomizeSwim = true,
                RandomizeElevatorPass = true,
                RandomizeNail = true,
                RandomizeFocus = true,
                SplitClaw = true,
                SplitCloak = true,
                EggShop = true,
            };

            NoveltyPresets = new()
            {
                { "Basic", Basic },
                { "Clawful", Clawful },
                { "Split Stuff", SplitStuff },
                { "Everything", Everything },
                { "None", None },
            };
        }
    }
}
