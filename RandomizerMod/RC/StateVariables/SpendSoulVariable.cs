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
        protected readonly StateInt SpentSoul;
        protected readonly StateInt SpentReserveSoul;
        protected readonly StateInt SoulLimiter;
        protected readonly StateInt RequiredMaxSoul;
        protected readonly StateBool SpentAllSoul;
        protected readonly Term VesselFragments;

        public SpendSoulVariable(string name, LogicManager lm, int amount)
        {
            Name = name;
            this.Amount = amount;
            try
            {
                SpentSoul = lm.StateManager.GetIntStrict("SPENTSOUL");
                SpentReserveSoul = lm.StateManager.GetIntStrict("SPENTRESERVESOUL");
                SoulLimiter = lm.StateManager.GetIntStrict("SOULLIMITER");
                RequiredMaxSoul = lm.StateManager.GetIntStrict("REQUIREDMAXSOUL");
                SpentAllSoul = lm.StateManager.GetBoolStrict("SPENTALLSOUL");
                VesselFragments = lm.GetTermStrict("VESSELFRAGMENTS");
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
            yield return VesselFragments;
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            if (TryModifySingle(Amount, pm, ref state)) yield return state;
        }

        /// <summary>
        /// Checks whether the state can spend the given amount of soul. If true, modifies the state in place. If false, does not modify the state.
        /// </summary>
        public virtual bool TryModifySingle(int amount, ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (state.GetBool(SpentAllSoul))
            {
                return false;
            }

            int soul = GetSoul(pm, state);
            if (soul < amount) return false;
            
            int reserveSoul = GetReserveSoul(pm, state);
            if (reserveSoul >= amount)
            {
                state.Increment(SpentReserveSoul, amount);
            }
            else
            {
                state.Increment(SpentReserveSoul, reserveSoul);
                state.Increment(SpentSoul, amount - reserveSoul);
            }

            int requiredMaxSoul = state.GetInt(RequiredMaxSoul);
            int altReq = Math.Max(amount, state.GetInt(SpentSoul));
            if (altReq > requiredMaxSoul) state.SetInt(RequiredMaxSoul, altReq);

            return true;
        }

        /// <summary>
        /// Checks whether the state can spend the given amount of soul, for the given number of iterations, assuming enough time between expenditures for reserves. 
        /// <br/>If true, modifies the state in place. If false, does not modify the state.
        /// </summary>
        public virtual bool TryModifySingle(int amountPerIteration, int iterations, ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (state.GetBool(SpentAllSoul))
            {
                return false;
            }

            int soul = GetSoul(pm, state);
            if (soul < amountPerIteration) return false;

            int reserveSoul = GetReserveSoul(pm, state);
            int totalAmount = amountPerIteration * iterations;
            if (soul + reserveSoul < totalAmount) return false;

            if (reserveSoul >= totalAmount)
            {
                state.Increment(SpentReserveSoul, totalAmount);
            }
            else
            {
                state.Increment(SpentReserveSoul, reserveSoul);
                state.Increment(SpentSoul, totalAmount - reserveSoul);
            }

            int requiredMaxSoul = state.GetInt(RequiredMaxSoul);
            int altReq = Math.Max(amountPerIteration, state.GetInt(SpentSoul));
            if (altReq > requiredMaxSoul) state.SetInt(RequiredMaxSoul, altReq);

            return true;
        }


        protected virtual int GetSoul(ProgressionManager pm, LazyStateBuilder state)
        {
            return 99 - state.GetInt(SoulLimiter) - state.GetInt(SpentSoul);
        }

        protected virtual int GetReserveSoul(ProgressionManager pm, LazyStateBuilder state)
        {
            return (pm.Get(VesselFragments) / 3) * 33 - state.GetInt(SpentReserveSoul);
        }
    }
}
