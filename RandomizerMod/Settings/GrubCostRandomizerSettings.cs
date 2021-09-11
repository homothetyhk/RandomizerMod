using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class GrubCostRandomizerSettings : SettingsModule
    {
        [MinValue(0)]
        [MaxValue(46)]
        public int MinimumGrubCost;
        [MinValue(0)]
        [MaxValue(46)]
        public int MaximumGrubCost;
        [MinValue(0)]
        [MaxValue(46)]
        public int GrubTolerance;

        public override void Clamp(GenerationSettings gs)
        {
            if (MaximumGrubCost < MinimumGrubCost) MaximumGrubCost = MinimumGrubCost;
            if (GrubTolerance + MaximumGrubCost > 46) GrubTolerance = 46 - MaximumGrubCost;
        }
    }
}
