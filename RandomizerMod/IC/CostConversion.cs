using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemChanger;
using ItemChanger.Placements;
using ItemChanger.Tags;
using RandomizerMod.RandomizerData;
using RandomizerCore.Logic;
using static RandomizerMod.LogHelper;

namespace RandomizerMod.IC
{
    public static class CostConversion
    {
        public static void HandleCosts(List<LogicCost> lcs, AbstractItem i, AbstractPlacement p)
        {
            List<Cost> existingCosts = new();
            bool includeNoninherentCosts = (p as ISingleCostPlacement)?.Cost == null;

            existingCosts.AddRange(p.GetTags<ImplicitCostTag>().Where(t => t.Inherent || includeNoninherentCosts).Select(t => t.Cost));
            if (p is IPrimaryLocationPlacement ip) existingCosts.AddRange(ip.Location.GetTags<ImplicitCostTag>().Where(t => t.Inherent || includeNoninherentCosts).Select(t => t.Cost));

            if (p is IMultiCostPlacement)
            {
                if (i.GetOrAddTag<CostTag>().Cost is Cost c) existingCosts.Add(c);

                foreach (var lc in lcs)
                {
                    if (TryToRequiredCost(lc, existingCosts, out Cost cost))
                    {
                        i.GetTag<CostTag>().Cost += cost;
                    }
                }
            }
            else if (p is ISingleCostPlacement iscp)
            {
                if (iscp.Cost != null) existingCosts.Add(iscp.Cost);

                foreach (var lc in lcs)
                {
                    if (TryToRequiredCost(lc, existingCosts, out Cost cost))
                    {
                        iscp.Cost += cost;
                    }
                }
            }
            else
            {
                if (lcs.Any(lc => TryToRequiredCost(lc, existingCosts, out _)))
                {
                    throw new InvalidOperationException($"Attached cost {lcs[0]} to placement {p.Name} which does not support costs!");
                }
            }
        }

        public static bool TryToRequiredCost(LogicCost lc, List<Cost> existingCosts, out Cost newCost)
        {
            Cost cost = Convert(lc);

            if (existingCosts.Any(c => c.Includes(cost)))
            {
                newCost = null;
                return false;
            }
            else
            {
                newCost = cost;
                return true;
            }
        }

        public static Cost Convert(LogicCost cost)
        {
            if (cost is SimpleCost sc)
            {
                switch (sc.term.Name)
                {
                    case "ESSENCE":
                        return Cost.NewEssenceCost(sc.threshold);
                    case "GRUBS":
                        return Cost.NewGrubCost(sc.threshold);
                    case "RANCIDEGGS":
                        return new ItemChanger.Modules.CumulativeRancidEggCost(sc.threshold);
                    case "GEO":
                        return Cost.NewGeoCost(sc.threshold);

                    case "SIMPLE": // not actually used, since godtuner no longer requires a separate cost
                        return new ConsumablePDIntCost(1, nameof(PlayerData.simpleKeys), "Use 1 Simple Key");
                    
                    case "Spore_Shroom": // not actually used, since lore tablet locations no longer become shiny
                        return new PDBoolCost(nameof(PlayerData.equippedCharm_17), "Requires Spore Shroom Equipped");

                    case "CHARMS":
                        return new PDIntCost(sc.threshold, nameof(PlayerData.charmsOwned), $"Once you own {sc.threshold} charm{(sc.threshold != 1 ? "s" : "")}, I'll gladly sell it to you.");

                    case "DREAMNAIL":
                        return new PDBoolCost(nameof(PlayerData.hasDreamNail), "Requires Dream Nail");

                    case "SCREAM":
                        return new PDIntCost(sc.threshold, nameof(PlayerData.screamLevel), "Requires Howling Wraiths");
                }
            }

            if (cost is RC.LogicGeoCost gc)
            {
                return Cost.NewGeoCost(gc.GeoAmount);
            }

            throw new NotSupportedException($"Cost {cost} conversion is not supported.");
        }

        public static Cost Convert(IEnumerable<LogicCost> costs)
        {
            if (costs == null) return null;
            return costs.Aggregate(null, (Cost c, LogicCost d) => c + Convert(d));
        }

    }
}
