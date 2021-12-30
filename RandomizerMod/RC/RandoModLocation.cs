using ItemChanger;
using Newtonsoft.Json;
using RandomizerCore;

namespace RandomizerMod.RC
{
    public class RandoModLocation : RandoLocation
    {
        [field: JsonIgnore] public Action<RandoPlacement> onRandomizerFinish;
        [field: JsonIgnore] public Func<ICFactory, RandoPlacement, AbstractPlacement> customPlacementFetch;
        [field: JsonIgnore] public Action<ICFactory, RandoPlacement, AbstractPlacement> onPlacementFetch;
        [field: JsonIgnore] public Action<ICFactory, RandoPlacement, AbstractPlacement, AbstractItem> customAddToPlacement;
    }
}
