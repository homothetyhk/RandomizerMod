using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $BENCHRESET
     * Required Parameters: none
     * Optional Parameters: "noDG" if dreamgate is not possible from the bench
    */
    public class BenchResetVariable : StateResetter
    {
        public override string Name { get; }
        protected override State ResetState { get; }
        protected override string ResetLogicProperty => "BenchResetCondition";

        public const string Prefix = "$BENCHRESET";
        public bool canDreamgate;
        public Term salubrasBlessing;
        public Term dreamnail;
        public Term essence;
        public State dgState;

        public BenchResetVariable(string name, bool canDreamgate, LogicManager lm) : base(lm)
        {
            Name = name;
            this.canDreamgate = canDreamgate;
            try
            {
                salubrasBlessing = lm.GetTermStrict("Salubra's_Blessing");
                dreamnail = lm.GetTermStrict("DREAMNAIL");
                essence = lm.GetTermStrict("ESSENCE");

                ResetState = lm.StateManager.GetNamedStateStrict("BenchResetState");
                dgState = lm.StateManager.StartState;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing BenchResetVariable", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                variable = new BenchResetVariable(term, !parameters.Contains("noDG"), lm);
                return true;
            }
            variable = default;
            return false;
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return salubrasBlessing;
            yield return dreamnail;
            yield return essence;
            foreach (Term t in base.GetTerms()) yield return t;
        }

        public override IEnumerable<LazyStateBuilder>? ProvideState(object? sender, ProgressionManager pm)
        {
            if (canDreamgate && pm.Has(dreamnail, 2) && pm.Has(essence))
            {
                yield return new(dgState);
            }
        }
    }
}
