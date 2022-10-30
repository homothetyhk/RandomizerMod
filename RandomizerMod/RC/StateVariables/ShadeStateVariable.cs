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
        public bool canDreamgate;
        public StateBool UsedShadeBool;
        public StateBool CannotShadeSkip;
        public const string Prefix = "$SHADESKIP";

        public ShadeStateVariable(string name)
        {
            Name = name;
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                ShadeStateVariable ssv = new(term)
                {
                    canDreamgate = !parameters.Contains("noDG"),
                    ShadeskipTerm = lm.GetTerm("SHADESKIPS"),
                    DreamgateTerm = lm.GetTerm("DREAMNAIL"),
                    EssenceTerm = lm.GetTerm("ESSENCE"),
                    UsedShadeBool = lm.StateManager.GetBool("USEDSHADE"),
                    CannotShadeSkip = lm.StateManager.GetBool("CANNOTSHADESKIP"),
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
        }

        public override int GetValue(object sender, ProgressionManager pm, StateUnion? localState)
        {
            if (!pm.Has(ShadeskipTerm)) return FALSE;
            if (canDreamgate && pm.Has(DreamgateTerm, 2) && pm.Has(EssenceTerm)) return TRUE;
            if (localState is null) return FALSE;
            for (int i = 0; i < localState.Count; i++)
            {
                if (!localState[i].GetBool(UsedShadeBool) && !localState[i].GetBool(CannotShadeSkip)) return TRUE;
            }
            return FALSE;
        }

        public override bool ModifyState(object sender, ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (!pm.Has(ShadeskipTerm)) return false;
            if (canDreamgate && pm.Has(DreamgateTerm, 2)) return true;
            if (state.GetBool(CannotShadeSkip)) return false;
            return state.TrySetBoolTrue(UsedShadeBool);
        }
    }
}
