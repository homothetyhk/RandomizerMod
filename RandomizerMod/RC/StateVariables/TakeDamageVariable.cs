using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $TAKEDAMAGE
     * Required Parameters:
         - If any parameters are provided, the first parameter must parse to int to give the damage amount. If absent, defaults to 1.
     * Optional Parameters: none
     * Implements taking damage from a single hit. Assumes enough time to focus/hiveblood before and after the hit.
    */
    public class TakeDamageVariable : StateModifier
    {
        public override string Name { get; }
        public const string Prefix = "$TAKEDAMAGE";
        protected readonly int Amount;
        protected readonly IHPStateManager HPSM;

        public TakeDamageVariable(string name, LogicManager lm, int amount)
        {
            Name = name;
            Amount = amount;
            try
            {
                HPSM = (IHPStateManager)lm.GetVariableStrict(HPStateManager.Prefix);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing TakeDamageVariable", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                int amount = parameters.Length == 0 ? 1 : int.Parse(parameters[0]);
                variable = new TakeDamageVariable(term, lm, amount);
                return true;
            }
            variable = default;
            return false;
        }

        public override IEnumerable<Term> GetTerms()
        {
            return HPSM.GetTerms(IHPStateManager.HPSMOperation.TakeDamage);
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            return HPSM.TakeDamage(pm, state, Amount);
        }
    }
}
