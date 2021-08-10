using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class GrubCostRandomizerSettings : ICloneable
    {
        public bool RandomizeGrubItemCosts;
        public int MinimumGrubCost;
        public int MaximumGrubCost;
        public int GrubTolerance;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
