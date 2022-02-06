using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;

namespace RandomizerMod.RC
{
    public class RandoModContext : RandoContext
    {
        public RandoModContext(LogicManager LM) : base(LM) { }

        public RandoModContext(GenerationSettings gs, StartDef startDef) : base(RCData.GetNewLogicManager(gs))
        {
            base.InitialProgression = new ProgressionInitializer(LM, gs, startDef);
            this.GenerationSettings = gs;
            this.StartDef = startDef;
        }

        public GenerationSettings GenerationSettings { get; init; }
        public StartDef StartDef { get; init; }

        public List<GeneralizedPlacement> Vanilla;
        public List<ItemPlacement> itemPlacements;
        public List<TransitionPlacement> transitionPlacements;
        public List<int> notchCosts;

        public override IEnumerable<GeneralizedPlacement> EnumerateExistingPlacements()
        {
            if (Vanilla != null) foreach (GeneralizedPlacement p in Vanilla) yield return p;
            if (itemPlacements != null) foreach (ItemPlacement p in itemPlacements) yield return p;
            if (transitionPlacements != null) foreach (TransitionPlacement p in transitionPlacements) yield return p;
        }

    }
}
