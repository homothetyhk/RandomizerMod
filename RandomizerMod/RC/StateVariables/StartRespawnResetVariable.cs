using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $STARTRESPAWN
     * Required Parameters: none
     * Optional Parameters: none
     * Provides the effect of the start respawn. Typically applied in sequence after $SAVEQUITRESET as part of $WARPTOSTART.
    */
    public class StartRespawnResetVariable : StateResetter
    {
        public override string Name { get; }
        public const string Prefix = "$STARTRESPAWN";
        protected override State ResetState { get; }
        protected override string? ResetLogicProperty => "StartRespawnResetCondition";
        protected override bool OptIn => true;

        public StartRespawnResetVariable(string term, LogicManager lm) : base(lm)
        {
            Name = term;
            try
            {
                ResetState = lm.StateManager.GetNamedStateStrict("StartRespawnResetState");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing StartRespawnResetVariable", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out _))
            {
                variable = new StartRespawnResetVariable(term, lm);
                return true;
            }
            variable = default;
            return false;
        }

        new public LazyStateBuilder ResetSingle(ProgressionManager pm, LazyStateBuilder state) => base.ResetSingle(pm, state);
    }
}
