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
    public class SplitCloakItemTemplate : LogicItemTemplate
    {
        public bool leftBiased;

        public override IEnumerable<string> GetItemFlags()
        {
            yield return "LEFTDASH"; 
            yield return "RIGHTDASH";
        }

        public override LogicItem ToLogicItem(ILogicManager lm)
        {
            return new SplitCloakItem(name, leftBiased, lm);
        }
    }

    public record SplitCloakItem : LogicItem
    {
        public SplitCloakItem(string Name, bool leftBiased, ILogicManager lm) : base(Name)
        {
            LeftBiased = leftBiased;
            LeftDashIndex = lm.GetTermIndex("LEFTDASH");
            RightDashIndex = lm.GetTermIndex("RIGHTDASH");
        }

        public bool LeftBiased { get; init; }
        public int LeftDashIndex { get; init; }
        public int RightDashIndex { get; init; }

        public override void AddTo(ProgressionManager pm)
        {
            bool noLeftDash = pm.Get(LeftDashIndex) < 1;
            bool noRightDash = pm.Get(RightDashIndex) < 1;

            // Left Dash behavior
            if (noLeftDash && (LeftBiased || !noRightDash)) pm.Incr(LeftDashIndex, 1);
            // Right Dash behavior
            else if (noRightDash && (!LeftBiased || !noLeftDash)) pm.Incr(RightDashIndex, 1);
            // Shade Cloak behavior (increments both flags)
            else
            {
                pm.Incr(LeftDashIndex, 1);
                pm.Incr(RightDashIndex, 1);
            }
        }

        public override IEnumerable<int> GetAffectedTerms()
        {
            yield return LeftDashIndex;
            yield return RightDashIndex;
        }
    }
}
