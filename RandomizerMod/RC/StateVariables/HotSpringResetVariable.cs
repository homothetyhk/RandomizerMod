using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $HOTSPRINGRESET
     * Required Parameters: none
     * Optiional Parameters: none
    */
    public class HotSpringResetVariable : StateModifyingVariable
    {
        public override string Name { get; }

        public StateInt spentReserveSoul;
        public StateInt spentSoul;
        public StateInt spentHP;
        public StateBool spentAllSoul;
        public StateBool cannotRegainSoul;
        public const string Prefix = "$HOTSPRINGRESET";

        protected HotSpringResetVariable(string name)
        {
            Name = name;
        }

        public HotSpringResetVariable(string name, LogicManager lm)
        {
            Name = name;
            try
            {
                spentReserveSoul = lm.StateManager.GetIntStrict("SPENTRESERVESOUL");
                spentSoul = lm.StateManager.GetIntStrict("SPENTSOUL");
                spentHP = lm.StateManager.GetIntStrict("SPENTHP");
                spentAllSoul = lm.StateManager.GetBoolStrict("SPENTALLSOUL");
                cannotRegainSoul = lm.StateManager.GetBoolStrict("CANNOTREGAINSOUL");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing HotSpringResetVariable", e);
            }
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

        public override IEnumerable<Term> GetTerms()
        {
            return Enumerable.Empty<Term>();
        }

        public override int GetValue(object sender, ProgressionManager pm, StateUnion? localState)
        {
            return TRUE;
        }

        public override bool ModifyState(object sender, ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (!state.GetBool(cannotRegainSoul))
            {
                state.SetBool(spentAllSoul, false);
                state.SetInt(spentSoul, 0);
                state.SetInt(spentReserveSoul, 0);
            }
            state.SetInt(spentHP, 0);
            return true;
        }
    }
}
