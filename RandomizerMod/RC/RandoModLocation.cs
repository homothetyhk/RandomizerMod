using ItemChanger;
using Newtonsoft.Json;
using RandomizerCore;
using RandomizerMod.RandomizerData;

namespace RandomizerMod.RC
{
    public class RandoModLocation : RandoLocation
    {
        [JsonIgnore] public LocationRequestInfo? info;
        public bool TryGetLocationDef(out LocationDef def)
        {
            def = info?.getLocationDef != null ? info.getLocationDef() : Data.GetLocationDef(Name);
            return def is not null;
        }
    }
}
