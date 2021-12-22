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
        public ProgressionInitializer() { }
        public ProgressionInitializer(LogicManager lm, GenerationSettings gs)
        {
            foreach (string setting in Data.GetApplicableLogicSettings(gs))
            {
                Setters.Add(new(lm.GetTerm(setting), 1));
            }

            switch (gs.TransitionSettings.Mode)
            {
                case TransitionSettings.TransitionMode.None:
                    Setters.Add(new(lm.GetTerm("ITEMRANDO"), 1));
                    break;
                case TransitionSettings.TransitionMode.AreaRandomizer:
                    Setters.Add(new(lm.GetTerm("AREARANDO"), 1));
                    break;
                case TransitionSettings.TransitionMode.RoomRandomizer:
                    Setters.Add(new(lm.GetTerm("ROOMRANDO"), 1));
                    break;
            }

            StartDef start = Data.GetStartDef(gs.StartLocationSettings.StartLocation);
            Setters.Add(new(lm.GetTerm(start.Transition), 1));

            // use these baseline numbers for cursed settings and add shards/notches as vanilla items at start if necessary
            Setters.Add(new(lm.GetTerm("MASKSHARDS"), 4));
            Setters.Add(new(lm.GetTerm("NOTCHES"), 1));
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
