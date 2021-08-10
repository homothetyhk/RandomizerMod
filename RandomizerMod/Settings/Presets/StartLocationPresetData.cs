using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings.Presets
{
    public static class StartLocationPresetData
    {
        public static StartLocationSettings KingsPass;
        public static StartLocationSettings RandomNoKP;
        public static StartLocationSettings RandomWithKP;
        public static StartLocationSettings MoreRandom;
        public static StartLocationSettings MostRandom;

        public static Dictionary<string, StartLocationSettings> StartLocationPresets;

        static StartLocationPresetData()
        {
            KingsPass = new StartLocationSettings
            {
                StartLocationType = StartLocationSettings.RandomizeStartLocationType.Fixed,
                StartLocation = "King's Pass",
            };

            RandomNoKP = new StartLocationSettings
            {
                StartLocationType = StartLocationSettings.RandomizeStartLocationType.RandomExcludingKP,
                StartLocation = null,
            };

            RandomWithKP = new StartLocationSettings
            {
                StartLocationType = StartLocationSettings.RandomizeStartLocationType.Random,
                StartLocation = null,
            };

            MoreRandom = new StartLocationSettings
            {
                StartLocationType = StartLocationSettings.RandomizeStartLocationType.RandomWithMinorForcedStartItems,
                StartLocation = null,
            };

            MostRandom = new StartLocationSettings
            {
                StartLocationType = StartLocationSettings.RandomizeStartLocationType.RandomWithAnyForcedStartItems,
                StartLocation = null,
            };

            StartLocationPresets = new Dictionary<string, StartLocationSettings>
            {
                { "King's Pass", KingsPass },
                { "Random (no King's Pass)", RandomNoKP },
                { "Random (allow King's Pass)", RandomWithKP },
                { "Random (More Starts)", MoreRandom },
                { "Random (All Starts)", MostRandom },
            };
        }
    }
}
