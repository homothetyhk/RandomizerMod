using Newtonsoft.Json;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.Logging;
using RandomizerMod.RC;

namespace RandomizerMod.Settings
{
    // class for storing data related to seed progress
    // updating handled through IC.TrackerUpdate
    // Note: tracking may fail if placement names do not match the corresponding RandoLocation names in the RawSpoiler.
    // tracking does not depend on item names
    public class TrackerData
    {
        /// <summary>
        /// The CTX indices of the items that have been obtained.
        /// </summary>
        public HashSet<int> obtainedItems = new();
        /// <summary>
        /// A set which tracks the placements which have been previewed, by the Name property of the corresponding RandoLocation.
        /// </summary>
        public HashSet<string> previewedLocations = new();
        /// <summary>
        /// A dictionary which tracks the transitions that have been visited. Keys are sources and values are their targets.
        /// </summary>
        public Dictionary<string, string> visitedTransitions = new();
        /// <summary>
        /// A set which tracks the placements which have all items obtained, by the Name property of the corresponding RandoLocation.
        /// </summary>
        public HashSet<string> clearedLocations = new();
        /// <summary>
        /// A set which tracks the placements which are reachable in logic and have items remaining and have not been previewed, by the Name property of the corresponding RandoLocation.
        /// </summary>
        public HashSet<string> uncheckedReachableLocations = new();
        /// <summary>
        /// A set which tracks the transitions which are reachable in logic and have not been visited.
        /// </summary>
        public HashSet<string> uncheckedReachableTransitions = new();

        /// <summary>
        /// Should out of logic items and transitions be immediately added to current progression when acquired, or deferred until their locations are reachable?
        /// </summary>
        public bool AllowSequenceBreaks;
        /// <summary>
        /// The subset of obtainedItems that are currently out of logic, and were obtained by sequence breaking. Entries are removed as they become in logic.
        /// </summary>
        public HashSet<int> outOfLogicObtainedItems = new();
        /// <summary>
        /// The subset of visited transitions that are currently out of logic, and were visited by sequence breaking.
        /// </summary>
        public HashSet<string> outOfLogicVisitedTransitions = new();
        /// <summary>
        /// The ProgressionManager for the current state, with the information available to the player.
        /// </summary>
        [JsonIgnore] public ProgressionManager pm;
        [JsonIgnore] public LogicManager lm;
        [JsonIgnore] public RandoModContext ctx;
        private MainUpdater mu;
        public string logFileName;

        public void Setup(GenerationSettings gs, RandoModContext ctx)
        {
            this.ctx = ctx;
            lm = ctx.LM;

            List<RandoModItem> items = obtainedItems.Where(i => !outOfLogicObtainedItems.Contains(i)).Select(i => ctx.itemPlacements[i].Item).ToList();

            HashSet<string> transitionProgression = new();
            foreach (KeyValuePair<string, string> kvp in visitedTransitions)
            {
                if (outOfLogicVisitedTransitions.Contains(kvp.Key)) continue;
                transitionProgression.Add(kvp.Key);
                transitionProgression.Add(kvp.Value);
            }

            pm = new(lm, ctx);
            pm.Add(items);
            pm.Add(transitionProgression.Select(t => lm.GetTransition(t)));
            LogManager.Write(tw =>
            {
                tw.WriteLine("Setting up TrackerData...");
                foreach ((RandoItem ri, RandoLocation rl) in obtainedItems
                    .Where(i => !outOfLogicObtainedItems.Contains(i))
                    .Select(i => ctx.itemPlacements[i]))
                {
                    tw.WriteLine($"Adding randomized item obtained from {rl.Name} to progression: {ri.Name}");
                }
                foreach (string t in transitionProgression)
                {
                    tw.WriteLine("Adding randomized transition to progression: " + t);
                }
            }, logFileName);

            // note: location costs are ignored in the tracking, to prevent providing unintended information, by using p.location.logic rather than p.location
            // it is assumed that no information is divulged from the regular location logic and transition logic

            mu = new(lm);
            mu.AddEntries(lm.Waypoints.Select(w => new DelegateUpdateEntry(w, pm =>
            {
                AppendWaypointToDebug(w);
                pm.Add(w);
            })));
            mu.AddEntries(ctx.Vanilla.Select(v => new DelegateUpdateEntry(v.Location, pm =>
            {
                AppendVanillaToDebug(v);
                pm.Add(v.Item);
                if (v.Location is ILogicItem li) // e.g. add vanilla source transition term to progression
                {
                    pm.Add(li);
                }
            })));
            if (ctx.itemPlacements != null)
            {
                mu.AddEntries(ctx.itemPlacements.Select((p, id) => new DelegateUpdateEntry(p.Location.logic, OnCanGetLocation(id))));
            }

            if (ctx.transitionPlacements != null)
            {
                mu.AddEntries(ctx.transitionPlacements.Select((p, id) => new DelegateUpdateEntry(p.Source, OnCanGetTransition(id))));
            }

            mu.Hook(pm); // automatically handle tracking reachable unobtained locations/transitions and adding vanilla progression to pm
        }

        private Action<ProgressionManager> OnCanGetLocation(int id)
        {
            return pm =>
            {
                (RandoItem item, RandoLocation location) = ctx.itemPlacements[id];
                AppendRandoLocationToDebug(location);
                if (outOfLogicObtainedItems.Remove(id))
                {
                    AppendRandoItemToDebug(item, location);
                    pm.Add(item);
                }
                if (!clearedLocations.Contains(location.Name) && !previewedLocations.Contains(location.Name))
                {
                    uncheckedReachableLocations.Add(location.Name);
                }
            };
        }

        private Action<ProgressionManager> OnCanGetTransition(int id)
        {
            return pm =>
            {
                (RandoTransition target, RandoTransition source) = ctx.transitionPlacements[id];
                AppendReachableTransitionToDebug(source.lt);
                
                if (!pm.Has(source.lt.term))
                {
                    AppendProgressionTransitionToDebug(source.lt);
                    pm.Add(source);
                }
                
                if (outOfLogicVisitedTransitions.Remove(source.Name) && !pm.Has(target.lt.term))
                {
                    AppendProgressionTransitionToDebug(target.lt);
                    pm.Add(target);
                }

                if (!visitedTransitions.ContainsKey(source.Name))
                {
                    uncheckedReachableTransitions.Add(source.Name);
                }
            };
        }

        public void OnItemObtained(int id, string itemName, string placementName)
        {
            (RandoItem ri, RandoLocation rl) = ctx.itemPlacements[id];
            obtainedItems.Add(id);
            if (AllowSequenceBreaks || rl.logic.CanGet(pm))
            {
                AppendRandoItemToDebug(ri, rl);
                pm.Add(ri);
            }
            else
            {
                outOfLogicObtainedItems.Add(id);
            }
        }

        public void OnPlacementPreviewed(string placementName)
        {
            previewedLocations.Add(placementName);
            uncheckedReachableLocations.Remove(placementName);
        }

        public void OnPlacementCleared(string placementName)
        {
            clearedLocations.Add(placementName);
            previewedLocations.Remove(placementName);
            uncheckedReachableLocations.Remove(placementName);
        }

        public void OnTransitionVisited(string source, string target)
        {
            visitedTransitions[source] = target;
            uncheckedReachableTransitions.Remove(source);

            LogicTransition st = lm.GetTransition(source);
            if (AllowSequenceBreaks || st.CanGet(pm))
            {
                LogicTransition tt = lm.GetTransition(target);
                if (!pm.Has(st.term))
                {
                    AppendProgressionTransitionToDebug(st);
                    pm.Add(st);
                }

                if (!pm.Has(tt.term))
                {
                    AppendProgressionTransitionToDebug(tt);
                    pm.Add(tt);
                }
            }
            else
            {
                outOfLogicVisitedTransitions.Add(source);
            }
        }

        private void AppendToDebug(string line)
        {
            LogManager.Append(line + Environment.NewLine, logFileName);
        }

        private void AppendWaypointToDebug(LogicWaypoint w)
        {
            AppendToDebug("New reachable waypoint: " + w.Name);
        }

        private void AppendVanillaToDebug(GeneralizedPlacement v)
        {
            AppendToDebug($"New reachable vanilla placement: {v.Item.Name} at {v.Location.Name}");
        }

        private void AppendRandoItemToDebug(RandoItem ri, RandoLocation rl)
        {
            AppendToDebug($"Adding randomized item obtained from {rl.Name} to progression: {ri.Name}");
        }

        private void AppendRandoLocationToDebug(RandoLocation rl)
        {
            AppendToDebug("New reachable randomized location: " + rl.Name);
        }

        private void AppendReachableTransitionToDebug(LogicTransition lt)
        {
            AppendToDebug("New reachable randomized transition: " + lt.Name);
        }

        private void AppendProgressionTransitionToDebug(LogicTransition lt)
        {
            AppendToDebug("Adding randomized transition to progression: " + lt.Name);
        }

        public bool HasVisited(string transition) => visitedTransitions.ContainsKey(transition);

        public class DelegateUpdateEntry : UpdateEntry
        {
            readonly Action<ProgressionManager> onAdd;
            readonly ILogicDef location;

            public DelegateUpdateEntry(ILogicDef location, Action<ProgressionManager> onAdd)
            {
                this.location = location;
                this.onAdd = onAdd;
            }

            public override bool CanGet(ProgressionManager pm)
            {
                return location.CanGet(pm);
            }

            public override IEnumerable<Term> GetTerms()
            {
                return location.GetTerms();
            }

            public override void OnAdd(ProgressionManager pm)
            {
                onAdd?.Invoke(pm);
            }

            public override void OnRemove(ProgressionManager pm) { }
        }
    }
}
