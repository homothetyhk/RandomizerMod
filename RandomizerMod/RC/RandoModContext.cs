using Newtonsoft.Json;
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

        public RandoModContext(RandoModContext ctx) : base(ctx.LM)
        {
            notchCosts = ctx.notchCosts.ToList();
            itemPlacements = ctx.itemPlacements.ToList();
            transitionPlacements = ctx.transitionPlacements.ToList();
            StartDef = ctx.StartDef;
            InitialProgression = ctx.InitialProgression;
            Vanilla = ctx.Vanilla.ToList();
            GenerationSettings = ctx.GenerationSettings;
        }

        public GenerationSettings GenerationSettings { get; init; }
        public StartDef StartDef { get; init; }

        public List<GeneralizedPlacement> Vanilla = new();
        public List<ItemPlacement> itemPlacements = new();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<TransitionPlacement> transitionPlacements = new();
        public List<int> notchCosts = new();

        public override IEnumerable<GeneralizedPlacement> EnumerateExistingPlacements()
        {
            foreach (GeneralizedPlacement p in Vanilla) yield return p;
            foreach (ItemPlacement p in itemPlacements) yield return p;
            foreach (TransitionPlacement p in transitionPlacements) yield return p;
        }

    }
}
