using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    public class ShriekPogoVariable : StateModifierWrapper<CastSpellVariable>
    {
        public override string Name { get; }

        protected readonly Term shriekPogoSkips;
        protected readonly Term scream;
        protected readonly Term wings;
        protected readonly CastSpellVariable? stalledCasts;
        protected readonly Term? difficultSkips;
        protected readonly Term? leftDash;
        protected readonly Term? rightDash;
        protected bool noLeftStall;
        protected bool noRightStall;

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
                wings = lm.GetTermStrict("WINGS");

                if (InnerVariable.SpellCasts.Any(i => i > 1) && (!noLeftStall || !noRightStall))
                {
                    VariableResolver.TryMatchPrefix(name, Prefix, out string[] ps);
                    stalledCasts = (CastSpellVariable)lm.GetVariableStrict(
                        CastSpellVariable.Prefix + 
                        '[' + string.Join(",", ps.SelectMany(p => int.TryParse(p, out int i) ? Enumerable.Repeat("1", i) : Enumerable.Repeat(p, 1))) + ']'
                        );
                    if (!noLeftStall) leftDash = lm.GetTermStrict("LEFTDASH");
                    if (!noRightStall) rightDash = lm.GetTermStrict("RIGHTDASH");
                }
                if (InnerVariable.SpellCasts.Sum() > 3)
                {
                    difficultSkips = lm.GetTermStrict("DIFFICULTSKIPS");
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing ShriekPogoVariable", e);
            }
        }

        protected override bool Consume(string parameter)
        {
            switch (parameter)
            {
                case "NOSTALL":
                    noLeftStall = noRightStall = true;
                    return true;
                case "NOLEFTSTALL":
                    noLeftStall = true;
                    return true;
                case "NORIGHTSTALL":
                    noRightStall = true;
                    return true;
                default: return false;
            }
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return shriekPogoSkips;
            yield return scream;
            foreach (Term t in InnerVariable.GetTerms()) yield return t;
            if (difficultSkips is not null) yield return difficultSkips;
            if (stalledCasts is not null) foreach (Term t in stalledCasts.GetTerms()) yield return t;
            if (leftDash is not null) yield return leftDash;
            if (rightDash is not null) yield return rightDash;
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            if (!pm.Has(shriekPogoSkips) || !pm.Has(scream, 2) || !pm.Has(wings) || difficultSkips is not null && !pm.Has(difficultSkips)) return Enumerable.Empty<LazyStateBuilder>();

            if (stalledCasts is not null && (leftDash is not null && pm.Has(leftDash) || rightDash is not null && pm.Has(rightDash)))
            {
                return stalledCasts.ModifyState(sender, pm, state);
            }

            return InnerVariable.ModifyState(sender, pm, state);
        }
    }
}
