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
    public class StartRespawnResetVariable : StateModifier
    {
        public override string Name { get; }
        public const string Prefix = "$STARTRESPAWN";

        protected readonly ISoulStateManager SSM;

        public StartRespawnResetVariable(string term, LogicManager lm)
        {
            Name = term;
            try
            {
                SSM = (ISoulStateManager)lm.GetVariableStrict(SoulStateManager.Prefix);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing StartRespawnResetVariable", e);
            }
        }

        public override IEnumerable<Term> GetTerms()
        {
            return SSM.GetTerms(ISoulStateManager.SSMOperation.RestoreSoul);
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

        public override IEnumerable<LazyStateBuilder>? ProvideState(object? sender, ProgressionManager pm)
        {
            return [];
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            return SSM.RestoreAllSoul(pm, state, true);
        }
    }
}
