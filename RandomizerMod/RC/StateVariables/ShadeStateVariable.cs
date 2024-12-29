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
        protected readonly StateBool UsedShadeBool;
        protected readonly StateBool CannotShadeSkip;
        protected readonly StateBool NoFlower;
        protected readonly FragileCharmVariable FragileHeartEquip;
        protected readonly EquipCharmVariable VoidHeartEquip;
        protected readonly EquipCharmVariable JoniEquip;
        protected readonly ISoulStateManager SSM;
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
                UsedShadeBool = lm.StateManager.GetBoolStrict("USEDSHADE");
                CannotShadeSkip = lm.StateManager.GetBoolStrict("CANNOTSHADESKIP");
                NoFlower = lm.StateManager.GetBoolStrict("NOFLOWER");

                FragileHeartEquip = (FragileCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Fragile_Heart"));
                VoidHeartEquip = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Kingsoul")); // we have to check against either Kingsoul or Void Heart equipped to ensure monotonicity
                JoniEquip = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Joni's_Blessing"));
                SSM = (ISoulStateManager)lm.GetVariableStrict(SoulStateManager.Prefix);
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
            if (DoShadeSkip(pm, ref state))
            {
                yield return state;
            }
        }

        // rem: this reports success, but does not attempt to avoid mutation in the case of failure
        private bool DoShadeSkip(ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (!pm.Has(Shadeskips))
            {
                return false;
            }

            if (VoidHeartEquip.IsEquipped(state) || state.GetBool(CannotShadeSkip) || state.GetBool(UsedShadeBool))
            {
                return false;
            }
            VoidHeartEquip.SetUnequippable(ref state);
            state.SetBool(UsedShadeBool, true);

            if (!SSM.TrySetSoulLimit(pm, ref state, limiter: 33, appliesToPriorPath: true) || !SSM.TrySetSoulLimit(pm, ref state, 0, false))
            {
                /*
                 * Setting up a shade imposes a soul cap on the path up to the shade. After killing the shade, the soul cap is removed.
                 * Thus, if the pre skip state had more than 66 soul in its meter, the post skip state will have soul reduced accordingly.
                 * Additionally, if the state path previously spent soul, the shade skip may fail if those soul expenditures were not possible with the soul cap.
                 */
                return false;
            }

            if (!CheckHealthRequirement(pm, ref state))
            {
                // ref, in order to manage Joni and FHeart as needed
                return false;
            }
            
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

                // this probably doesn't need to run through HPSM since analyzing max hp is less complicated.
                int hp = (pm.Get(MaskShards) / 4) / 2;
                if (hp >= RequiredShadeHealth || RequiredShadeHealth == hp + 1 && FragileHeartEquip.CanEquip(pm, state) != EquipCharmVariable.EquipResult.None)
                {
                    FragileHeartEquip.BreakCharm(pm, ref state);
                    return true;
                }

                return false;
            }
        }
    }
}
