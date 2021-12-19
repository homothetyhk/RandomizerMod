using ItemChanger;
using RandomizerCore;

namespace RandomizerMod.RC
{
    public class ItemRequestInfo
    {
        public Func<RandoFactory, RandoModItem> randoItemCreator;
        public Action<RandoFactory, RandoModItem> onRandoItemCreation;
        public Action<RandoPlacement> onRandomizerFinish;
        public Func<ICFactory, RandoPlacement, AbstractItem> realItemCreator;
    }
}
