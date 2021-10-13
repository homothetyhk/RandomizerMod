using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomizerMod.Settings
{
    public class CostSettings : SettingsModule
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

        [MinValue(0)]
        [MaxValue(2800)]
        public int MinimumEssenceCost;
        [MinValue(0)]
        [MaxValue(2800)]
        public int MaximumEssenceCost;
        [MinValue(0)]
        [MaxValue(250)]
        public int EssenceTolerance;

        [MinValue(0)]
        [MaxValue(20)]
        public int MinimumEggCost;
        [MinValue(0)]
        [MaxValue(20)]
        public int MaximumEggCost;
        [MinValue(0)]
        [MaxValue(20)]
        public int EggTolerance;

        [MinValue(0)]
        [MaxValue(40)]
        public int MinimumCharmCost;
        [MinValue(0)]
        [MaxValue(40)]
        public int MaximumCharmCost;
        [MinValue(0)]
        [MaxValue(40)]
        public int CharmTolerance;



        public override void Clamp(GenerationSettings gs)
        {
            if (MaximumGrubCost < MinimumGrubCost) MaximumGrubCost = MinimumGrubCost;
            if (GrubTolerance + MaximumGrubCost > 46) GrubTolerance = 46 - MaximumGrubCost;

            if (MaximumEssenceCost < MinimumEssenceCost) MaximumEssenceCost = MinimumEssenceCost;

            if (MaximumEggCost < MinimumEggCost) MaximumEggCost = MinimumEggCost;
            if (EggTolerance + MaximumEggCost > 20) EggTolerance = 20 - MaximumEggCost;

            if (MaximumCharmCost < MinimumCharmCost) MaximumCharmCost = MinimumCharmCost;
            if (CharmTolerance + MaximumCharmCost > 40) CharmTolerance = 40 - MaximumCharmCost;
        }
    }
}
