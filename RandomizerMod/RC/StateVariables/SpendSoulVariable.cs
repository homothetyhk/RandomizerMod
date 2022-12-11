using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    [Obsolete("Not fully implemented.")] // TODO: decide whether to keep this?
    public class SpendSoulVariable : StateModifier
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

        protected SpendSoulVariable(string name)
        {
            Name = name;
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return vesselFragments;
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            if (spendAll)
            {
                SpendAllSoul(pm, ref state, canDreamgate);
                yield return state;
            }
            else
            {
                if (TrySpendSoul(spendAmount, pm, ref state, canDreamgate)) yield return state;
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
