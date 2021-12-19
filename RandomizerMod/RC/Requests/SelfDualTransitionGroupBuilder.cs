using RandomizerCore;
using RandomizerCore.Randomization;

namespace RandomizerMod.RC
{
    public class SelfDualTransitionGroupBuilder : GroupBuilder
    {
        public bool coupled;
        public readonly Bucket<string> Transitions = new();

        public override void Apply(List<RandomizationGroup> groups, RandoFactory factory)
        {
            List<IRandoCouple> ts = new();
            foreach (string s in Transitions.EnumerateWithMultiplicity())
            {
                ts.Add(factory.MakeTransition(s));
            }

            if (coupled)
            {
                CoupledRandomizationGroup g = new()
                {
                    Items = ts.ToArray<IRandoItem>(),
                    Locations = ts.ToArray<IRandoLocation>(),
                    Label = label,
                    Strategy = strategy ?? new DefaultGroupPlacementStrategy(0),
                };
                g.Dual = g;
                groups.Add(g);
            }
            else
            {
                RandomizationGroup g = new()
                {
                    Items = ts.ToArray<IRandoItem>(),
                    Locations = ts.ToArray<IRandoLocation>(),
                    Label = label,
                    Strategy = strategy ?? new DefaultGroupPlacementStrategy(0),
                };
                groups.Add(g);
            }
        }

    }
}
