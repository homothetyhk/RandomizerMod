using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.RC.StateVariables;

namespace RandomizerMod.RC.LogicInts
{
    /// <summary>
    /// LogicInt which is true exactly when the GenerationSettings StartLocation equals its argument.
    /// </summary>
    public class StartLocationDelta : StateProvider
    {
        protected readonly Term? StartStateTerm;
        public override string Name { get; }
        public string Location { get; }
        public const string Prefix = "$StartLocation";

        public StartLocationDelta(string name, LogicManager lm, string location)
        {
            Location = location;
            Name = name;
            StartStateTerm = lm.GetTerm("Start_State");
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                string location = parameters[0];
                variable = new StartLocationDelta(term, lm, location);
                return true;
            }
            variable = null;
            return false;
        }

        public override StateUnion? GetInputState(object? sender, ProgressionManager pm)
        {
            return pm.GetState(StartStateTerm);
        }

        public override IEnumerable<Term> GetTerms()
        {
            if (StartStateTerm is not null) yield return StartStateTerm;
        }

        public override int GetValue(object? sender, ProgressionManager pm)
        {
            return ((RandoModContext)pm.ctx).GenerationSettings.StartLocationSettings.StartLocation == Location ? TRUE : FALSE;
        }
    }
}
