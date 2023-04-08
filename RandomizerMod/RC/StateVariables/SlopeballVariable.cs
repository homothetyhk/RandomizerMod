using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    public class SlopeballVariable : StateModifierWrapper<CastSpellVariable>
    {
        public override string Name { get; }

        protected readonly Term slopeballSkips;
        protected readonly Term fireball;
        public const string Prefix = "$SLOPEBALL";
        protected override string InnerPrefix => CastSpellVariable.Prefix;

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (term.StartsWith(Prefix))
            {
                variable = new SlopeballVariable(term, lm);
                return true;
            }
            variable = default;
            return false;
        }

        public SlopeballVariable(string name, LogicManager lm) : base(name, lm)
        {
            Name = name;
            try
            {
                slopeballSkips = lm.GetTermStrict("SLOPEBALLSKIPS");
                fireball = lm.GetTermStrict("FIREBALL");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing SlopeballVariable", e);
            }
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return slopeballSkips;
            yield return fireball;
            foreach (Term t in InnerVariable.GetTerms()) yield return t;
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            if (!pm.Has(slopeballSkips) || !pm.Has(fireball)) return Enumerable.Empty<LazyStateBuilder>();
            return InnerVariable.ModifyState(sender, pm, state);
        }
    }
}
