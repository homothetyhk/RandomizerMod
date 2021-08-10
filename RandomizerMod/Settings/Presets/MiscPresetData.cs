using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings.Presets
{
    public static class MiscPresetData
    {
        public static MiscSettings Standard;
        public static MiscSettings Classic;
        public static MiscSettings Special;
        public static Dictionary<string, MiscSettings> MiscPresets;

        static MiscPresetData()
        {
            Standard = new MiscSettings
            {
                MaskShards = MiscSettings.MaskShardType.FourShardsPerMask,
                VesselFragments = MiscSettings.VesselFragmentType.ThreeFragmentsPerVessel,
                RandomizeNotchCosts = true,
            };

            Classic = new MiscSettings
            {
                MaskShards = MiscSettings.MaskShardType.FourShardsPerMask,
                VesselFragments = MiscSettings.VesselFragmentType.ThreeFragmentsPerVessel,
                RandomizeNotchCosts = false,
            };

            Special = new MiscSettings
            {
                MaskShards = MiscSettings.MaskShardType.OneShardPerMask,
                VesselFragments = MiscSettings.VesselFragmentType.OneFragmentPerVessel,
                RandomizeNotchCosts = true,
            };

            MiscPresets = new Dictionary<string, MiscSettings>
            {
                { "Standard", Standard },
                { "Classic", Classic },
                { "Special", Special },
            };
        }
    }
}
