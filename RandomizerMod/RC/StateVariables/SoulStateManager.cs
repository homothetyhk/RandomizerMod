using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using static RandomizerMod.RC.StateVariables.ISoulStateManager;

namespace RandomizerMod.RC.StateVariables
{
    public interface ISoulStateManager : ILogicVariable
    {
        /// <summary>
        /// Applies the effect of spending the given amount of soul.
        /// </summary>
        IEnumerable<LazyStateBuilder> SpendSoul(ProgressionManager pm, LazyStateBuilder state, int amount);
        /// <summary>
        /// Applies the effect of the given sequence of casts. The provided array should contains counts for sequences of consecutive casts.
        /// Soul vessels will be applied between sequences of consecutive casts, but not withn sequences.
        /// </summary>
        IEnumerable<LazyStateBuilder> SpendSoulSequence(ProgressionManager pm, LazyStateBuilder state, int amount, int[] castGroups);
        /// <summary>
        /// Equivalent to <see cref="SpendSoulSequence(ProgressionManager, LazyStateBuilder, int, int[])"/>, in the case where int[] casts is an array of ones. That is, allows soul vessels to refill between each cast.
        /// </summary>
        IEnumerable<LazyStateBuilder> SpendSoulSlow(ProgressionManager pm, LazyStateBuilder state, int amount, int casts);
        /// <summary>
        /// Determines whether <see cref="SpendSoul(ProgressionManager, LazyStateBuilder, int)"/> would succeed, and if so modifies the state and returns true.
        /// <br/>On false return, the state is not modified.
        /// <br/>Per its signature, the result is a single state. Implementations where <see cref="SpendSoul(ProgressionManager, LazyStateBuilder, int)"/> would produce multiple states must admit a consistent truncated result.
        /// </summary>
        bool TrySpendSoul(ProgressionManager pm, ref LazyStateBuilder state, int amount);
        /// <summary>
        /// Alternate version of <see cref="SpendSoulSequence(ProgressionManager, LazyStateBuilder, int, int[])"/> which is guaranteed to produce a single output, and which modifies the input only on success.
        /// </summary>
        bool TrySpendSoulSequence(ProgressionManager pm, ref LazyStateBuilder state, int amount, int[] castGroups);
        /// <summary>
        /// Alternate version of <see cref="SpendSoulSequence(ProgressionManager, LazyStateBuilder, int, int[])"/> which is guaranteed to produce a single output, and which modifies the input only on success.
        /// </summary>
        bool TrySpendSoulSlow(ProgressionManager pm, ref LazyStateBuilder state, int amount, int casts);
        
        IEnumerable<LazyStateBuilder> SpendAllSoul(ProgressionManager pm, LazyStateBuilder state);
        bool TrySpendAllSoul(ProgressionManager pm, ref LazyStateBuilder state);

        /// <summary>
        /// Attempts to restore the requested amount of soul to the state.
        /// Note that this may fail to restore soul (e.g. if CannotRegainSoul is set).
        /// In any case, this succeeds (does not return empty).
        /// </summary>
        IEnumerable<LazyStateBuilder> RestoreSoul(ProgressionManager pm, LazyStateBuilder state, int amount);
        /// <summary>
        /// Attempts to restore soul to the state.
        /// Note that this may fail to restore soul (e.g. if CannotRegainSoul is set).
        /// In any case, this succeeds (does not return empty).
        /// </summary>
        IEnumerable<LazyStateBuilder> RestoreAllSoul(ProgressionManager pm, LazyStateBuilder state, bool restoreReserves);
        /// <summary>
        /// Attempts to restore the requested amount of soul to the state.
        /// Returns false if soul cannot be restored (e.g. if CannotRegainSoul is set).
        /// Returns true if the full amount is restored (including if it brings the state to full or the state is already full).
        /// </summary>
        bool TryRestoreSoul(ProgressionManager pm, ref LazyStateBuilder state, int amount);
        /// <summary>
        /// Attempts to restore soul to the state.
        /// Returns false if soul cannot be restored (e.g. if CannotRegainSoul is set).
        /// Returns true if the state is restored (including if the state is already full).
        /// </summary>
        bool TryRestoreAllSoul(ProgressionManager pm, ref LazyStateBuilder state, bool restoreReserves);
        /// <summary>
        /// Returns info on the current and maximum soul and reserves.
        /// </summary>
        SoulInfo GetSoulInfo(ProgressionManager pm, LazyStateBuilder state);

        /// <summary>
        /// Changes the maximum soul cap, similar to the effect after death.
        /// <br/>For example, if <paramref name="limiter"/> is 33, the soul cap is reduced to 99 - 33 = 66. With consecutive uses, the new value is kept.
        /// <br/>If the cap is increased, the new portion of the meter is treated as empty, and reserves are rebalanced.
        /// <br/>If the soul cap <paramref name="appliesToPriorPath"/>, this can fail if previous soul usage required a higher soul cap. If the lowered cap only applies going forward, this should not fail.
        /// </summary>
        bool TrySetSoulLimit(ProgressionManager pm, ref LazyStateBuilder state, int limiter, bool appliesToPriorPath);
        /// <summary>
        /// Changes the maximum soul cap, similar to the effect after death.
        /// <br/>For example, if <paramref name="limiter"/> is 33, the soul cap is reduced to 99 - 33 = 66. With consecutive uses, the new value is kept.
        /// <br/>If the cap is increased, the new portion of the meter is treated as empty, and reserves are rebalanced.
        /// <br/>If the soul cap <paramref name="appliesToPriorPath"/>, this can fail if previous soul usage required a higher soul cap. If the lowered cap only applies going forward, this should not fail.
        /// </summary>
        IEnumerable<LazyStateBuilder> LimitSoul(ProgressionManager pm, LazyStateBuilder state, int limiter, bool appliesToPriorPath);

        public readonly record struct SoulInfo(int Soul, int MaxSoul, int ReserveSoul, int MaxReserveSoul);
    }

    internal class SoulStateManager : LogicVariable, ISoulStateManager
    {
        public override string Name { get; }
        public const string Prefix = "$SSM";

        protected readonly StateInt SpentSoul;
        protected readonly StateInt SpentReserveSoul;
        protected readonly StateInt SoulLimiter;
        protected readonly StateInt MaxRequiredSoul;
        protected readonly StateInt UsedNotches;
        protected readonly StateBool CannotRegainSoul;
        protected readonly Term VesselFragments;

        public SoulStateManager(string name, LogicManager lm)
        {
            Name = name;
            try
            {
                SpentSoul = lm.StateManager.GetIntStrict("SPENTSOUL");
                SpentReserveSoul = lm.StateManager.GetIntStrict("SPENTRESERVESOUL");
                SoulLimiter = lm.StateManager.GetIntStrict("SOULLIMITER");
                MaxRequiredSoul = lm.StateManager.GetIntStrict("REQUIREDMAXSOUL");
                UsedNotches = lm.StateManager.GetIntStrict("USEDNOTCHES");
                CannotRegainSoul = lm.StateManager.GetBoolStrict("CANNOTREGAINSOUL");
                VesselFragments = lm.GetTermStrict("VESSELFRAGMENTS");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing CastSpellVariable", e);
            }
        }

        public override IEnumerable<Term> GetTerms()
        {
            return [VesselFragments];
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (term == Prefix)
            {
                variable = new SoulStateManager(term, lm);
                return true;
            }
            variable = default;
            return false;
        }


        public IEnumerable<LazyStateBuilder> SpendSoul(ProgressionManager pm, LazyStateBuilder state, int amount)
        {
            if (TrySpendSoul(pm, ref state, amount))
            {
                return [state];
            }
            return [];
        }

        public IEnumerable<LazyStateBuilder> SpendSoulSequence(ProgressionManager pm, LazyStateBuilder state, int amount, int[] castGroups)
        {
            if (TrySpendSoulSequence(pm, ref state, amount, castGroups))
            {
                return SpendSoulSequence(pm, state, amount, castGroups);
            }
            return [];
        }

        public IEnumerable<LazyStateBuilder> SpendSoulSlow(ProgressionManager pm, LazyStateBuilder state, int amount, int casts)
        {
            if (TrySpendSoulSlow(pm, ref state, amount, casts))
            {
                return SpendSoulSlow(pm, state, amount, casts);
            }
            return [];
        }

        public bool TrySpendSoul(ProgressionManager pm, ref LazyStateBuilder state, int amount)
        {
            SoulInfo soul = GetSoulInfo(pm, state);
            if (soul.Soul < amount) return false;
            SpendAndRebalance(amount, ref soul, ref state);
            return true;
        }

        public bool TrySpendSoulSequence(ProgressionManager pm, ref LazyStateBuilder state, int amount, int[] castGroups)
        {
            SoulInfo soul = GetSoulInfo(pm, state);
            LazyStateBuilder operand = new(state);
            foreach (int num in castGroups)
            {
                int groupTotal = num * amount;
                if (soul.Soul < groupTotal) return false;
                SpendAndRebalance(groupTotal, ref soul, ref state);
            }
            state = operand;
            return true;
        }

        public bool TrySpendSoulSlow(ProgressionManager pm, ref LazyStateBuilder state, int amount, int casts)
        {
            SoulInfo soul = GetSoulInfo(pm, state);
            LazyStateBuilder operand = new(state);
            for (int i = 0; i < casts; i++)
            {
                if (soul.Soul < amount) return false;
                SpendAndRebalance(amount, ref soul, ref state);
            }
            state = operand;
            return true;
        }

        public bool TrySpendAllSoul(ProgressionManager pm, ref LazyStateBuilder state)
        {
            SoulInfo info = GetSoulInfo(pm, state);
            state.SetInt(SpentSoul, info.MaxSoul);
            state.SetInt(SpentReserveSoul, info.MaxReserveSoul);
            return true;
        }

        public IEnumerable<LazyStateBuilder> SpendAllSoul(ProgressionManager pm, LazyStateBuilder state)
        {
            TrySpendAllSoul(pm, ref state);
            return [state];
        }

        public bool TryRestoreSoul(ProgressionManager pm, ref LazyStateBuilder state, int amount)
        {
            if (state.GetBool(CannotRegainSoul))
            {
                return false;
            }
            SoulInfo soul = GetSoulInfo(pm, state);
            int soulGain = Math.Min(soul.MaxSoul - soul.Soul, amount);
            int reserveGain = Math.Min(soul.MaxReserveSoul - soul.ReserveSoul, amount - soulGain);
            if (soulGain > 0) state.Increment(SpentSoul, -soulGain);
            if (reserveGain > 0) state.Increment(SpentReserveSoul, -reserveGain);
            return true;
        }

        public IEnumerable<LazyStateBuilder> RestoreSoul(ProgressionManager pm, LazyStateBuilder state, int amount)
        {
            TryRestoreSoul(pm, ref state, amount);
            return [state];
        }

        public bool TryRestoreAllSoul(ProgressionManager pm, ref LazyStateBuilder state, bool restoreReserves)
        {
            if (state.GetBool(CannotRegainSoul))
            {
                return false;
            }
            state.SetInt(SpentSoul, 0);
            if (restoreReserves) state.SetInt(SpentReserveSoul, 0);
            return true;
        }

        public IEnumerable<LazyStateBuilder> RestoreAllSoul(ProgressionManager pm, LazyStateBuilder state, bool restoreReserves)
        {
            TryRestoreAllSoul(pm, ref state, restoreReserves);
            return [state];
        }

        public bool TrySetSoulLimit(ProgressionManager pm, ref LazyStateBuilder state, int limiter, bool appliesToPriorPath)
        {
            if (appliesToPriorPath && state.GetInt(MaxRequiredSoul) > limiter) return false;

            if (limiter > state.GetInt(SoulLimiter))
            {
                state.SetInt(SoulLimiter, limiter);
            }
            return true;
        }

        public IEnumerable<LazyStateBuilder> LimitSoul(ProgressionManager pm, LazyStateBuilder state, int limiter, bool appliesToPriorPath)
        {
            if (TrySetSoulLimit(pm, ref state, limiter, appliesToPriorPath))
            {
                return [state];
            }
            return [];
        }

        protected void SpendWithoutRebalance(int amount, ref SoulInfo soul, ref LazyStateBuilder state)
        {
            state.Increment(SpentSoul, amount);
            if (amount > state.GetInt(MaxRequiredSoul)) state.SetInt(MaxRequiredSoul, amount);
            soul = soul with { Soul = soul.Soul - amount };
        }

        protected void RebalanceReserve(ref SoulInfo soul, ref LazyStateBuilder state)
        {
            int transfer = Math.Min(soul.MaxSoul - soul.Soul, soul.ReserveSoul);
            if (transfer > 0)
            {
                state.Increment(SpentSoul, -transfer);
                state.Increment(SpentReserveSoul, transfer);
                soul = soul with { Soul = soul.Soul + transfer, ReserveSoul = soul.ReserveSoul - transfer };
            }
        }

        protected void SpendAndRebalance(int amount, ref SoulInfo soul, ref LazyStateBuilder state)
        {
            SpendWithoutRebalance(amount, ref soul, ref state);
            RebalanceReserve(ref soul, ref state);
        }

        public SoulInfo GetSoulInfo(ProgressionManager pm, LazyStateBuilder state)
        {
            int maxSoul = 99 - state.GetInt(SoulLimiter);
            int soul = maxSoul - state.GetInt(SpentSoul);
            int vessels = pm.Get(VesselFragments) / 3;
            int maxReserveSoul = vessels * 33;
            int reserveSoul = maxReserveSoul - state.GetInt(SpentReserveSoul);
            return new(soul, maxSoul, reserveSoul, maxReserveSoul);
        }
    }
}
