using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $STAGSTATEMODIFIER
     * Required Parameters: none
     * Optiional Parameters: none
    */
    public class StagStateVariable : StateModifyingVariable
    {
        public override string Name { get; }
        public StateBool NoFlower;
        public const string Prefix = "$STAGSTATEMODIFIER";

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (term == Prefix)
            {
                variable = new StagStateVariable(term, lm);
                return true;
            }
            variable = default;
            return false;
        }

        protected StagStateVariable(string name)
        {
            Name = name;
        }

        public StagStateVariable(string name, LogicManager lm)
        {
            Name = name;
            try
            {
                NoFlower = lm.StateManager.GetBoolStrict("NOFLOWER");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing StagStateVariable", e);
            }
        }

        public override IEnumerable<Term> GetTerms()
        {
            return Enumerable.Empty<Term>();
        }

        public override int GetValue(object sender, ProgressionManager pm, StateUnion? localState)
        {
            return TRUE;
        }

        public override bool ModifyState(object sender, ProgressionManager pm, ref LazyStateBuilder state)
        {
            state.SetBool(NoFlower, true);
            return true;
        }
    }
}
