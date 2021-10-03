using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomizerMod.Settings.Presets
{
    public static class CostPresetData
    {
        public static CostSettings Standard;
        public static CostSettings More;
        public static CostSettings Less;
        public static CostSettings Expert;
        public static Dictionary<string, CostSettings> CostPresets;

        static CostPresetData()
        {
            Standard = new CostSettings
            {
                GrubTolerance = 2,
                MinimumGrubCost = 1,
                MaximumGrubCost = 23,
                EssenceTolerance = 150,
                MinimumEssenceCost = 1,
                MaximumEssenceCost = 900,
            };
            Less = new CostSettings
            {
                GrubTolerance = 2,
                MinimumGrubCost = 1,
                MaximumGrubCost = 15,
                EssenceTolerance = 150,
                MinimumEssenceCost = 1,
                MaximumEssenceCost = 600,
            };
            More = new CostSettings
            {
                GrubTolerance = 4,
                MinimumGrubCost = 1,
                MaximumGrubCost = 42,
                EssenceTolerance = 200,
                MinimumEssenceCost = 1,
                MaximumEssenceCost = 1800,
            };

            Expert = new CostSettings
            {
                GrubTolerance = 0,
                MinimumGrubCost = 5,
                MaximumGrubCost = 42,
                EssenceTolerance = 20,
                MinimumEssenceCost = 1,
                MaximumEssenceCost = 1800,
            };

            CostPresets = new Dictionary<string, CostSettings>
            {
                { "Standard", Standard },
                { "More", More },
                { "Less", Less },
                { "Expert", Expert },
            };
        }
    }
}
