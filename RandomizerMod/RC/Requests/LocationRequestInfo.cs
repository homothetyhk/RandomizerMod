using ItemChanger;
using RandomizerCore;
using RandomizerMod.Settings;

namespace RandomizerMod.RC
{
    public class LocationRequestInfo
    {
        public Func<RandoFactory, RandoModLocation> randoLocationCreator;
        public Action<RandoFactory, RandoModLocation> onRandoLocationCreation;
        public Action<RandoPlacement> onRandomizerFinish;
        public Func<ICFactory, RandoPlacement, AbstractPlacement> customPlacementFetch;
        public Action<ICFactory, RandoPlacement, AbstractPlacement, AbstractItem> customAddToPlacement;
    }
}
