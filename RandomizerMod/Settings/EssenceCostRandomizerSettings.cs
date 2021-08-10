using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class EssenceCostRandomizerSettings : ICloneable
    {
        public bool RandomizeEssenceItemCosts;
        public int MinimumEssenceCost;
        public int MaximumEssenceCost;
        public int EssenceTolerance;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
