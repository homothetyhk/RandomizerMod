using RandomizerCore;
using RandomizerCore.Logic;

namespace RandomizerMod.RC
{
    [Obsolete("Use StringItem for split cloak items")]
    public record SplitCloakItem(string Name, bool LeftBiased, Term LeftDashTerm, Term RightDashTerm) : LogicItem(Name)
    {
        public override void AddTo(ProgressionManager pm)
        {
            /*
            // behavior when left and right must be obtained before shade cloak
            bool noLeftDash = pm.Get(LeftDashTerm.Id) < 1;
            bool noRightDash = pm.Get(RightDashTerm.Id) < 1;
            // Left Dash behavior
            if (noLeftDash && (LeftBiased || !noRightDash)) pm.Incr(LeftDashTerm.Id, 1);
            // Right Dash behavior
            else if (noRightDash && (!LeftBiased || !noLeftDash)) pm.Incr(RightDashTerm.Id, 1);
            // Shade Cloak behavior (increments both flags)
            else
            {
                pm.Incr(LeftDashTerm.Id, 1);
                pm.Incr(RightDashTerm.Id, 1);
            }
            */

            // behavior when split shade cloak of one direction can be obtained, but not the other
            bool hasLeftDash = pm.Has(LeftDashTerm.Id);
            bool hasRightDash = pm.Has(RightDashTerm.Id);
            bool hasAnyShadowDash = pm.Has(LeftDashTerm.Id, 2) || pm.Has(RightDashTerm.Id, 2);

            if (hasLeftDash && hasRightDash && hasAnyShadowDash)
            {
                return; // dupe
            }
            else if (hasLeftDash && hasRightDash) // full shade cloak behavior
            {
                pm.Incr(LeftDashTerm, 1);
                pm.Incr(RightDashTerm, 1);
                return;
            }
            else if (LeftBiased)
            {
                if (!hasLeftDash && hasAnyShadowDash) // left shade cloak behavior
                {
                    pm.Incr(LeftDashTerm, 2);
                    return;
                }
                else  // left cloak behavior
                {
                    pm.Incr(LeftDashTerm, 1);
                    return;
                }
            }
            else
            {
                if (!hasRightDash && hasAnyShadowDash) // right shade cloak behavior
                {
                    pm.Incr(RightDashTerm, 2);
                    return;
                }
                else // right cloak behavior
                {
                    pm.Incr(RightDashTerm, 1);
                    return;
                }
            }
        }

        public override IEnumerable<Term> GetAffectedTerms()
        {
            yield return LeftDashTerm;
            yield return RightDashTerm;
        }
    }
}
