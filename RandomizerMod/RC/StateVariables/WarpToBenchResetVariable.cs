using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $WARPTOBENCH
     * Required Parameters: none
     * Optional Parameters: none
     * Provides the effect of warping to a bench via Benchwarp or savequit. Does not verify whether the player can warp to a bench.
    */
    public class WarpToBenchResetVariable : StateModifier
    {
        public override string Name { get; }
        public const string Prefix = "$WARPTOBENCH";

        protected readonly SaveQuitResetVariable SaveQuitReset;
        protected readonly BenchResetVariable BenchReset;

        public WarpToBenchResetVariable(string name, LogicManager lm)
        {
            Name = name;
            try
            {
                SaveQuitReset = (SaveQuitResetVariable)lm.GetVariableStrict(SaveQuitResetVariable.Prefix);
                BenchReset = (BenchResetVariable)lm.GetVariableStrict(BenchResetVariable.Prefix);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing WarpToBenchResetVariable", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out _))
            {
                variable = new WarpToBenchResetVariable(term, lm);
                return true;
            }
            variable = default;
            return false;
        }

        public override IEnumerable<LazyStateBuilder>? ProvideState(object? sender, ProgressionManager pm)
        {
            return Enumerable.Empty<LazyStateBuilder>();
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            yield return ResetSingle(pm, state);
        }

        public override IEnumerable<Term> GetTerms()
        {
            return SaveQuitReset.GetTerms().Concat(BenchReset.GetTerms());
        }

        public LazyStateBuilder ResetSingle(ProgressionManager pm, LazyStateBuilder state) => BenchReset.ResetSingle(pm, SaveQuitReset.ResetSingle(pm, state));
    }
}
