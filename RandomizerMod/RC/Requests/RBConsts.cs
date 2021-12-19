namespace RandomizerMod.RC
{
    /// <summary>
    /// Strings used by the RequestBuilder as labels.
    /// </summary>
    public static class RBConsts
    {
        /// <summary>
        /// Label for the transition stage, in transition rando only.
        /// </summary>
        public const string MainTransitionStage = "Main Transition Stage";
        /// <summary>
        /// Label for the default stage, in any rando.
        /// </summary>
        public const string MainItemStage = "Main Item Stage";

        /// <summary>
        /// Label for the default group of the default stage, in any rando.
        /// </summary>
        public const string MainItemGroup = "Main Item Group";
        /// <summary>
        /// Label for the group of grubs and mimics, when mimics are randomized but grubs are not randomized.
        /// </summary>
        public const string GrubMimicGroup = "Grub Mimic Group";

        // TODO: move these to settings
        public const int MIN_MIMIC_COUNT = 4;
        public const int MAX_MIMIC_COUNT = 14;

        /// <summary>
        /// Label for the corresponding matched transition group.
        /// </summary>
        public const string InLeftOutRightGroup = "Left -> Right";
        /// <summary>
        /// Label for the corresponding matched transition group.
        /// </summary>
        public const string InRightOutLeftGroup = "Right -> Left";
        /// <summary>
        /// Label for the corresponding matched transition group.
        /// </summary>
        public const string InBotOutTopGroup = "Bot -> Top";
        /// <summary>
        /// Label for the corresponding matched transition group.
        /// </summary>
        public const string InTopOutBotGroup = "Top -> Bot";
        /// <summary>
        /// Label for the group of one-way transitions in any transition rando.
        /// </summary>
        public const string OneWayGroup = "One Way Transitions";
        /// <summary>
        /// Label for the group of two-way transitions, in non-matched transition rando.
        /// </summary>
        public const string TwoWayGroup = "Two Way Transitions";
    }
}
