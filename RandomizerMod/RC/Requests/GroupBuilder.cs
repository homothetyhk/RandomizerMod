using RandomizerCore.Randomization;

namespace RandomizerMod.RC
{
    public abstract class GroupBuilder
    {
        public string label;
        public string stageLabel;
        public Action<Random, RandomizationGroup> onPermute;
        public GroupPlacementStrategy strategy;

        public abstract void Apply(List<RandomizationGroup> groups, RandoFactory factory);
    }
}
