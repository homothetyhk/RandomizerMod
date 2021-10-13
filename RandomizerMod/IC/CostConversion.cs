using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemChanger;
using RandomizerMod.RandomizerData;
using RandomizerCore.Logic;

namespace RandomizerMod.IC
{
    public static class CostConversion
    {
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
                        return new ItemChanger.Modules.CumulativeRancidEggCost
                        {
                            Total = sc.threshold,
                        };
                    case "GEO":
                        return Cost.NewGeoCost(sc.threshold);

                    case "SIMPLE": // not actually used, since godtuner no longer requires a separate cost
                        return new ConsumablePDIntCost
                        {
                            fieldName = nameof(PlayerData.simpleKeys),
                            amount = 1,
                            uiText = "Use 1 Simple Key",
                        };
                    
                    case "Spore_Shroom": // not actually used, since lore tablet locations no longer become shiny
                        return new PDBoolCost
                        {
                            fieldName = nameof(PlayerData.equippedCharm_17),
                            uiText = "Equip Spore Shroom",
                        };

                    case "CHARMS":
                        return new PDIntCost
                        {
                            amount = sc.threshold,
                            fieldName = nameof(PlayerData.charmsOwned),
                            uiText = $"Once you own {sc.threshold} charm{(sc.threshold != 1 ? "s" : "")}, I'll gladly sell it to you."
                        };
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
