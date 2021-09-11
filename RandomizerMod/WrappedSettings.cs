using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomizerMod.Settings;
using RandomizerCore.Extensions;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;
using System.IO;
using static RandomizerMod.LogHelper;
using RandomizerCore.Randomizers;

namespace RandomizerMod
{
    // cursed settings manipulation code
    // some settings are implemented through RandoController instead
    public class WrappedSettings : IRandomizerSettings, IItemRandomizerSettings, ITransitionRandomizerSettings
    {
        public WrappedSettings(GenerationSettings gs)
        {
            if (!RCData.Loaded) RCData.Load();

            this.gs = gs;
            LM = gs.TransitionSettings.GetLogicMode() switch
            {
                LogicMode.Room => RCData.RoomLM,
                LogicMode.Area => RCData.AreaLM,
                _ => RCData.ItemLM,
            };
        }

        List<RandoItem> items;
        List<RandoLocation> locations;


        void IRandomizerSettings.Initialize(Random rng)
        {
            SetRandomizedItems(rng);
            SetRandomizedLocations(rng);
            int diff = items.Count - locations.Count;
            if (diff > 0) PadLocations(rng, locations, diff);
            else if (diff < 0) PadItems(rng, items, -diff);

            SetLocationCosts(rng);
        }


        readonly GenerationSettings gs;

        // TODO: fix LogicManager loading
        public LogicManager LM { get; }

        int IRandomizerSettings.Seed => gs.Seed;

        bool ITransitionRandomizerSettings.Matched => gs.TransitionSettings.Matched;
        bool ITransitionRandomizerSettings.Coupled => gs.TransitionSettings.Coupled;

        void IRandomizerSettings.ApplySettings(ProgressionManager pm)
        {
            foreach (string setting in Data.GetApplicableLogicSettings(gs))
            {
                pm.Set(LM.GetTermIndex(setting), 1);
            }

            var mode = gs.TransitionSettings.GetLogicMode();
            StartDef start = Data.GetStartDef(gs.StartLocationSettings.StartLocation);

            if (mode != LogicMode.Room) pm.Set(LM.GetTermIndex(start.waypoint), 1);
            if (mode == LogicMode.Area) pm.Set(LM.GetTermIndex(start.areaTransition), 1);
            if (mode == LogicMode.Room) pm.Set(LM.GetTermIndex(start.roomTransition), 1);

            if (gs.CursedSettings.CursedMasks) pm.Set(LM.GetTermIndex("MASKSHARDS"), 4);
            else pm.Set(LM.GetTermIndex("MASKSHARDS"), 20);

            if (gs.CursedSettings.CursedNotches) pm.Set(LM.GetTermIndex("NOTCHES"), 1);
            else pm.Set(LM.GetTermIndex("NOTCHES"), 3);

        }

        void SetRandomizedItems(Random rng)
        {
            items = new();
            foreach (var pool in Data.Pools)
            {
                if (pool.IsIncluded(gs))
                {
                    items.AddRange(pool.includeItems.Select(i => new RandoItem { item = LM.GetItem(i) }));
                }
            }

            HashSet<string> removeItems = new();
            if (gs.CursedSettings.RemoveSpellUpgrades)
            {
                removeItems.Add("Abyss_Shriek");
                removeItems.Add("Shade_Soul");
                removeItems.Add("Descending_Dark");
            }

            if (gs.CursedSettings.SplitClaw)
            {
                removeItems.Add("Mantis_Claw");
            }

            if (gs.CursedSettings.SplitCloak)
            {
                removeItems.Add("Mothwing_Cloak");
            }

            if (gs.CursedSettings.ReplaceJunkWithOneGeo)
            {
                foreach (PoolDef pool in Data.Pools)
                {
                    switch (pool.name)
                    {
                        case "Mask":
                        case "CursedMask":
                        case "Vessel":
                        case "Ore":
                        case "Notch":
                        case "CursedNotch":
                        case "Geo":
                        case "Egg":
                        case "Relic":
                        case "Rock":
                        case "Soul":
                        case "PalaceSoul":
                        case "Boss_Geo":
                            removeItems.UnionWith(pool.includeItems);
                            break;
                    }
                }
            }

            int removeCount = items.RemoveAll(i => removeItems.Contains(i.Name));
            PadItems(rng, items, removeCount);
        }

        private void PadItems(Random rng, List<RandoItem> items, int count)
        {
            for (int i = 0; i < count; i++)
            {
                items.Add(new RandoItem { item = LM.GetItem("One_Geo") });
            }
        }

        List<RandoItem> IItemRandomizerSettings.GetRandomizedItems() => items ?? throw new InvalidOperationException("Randomized items were not initialized.");

        List<RandoLocation> multiLocationPrefabs;

        void SetRandomizedLocations(Random rng)
        {
            locations = new();

            foreach (var pool in Data.Pools)
            {
                if (pool.IsIncluded(gs))
                {
                    locations.AddRange(pool.includeLocations.Select(l => new RandoLocation { logic = LM.GetLogicDef(l) }));
                }
            }

            multiLocationPrefabs = locations.Where(l => Data.GetLocationDef(l.Name).multi).GroupBy(l => l.Name).Select(g => g.First()).ToList();
            HashSet<string> multiNames = new HashSet<string>(multiLocationPrefabs.Select(l => l.Name));
            int multiCount = locations.RemoveAll(l => multiNames.Contains(l.Name));

            foreach (RandoLocation prefab in multiLocationPrefabs)
            {
                locations.Add(prefab);
                multiCount--;
            }

            for (int i = 0; i < multiCount; i++)
            {
                locations.Add(new RandoLocation { logic = rng.Next(multiLocationPrefabs).logic });
            }
        }

        void SetLocationCosts(Random rng)
        {
            foreach (var loc in locations)
            {
                switch (loc.Name)
                {
                    case "Grubfather":
                        loc.costs?.Clear();
                        loc.AddCost(new SimpleCost(LM, "GRUBS", rng.Next(gs.GrubCostRandomizerSettings.MinimumGrubCost, gs.GrubCostRandomizerSettings.MaximumGrubCost + 1)));
                        LogDebug("Set Grubfather grub cost to " + ((SimpleCost)loc.costs[0]).threshold);
                        break;
                    case "Seer":
                        loc.costs?.Clear();
                        loc.AddCost(new SimpleCost(LM, "ESSENCE", rng.Next(gs.EssenceCostRandomizerSettings.MinimumEssenceCost, gs.EssenceCostRandomizerSettings.MaximumEssenceCost + 1)));
                        LogDebug("Set Seer essence cost to " + ((SimpleCost)loc.costs[0]).threshold);
                        break;
                }
            }
        }

        private void PadLocations(Random rng, List<RandoLocation> locations, int count)
        {
            var lookup = locations.ToLookup(l => Data.GetLocationDef(l.Name).multi);
            locations.Clear();
            locations.AddRange(lookup[false]);

            var multi = lookup[true].GroupBy(l => l.Name).Select(g => new { count = g.Count(), prefab = g.First() }).ToArray();
            foreach (var q in multi)
            {
                locations.Add(q.prefab);
            }

            count += multi.Sum(q => q.count - 1);

            for (int i = 0; i < count; i++)
            {
                locations.Add(new RandoLocation { logic = rng.Next(multi).prefab.logic });
            }
        }

        List<RandoLocation> IItemRandomizerSettings.GetRandomizedLocations() => locations ?? throw new InvalidOperationException("Randomized locations were not initialized.");

        

        List<RandoTransition> ITransitionRandomizerSettings.GetRandomizedTransitions()
        {
            return gs.TransitionSettings.GetLogicMode() switch
            {
                LogicMode.Room => Data.GetRoomTransitionNames().Select(t => new RandoTransition(LM.GetTransition(t))).ToList(),
                LogicMode.Area => Data.GetAreaTransitionNames().Select(t => new RandoTransition(LM.GetTransition(t))).ToList(),
                _ => new(),
            };
        }

        List<ItemPlacement> IRandomizerSettings.GetVanillaPlacements()
        {
            List<ItemPlacement> placements = new();
            foreach (PoolDef pool in Data.Pools)
            {
                if (pool.IsVanilla(gs))
                {
                    placements.AddRange(pool.vanilla.Select(p => new ItemPlacement(new RandoItem { item = LM.GetItem(p.item) }, new RandoLocation { logic = LM.GetLogicDef(p.location) })));
                }
            }
            return placements;
        }

        ItemPlacementStrategy IItemRandomizerSettings.GetItemPlacementStrategy()
        {
            return new DefaultItemPlacementStrategy
            {
                depthPriorityTransform = (depth, location) => location.priority - 5 * depth,
                placementRecorder = (depth, ps) =>
                {
                    LogDebug("SPHERE " + depth);
                    foreach (var (item, location) in ps)
                    {
                        LogDebug($"{item} at {location}");
                    }
                    LogDebug("");
                }
            };
        }

        void IItemRandomizerSettings.PostPermuteItems(Random rng, IReadOnlyList<RandoItem> items)
        {
            bool majorPenalty = gs.CursedSettings.LongerProgressionChains;

            if (!majorPenalty) return;

            for (int i = 0; i < items.Count; i++)
            {
                if (majorPenalty && (Data.GetItemDef(items[i].Name)?.majorItem ?? false))
                {
                    try
                    {
                        checked { items[i].priority += rng.Next(items.Count - i); }
                    }
                    catch (OverflowException)
                    {
                        items[i].priority = int.MaxValue;
                    }
                }
            }
        }

        void IItemRandomizerSettings.PostPermuteLocations(Random rng, IReadOnlyList<RandoLocation> locations)
        {
            bool shopPenalty = true;

            if (!shopPenalty) return;

            HashSet<string> shops = new();
            for (int i = 0; i < locations.Count; i++)
            {
                if (shopPenalty && (Data.GetLocationDef(locations[i].Name)?.multi ?? false))
                {
                    // shops keep their lowest priority slot, but all other slots are moved to the end.
                    if (!shops.Add(locations[i].Name)) locations[i].priority = Math.Max(locations[i].priority, locations.Count);
                }
            }
        }

        TransitionPlacementStrategy ITransitionRandomizerSettings.GetTransitionPlacementStrategy()
        {
            DefaultTransitionPlacementStrategy tps = gs.TransitionSettings.ConnectAreas // && gs.TransitionSettings.Mode == TransitionSettings.TransitionMode.RoomRandomizer
                ? new ConnectedAreaTPS() : new DefaultTransitionPlacementStrategy();

            tps.depthPriorityTransform = DepthPriorityTransform;
            tps.placementRecorder = PlacementRecorder;
            return tps;
        }

        private int DepthPriorityTransform(int depth, RandoTransition t)
        {
            return t.priority - depth;
        }

        private void PlacementRecorder(int depth, List<TransitionPlacement> ps)
        {
            LogDebug("SPHERE " + depth);
            foreach (var (source, target) in ps)
            {
                LogDebug($"{source.Name}-->{target.Name}");
            }
            LogDebug("");
        }



        private class ConnectedAreaTPS : DefaultTransitionPlacementStrategy
        {
            private readonly struct AreaTransition
            {
                public AreaTransition(RandoTransition t)
                {
                    transition = t;
                    areaName = Data.GetTransitionDef(t.Name).areaName;
                }

                public readonly RandoTransition transition;
                public readonly string areaName;
            }

            public override List<TransitionPlacement> Export(TransitionInitializer ti, IList<TransitionSphere> spheres)
            {
                List<TransitionPlacement> placements = new();
                List<TransitionPlacement> current = new();
                List<AreaTransition> reachable = new();

                RandoTransition SelectSource(RandoTransition target)
                {
                    string area = Data.GetTransitionDef(target.Name).areaName;
                    RandoTransition t2 = null;
                    int i = -1;
                    for (int j = 0; j < reachable.Count; j++)
                    {
                        if (reachable[j].transition.targetDir == target.dir)
                        {
                            if (reachable[j].areaName == area)
                            {
                                LogDebug($"Found same area source {reachable[j].transition.Name} for {target.Name}");
                                t2 = reachable[j].transition;
                                i = j;
                                break;
                            }
                            else if (t2 == null)
                            {
                                t2 = reachable[j].transition;
                                i = j;
                            }
                        }
                    }

                    if (i >= 0) reachable.RemoveAt(i);
                    return t2 ?? throw new RandomizerCore.Exceptions.OutOfLocationsException("Ran out of transitions during Export.");
                }


                for (int i = 0; i < spheres.Count; i++)
                {
                    foreach (RandoTransition t in spheres[i].reachableTransitions)
                    {
                        if (depthPriorityTransform != null)
                        {
                            t.priority = depthPriorityTransform(i, t);
                        }

                        reachable.Add(new AreaTransition(t));
                    }
                    reachable.Sort((t, u) => t.transition.priority - u.transition.priority);

                    foreach (RandoTransition target in spheres[i].placedTransitions)
                    {
                        RandoTransition source = SelectSource(target);
                        current.Add(new(source, target));
                        if (target.coupled || source.coupled)
                        {
                            current.Add(new(target, source));
                        }
                    }

                    placementRecorder?.Invoke(i, current);
                    placements.AddRange(current);
                    current.Clear();
                }

                // This cleans up unpaired reachable source transitions
                // There shouldn't be any decoupled transitions left over at this point
                while (reachable.Count > 0)
                {
                    RandoTransition t1 = reachable[0].transition;
                    reachable.RemoveAt(0);
                    RandoTransition t2 = SelectSource(t1);

                    current.Add(new(t2, t1));
                    if (t2.coupled || t1.coupled)
                    {
                        current.Add(new(t1, t2));
                    }
                }

                if (placementRecorder != null)
                {
                    placementRecorder.Invoke(spheres.Count, current);
                }

                placements.AddRange(current);

                return placements;


            }
        }


        void ITransitionRandomizerSettings.PostPermuteTransitions(Random rng, IReadOnlyList<RandoTransition> transitions)
        {
            if (gs.TransitionSettings.ConnectAreas)
            {
                Dictionary<string, int> areaOrder = new();
                for (int i = 0; i < transitions.Count; i++)
                {
                    string area = Data.GetTransitionDef(transitions[i].Name).areaName ?? string.Empty;
                    if (!areaOrder.TryGetValue(area, out int modifier)) areaOrder.Add(area, modifier = areaOrder.Count);
                    transitions[i].priority += modifier * 10; 
                    // weakly group transitions by area in the order
                    // so that selector eliminates one area before moving onto the next
                    // "weakly", since we ideally do not want to prevent bottlenecked layouts like vanilla city
                }
            }

            for (int i = 0; i < transitions.Count; i++)
            {
                LogDebug($"{transitions[i].priority}--{transitions[i].Name}");
            }


            return;
        }
    }
}
