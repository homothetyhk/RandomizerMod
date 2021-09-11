using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings.Presets
{
    public static class EssenceCostPresetData
    {
        public static EssenceCostRandomizerSettings Standard;
        public static EssenceCostRandomizerSettings More;
        public static EssenceCostRandomizerSettings Less;
        public static EssenceCostRandomizerSettings Expert;
        public static EssenceCostRandomizerSettings Vanilla;
        public static Dictionary<string, EssenceCostRandomizerSettings> EssencePresets;

        static EssenceCostPresetData()
        {
            Standard = new EssenceCostRandomizerSettings
            {
                EssenceTolerance = 150,
                MinimumEssenceCost = 1,
                MaximumEssenceCost = 900,
            };
            More = new EssenceCostRandomizerSettings
            {
                EssenceTolerance = 200,
                MinimumEssenceCost = 1,
                MaximumEssenceCost = 1800,
            };
            Less = new EssenceCostRandomizerSettings
            {
                EssenceTolerance = 150,
                MinimumEssenceCost = 1,
                MaximumEssenceCost = 600,
            };
            Expert = new EssenceCostRandomizerSettings
            {
                EssenceTolerance = 20,
                MinimumEssenceCost = 1,
                MaximumEssenceCost = 1800,
            };
            EssencePresets = new Dictionary<string, EssenceCostRandomizerSettings>
            {
                { "Standard", Standard },
                { "More", More },
                { "Less", Less },
                { "Expert", Expert },
            };
        }
    }
}
