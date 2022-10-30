using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    public class HotSpringResetVariable : StateModifyingVariable
    {
        public override string Name { get; }

        public StateInt spentReserveSoul;
        public StateInt spentSoul;
        public StateInt spentHP;
        public StateBool spentAllSoul;
        public StateBool cannotRegainSoul;
        public const string Prefix = "$HOTSPRINGRESET";

        public HotSpringResetVariable(string name)
        {
            Name = name;
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (term == Prefix)
            {
                variable = new HotSpringResetVariable(term)
                {
                    spentReserveSoul = lm.StateManager.GetInt("SPENTRESERVESOUL"),
                    spentSoul = lm.StateManager.GetInt("SPENTSOUL"),
                    spentHP = lm.StateManager.GetInt("SPENTHP"),
                    spentAllSoul = lm.StateManager.GetBool("SPENTALLSOUL"),
                    cannotRegainSoul = lm.StateManager.GetBool("CANNOTREGAINSOUL"),
                };
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
