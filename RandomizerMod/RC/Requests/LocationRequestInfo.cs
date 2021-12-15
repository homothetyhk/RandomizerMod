using ItemChanger;
using RandomizerCore;

namespace RandomizerMod.RC
{
    public class LocationRequestInfo
    {
        public string stageLabel;
        public string groupLabel;
        public Func<RandoFactory, IRandoLocation> randoLocationCreator;
        public Func<RealLocationCreatorArgs, AbstractPlacement> realPlacementCreator;
        public Action<RandomizerFinishArgs> onRandomizerFinish;
        public Action<AbstractPlacement, RandoPlacement> addToPlacement;

        public class RandomizerFinishArgs
        {
            public RandoPlacement placement;
        }
    }
}
