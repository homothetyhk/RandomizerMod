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
        public static MiscSettings ConsolidatedItems;
        public static Dictionary<string, MiscSettings> MiscPresets;

        static MiscPresetData()
        {
            Standard = new MiscSettings
            {
                MaskShards = MiscSettings.MaskShardType.FourShardsPerMask,
                VesselFragments = MiscSettings.VesselFragmentType.ThreeFragmentsPerVessel,
                RandomizeNotchCosts = true,
                ExtraPlatforms = true,
                SalubraNotches = MiscSettings.SalubraNotchesSetting.GroupedWithCharmNotchesPool,
                SteelSoul = false,
            };

            Classic = new MiscSettings
            {
                MaskShards = MiscSettings.MaskShardType.FourShardsPerMask,
                VesselFragments = MiscSettings.VesselFragmentType.ThreeFragmentsPerVessel,
                RandomizeNotchCosts = false,
                ExtraPlatforms = true,
                SalubraNotches = MiscSettings.SalubraNotchesSetting.AutoGivenAtCharmThreshold,
                SteelSoul = false,
            };

            ConsolidatedItems = new MiscSettings
            {
                MaskShards = MiscSettings.MaskShardType.OneShardPerMask,
                VesselFragments = MiscSettings.VesselFragmentType.OneFragmentPerVessel,
                RandomizeNotchCosts = true,
                ExtraPlatforms = true,
                SalubraNotches = MiscSettings.SalubraNotchesSetting.GroupedWithCharmNotchesPool,
                SteelSoul = false,
            };

            MiscPresets = new Dictionary<string, MiscSettings>
            {
                { "Standard", Standard },
                { "Classic", Classic },
                { "Consolidated Items", ConsolidatedItems },
            };
        }
    }
}
