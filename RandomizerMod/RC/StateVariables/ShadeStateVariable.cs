using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $SHADESKIP
     * Required Parameters: none
     * Optional Parameters:
     *   - a parameter ending in "HITS": the head of the parameter must parse to int, and is the required shade hp. Defaults to 1.
    */
    public class ShadeStateVariable : StateModifier
    {
        public override string Name { get; }
        protected readonly Term Shadeskips;
        protected readonly Term MaskShards;
        protected readonly Term VesselFragments;
        protected readonly StateBool UsedShadeBool;
        protected readonly StateBool CannotShadeSkip;
        protected readonly StateBool NoFlower;
        protected readonly StateInt RequiredMaxSoul;
        protected readonly StateInt SpentSoul;
        protected readonly StateInt SpentReserveSoul;
        protected readonly StateInt SoulLimiter;
        protected readonly FragileCharmVariable FragileHeartEquip;
        protected readonly EquipCharmVariable VoidHeartEquip;
        protected readonly EquipCharmVariable JoniEquip;
        protected readonly int RequiredShadeHealth;
        public const string Prefix = "$SHADESKIP";
        
        public ShadeStateVariable(string name, LogicManager lm, int requiredShadeHealth)
        {
            Name = name;
            RequiredShadeHealth = requiredShadeHealth;
            try
            {
                Shadeskips = lm.GetTermStrict("SHADESKIPS");
                MaskShards = lm.GetTermStrict("MASKSHARDS");
                VesselFragments = lm.GetTermStrict("VESSELFRAGMENTS");
                UsedShadeBool = lm.StateManager.GetBoolStrict("USEDSHADE");
                CannotShadeSkip = lm.StateManager.GetBoolStrict("CANNOTSHADESKIP");
                NoFlower = lm.StateManager.GetBoolStrict("NOFLOWER");
                RequiredMaxSoul = lm.StateManager.GetIntStrict("REQUIREDMAXSOUL");
                SpentSoul = lm.StateManager.GetIntStrict("SPENTSOUL");
                SpentReserveSoul = lm.StateManager.GetIntStrict("SPENTRESERVESOUL");
                SoulLimiter = lm.StateManager.GetIntStrict("SOULLIMITER");

                FragileHeartEquip = (FragileCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Fragile_Heart"));
                VoidHeartEquip = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Kingsoul")); // we have to check against either Kingsoul or Void Heart equipped to ensure monotonicity
                JoniEquip = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Joni's_Blessing"));
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing ShadeStateVariable", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                int requiredShadeHealth = 1;
                for (int i = 0; i < parameters.Length; i++) if (parameters[i].EndsWith("HITS")) requiredShadeHealth = int.Parse(parameters[i].Substring(0, parameters[i].Length - 4));

                variable = new ShadeStateVariable(term, lm, requiredShadeHealth);
                return true;
            }
            variable = null;
            return false;
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return Shadeskips;
            if (RequiredShadeHealth > 1) yield return MaskShards;
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            if (TryDoShadeSkip(pm, ref state))
            {
                yield return state;
            }
        }

        public bool TryDoShadeSkip(ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (!pm.Has(Shadeskips))
            {
                return false;
            }

            if (VoidHeartEquip.IsEquipped(state))
            {
                return false;
            }

            if (state.GetBool(CannotShadeSkip) || !CheckSoulRequirement(state) || !CheckHealthRequirement(pm, ref state) || !state.TrySetBoolTrue(UsedShadeBool))
            {
                return false;
            }

            PostAdjustSoul(pm, ref state);
            VoidHeartEquip.SetUnequippable(ref state);

            if (!state.GetBool(NoFlower))
            {
                state.SetBool(NoFlower, true); // don't require flower shade skips, also avoids thorny issues with reacquiring flower after setting up the shade.
            }

            return true;
        }

        public bool CheckHealthRequirement(ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (RequiredShadeHealth == 1)
            {
                return true;
            }
            else
            {
                if (JoniEquip.IsEquipped(state)) return false;
                JoniEquip.SetUnequippable(ref state);

                int hp = (pm.Get(MaskShards) / 4) / 2;
                if (hp >= RequiredShadeHealth || RequiredShadeHealth == hp + 1 && FragileHeartEquip.CanEquip(pm, state) != EquipCharmVariable.EquipResult.None)
                {
                    FragileHeartEquip.BreakCharm(pm, ref state);
                    return true;
                }

                return false;
            }
        }

        public bool CheckSoulRequirement<T>(T state) where T : IState
        {
            return state.GetInt(RequiredMaxSoul) <= 66;
        }

        protected virtual void PostAdjustSoul(ProgressionManager pm, ref LazyStateBuilder state)
        {
            int spentSoul = state.GetInt(SpentSoul);
            if (spentSoul >= 33) return;

            int debit = 33 - spentSoul;
            int currentReserve = (pm.Get(VesselFragments) / 3) * 33 - state.GetInt(SpentReserveSoul);

            if (currentReserve >= debit)
            {
                state.Increment(SpentReserveSoul, debit);
            }
            else if (currentReserve > 0)
            {
                state.Increment(SpentReserveSoul, currentReserve);
                state.Increment(SpentSoul, debit - currentReserve);
            }
            else
            {
                state.Increment(SpentSoul, debit);
            }
        }
    }
}
