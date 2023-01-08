using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $REGAINSOUL
     * Required Parameters:
         - The first parameter must parse to int to give the regain amount.
     * Optional Parameters: none
    */
    public class RegainSoulVariable : StateModifier
    {
        public override string Name { get; }
        public const string Prefix = "$REGAINSOUL";

        protected readonly int Amount;
        protected readonly StateInt SpentSoul;
        protected readonly StateInt SpentReserveSoul;
        protected readonly StateInt SoulLimiter;
        protected readonly StateBool CannotRegainSoul;
        protected readonly StateBool SpentAllSoul;
        protected readonly Term VesselFragments;

        public RegainSoulVariable(string name, LogicManager lm, int amount)
        {
            Name = name;
            this.Amount = amount;
            try
            {
                SpentSoul = lm.StateManager.GetIntStrict("SPENTSOUL");
                SpentReserveSoul = lm.StateManager.GetIntStrict("SPENTRESERVESOUL");
                SoulLimiter = lm.StateManager.GetIntStrict("SOULLIMITER");
                CannotRegainSoul = lm.StateManager.GetBoolStrict("CANNOTREGAINSOUL");
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
                    throw new ArgumentException($"{term} is missing amount argument for RegainSoulVariable.");
                }

                variable = new RegainSoulVariable(term, lm, amount);
                return true;
            }
            variable = default;
            return false;
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return VesselFragments;
        }

        public override IEnumerable<LazyStateBuilder>? ProvideState(object? sender, ProgressionManager pm)
        {
            return Enumerable.Empty<LazyStateBuilder>();
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            ModifySingle(Amount, pm, ref state);
            yield return state;
        }

        public virtual void ModifySingle(int amount, ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (state.GetBool(CannotRegainSoul))
            {
                return;
            }
            if (state.GetBool(SpentAllSoul))
            {
                state.SetInt(SpentReserveSoul, GetMaxReserveSoul(pm, state));
                state.SetInt(SpentSoul, GetMaxSoul(pm, state));
                state.SetBool(SpentAllSoul, false);
            }
            int soulDiff = 99 - state.GetInt(SoulLimiter) - state.GetInt(SpentSoul);

            if (amount < soulDiff)
            {
                state.Increment(SpentSoul, -amount);
            }
            else
            {
                state.SetInt(SpentSoul, 0);
                amount -= soulDiff;
                int reserveDiff = (pm.Get(VesselFragments) / 3) * 33 - state.GetInt(SpentReserveSoul);

                if (amount < reserveDiff)
                {
                    state.Increment(SpentReserveSoul, -amount);
                }
                else
                {
                    state.SetInt(SpentReserveSoul, 0);
                }
            }
        }

        protected virtual int GetMaxSoul(ProgressionManager pm, LazyStateBuilder state)
        {
            return 99 - state.GetInt(SoulLimiter);
        }

        protected virtual int GetMaxReserveSoul(ProgressionManager pm, LazyStateBuilder state)
        {
            return (pm.Get(VesselFragments) / 3) * 33;
        }
    }
}
