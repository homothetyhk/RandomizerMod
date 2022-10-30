using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    public class CastSpellVariable : StateSplittingVariable
    {
        public override string Name { get; }
        public int spellCasts;
        public bool canDreamgate;
        public StateInt spentSoul;
        public StateInt spentReserveSoul;
        public StateInt usedNotches;
        public StateBool usedShade;
        public StateBool cannotShadeSkip;
        public Term vesselFragments;
        public Term dreamnail;
        public Term essence;
        public EquipCharmVariable equipSpellTwister;
        public const string Prefix = "$CASTSPELL";

        public CastSpellVariable(string name)
        {
            Name = name;
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                int spellCasts = 1;
                bool canDreamgate = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    int.TryParse(parameters[i], out spellCasts);
                    if (parameters[i] == "noDG") canDreamgate = false;
                }

                variable = new CastSpellVariable(term)
                {
                    spellCasts = spellCasts,
                    canDreamgate = canDreamgate,
                    spentSoul = lm.StateManager.GetInt("SPENTSOUL"),
                    spentReserveSoul = lm.StateManager.GetInt("SPENTRESERVESOUL"),
                    usedNotches = lm.StateManager.GetInt("USEDNOTCHES"),
                    usedShade = lm.StateManager.GetBool("USEDSHADE"),
                    cannotShadeSkip = lm.StateManager.GetBool("CANNOTSHADESKIP"),
                    vesselFragments = lm.GetTerm("VESSELFRAGMENTS"),
                    dreamnail = lm.GetTerm("DREAMNAIL"),
                    essence = lm.GetTerm("ESSENCE"),
                    equipSpellTwister = (EquipCharmVariable)lm.GetVariable(EquipCharmVariable.GetName("Spell_Twister")),
                };
                return true;
            }
            variable = default;
            return false;
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return vesselFragments;
            yield return dreamnail;
            yield return essence;
            foreach (Term t in equipSpellTwister.GetTerms()) yield return t;
        }

        public override int GetValue(object sender, ProgressionManager pm, StateUnion? localState)
        {
            if (canDreamgate && pm.Has(dreamnail, 2) && pm.Has(essence)) return TRUE;
            if (localState is null) return FALSE;

            for (int i = 0; i < localState.Count; i++)
            {
                int soul = (localState[i].GetBool(usedShade) ? 66 : 99) - (int)localState[i].GetInt(spentSoul);
                if (soul >= spellCasts * 33) return TRUE;
                else if (soul >= spellCasts * 25)
                {
                    if (equipSpellTwister.GetValue(sender, pm, localState) != FALSE) return TRUE;
                }
            }
            return FALSE;
        }

        public override IEnumerable<LazyStateBuilder>? ModifyState(object sender, ProgressionManager pm, LazyStateBuilder state)
        {
            if (canDreamgate && pm.Has(dreamnail, 2) && pm.Has(essence))
            {
                yield return state;
                yield break;
            }

            int soul = (state.GetBool(usedShade) ? 66 : 99) - (int)state.GetInt(spentSoul);
            int soulReserve = (pm.Get(vesselFragments) / 3) * 33 - (int)state.GetInt(spentReserveSoul);
            if (soul >= spellCasts * 33) // only need to check soul, since soul reserve is depleted before soul
            {
                LazyStateBuilder state33 = new(state);
                SpendSoul(spellCasts * 33, soulReserve, ref state33);
                yield return state33;
            }
            if (soul >= spellCasts * 25)
            {
                if (equipSpellTwister.ModifyState(sender, pm, ref state))
                {
                    SpendSoul(spellCasts * 25, soulReserve, ref state);
                    yield return state;
                }
            }
        }

        public void SpendSoul(int amount, int currentReserve, ref LazyStateBuilder state)
        {
            if (currentReserve >= amount)
            {
                state.Increment(spentReserveSoul, amount);
            }
            else if (currentReserve > 0)
            {
                state.Increment(spentReserveSoul, currentReserve);
                state.Increment(spentSoul, amount - currentReserve);
            }
            else
            {
                state.Increment(spentSoul, amount);
            }
            if (amount > 66 || state.GetInt(spentSoul) > 66) state.SetBool(cannotShadeSkip, true); // even with reserves, we need the full meter to cast all at once 
            // TODO: spentSoulSinceRefill to replace the spentSoul check here, to give the right behavior with (remove all soul after benchwarp) => (regain some soul) => (spend some soul)
        }
    }
}
