using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RandomizerMod.LogHelper;

namespace RandomizerMod.Logic
{
    public readonly struct RandoPlacement
    {
        public readonly LogicItem item;
        public readonly RandoLocation location;
        

        public RandoPlacement(LogicItem item, RandoLocation location)
        {
            this.item = item;
            this.location = location;
        }

        public void Deconstruct(out LogicItem item, out RandoLocation location)
        {
            item = this.item;
            location = this.location;
        }
    }

    public readonly struct PriorityLocation
    {
        public readonly RandoLocation location;
        public readonly int priority;

        public string name => location.name;

        public PriorityLocation(RandoLocation location, int priority)
        {
            this.location = location;
            this.priority = priority;
        }

        public bool CanGet(ProgressionManager pm)
        {
            return location.CanGet(pm);
        }
    }

    public class RandoLocation
    {
        public LogicDef logic;
        public List<LogicCost> costs;
        public bool multi;

        public string name => logic.name;

        public bool CanGet(ProgressionManager pm)
        {
            if (costs != null)
            {
                if (costs.Any(l => !l.CanGet(pm.obtained))) return false;
            }
            return logic.CanGet(pm.obtained);
        }

        public void AddCost(LogicCost cost)
        {
            if (costs == null) costs = new List<LogicCost>();
            costs.Add(cost);
            Log($"Added cost {cost} to location {name}");
        }

        public RandoLocation Clone()
        {
            RandoLocation rl = MemberwiseClone() as RandoLocation;
            rl.costs = rl.costs?.ToList();
            return rl;
        }

        public IEnumerable<RandoLocation> CloneMany(int count)
        {
            for (int i = 0; i < count; i++) yield return Clone();
        }
    }
}
