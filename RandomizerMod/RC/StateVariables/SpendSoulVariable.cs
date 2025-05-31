using RandomizerCore.Logic.StateLogic;
using RandomizerCore.Logic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $SPENDSOUL
     * Required Parameters:
         - The first parameter must parse to int to give the spend amount.
     * Optional Parameters: none
    */
    public class SpendSoulVariable : StateModifier
    {
        public override string Name { get; }
        public const string Prefix = "$SPENDSOUL";

        protected readonly int Amount;
        protected readonly ISoulStateManager SSM;

        public SpendSoulVariable(string name, LogicManager lm, int amount)
        {
            Name = name;
            this.Amount = amount;
            try
            {
                SSM = (ISoulStateManager)lm.GetVariableStrict(SoulStateManager.Prefix);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error constructing SpendSoulVariable", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                if (parameters.Length < 1 || !int.TryParse(parameters[0], out int amount))
                {
                    throw new ArgumentException($"{term} is missing amount argument for SpendSoulVariable.");
                }

                variable = new SpendSoulVariable(term, lm, amount);
                return true;
            }
            variable = default;
            return false;
        }

        public override IEnumerable<Term> GetTerms()
        {
            return SSM.GetTerms(ISoulStateManager.SSMOperation.SpendSoul);
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            return SSM.SpendSoul(pm, state, Amount);
        }
    }
}
