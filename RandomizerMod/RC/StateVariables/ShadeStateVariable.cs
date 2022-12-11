using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $SHADESKIP
     * Required Parameters: none
     * Optional Parameters:
     *   - a parameter equal to "noDG": indicates that dream gate is not possible after the skip
     *   - a parameter ending in "HITS": the head of the parameter must parse to int, and is the required shade hp. Defaults to 1.
    */
    public class ShadeStateVariable : StateModifier
    {
        public override string Name { get; }
        public Term ShadeskipTerm;
        public Term DreamgateTerm;
        public Term EssenceTerm;
        public Term MaskShardsTerm;
        public bool canDreamgate;
        public StateBool UsedShadeBool;
        public StateBool CannotShadeSkip;
        public StateBool NoFlower;
        public StateInt RequiredMaxSoul;
        public FragileCharmVariable FragileHeartEquip;
        public int RequiredShadeHealth;
        public const string Prefix = "$SHADESKIP";

        protected ShadeStateVariable(string name)
        {
            Name = name;
        }

        public ShadeStateVariable(string name, LogicManager lm, bool canDreamgate, int requiredShadeHealth)
        {
            Name = name;
            this.canDreamgate = canDreamgate;
            RequiredShadeHealth = requiredShadeHealth;
            try
            {
                ShadeskipTerm = lm.GetTermStrict("SHADESKIPS");
                DreamgateTerm = lm.GetTermStrict("DREAMNAIL");
                EssenceTerm = lm.GetTermStrict("ESSENCE");
                MaskShardsTerm = lm.GetTermStrict("MASKSHARDS");
                UsedShadeBool = lm.StateManager.GetBoolStrict("USEDSHADE");
                CannotShadeSkip = lm.StateManager.GetBoolStrict("CANNOTSHADESKIP");
                NoFlower = lm.StateManager.GetBoolStrict("NOFLOWER");
                RequiredMaxSoul = lm.StateManager.GetIntStrict("REQUIREDMAXSOUL");
                FragileHeartEquip = (FragileCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Fragile_Heart"));
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
                bool canDreamgate = !parameters.Contains("noDG");
                int requiredShadeHealth = 1;
                for (int i = 0; i < parameters.Length; i++) if (parameters[i].EndsWith("HITS")) requiredShadeHealth = int.Parse(parameters[i].Substring(0, parameters[i].Length - 4));

                variable = new ShadeStateVariable(term, lm, canDreamgate, requiredShadeHealth);
                return true;
            }
            variable = null;
            return false;
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return ShadeskipTerm;
            yield return DreamgateTerm;
            yield return EssenceTerm;
            if (RequiredShadeHealth > 1) yield return MaskShardsTerm;
        }


        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            if (!pm.Has(ShadeskipTerm))
            {
                yield break;
            }

            if (!state.GetBool(NoFlower))
            {
                state.SetBool(NoFlower, true); // don't require flower shade skips, also avoids thorny issues with reacquiring flower after setting up the shade.
            }

            if (canDreamgate && pm.Has(DreamgateTerm, 2) && pm.Has(EssenceTerm) && CheckHealthRequirementDG(pm))
            {
                yield return state;
                yield break;
            }

            if (state.GetBool(CannotShadeSkip) || !CheckSoulRequirement(state) || !CheckHealthRequirement(pm, ref state) || !state.TrySetBoolTrue(UsedShadeBool))
            {
                yield break;
            }

            yield return state;
        }

        public bool CheckHealthRequirement(ProgressionManager pm, State state)
        {
            if (RequiredShadeHealth == 1)
            {
                return true;
            }
            else
            {
                int hp = (pm.Get(MaskShardsTerm) / 4) / 2;
                if (hp >= RequiredShadeHealth || RequiredShadeHealth == hp + 1 && FragileHeartEquip.CanEquip(pm, state) != EquipCharmVariable.EquipResult.None) return true;
                return false;
            }
        }

        public bool CheckHealthRequirement(ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (RequiredShadeHealth == 1)
            {
                return true;
            }
            else
            {
                int hp = (pm.Get(MaskShardsTerm) / 4) / 2;
                if (hp >= RequiredShadeHealth || RequiredShadeHealth == hp + 1 && FragileHeartEquip.CanEquip(pm, state) != EquipCharmVariable.EquipResult.None)
                {
                    FragileHeartEquip.BreakCharm(pm, ref state);
                    return true;
                }

                return false;
            }
        }

        public bool CheckHealthRequirementDG(ProgressionManager pm)
        {
            if (RequiredShadeHealth == 1)
            {
                return true;
            }
            else
            {
                int hp = (pm.Get(MaskShardsTerm) / 4) / 2;
                if (hp >= RequiredShadeHealth) return true;
                else if (RequiredShadeHealth == hp + 1 && FragileHeartEquip.HasCharmProgression(pm)) return true;
                return false;
            }
        }
        
        public bool CheckHealthRequirementDG(ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (RequiredShadeHealth == 1)
            {
                return true;
            }
            else
            {
                int hp = (pm.Get(MaskShardsTerm) / 4) / 2;
                if (hp >= RequiredShadeHealth) return true;
                else if (RequiredShadeHealth == hp + 1 && FragileHeartEquip.HasCharmProgression(pm))
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
    }
}
