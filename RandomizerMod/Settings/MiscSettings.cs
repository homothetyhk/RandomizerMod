using MenuChanger.Attributes;

namespace RandomizerMod.Settings
{
    public class MiscSettings : SettingsModule
    {
        public bool RandomizeNotchCosts;
        [MenuRange(0, 240)][DynamicBound(nameof(MaxRandomNotchTotal), true)] public int MinRandomNotchTotal = 70;
        [MenuRange(0, 240)][DynamicBound(nameof(MinRandomNotchTotal), false)] public int MaxRandomNotchTotal = 110;
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
            if (SteelSoul) gs.SkipSettings.ShadeSkips = false;
        }
    }
}
