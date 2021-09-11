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
        public static Dictionary<string, GrubCostRandomizerSettings> GrubCostPresets;

        static GrubCostPresetData()
        {
            Standard = new GrubCostRandomizerSettings
            {
                GrubTolerance = 2,
                MinimumGrubCost = 1,
                MaximumGrubCost = 23,
            };
            Less = new GrubCostRandomizerSettings
            {
                GrubTolerance = 2,
                MinimumGrubCost = 1,
                MaximumGrubCost = 15,
            };
            More = new GrubCostRandomizerSettings
            {
                GrubTolerance = 4,
                MinimumGrubCost = 1,
                MaximumGrubCost = 42,
            };

            Expert = new GrubCostRandomizerSettings
            {
                GrubTolerance = 0,
                MinimumGrubCost = 5,
                MaximumGrubCost = 42,
            };

            GrubCostPresets = new Dictionary<string, GrubCostRandomizerSettings>
            {
                { "Standard", Standard },
                { "More", More },
                { "Less", Less },
                { "Expert", Expert },
            };
        }
    }
}
