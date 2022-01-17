using RandomizerCore;
using RandomizerMod.Settings;

namespace RandomizerMod.RC
{
    public class RandoModContext : RandoContext
    {
        public RandoModContext(GenerationSettings gs)
        {
            base.LM = RCData.GetNewLogicManager(gs);
            base.InitialProgression = new ProgressionInitializer(LM, gs);
        }
    }
}
