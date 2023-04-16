using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace RandomizerModTests
{
    public class MiscLogicTests
    {

        [Fact]
        public void NoLocationsUnlockedWithoutState()
        {
            Data.Load();
            GenerationSettings gs = new();
            gs.StartLocationSettings.StartLocation = "";
            LogicManager lm = RCData.GetNewLogicManager(gs);
            RandoModContext ctx = new(lm)
            {
                GenerationSettings = gs,
            };
            ctx.notchCosts.AddRange(CharmNotchCosts._vanillaCosts);
            ProgressionManager pm = new(lm, ctx);

            // set all item terms to max value, and zero out all waypoints and transitions.
            foreach (Term t in lm.Terms)
            {
                if (t.Type == TermType.State) pm.SetState(t, null);
                else pm.Set(t, int.MaxValue);
            }
            foreach (LogicWaypoint lw in lm.Waypoints)
            {
                if (lw.term.Type != TermType.State) pm.Set(lw.term, 0);
            }

            HashSet<string> allowList = new()
            {
                "Start",
                "Start_State",
                "Nightmare_Lantern_Lit", // Grimmchild
                "Opened_Shaman_Pillar" // infection
            };

            Assert.DoesNotContain(lm.LogicLookup.Values, ld => ld.CanGet(pm) && !allowList.Contains(ld.Name));
        }

    }
}
