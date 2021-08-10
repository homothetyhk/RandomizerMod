using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings.Presets
{
    public static class GrubCostPresetData
    {
        public static GrubCostRandomizerSettings Standard;
        public static GrubCostRandomizerSettings More;
        public static GrubCostRandomizerSettings Less;
        public static GrubCostRandomizerSettings Expert;
        public static GrubCostRandomizerSettings Vanilla;
        public static Dictionary<string, GrubCostRandomizerSettings> GrubCostPresets;

        static GrubCostPresetData()
        {
            Standard = new GrubCostRandomizerSettings
            {
                RandomizeGrubItemCosts = true,
                GrubTolerance = 2,
                MinimumGrubCost = 1,
                MaximumGrubCost = 23,
            };
            Less = new GrubCostRandomizerSettings
            {
                RandomizeGrubItemCosts = true,
                GrubTolerance = 2,
                MinimumGrubCost = 1,
                MaximumGrubCost = 15,
            };
            More = new GrubCostRandomizerSettings
            {
                RandomizeGrubItemCosts = true,
                GrubTolerance = 4,
                MinimumGrubCost = 1,
                MaximumGrubCost = 42,
            };

            Expert = new GrubCostRandomizerSettings
            {
                RandomizeGrubItemCosts = true,
                GrubTolerance = 0,
                MinimumGrubCost = 5,
                MaximumGrubCost = 42,
            };

            Vanilla = new GrubCostRandomizerSettings
            {
                RandomizeGrubItemCosts = false,
                GrubTolerance = 0,
                MinimumGrubCost = 1,
                MaximumGrubCost = 1,
            };

            GrubCostPresets = new Dictionary<string, GrubCostRandomizerSettings>
            {
                { "Standard", Standard },
                { "More", More },
                { "Less", Less },
                { "Expert", Expert },
                { "Vanilla", Vanilla },
            };
        }
    }
}
