using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $REGAINSOUL
     * Required Parameters:
         
     * Optional Parameters:
         - The first parameter, if given, must parse to int to give the regain amount. Otherwise, fully restores soul.
    */
    public class RegainSoulVariable : StateModifier
    {
        public override string Name { get; }
        public const string Prefix = "$REGAINSOUL";

        protected readonly int? Amount;
        protected readonly ISoulStateManager SSM;

        public RegainSoulVariable(string name, LogicManager lm, int? amount)
        {
            Name = name;
            this.Amount = amount;
            try
            {
                SSM = (ISoulStateManager)lm.GetVariableStrict(SoulStateManager.Prefix);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error constructing RegainSoulVariable", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                int amount;
                if (parameters.Length == 0)
                {
                    amount = -1;
                }
                else if (parameters.Length == 1 && int.TryParse(parameters[0], out amount)) { }
                else
                {
                    throw new ArgumentException($"{term} is missing amount argument for RegainSoulVariable.");
                }

                variable = new RegainSoulVariable(term, lm, amount >= 0 ? amount : null);
                return true;
            }
            variable = default;
            return false;
        }

        public override IEnumerable<Term> GetTerms()
        {
            return SSM.GetTerms();
        }

        public override IEnumerable<LazyStateBuilder>? ProvideState(object? sender, ProgressionManager pm)
        {
            return [];
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            return Amount.HasValue ? SSM.RestoreSoul(pm, state, Amount.Value) : SSM.RestoreAllSoul(pm, state, restoreReserves: true);
        }
    }
}
