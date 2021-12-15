using ItemChanger;
using RandomizerCore.Logic;

namespace RandomizerMod.RC
{
    public abstract class RealLocationCreatorArgs
    {
        public readonly LogicManager lm;
        public abstract AbstractPlacement GetDefault(string name);
    }
}
