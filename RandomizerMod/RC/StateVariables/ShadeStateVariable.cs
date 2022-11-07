using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    public class ShadeStateVariable : StateModifyingVariable
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
        public FragileCharmVariable FragileHeartEquip;
        public int requiredShadeHealth;
        public const string Prefix = "$SHADESKIP";

        public ShadeStateVariable(string name)
        {
            Name = name;
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                bool canDreamgate = !parameters.Contains("noDG");
                int requiredShadeHealth = 1;
                for (int i = 0; i < parameters.Length; i++) if (parameters[i].EndsWith("HITS")) requiredShadeHealth = int.Parse(parameters[i].Substring(0, parameters[i].Length - 4));

                ShadeStateVariable ssv = new(term)
                {
                    canDreamgate = canDreamgate,
                    requiredShadeHealth = requiredShadeHealth,
                    ShadeskipTerm = lm.GetTerm("SHADESKIPS"),
                    DreamgateTerm = lm.GetTerm("DREAMNAIL"),
                    EssenceTerm = lm.GetTerm("ESSENCE"),
                    MaskShardsTerm = lm.GetTerm("MASKSHARDS"),
                    UsedShadeBool = lm.StateManager.GetBool("USEDSHADE"),
                    CannotShadeSkip = lm.StateManager.GetBool("CANNOTSHADESKIP"),
                    NoFlower = lm.StateManager.GetBool("NOFLOWER"),
                    FragileHeartEquip = (FragileCharmVariable)lm.GetVariable(EquipCharmVariable.GetName("Fragile_Heart")),
                };

                variable = ssv;
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
            if (requiredShadeHealth > 1) yield return MaskShardsTerm;
        }

        public override int GetValue(object sender, ProgressionManager pm, StateUnion? localState)
        {
            if (!pm.Has(ShadeskipTerm)) return FALSE;
            if (canDreamgate && pm.Has(DreamgateTerm, 2) && pm.Has(EssenceTerm)) return CheckHealthRequirementDG(pm) ? TRUE : FALSE;
            if (localState is null) return FALSE;
            for (int i = 0; i < localState.Count; i++)
            {
                if (!localState[i].GetBool(UsedShadeBool) && !localState[i].GetBool(CannotShadeSkip) && CheckHealthRequirement(pm, localState[i])) return TRUE;
            }
            return FALSE;
        }

        public override bool ModifyState(object sender, ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (!pm.Has(ShadeskipTerm)) return false;
            if (!state.GetBool(NoFlower)) state.SetBool(NoFlower, true); // don't require flower shade skips, also avoids thorny issues with reacquiring flower after setting up the shade.
            if (canDreamgate && pm.Has(DreamgateTerm, 2) && pm.Has(EssenceTerm) && CheckHealthRequirementDG(pm)) return true;
            if (state.GetBool(CannotShadeSkip)) return false;
            return CheckHealthRequirement(pm, ref state) && state.TrySetBoolTrue(UsedShadeBool);
        }

        public bool CheckHealthRequirement(ProgressionManager pm, State state)
        {
            if (requiredShadeHealth == 1)
            {
                return true;
            }
            else
            {
                int hp = (pm.Get(MaskShardsTerm) / 4) / 2;
                if (hp >= requiredShadeHealth || requiredShadeHealth == hp + 1 && FragileHeartEquip.CanEquip(pm, state) != EquipCharmVariable.EquipResult.None) return true;
                return false;
            }
        }

        public bool CheckHealthRequirement(ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (requiredShadeHealth == 1)
            {
                return true;
            }
            else
            {
                int hp = (pm.Get(MaskShardsTerm) / 4) / 2;
                if (hp >= requiredShadeHealth || requiredShadeHealth == hp + 1 && FragileHeartEquip.CanEquip(pm, state) != EquipCharmVariable.EquipResult.None)
                {
                    FragileHeartEquip.BreakCharm(pm, ref state);
                    return true;
                }

                return false;
            }
        }

        public bool CheckHealthRequirementDG(ProgressionManager pm)
        {
            if (requiredShadeHealth == 1)
            {
                return true;
            }
            else
            {
                int hp = (pm.Get(MaskShardsTerm) / 4) / 2;
                if (hp >= requiredShadeHealth) return true;
                else if (requiredShadeHealth == hp + 1 && FragileHeartEquip.HasCharmProgression(pm)) return true;
                return false;
            }
        }

        // TODO: broke fragile charm state fields, so that BENCHRESET, etc know not to revert anticharm bool
        
        public bool CheckHealthRequirementDG(ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (requiredShadeHealth == 1)
            {
                return true;
            }
            else
            {
                int hp = (pm.Get(MaskShardsTerm) / 4) / 2;
                if (hp >= requiredShadeHealth) return true;
                else if (requiredShadeHealth == hp + 1 && FragileHeartEquip.HasCharmProgression(pm))
                {
                    FragileHeartEquip.BreakCharm(pm, ref state);
                    return true;
                }

                return false;
            }
        }
    }
}
