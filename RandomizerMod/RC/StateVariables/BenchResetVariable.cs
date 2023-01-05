using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $BENCHRESET
     * Required Parameters: none
     * Optional Parameters: none
     * Provides the effect of resting at a bench. Does not consider warping, etc.
    */
    public class BenchResetVariable : StateResetter
    {
        public override string Name { get; }
        protected override State ResetState { get; }
        protected override string ResetLogicProperty => "BenchResetCondition";

        public const string Prefix = "$BENCHRESET";

        public BenchResetVariable(string name, LogicManager lm) : base(lm)
        {
            Name = name;
            try
            {
                ResetState = lm.StateManager.GetNamedStateStrict("BenchResetState");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing BenchResetVariable", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                variable = new BenchResetVariable(term, lm);
                return true;
            }
            variable = default;
            return false;
        }

        new public LazyStateBuilder ResetSingle(ProgressionManager pm, LazyStateBuilder state) => base.ResetSingle(pm, state);
    }
}
