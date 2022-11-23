using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $FLOWER
     * Required Parameters: none
     * Optiional Parameters: none
    */
    public class RequireFlowerVariable : StateVariable
    {
        public override string Name { get; }
        public StateBool NoFlower;
        public const string Prefix = "$FLOWER";

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (term == Prefix)
            {
                variable = new RequireFlowerVariable(term, lm);
                return true;
            }
            variable = default;
            return false;
        }

        protected RequireFlowerVariable(string name)
        {
            Name = name;
        }

        public RequireFlowerVariable(string name, LogicManager lm)
        {
            Name = name;
            try
            {
                NoFlower = lm.StateManager.GetBoolStrict("NOFLOWER");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing RequireFlowerVariable", e);
            }
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
