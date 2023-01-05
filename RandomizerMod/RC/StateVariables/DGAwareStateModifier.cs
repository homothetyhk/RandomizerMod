using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /// <summary>
    /// Base class for state modifiers which automatically and consistently handles checking for dream gate reset opportunities before and after modification.
    /// </summary>
    public abstract class DGAwareStateModifier : StateModifier
    {
        public enum DGResetType
        {
            AlwaysBeforeAlwaysAfter,
            AlwaysBeforeNeverAfter,
            LazyBeforeAlwaysAfter,
            LazyBeforeNeverAfter,
            NeverBeforeAlwaysAfter,
            NeverBeforeNeverAfter,
        }

        /// <summary>
        /// Refers to whether it is possible to dream gate in the vicinity of the modifier.
        /// If true, the state modifier will account for possible dream gate resets before and after the modifier executes.
        /// </summary>
        protected virtual bool CanDreamGate { get; init; }
        protected virtual DGResetType DreamGateResetType => DGResetType.LazyBeforeNeverAfter;

        protected DreamGateResetVariable DreamGateReset { get; }

        public DGAwareStateModifier(LogicManager lm)
        {
            try
            {
                DreamGateReset = (DreamGateResetVariable)lm.GetVariableStrict(DreamGateResetVariable.Prefix);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing RandoStateModifier of type " + GetType().Name, e);
            }
        }

        protected abstract IEnumerable<LazyStateBuilder> ModifyStateInternal(object? sender, ProgressionManager pm, LazyStateBuilder state);
        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            if (!CanDreamGate) return ModifyStateInternal(sender, pm, state);

            return DreamGateResetType switch
            {
                DGResetType.AlwaysBeforeAlwaysAfter => DreamGateReset.ModifyState(sender, pm, state)
                                                                     .SelectMany(lsb => ModifyStateInternal(sender, pm, lsb))
                                                                     .SelectMany(lsb => DreamGateReset.ModifyState(sender, pm, lsb)),
                DGResetType.AlwaysBeforeNeverAfter => DreamGateReset.ModifyState(sender, pm, state).SelectMany(lsb => ModifyStateInternal(sender, pm, lsb)),
                DGResetType.LazyBeforeAlwaysAfter => ModifyStateLazyDG(sender, pm, state).SelectMany(lsb => DreamGateReset.ModifyState(sender, pm, lsb)),
                DGResetType.LazyBeforeNeverAfter => ModifyStateLazyDG(sender, pm, state),
                DGResetType.NeverBeforeAlwaysAfter => ModifyStateInternal(sender, pm, state).SelectMany(lsb => DreamGateReset.ModifyState(sender, pm, lsb)),
                DGResetType.NeverBeforeNeverAfter => ModifyStateInternal(sender, pm, state),
                _ => throw new NotImplementedException(),
            };
        }

        private IEnumerable<LazyStateBuilder> ModifyStateLazyDG(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            bool empty = true;
            LazyStateBuilder backup = new(state);
            foreach (LazyStateBuilder lsb in ModifyStateInternal(sender, pm, state))
            {
                empty = false;
                yield return lsb;
            }

            if (empty)
            {
                foreach (LazyStateBuilder lsb in DreamGateReset.ModifyState(sender, pm, backup))
                {
                    foreach (LazyStateBuilder lsb2 in DreamGateReset.ModifyState(sender, pm, lsb))
                    {
                        yield return lsb2;
                    }
                }
            }
        }


        public override IEnumerable<Term> GetTerms()
        {
            foreach (Term t in DreamGateReset.GetTerms()) yield return t;
        }
    }
}
