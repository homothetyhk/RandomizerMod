using RandomizerCore.Randomization;

namespace RandomizerMod.RC
{
    public abstract class GroupBuilder
    {
        public string label;
        public string stageLabel;
        public Action<Random, RandomizationGroup>? onPermute;
        public GroupPlacementStrategy? strategy;
        /// <summary>
        /// An action invoked on each group created by the GroupBuilder. Note that some GroupBuilders may create multiple groups.
        /// </summary>
        public Action<RandomizationGroup>? OnCreateGroup;

        public abstract void Apply(List<RandomizationGroup> groups, RandoFactory factory);
    }
}
