using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuChanger.Attributes;

namespace RandomizerMod.Settings
{
    public class CostSettings : SettingsModule
    {
        [DynamicBound(nameof(MaximumGrubCost), true)]
        [MenuRange(0, 46)]
        public int MinimumGrubCost;

        [TriggerValidation(nameof(GrubTolerance))]
        [DynamicBound(nameof(MinimumGrubCost), false)]
        [MenuRange(0, 46)]
        public int MaximumGrubCost;

        [DynamicBound(nameof(GrubToleranceUB), true)]
        [MenuRange(0, 46)]
        public int GrubTolerance;
        private int GrubToleranceUB => 46 - MaximumGrubCost;


        [DynamicBound(nameof(MaximumEssenceCost), true)]
        [MenuRange(0, 2800)]
        public int MinimumEssenceCost;

        [DynamicBound(nameof(MinimumEssenceCost), false)]
        [MenuRange(0, 2800)]
        public int MaximumEssenceCost;

        [MenuRange(0, 250)]
        public int EssenceTolerance;


        [DynamicBound(nameof(MaximumEggCost), true)]
        [MenuRange(0, 21)]
        public int MinimumEggCost;
        
        [TriggerValidation(nameof(EggTolerance))]
        [DynamicBound(nameof(MinimumEggCost), false)]
        [MenuRange(0, 21)]
        public int MaximumEggCost;

        [DynamicBound(nameof(EggToleranceUB), true)]
        [MenuRange(0, 21)]
        public int EggTolerance;
        private int EggToleranceUB => 21 - MaximumEggCost;


        [DynamicBound(nameof(MaximumCharmCost), true)]
        [MenuRange(0, 40)]
        public int MinimumCharmCost;
        
        [TriggerValidation(nameof(CharmTolerance))]
        [DynamicBound(nameof(MinimumCharmCost), false)]
        [MenuRange(0, 40)]
        public int MaximumCharmCost;
        
        [DynamicBound(nameof(CharmToleranceUB), true)]
        [MenuRange(0, 40)]
        public int CharmTolerance;
        private int CharmToleranceUB => 40 - MaximumCharmCost;


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
