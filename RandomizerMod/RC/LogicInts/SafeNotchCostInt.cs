using RandomizerCore.Logic;

namespace RandomizerMod.RC.LogicInts
{
    /// <summary>
    /// LogicInt which returns 1 less than the number of notches needed to equip the charm without overcharming.
    /// </summary>
    public class SafeNotchCostInt : LogicInt
    {
        // the ids should correspond to the 1-40 charm nums (i.e. 1-indexed)
        public readonly int[] charmIDs;

        public SafeNotchCostInt(params int[] charmIDs)
        {
            this.charmIDs = charmIDs;
            Array.Sort(charmIDs);
            Name = $"$SafeNotchCost[{string.Join(",", charmIDs)}]";
        }

        public override string Name { get; }

        public override int GetValue(object sender, ProgressionManager pm)
        {
            List<int> notchCosts = (pm.ctx as RandoModContext)?.notchCosts;
            if (notchCosts != null && notchCosts.Count >= charmIDs[charmIDs.Length - 1])
            {
                return charmIDs.Sum(i => notchCosts[i - 1]) - 1;
            }
            else
            {
                return charmIDs.Sum(i => CharmNotchCosts.GetVanillaCost(i)) - 1;
            }
        }

        public override IEnumerable<Term> GetTerms() => Enumerable.Empty<Term>();
    }
}
