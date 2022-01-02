namespace RandomizerMod.Settings
{
    public class MiscSettings : SettingsModule
    {
        public bool RandomizeNotchCosts;
        public bool ExtraPlatforms;
        public SalubraNotchesSetting SalubraNotches;
        public MaskShardType MaskShards;
        public VesselFragmentType VesselFragments;

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
    }
}
