namespace RandomizerMod.Settings.Presets
{
    public static class ProgressionDepthPresetData
    {
        public static ProgressionDepthSettings Default;
        public static ProgressionDepthSettings Relaxed;
        public static ProgressionDepthSettings Unweighted;
        public static ProgressionDepthSettings DelayedWeight;
        public static Dictionary<string, ProgressionDepthSettings> Presets;

        static ProgressionDepthPresetData()
        {
            Default = new();
            Relaxed = new()
            {
                LocationPriorityTransformCoefficient = 2.5f,
                LocationPriorityTransformType = RandomizerCore.Randomization.PriorityTransformUtil.TransformType.SquareRoot
            };
            Unweighted = new()
            {
                ItemLocationPriorityInteraction = RandomizerCore.Randomization.PriorityTransformUtil.ItemPriorityDepthEffect.Ignore,
                LocationPriorityTransformCoefficient = 0f,
                LocationPriorityTransformType = RandomizerCore.Randomization.PriorityTransformUtil.TransformType.Linear,
                DuplicateItemPenalty = false,
                MultiLocationPenalty = false,
                TransitionPriorityTransformCoefficient = 0f,
                TransitionTransitionPriorityInteraction = RandomizerCore.Randomization.PriorityTransformUtil.ItemPriorityDepthEffect.Ignore,
                TransitionPriorityTransformType = RandomizerCore.Randomization.PriorityTransformUtil.TransformType.Linear,
            };
            DelayedWeight = new()
            {
                ItemLocationPriorityInteraction = RandomizerCore.Randomization.PriorityTransformUtil.ItemPriorityDepthEffect.Fade,
                LocationPriorityTransformCoefficient = 0.1f,
                LocationPriorityTransformType = RandomizerCore.Randomization.PriorityTransformUtil.TransformType.Quadratic,
            };

            Presets = new()
            {
                { "Default", Default },
                { "Relaxed", Relaxed },
                { "Unweighted", Unweighted },
                { "Delayed Weight", DelayedWeight},
            };
        }
    }
}
