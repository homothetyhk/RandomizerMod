using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;

namespace RandomizerMod.RC
{
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

            if (hasLeftDash && hasRightDash) // full shade cloak behavior
            {
                pm.Incr(LeftDashTerm.Id, 1);
                pm.Incr(RightDashTerm.Id, 1);
            }
            if (LeftBiased)
            {
                if (!hasLeftDash && hasAnyShadowDash) // left shade cloak behavior
                {
                    pm.Incr(LeftDashTerm.Id, 2);
                }
                else pm.Incr(LeftDashTerm.Id, 1); // left cloak behavior
            }
            else
            {
                if (!hasRightDash && hasAnyShadowDash) // right shade cloak behavior
                {
                    pm.Incr(RightDashTerm.Id, 2);
                }
                else pm.Incr(RightDashTerm.Id, 1); // right cloak behavior
            }
        }

        public override IEnumerable<Term> GetAffectedTerms()
        {
            yield return LeftDashTerm;
            yield return RightDashTerm;
        }
    }
}
