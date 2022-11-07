using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    public class RequireFlowerVariable : StateVariable
    {
        public override string Name { get; }
        public StateBool NoFlower;
        public const string Prefix = "$FLOWER";

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (term == Prefix)
            {
                variable = new RequireFlowerVariable(term)
                {
                    NoFlower = lm.StateManager.GetBool("NOFLOWER"),
                };
                return true;
            }
            variable = default;
            return false;
        }

        public RequireFlowerVariable(string name)
        {
            Name = name;
        }

        public override IEnumerable<Term> GetTerms()
        {
            return Enumerable.Empty<Term>();
        }

        public override int GetValue(object sender, ProgressionManager pm, StateUnion? localState)
        {
            if (localState is null) return FALSE;
            for (int i = 0; i < localState.Count; i++)
            {
                if (!localState[i].GetBool(NoFlower)) return TRUE;
            }
            return FALSE;
        }
    }
}
