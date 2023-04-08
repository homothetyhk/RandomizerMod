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
                MinRandomNotchTotal = 70,
                MaxRandomNotchTotal = 110,
                ExtraPlatforms = true,
                SalubraNotches = MiscSettings.SalubraNotchesSetting.GroupedWithCharmNotchesPool,
                SteelSoul = false,
            };

            Classic = new MiscSettings
            {
                MaskShards = MiscSettings.MaskShardType.FourShardsPerMask,
                VesselFragments = MiscSettings.VesselFragmentType.ThreeFragmentsPerVessel,
                RandomizeNotchCosts = false,
                MinRandomNotchTotal = 70,
                MaxRandomNotchTotal = 110,
                ExtraPlatforms = true,
                SalubraNotches = MiscSettings.SalubraNotchesSetting.AutoGivenAtCharmThreshold,
                SteelSoul = false,
            };

            ConsolidatedItems = new MiscSettings
            {
                MaskShards = MiscSettings.MaskShardType.OneShardPerMask,
                VesselFragments = MiscSettings.VesselFragmentType.OneFragmentPerVessel,
                RandomizeNotchCosts = true,
                MinRandomNotchTotal = 70,
                MaxRandomNotchTotal = 110,
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
