using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $FLOWERGET
     * Required Parameters: none
     * Optiional Parameters: none
    */
    public class FlowerProviderVariable : StateModifyingVariable
    {
        public override string Name { get; }
        public StateBool NoFlower;
        public const string Prefix = "$FLOWERGET";

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (term == Prefix)
            {
                variable = new FlowerProviderVariable(term, lm);
                return true;
            }
            variable = default;
            return false;
        }

        public FlowerProviderVariable(string name, LogicManager lm)
        {
            Name = name;
            try
            {
                NoFlower = lm.StateManager.GetBoolStrict("NOFLOWER");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing FlowerProviderVariable", e);
            }
        }

        protected FlowerProviderVariable(string name)
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
            state.SetBool(NoFlower, false);
            return true;
        }
    }
}
