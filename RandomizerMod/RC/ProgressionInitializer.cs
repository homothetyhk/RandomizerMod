using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;

namespace RandomizerMod.RC
{
    public class ProgressionInitializer : ILogicItem
    {
        /// <summary>
        /// Event invoked after base randomizer term modifiers are added to the initializer.
        /// </summary>
        public static event Action<LogicManager, GenerationSettings, ProgressionInitializer> OnCreateProgressionInitializer;

        public ProgressionInitializer() { }
        public ProgressionInitializer(LogicManager lm, GenerationSettings gs, StartDef startDef)
        {
            foreach (string setting in Data.GetApplicableLogicSettings(gs))
            {
                Setters.Add(new(lm.GetTermStrict(setting), 1));
            }

            Setters.Add(new(lm.GetTermStrict(gs.TransitionSettings.Mode switch
            {
                TransitionSettings.TransitionMode.None => "ITEMRANDO",
                TransitionSettings.TransitionMode.MapAreaRandomizer => "MAPAREARANDO",
                TransitionSettings.TransitionMode.FullAreaRandomizer => "FULLAREARANDO",
                _ => "ROOMRANDO",
            }), 1));

            foreach (TermValue tv in startDef.GetStartLocationProgression(lm))
            {
                if (tv.Term.Type == TermType.State) StartStateTerms.Add(tv.Term);
                else Setters.Add(tv);
            }

            Setters.Add(new(lm.GetTermStrict("GRUBS"), -gs.CostSettings.GrubTolerance));
            Setters.Add(new(lm.GetTermStrict("ESSENCE"), -gs.CostSettings.EssenceTolerance));
            Setters.Add(new(lm.GetTermStrict("RANCIDEGGS"), -gs.CostSettings.EggTolerance));
            Setters.Add(new(lm.GetTermStrict("CHARMS"), -gs.CostSettings.CharmTolerance));

            Setters.Add(new(lm.GetTermStrict("MASKSHARDS"), 20 - 4 * gs.CursedSettings.CursedMasks));
            Setters.Add(new(lm.GetTermStrict("NOTCHES"), 3 - gs.CursedSettings.CursedNotches));

            try
            {
                OnCreateProgressionInitializer?.Invoke(lm, gs, this);
            }
            catch (Exception e)
            {
                LogError($"Error invoking OnCreateProgressionInitializer:\n{e}");
            }
        }

        public List<TermValue> Setters = new();
        public List<TermValue> Increments = new();
        public List<Term> StartStateTerms = new();

        public string Name => "Progression Initializer";

        public void AddTo(ProgressionManager pm)
        {
            foreach (TermValue tv in Setters) pm.Set(tv);
            foreach (TermValue tv in Increments) pm.Incr(tv);
            foreach (Term t in StartStateTerms) pm.SetState(t, pm.lm.StateManager.StartStateSingleton);
        }

        public IEnumerable<Term> GetAffectedTerms()
        {
            foreach (TermValue tv in Setters) yield return tv.Term;
            foreach (TermValue tv in Increments) yield return tv.Term;
            foreach (Term t in StartStateTerms) yield return t;
        }
    }
}
