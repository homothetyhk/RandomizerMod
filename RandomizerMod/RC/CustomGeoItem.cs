using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;

namespace RandomizerMod.RC
{
    public class CustomGeoItem : RandoModItem
    {
        public int geo;

        [Newtonsoft.Json.JsonConstructor]
        private CustomGeoItem() { }

        public CustomGeoItem(LogicManager lm, int geo)
        {
            this.geo = geo;
            item = new SingleItem($"{geo}_Geo", new(lm.GetTerm("GEO"), geo));
        }
    }
}
