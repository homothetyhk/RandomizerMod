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
                Setters.Add(new(lm.GetTerm(setting), 1));
            }

            Setters.Add(new(lm.GetTerm(gs.TransitionSettings.Mode switch
            {
                TransitionSettings.TransitionMode.None => "ITEMRANDO",
                TransitionSettings.TransitionMode.MapAreaRandomizer => "MAPAREARANDO",
                TransitionSettings.TransitionMode.FullAreaRandomizer => "FULLAREARANDO",
                _ => "ROOMRANDO",
            }), 1));

            foreach (TermValue tv in startDef.GetStartLocationProgression(lm))
            {
                if (tv.Term.Type == TermType.State) FullStateTerms.Add(tv.Term);
                else Setters.Add(tv);
            }

            Setters.Add(new(lm.GetTerm("GRUBS"), -gs.CostSettings.GrubTolerance));
            Setters.Add(new(lm.GetTerm("ESSENCE"), -gs.CostSettings.EssenceTolerance));
            Setters.Add(new(lm.GetTerm("RANCIDEGGS"), -gs.CostSettings.EggTolerance));
            Setters.Add(new(lm.GetTerm("CHARMS"), -gs.CostSettings.CharmTolerance));

            Setters.Add(new(lm.GetTerm("MASKSHARDS"), 20 - 4 * gs.CursedSettings.CursedMasks));
            Setters.Add(new(lm.GetTerm("NOTCHES"), 3 - gs.CursedSettings.CursedNotches));

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
        public List<Term> FullStateTerms = new();

        public string Name => "Progression Initializer";

        public void AddTo(ProgressionManager pm)
        {
            foreach (TermValue tv in Setters) pm.Set(tv);
            foreach (TermValue tv in Increments) pm.Incr(tv);
            foreach (Term t in FullStateTerms) pm.SetState(t, pm.lm.StateManager.AbsorbingSet);
        }

        public IEnumerable<Term> GetAffectedTerms()
        {
            foreach (TermValue tv in Setters) yield return tv.Term;
            foreach (TermValue tv in Increments) yield return tv.Term;
            foreach (Term t in FullStateTerms) yield return t;
        }
    }
}
