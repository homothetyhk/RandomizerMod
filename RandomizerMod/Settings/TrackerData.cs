using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.Randomization;
using RandomizerMod.RC;

namespace RandomizerMod.Settings
{
    // class for storing data related to seed progress
    // updating handled through IC.TrackerUpdate
    public class TrackerData
    {
        public HashSet<int> obtainedItems = new();
        public HashSet<string> previewedLocations = new();
        public Dictionary<string, string> visitedTransitions = new();
        public HashSet<string> clearedLocations = new();
        public HashSet<string> uncheckedReachableLocations = new();
        public HashSet<string> uncheckedReachableTransitions = new();

        [JsonIgnore]
        public ProgressionManager pm;
        [JsonIgnore]
        public LogicManager lm;
        [JsonIgnore]
        public RandoContext ctx;
        private MainUpdater mu;

        public void Setup(GenerationSettings gs, RandoContext ctx)
        {
            this.ctx = ctx;
            lm = ctx.LM;

            pm = new(lm, ctx);
            pm.Add(obtainedItems.Select(i => ctx.itemPlacements[i].item));

            pm.Add(visitedTransitions.Keys.Select(t => lm.GetTransition(t)));

            mu = new(lm);
            mu.AddPlacements(lm.Waypoints);
            mu.AddPlacements(ctx.Vanilla);
            if (ctx.itemPlacements != null) mu.AddEntries(ctx.itemPlacements.Select(p => new HelperItemUpdateEntry(p.location)));
            if (ctx.transitionPlacements != null) mu.AddEntries(ctx.transitionPlacements.Select(p => new HelperTransitionUpdateEntry(p.source)));
            mu.Hook(pm); // automatically handle tracking reachable unobtained locations/transitions and adding vanilla progression to pm
        }


        private class HelperItemUpdateEntry : UpdateEntry
        {
            public readonly RandoLocation location;
            public HelperItemUpdateEntry(RandoLocation location)
            {
                this.location = location;
            }

            public override bool CanGet(ProgressionManager pm)
            {
                return location.logic.CanGet(pm);
            }

            public override IEnumerable<Term> GetTerms()
            {
                return location.GetTerms();
            }

            public override void OnAdd(ProgressionManager pm)
            {
                if (!RandomizerMod.RS.TrackerData.clearedLocations.Contains(location.Name) && !RandomizerMod.RS.TrackerData.previewedLocations.Contains(location.Name))
                {
                    RandomizerMod.RS.TrackerData.uncheckedReachableLocations.Add(location.Name);
                }
            }

            public override void OnRemove(ProgressionManager pm)
            {

            }
        }

        private class HelperTransitionUpdateEntry : UpdateEntry
        {
            public readonly RandoTransition transition;
            public HelperTransitionUpdateEntry(RandoTransition transition)
            {
                this.transition = transition;
            }

            public override bool CanGet(ProgressionManager pm)
            {
                return transition.CanGet(pm);
            }

            public override IEnumerable<Term> GetTerms()
            {
                return transition.GetTerms();
            }

            public override void OnAdd(ProgressionManager pm)
            {
                if (!RandomizerMod.RS.TrackerData.visitedTransitions.ContainsKey(transition.Name))
                {
                    RandomizerMod.RS.TrackerData.uncheckedReachableTransitions.Add(transition.Name);
                }
            }

            public override void OnRemove(ProgressionManager pm)
            {

            }
        }
    }
}
