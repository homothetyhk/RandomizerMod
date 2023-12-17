using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.LogicInts
{
    /// <summary>
    /// LogicInt which is true exactly when the GenerationSettings StartLocation equals its argument.
    /// </summary>
    public class StartLocationDelta : StateProvider
    {
        protected readonly Term? StartStateTerm;  // null in files generated before state logic
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
            return ((RandoModContext)pm.ctx).GenerationSettings.StartLocationSettings.StartLocation == Location
                ? StartStateTerm is not null
                ? pm.GetState(StartStateTerm)
                : StateUnion.Empty 
                : null;
        }

        public override IEnumerable<Term> GetTerms()
        {
            if (StartStateTerm is not null) yield return StartStateTerm;
        }
    }
}
