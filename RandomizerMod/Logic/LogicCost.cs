using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.Settings;

namespace RandomizerMod.Logic
{
    public abstract class LogicCost
    {
        public static LogicCost NewGrubCost(LogicManager lm, int cost)
        {
            return new GrubCost(lm, cost);
        }

        public static LogicCost NewEssenceCost(LogicManager lm, int cost)
        {
            return new EssenceCost(lm, cost);
        }

        public abstract bool CanGet(int[] obtained);
    }

    public class GrubCost : LogicCost
    {
        public readonly int cost;
        public readonly int index;

        public GrubCost(LogicManager lm, GenerationSettings gs, Random rng)
        {
            cost = rng.Next(gs.GrubCostRandomizerSettings.MinimumGrubCost, gs.GrubCostRandomizerSettings.MaximumGrubCost + 1);
            index = lm.GetIndex("GRUBS");
        }

        public GrubCost(LogicManager lm, int cost)
        {
            this.cost = cost;
            this.index = lm.GetIndex("GRUBS");
        }

        public override bool CanGet(int[] obtained)
        {
            return obtained[index] > cost - 1;
        }

        public override string ToString()
        {
            return $"Grub Cost: {cost}";
        }
    }

    public class EssenceCost : LogicCost
    {
        public readonly int cost;
        public readonly int index;

        public EssenceCost(LogicManager lm, GenerationSettings gs, Random rng)
        {
            cost = rng.Next(gs.EssenceCostRandomizerSettings.MinimumEssenceCost, gs.EssenceCostRandomizerSettings.MaximumEssenceCost + 1);
            index = lm.GetIndex("ESSENCE");
        }

        public EssenceCost(LogicManager lm, int cost)
        {
            this.cost = cost;
            this.index = lm.GetIndex("ESSENCE");
        }


        public override bool CanGet(int[] obtained)
        {
            return obtained[index] > cost - 1;
        }

        public override string ToString()
        {
            return $"Essence Cost: {cost}";
        }
    }




    public class LateRandomizableGeoCost
    {
        public int value;

        public void Finalize(GenerationSettings gs, Random rng, ProgressionManager pm)
        {
            int geo = pm.Get("GEO");
            if (geo < 100) value = 5;
            else
            {
                value = 5 * rng.Next(1, geo / 50);
            }

            pm.Incr("GEO", -value);
        }
    }

}
