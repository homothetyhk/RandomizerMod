using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    public class StagStateVariable : StateModifyingVariable
    {
        public override string Name { get; }
        public StateBool NoFlower;
        public const string Prefix = "$STAGSTATEMODIFIER";

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (term == Prefix)
            {
                variable = new StagStateVariable(term)
                {
                    NoFlower = lm.StateManager.GetBool("NOFLOWER"),
                };
                return true;
            }
            variable = default;
            return false;
        }

        public StagStateVariable(string name)
        {
            Name = name;
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
