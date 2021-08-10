using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings.Presets
{
    public static class SkipPresetData
    {
        public static SkipSettings Easy;
        public static SkipSettings Medium;
        public static SkipSettings Hard;
        public static Dictionary<string, SkipSettings> SkipPresets;

        static SkipPresetData()
        {
            Easy = new SkipSettings
            {
                MildSkips = false,
                ShadeSkips = false,
                FireballSkips = false,
                AcidSkips = false,
                SpikeTunnels = false,
                DarkRooms = false,
                SpicySkips = false,
            };

            Medium = new SkipSettings
            {
                MildSkips = true,
                ShadeSkips = true,
                FireballSkips = false,
                AcidSkips = false,
                SpikeTunnels = false,
                DarkRooms = false,
                SpicySkips = false,
            };

            Hard = new SkipSettings
            {
                MildSkips = true,
                ShadeSkips = true,
                FireballSkips = true,
                AcidSkips = true,
                SpikeTunnels = true,
                DarkRooms = true,
                SpicySkips = true,
            };

            SkipPresets = new Dictionary<string, SkipSettings>
            {
                { "Easy", Easy },
                { "Medium", Medium },
                { "Hard", Hard }
            };
        }
    }
}
