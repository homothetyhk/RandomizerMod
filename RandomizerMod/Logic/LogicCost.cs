using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.Settings;

namespace RandomizerMod.Logic
{
    public class LogicCost
    {
        public static LogicCost NewGrubCost(LogicManager lm, int cost)
        {
            return new LogicCost
            {
                logic = lm.FromString($"GRUBS>{cost - 1}", "Grub_Cost") 
            };
        }

        public static LogicCost NewEssenceCost(LogicManager lm, int cost)
        {
            return new LogicCost
            {
                logic = lm.FromString($"ESSENCE>{cost - 1}", "Grub_Cost")
            };
        }

        public LogicDef logic;

        /*
        public ItemChanger.Cost ToRealCost()
        {
            throw new NotImplementedException();
        }
        */

    }

    public class RandomizableGrubCost
    {
        public int value;

        public void Initialize(GenerationSettings gs, Random rng)
        {
            value = rng.Next(gs.GrubCostRandomizerSettings.MinimumGrubCost, gs.GrubCostRandomizerSettings.MaximumGrubCost + 1);
        }
    }

    public class RandomizableEssenceCost
    {
        public int value;

        public void Initialize(GenerationSettings gs, Random rng)
        {
            value = rng.Next(gs.EssenceCostRandomizerSettings.MinimumEssenceCost, gs.EssenceCostRandomizerSettings.MaximumEssenceCost + 1);
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
