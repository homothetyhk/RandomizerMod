using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $FLOWERGET
     * Required Parameters: none
     * Optiional Parameters: none
    */
    public class FlowerProviderVariable : StateModifier
    {
        public override string Name { get; }
        protected readonly StateBool NoFlower;
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

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            state.SetBool(NoFlower, false);
            yield return state;
        }
    }
}
