using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class StartLocationSettings : ICloneable
    {
        public enum RandomizeStartLocationType : byte
        {
            Fixed,
            RandomExcludingKP,
            Random,
            RandomWithMinorForcedStartItems,
            RandomWithAnyForcedStartItems
        }

        public RandomizeStartLocationType StartLocationType;

        public string StartLocation;

        public void SetStartLocation(string start) => StartLocation = start;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
