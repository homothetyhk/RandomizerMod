using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    public class SpendSoulVariable : StateModifyingVariable
    {
        public override string Name { get; }
        public StateInt spentSoul;
        public StateInt spentReserveSoul;
        public StateBool usedShade;
        public Term vesselFragments;
        public Term dreamnail;
        public Term essence;
        public bool spendAll;
        public int spendAmount;
        public bool canDreamgate;

        public SpendSoulVariable(string name)
        {
            Name = name;
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return vesselFragments;
        }

        public override int GetValue(object sender, ProgressionManager pm, StateUnion? localState)
        {
            if (spendAll) return TRUE;
            else return CanSpendSoul(spendAmount, pm, localState, canDreamgate) ? TRUE : FALSE;
        }

        public override bool ModifyState(object sender, ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (spendAll)
            {
                SpendAllSoul(pm, ref state, canDreamgate);
                return true;
            }
            else
            {
                return TrySpendSoul(spendAmount, pm, ref state, canDreamgate);
            }
        }

        public bool CanSpendSoul(int amount, ProgressionManager pm, StateUnion? state, bool canDreamgate)
        {
            if (canDreamgate && pm.Has(dreamnail, 2) && pm.Has(essence)) return true;
            if (state is null) return false;

            for (int i = 0; i < state.Count; i++)
            {
                int soul = (state[i].GetBool(usedShade) ? 66 : 99) - state[i].GetInt(spentSoul);
                if (soul >= amount) return true;
            }
            return false;
        }

        public bool TrySpendSoul(int amount, ProgressionManager pm, ref LazyStateBuilder state, bool canDreamgate)
        {
            if (canDreamgate && pm.Has(dreamnail, 2) && pm.Has(essence)) return true;

            int soul = (state.GetBool(usedShade) ? 66 : 99) - state.GetInt(spentSoul);
            if (soul < amount) return false;
            int reserveSoul = pm.Get(vesselFragments) / 3 * 33 - state.GetInt(spentReserveSoul);
            if (reserveSoul >= amount)
            {
                state.Increment(spentReserveSoul, amount);
                return true;
            }
            else if (reserveSoul > 0)
            {
                state.Increment(spentReserveSoul, reserveSoul);
                amount -= reserveSoul;
            }
            state.Increment(spentSoul, amount);
            return true;
        }

        public void SpendAllSoul(ProgressionManager pm, ref LazyStateBuilder state, bool canDreamgate)
        {
            if (canDreamgate && pm.Has(dreamnail, 2) && pm.Has(essence)) return;

            int soul = (state.GetBool(usedShade) ? 66 : 99) - state.GetInt(spentSoul);
            int reserveSoul = pm.Get(vesselFragments) / 3 * 33 - state.GetInt(spentReserveSoul);
            if (reserveSoul > 0) state.Increment(spentReserveSoul, reserveSoul);
            if (soul > 0) state.Increment(spentSoul, soul);
        }
    }
}
