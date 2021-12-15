using RandomizerCore;
using RandomizerCore.Randomization;

namespace RandomizerMod.RC
{
    public class TransitionGroupBuilder : GroupBuilder
    {
        public readonly Bucket<string> Sources = new();
        public readonly Bucket<string> Targets = new();

        public override void Apply(List<RandomizationGroup> groups, RandoFactory factory)
        {
            if (Sources.GetTotal() != Targets.GetTotal())
            {
                throw new InvalidOperationException($"Failed to build group {label} due to unbalanced counts.");
            }

            List<IRandoCouple> locations = new();
            foreach (string s in Sources.EnumerateWithMultiplicity())
            {
                locations.Add(factory.MakeTransition(s));
            }

            List<IRandoCouple> items = new();
            foreach (string s in Targets.EnumerateWithMultiplicity())
            {
                items.Add(factory.MakeTransition(s));
            }

            groups.Add(new RandomizationGroup
            {
                Label = label,
                Items = items.ToArray<IRandoItem>(),
                Locations = locations.ToArray<IRandoLocation>(),
            });
        }
    }
}
