using ItemChanger;
using RandomizerCore;

namespace RandomizerMod.RC
{
    public class ItemRequestInfo
    {
        public Func<RandoFactory, IRandoItem> randoItemCreator;
        public Func<RealItemCreatorArgs, AbstractItem> realItemCreator;
    }
}
