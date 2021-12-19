using RandomizerCore;
using RandomizerCore.Randomization;

namespace RandomizerMod.RC
{
    public class ItemGroupBuilder : GroupBuilder
    {
        public ItemGroupBuilder() { }

        public delegate IEnumerable<IRandoItem> ItemPaddingHandler(RandoFactory factory, int count);
        public delegate IEnumerable<IRandoLocation> LocationPaddingHandler(RandoFactory factory, int count);
        public ItemPaddingHandler ItemPadder;
        public LocationPaddingHandler LocationPadder;
        
        public readonly Bucket<string> Items = new();
        public readonly Bucket<string> Locations = new();


        public override void Apply(List<RandomizationGroup> groups, RandoFactory factory)
        {
            List<IRandoItem> items = new();
            foreach (string i in Items.EnumerateWithMultiplicity())
            {
                items.Add(factory.MakeItem(i));
            }
            List<IRandoLocation> locations = new();
            foreach (string i in Locations.EnumerateWithMultiplicity())
            {
                locations.Add(factory.MakeLocation(i));
            }

            int diff = items.Count - locations.Count;
            if (diff > 0 && LocationPadder != null)
            {
                locations.AddRange(LocationPadder(factory, diff));
            }
            else if (diff < 0 && ItemPadder != null)
            {
                items.AddRange(ItemPadder(factory, -diff));
            }

            if (items.Count != locations.Count) throw new InvalidOperationException($"Failed to build group {label} due to unbalanced counts.");

            RandomizationGroup group = new()
            {
                Items = items.ToArray<IRandoItem>(),
                Locations = locations.ToArray<IRandoLocation>(),
                Label = label,
                Strategy = strategy ?? new DefaultGroupPlacementStrategy(0),
            };
            group.OnPermute += onPermute;

            groups.Add(group);
        }
    }
}
