using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $HOTSPRINGRESET
     * Required Parameters: none
     * Optiional Parameters: none
    */
    public class HotSpringResetVariable : StateModifier
    {
        public override string Name { get; }
        protected readonly ISoulStateManager SSM;
        protected readonly IHPStateManager HPSM;
        public const string Prefix = "$HOTSPRINGRESET";

        public HotSpringResetVariable(string name, LogicManager lm)
        {
            Name = name;
            try
            {
                SSM = (ISoulStateManager)lm.GetVariableStrict(SoulStateManager.Prefix);
                HPSM = (IHPStateManager)lm.GetVariableStrict(HPStateManager.Prefix);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing HotSpringResetVariable", e);
            }
        }

        public override IEnumerable<Term> GetTerms()
        {
            foreach (Term t in SSM.GetTerms(ISoulStateManager.SSMOperation.RestoreSoul)) yield return t;
            foreach (Term t in HPSM.GetTerms(IHPStateManager.HPSMOperation.RestoreWhiteHealth)) yield return t;
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (term == Prefix)
            {
                variable = new HotSpringResetVariable(term, lm);
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
            SSM.TryRestoreAllSoul(pm, ref state, restoreReserves: true);
            return HPSM.RestoreWhiteHealth(pm, state);
        }
    }
}
