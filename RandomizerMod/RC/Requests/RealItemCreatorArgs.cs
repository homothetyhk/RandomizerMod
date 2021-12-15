using ItemChanger;
using RandomizerCore;

namespace RandomizerMod.RC
{
    public abstract class RealItemCreatorArgs
    {
        public readonly RandoItem item;
        public readonly RandoLocation location;
        public abstract AbstractItem GetDefault(string name);
    }
}
