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
        }

        public override IEnumerable<Term> GetAffectedTerms()
        {
            yield return LeftDashTerm;
            yield return RightDashTerm;
        }
    }
}
