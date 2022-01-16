using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomizerMod.RC
{
    public class ProgressionInitializer : ILogicItem
    {
        /// <summary>
        /// Event invoked after base randomizer term modifiers are added to the initializer.
        /// </summary>
        public static event Action<LogicManager, GenerationSettings, ProgressionInitializer> OnCreateProgressionInitializer;

        public ProgressionInitializer() { }
        public ProgressionInitializer(LogicManager lm, GenerationSettings gs)
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

            StartDef start = Data.GetStartDef(gs.StartLocationSettings.StartLocation);
            Setters.Add(new(lm.GetTerm(start.Transition), 1));

            Setters.Add(new(lm.GetTerm("GRUBS"), -gs.CostSettings.GrubTolerance));
            Setters.Add(new(lm.GetTerm("ESSENCE"), -gs.CostSettings.EssenceTolerance));
            Setters.Add(new(lm.GetTerm("RANCIDEGGS"), -gs.CostSettings.EggTolerance));
            Setters.Add(new(lm.GetTerm("CHARMS"), -gs.CostSettings.CharmTolerance));

            // use these baseline numbers for cursed settings and add shards/notches as vanilla items at start if necessary
            Setters.Add(new(lm.GetTerm("MASKSHARDS"), 4));
            Setters.Add(new(lm.GetTerm("NOTCHES"), 1));

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

        public string Name => "Progression Initializer";

        public void AddTo(ProgressionManager pm)
        {
            foreach (TermValue tv in Setters) pm.Set(tv);
            foreach (TermValue tv in Increments) pm.Incr(tv);
        }

        public IEnumerable<Term> GetAffectedTerms()
        {
            foreach (TermValue tv in Setters) yield return tv.Term;
            foreach (TermValue tv in Increments) yield return tv.Term;
        }
    }

    public class RandoModContext : RandoContext
    {
        public RandoModContext(GenerationSettings gs)
        {
            base.LM = RCData.GetNewLogicManager(gs);
            base.InitialProgression = new ProgressionInitializer(LM, gs);
        }

    }
}
