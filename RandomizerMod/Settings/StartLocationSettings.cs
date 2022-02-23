using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.RandomizerData;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class StartLocationSettings : SettingsModule
    {
        public enum RandomizeStartLocationType
        {
            Fixed,
            RandomExcludingKP,
            Random,
        }

        public RandomizeStartLocationType StartLocationType;

        public string StartLocation;

        public override void Clamp(GenerationSettings gs)
        {
            base.Clamp(gs);
            if (StartLocationType == RandomizeStartLocationType.Fixed && StartLocation == null)
            {
                LogWarn("Found null fixed start location during Clamp.");
                StartLocation = Data.GetStartNames().First();
            }
        }

        public void SetStartLocation(string start) => StartLocation = start;
    }
}
