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
            item = new SingleItem($"{geo}_Geo", new(lm.GetTermStrict("GEO"), geo));
        }
    }
}
