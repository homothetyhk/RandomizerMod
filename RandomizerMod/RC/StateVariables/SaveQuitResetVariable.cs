using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $SAVEQUITRESET
     * Required Parameters: none
     * Optional Parameters: none
     * Provides the effect of warping via Benchwarp or savequit, regardless of destination type.
    */
    public class SaveQuitResetVariable : StateResetter
    {
        public override string Name { get; }
        public const string Prefix = "$SAVEQUITRESET";
        protected override State ResetState { get; }

        protected override string? ResetLogicProperty => "SaveQuitConditionalReset";
        protected override bool OptIn => true;

        public SaveQuitResetVariable(string term, LogicManager lm) : base(lm)
        {
            Name = term;
            try
            {
                ResetState = lm.StateManager.GetNamedStateStrict("SaveQuitResetState");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing SaveQuitResetVariable", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out _))
            {
                variable = new SaveQuitResetVariable(term, lm);
                return true;
            }
            variable = default;
            return false;
        }

        new public LazyStateBuilder ResetSingle(ProgressionManager pm, LazyStateBuilder state) => base.ResetSingle(pm, state);
    }
}
