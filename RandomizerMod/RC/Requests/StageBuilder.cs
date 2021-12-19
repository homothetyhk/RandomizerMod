using System.Collections.ObjectModel;
using RandomizerCore.Randomization;

namespace RandomizerMod.RC
{
    public class StageBuilder
    {
        public readonly string label;
        public StagePlacementStrategy strategy;
        private readonly Dictionary<string, GroupBuilder> groupLookup;
        private readonly List<GroupBuilder> groups;
        public readonly ReadOnlyCollection<GroupBuilder> Groups;

        public StageBuilder(string label)
        {
            this.label = label;
            this.groupLookup = new();
            this.groups = new();
            this.Groups = new(groups);
        }

        public void Add(GroupBuilder gb)
        {
            if (gb == null) throw new ArgumentNullException(nameof(gb));
            if (gb.label == null || groupLookup.ContainsKey(gb.label)) throw new ArgumentException(nameof(gb));
            gb.stageLabel = label;
            groups.Add(gb);
            groupLookup.Add(gb.label, gb);
        }

        public void Insert(int index, GroupBuilder gb)
        {
            if (gb == null) throw new ArgumentNullException(nameof(gb));
            if (gb.label == null || groupLookup.ContainsKey(gb.label)) throw new ArgumentException(nameof(gb));
            gb.stageLabel = label;
            groups.Insert(index, gb);
            groupLookup.Add(gb.label, gb);
        }

        public ItemGroupBuilder AddItemGroup(string group)
        {
            return InsertItemGroup(group, groups.Count);
        }

        public ItemGroupBuilder InsertItemGroup(string group, int index)
        {
            if (groupLookup.ContainsKey(group)) throw new ArgumentException(nameof(group));

            ItemGroupBuilder gb = new()
            {
                label = group,
                stageLabel = label,
            };

            groups.Insert(index, gb);
            groupLookup.Add(group, gb);
            return gb;
        }

        public bool TryGetGroup(string group, out GroupBuilder gb)
        {
            return groupLookup.TryGetValue(group, out gb);
        }

        public GroupBuilder Get(string group)
        {
            return groupLookup[group];
        }

        public RandomizationStage ToRandomizationStage(RandoFactory factory)
        {
            List<RandomizationGroup> rgs = new();
            foreach (GroupBuilder gb in groupLookup.Values)
            {
                gb.Apply(rgs, factory);
            }

            return new RandomizationStage
            {
                label = label,
                groups = rgs.ToArray(),
                strategy = strategy ?? new StagePlacementStrategy(),
            };
        }
    }
}
