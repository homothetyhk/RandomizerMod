namespace RandomizerMod.Settings
{
    public class MiscSettings : SettingsModule
    {
        public bool RandomizeNotchCosts;
        public bool ExtraPlatforms;
        public SalubraNotchesSetting SalubraNotches;
        public MaskShardType MaskShards;
        public VesselFragmentType VesselFragments;
        public bool SteelSoul;

        public enum MaskShardType
        {
            FourShardsPerMask,
            TwoShardsPerMask,
            OneShardPerMask
        }
        
        public enum VesselFragmentType
        {
            ThreeFragmentsPerVessel,
            TwoFragmentsPerVessel,
            OneFragmentPerVessel
        }
        
        public enum SalubraNotchesSetting
        {
            GroupedWithCharmNotchesPool,
            Vanilla,
            Randomized,
            AutoGivenAtCharmThreshold
        }

        public override void Clamp(GenerationSettings gs)
        {
            base.Clamp(gs);
            if (gs.SkipSettings.ShadeSkips) gs.SkipSettings.ShadeSkips = false;
        }
    }
}
