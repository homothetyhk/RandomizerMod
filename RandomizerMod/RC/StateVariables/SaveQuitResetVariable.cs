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
    public class SaveQuitResetVariable : StateModifier
    {
        public override string Name { get; }
        public const string Prefix = "$SAVEQUITRESET";

        protected readonly StateBool NoFlower;
        protected readonly StateBool UsedShade;
        protected readonly StateInt MaxRequiredSoul;
        protected readonly ISoulStateManager SSM;
        protected readonly IHPStateManager HPSM;

        public SaveQuitResetVariable(string term, LogicManager lm)
        {
            Name = term;
            try
            {
                NoFlower = lm.StateManager.GetBoolStrict("NOFLOWER");
                UsedShade = lm.StateManager.GetBoolStrict("USEDSHADE");
                MaxRequiredSoul = lm.StateManager.GetIntStrict("MAXREQUIREDSOUL");

                SSM = (ISoulStateManager)lm.GetVariableStrict(SoulStateManager.Prefix);
                HPSM = (IHPStateManager)lm.GetVariableStrict(HPStateManager.Prefix);
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

        public override IEnumerable<Term> GetTerms()
        {
            foreach (Term t in SSM.GetTerms()) yield return t;
            foreach (Term t in HPSM.GetTerms()) yield return t;
        }

        public override IEnumerable<LazyStateBuilder>? ProvideState(object? sender, ProgressionManager pm)
        {
            return [];
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            state.SetBool(NoFlower, true); // not game accurate, but we do this to prevent warps from being required for flower quest.
            SSM.TrySpendAllSoul(pm, ref state); // zero out soul. A subsequent modifier will handle bench / start respawn soul effects.
            state.SetBool(UsedShade, false); // not necessary to reset shade variables for typical use, but in the case of warping to a non-start hard respawn, it would be correct to reset them here.
            state.SetInt(MaxRequiredSoul, 0);
            return HPSM.RestoreAllHealth(pm, state, restoreBlueHealth: false);
        }
    }
}
