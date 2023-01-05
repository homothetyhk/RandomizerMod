using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $DREAMGATERESET
     * Required Parameters: none
     * Optional Parameters: none
     * Provides the effect of warping to a bench or start, and dreamgating back.
     * Warping to a bench is only considered if the state has a bench respawn, or if the progression includes qualifying benches and warp select is allowed.
     * Warping to start is always considered. If warping to a bench is possible, the warp to start is done upon the result of first warping to a bench.
    */
    public class DreamGateResetVariable : StateResetter
    {
        public override string Name { get; }
        public const string Prefix = "$DREAMGATERESET";
        protected override State ResetState { get; }
        protected override string? ResetLogicProperty => "DreamGateResetCondition";
        protected override bool OptIn => true;

        protected readonly Term Dreamnail;
        protected readonly Term Essence;
        protected readonly Term? WarpToDGBench;
        protected readonly StateBool NoFlower;
        protected readonly StateBool MovedDreamgate;
        protected readonly StateBool LacksBenchRespawn;
        protected readonly WarpToStartResetVariable WarpToStartResetter;
        protected readonly WarpToBenchResetVariable WarpToBenchResetter;
        protected readonly BenchResetVariable BenchResetter;

        public DreamGateResetVariable(string name, LogicManager lm) : base(lm)
        {
            try
            {
                Name = name;
                Dreamnail = lm.GetTermStrict("DREAMNAIL");
                Essence = lm.GetTermStrict("ESSENCE");
                WarpToDGBench = lm.GetTerm("Can_Warp_To_DG_Bench");
                LacksBenchRespawn = lm.StateManager.GetBoolStrict("NOBENCHRESPAWN");
                NoFlower = lm.StateManager.GetBoolStrict("NOFLOWER");
                MovedDreamgate = lm.StateManager.GetBoolStrict("MOVEDDREAMGATE");
                WarpToStartResetter = (WarpToStartResetVariable)lm.GetVariableStrict("$WARPTOSTART");
                WarpToBenchResetter = (WarpToBenchResetVariable)lm.GetVariableStrict("$WARPTOBENCH");
                BenchResetter = (BenchResetVariable)lm.GetVariableStrict("$BENCHRESET");
                ResetState = lm.StateManager.GetNamedStateStrict("DreamGateResetState");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing DreamgateResetVariable", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out _))
            {
                variable = new DreamGateResetVariable(term, lm);
                return true;
            }
            variable = default;
            return false;
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            if (!CanDreamgate(pm)) yield break;

            bool bench = !state.GetBool(LacksBenchRespawn) || WarpToDGBench is not null && pm.Has(WarpToDGBench);

            SetDreamgate(pm, ref state);
            if (bench)
            {
                state = WarpToBenchResetter.ResetSingle(pm, state); // we assume that it is beneficial in any path to stop by the bench
                LazyStateBuilder benchState = new(state);
                DreamgateBack(pm, ref benchState);
                yield return benchState;

                state = WarpToStartResetter.ResetSingle(pm, state);
                DreamgateBack(pm, ref state);
                yield return state;
            }
            else
            {
                state = WarpToStartResetter.ResetSingle(pm, state);
                DreamgateBack(pm, ref state);
                yield return state;
            }
        }

        public override IEnumerable<LazyStateBuilder>? ProvideState(object? sender, ProgressionManager pm)
        {
            if (!CanDreamgate(pm)) return null;

            return Enumerable.Empty<LazyStateBuilder>();
        }

        public override IEnumerable<Term> GetTerms()
        {
            foreach (Term t in base.GetTerms()) yield return t;
            yield return Dreamnail;
            yield return Essence;
        }

        /// <summary>
        /// Outputs the result of the operation, with all combinations of dream gate reset / no dream gate reset before and/or after the operation.
        /// </summary>
        public virtual IEnumerable<LazyStateBuilder> WrapOperation(object? sender, ProgressionManager pm, LazyStateBuilder state,
            Func<object?, ProgressionManager, LazyStateBuilder, IEnumerable<LazyStateBuilder>> op)
        {
            return AdjustForDGReset(sender, pm, state)
                .SelectMany(lsb => op(sender, pm, state))
                .SelectMany(lsb => AdjustForDGReset(sender, pm, state));
        }

        /// <summary>
        /// If the player can dream gate, outputs the result of a dg state reset on the input.
        /// Also outputs the unmodified input, unless the dg state reset results in a comparably better state.
        /// </summary>
        protected virtual IEnumerable<LazyStateBuilder> AdjustForDGReset(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            bool shortCircuit = false;
            if (CanDreamgate(pm))
            {
                foreach (LazyStateBuilder lsb in ModifyState(sender, pm, new(state)))
                {
                    shortCircuit = shortCircuit || LazyStateBuilder.IsComparablyLE(lsb, state);
                    yield return lsb;
                }
            }
            if (!shortCircuit)
            {
                yield return state;
            }
        }

        private bool CanDreamgate(ProgressionManager pm)
        {
            return pm.Has(Dreamnail, 2) && pm.Has(Essence);
        }

        private void SetDreamgate(ProgressionManager pm, ref LazyStateBuilder lsb)
        {
            lsb.SetBool(MovedDreamgate, true);
        }

        private void DreamgateBack(ProgressionManager pm, ref LazyStateBuilder lsb)
        {
            lsb = ResetSingle(pm, lsb);
        }
    }
}
