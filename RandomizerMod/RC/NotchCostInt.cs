using RandomizerCore.Logic;

namespace RandomizerMod.RC
{
    public class NotchCostInt : LogicInt
    {
        // the ids should correspond to the 1-40 charm nums (i.e. 1-indexed)
        public readonly int[] charmIDs;

        public NotchCostInt(params int[] charmIDs)
        {
            this.charmIDs = charmIDs;
            Array.Sort(charmIDs);
            Name = $"$NotchCost[{string.Join(",", charmIDs)}]";
        }

        public override string Name { get; }

        public override int GetValue(object sender, ProgressionManager pm)
        {
            List<int> notchCosts = (pm.ctx as RandoModContext)?.notchCosts;
            if (notchCosts != null && notchCosts.Count >= charmIDs[charmIDs.Length - 1])
            {
                return charmIDs.Sum(i => notchCosts[i - 1]) - charmIDs.Max(i => notchCosts[i - 1]);
            }
            else
            {
                return charmIDs.Sum(i => CharmNotchCosts.GetVanillaCost(i)) - charmIDs.Max(i => CharmNotchCosts.GetVanillaCost(i));
            }
        }

        public override IEnumerable<Term> GetTerms() => Enumerable.Empty<Term>();
    }
}
