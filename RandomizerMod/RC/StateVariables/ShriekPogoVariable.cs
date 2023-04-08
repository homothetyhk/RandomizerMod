using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    public class ShriekPogoVariable : StateModifierWrapper<CastSpellVariable>
    {
        public override string Name { get; }

        protected readonly Term shriekPogoSkips;
        protected readonly Term scream;
        public const string Prefix = "$SHRIEKPOGO";
        protected override string InnerPrefix => CastSpellVariable.Prefix;

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (term.StartsWith(Prefix))
            {
                variable = new ShriekPogoVariable(term, lm);
                return true;
            }
            variable = default;
            return false;
        }

        public ShriekPogoVariable(string name, LogicManager lm) : base(name, lm)
        {
            Name = name;
            try
            {
                shriekPogoSkips = lm.GetTermStrict("SHRIEKPOGOSKIPS");
                scream = lm.GetTermStrict("SCREAM");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing ShriekPogoVariable", e);
            }
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return shriekPogoSkips;
            yield return scream;
            foreach (Term t in InnerVariable.GetTerms()) yield return t;
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            if (!pm.Has(shriekPogoSkips) || !pm.Has(scream, 2)) return Enumerable.Empty<LazyStateBuilder>();
            return InnerVariable.ModifyState(sender, pm, state);
        }
    }
}
