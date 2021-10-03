using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomizerCore.Logic;

namespace RandomizerMod.RC
{
    public class LogicGeoCost : LogicCost
    {
        public readonly int canReplenishGeoWaypoint;
        public int geoAmount;

        public LogicGeoCost(LogicManager lm, int amount)
        {
            canReplenishGeoWaypoint = lm.GetTermIndex("Can_Replenish_Geo");
            geoAmount = amount;
        }

        public override bool CanGet(ProgressionManager pm)
        {
            return pm.Has(canReplenishGeoWaypoint);
        }

        public override IEnumerable<int> GetTerms()
        {
            yield return canReplenishGeoWaypoint;
        }
    }
}
