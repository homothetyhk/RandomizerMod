using RandomizerCore.Randomization;
using static RandomizerCore.Randomization.PriorityTransformUtil;

namespace RandomizerMod.Settings
{
    public class ProgressionDepthSettings : SettingsModule
    {
        public bool MultiLocationPenalty = true;
        public bool DuplicateItemPenalty = true;

        public TransformType LocationPriorityTransformType = TransformType.Linear;
        public ItemPriorityDepthEffect ItemLocationPriorityInteraction = ItemPriorityDepthEffect.Cliff;
        public float LocationPriorityTransformCoefficient = 3f;

        public TransformType TransitionPriorityTransformType = TransformType.SquareRoot;
        public ItemPriorityDepthEffect TransitionTransitionPriorityInteraction = ItemPriorityDepthEffect.Cliff;
        public float TransitionPriorityTransformCoefficient = 1f;

        public DefaultGroupPlacementStrategy GetItemPlacementStrategy()
        {
            return new(CreateTransform(LocationPriorityTransformCoefficient, LocationPriorityTransformType, ItemLocationPriorityInteraction));
        }

        public DefaultGroupPlacementStrategy GetTransitionPlacementStrategy()
        {
            return new(CreateTransform(TransitionPriorityTransformCoefficient, TransitionPriorityTransformType, TransitionTransitionPriorityInteraction));
        }
    }
}
