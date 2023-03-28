using RandomizerCore.Logic;
using RandomizerMod.RC;

namespace RandomizerMod.RandomizerData
{
    public record CostDef(string Term, int Amount)
    {
        public virtual LogicCost ToLogicCost(LogicManager lm)
        {
            return Term switch
            {
                "GEO" => new LogicGeoCost(lm, Amount),
                _ => new SimpleCost(lm.GetTermStrict(Term), Amount),
            };
        }
    }
}
