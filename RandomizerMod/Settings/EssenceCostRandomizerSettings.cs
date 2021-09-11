using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class EssenceCostRandomizerSettings : SettingsModule
    {
        [MinValue(0)]
        [MaxValue(2800)]
        public int MinimumEssenceCost;
        [MinValue(0)]
        [MaxValue(2800)]
        public int MaximumEssenceCost;
        [MinValue(0)]
        [MaxValue(250)]
        public int EssenceTolerance;

        public override void Clamp(GenerationSettings gs)
        {
            if (MaximumEssenceCost < MinimumEssenceCost) MaximumEssenceCost = MinimumEssenceCost;
        }
    }
}
