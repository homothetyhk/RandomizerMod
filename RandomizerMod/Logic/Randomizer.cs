using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RandomizerMod.Extensions;
using RandomizerMod.Settings;
using static RandomizerMod.LogHelper;
//using ItemChanger;

namespace RandomizerMod.Logic
{
    public class Randomizer
    {
        readonly LogicItem[] _baseItems;
        readonly RandoLocation[] _baseLocations;
        readonly LogicManager _lm;
        readonly GenerationSettings _gs;

        private List<LogicItem> items;
        private List<PriorityLocation> locations;
        private List<PriorityLocation> unreachableLocations;

        private List<List<LogicItem>> orderedItems;
        private List<List<PriorityLocation>> orderedLocations;

        private List<Stopwatch> ascent = new List<Stopwatch>();
        private List<Stopwatch> descent = new List<Stopwatch>();
        private List<int> peak = new List<int>();

        public List<RandoPlacement> placements;

        int bookmark = 0;

        Random rng;
        ProgressionManager pm;

        public Randomizer(LogicItem[] items, RandoLocation[] locations, LogicManager lm, GenerationSettings gs)
        {
            _baseItems = items;
            _baseLocations = locations;
            _lm = lm;
            _gs = gs;

            /*
            Log("Items");
            foreach (var item in items) Log(item.name);
            Log();
            Log("Locations");
            foreach (var loc in locations) Log(loc.name);
            */

            this.items = _baseItems.ToList();

            orderedItems = new List<List<LogicItem>>();
            orderedLocations = new List<List<PriorityLocation>>();
        }

        public void SortLocations()
        {
            List<RandoLocation> singleLocations = _baseLocations.Where(l => !l.multi).ToList();
            List<RandoLocation> multiLocations = _baseLocations.Where(l => l.multi).ToList();

            RandoLocation[] multiNames = multiLocations.GroupBy(l => l.name).Select(g => g.First()).ToArray();
            int[] multiCounts = new int[multiNames.Length];

            int total = _baseItems.Length - singleLocations.Count;
            if (total < multiNames.Length) throw new InvalidOperationException("Not enough items to fill all locations!");

            for (int i = 0; i < total; i++)
            {
                if (i < multiCounts.Length) multiCounts[i]++;
                else multiCounts[rng.Next(multiCounts.Length)]++;
            }

            Log("Randomized multi counts:");
            for (int i = 0; i < multiCounts.Length; i++)
            {
                Log($" {multiNames[i].name}: {multiCounts[i]}");
            }

            bool antiShopWeight = true;
            if (antiShopWeight)
            {
                singleLocations.AddRange(multiNames.Select(l => NewVendorLocation(l)));
                // move all but first location to end of the list
                List<RandoLocation> excess = multiCounts.SelectMany((count, index)
                => NewVendorLocations(multiNames[index], count - 1)).ToList();

                rng.PermuteInPlace(singleLocations);
                rng.PermuteInPlace(excess);
                singleLocations.AddRange(excess);
            }
            else
            {
                singleLocations.AddRange(multiCounts.SelectMany((count, index)
                => NewVendorLocations(multiNames[index], count)));
                rng.PermuteInPlace(singleLocations);
            }

            locations = singleLocations.Select((l, i) => new PriorityLocation(l, i)).ToList();
            foreach (var loc in locations)
            {
                switch (loc.name)
                {
                    case "Grubfather":
                        loc.location.AddCost(LogicCost.NewGrubCost(
                            _lm,
                            rng.Next(_gs.GrubCostRandomizerSettings.MinimumGrubCost, _gs.GrubCostRandomizerSettings.MaximumGrubCost + 1)));
                        break;
                    case "Seer":
                        loc.location.AddCost(LogicCost.NewEssenceCost(
                            _lm,
                            rng.Next(_gs.EssenceCostRandomizerSettings.MinimumEssenceCost, _gs.EssenceCostRandomizerSettings.MaximumEssenceCost + 1)));
                        break;
                }
            }
        }

        public RandoLocation NewVendorLocation(RandoLocation template)
        {
            var rl = template.Clone();
            switch (rl.name)
            {
                case "Grubfather":
                    rl.AddCost(LogicCost.NewGrubCost(
                        _lm,
                        rng.Next(_gs.GrubCostRandomizerSettings.MinimumGrubCost, _gs.GrubCostRandomizerSettings.MaximumGrubCost + 1)));
                    break;
                case "Seer":
                    rl.AddCost(LogicCost.NewEssenceCost(
                        _lm,
                        rng.Next(_gs.EssenceCostRandomizerSettings.MinimumEssenceCost, _gs.EssenceCostRandomizerSettings.MaximumEssenceCost + 1)));
                    break;
                default:
                    // geo cost
                    break;
            }
            return rl;
        }
        public IEnumerable<RandoLocation> NewVendorLocations(RandoLocation template, int count)
        {
            for (int i = 0; i < count; i++) yield return NewVendorLocation(template);
        }

        public void Randomize(int seed)
        {
            Log("Beginning randomization...");
            Log();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            rng = new Random(seed);
            pm = new ProgressionManager(_lm, _gs);
            pm.Add(_lm.Waypoints[0].item);
            items = rng.Permute((IEnumerable<LogicItem>)_baseItems);
            SortLocations();

            orderedLocations.Add(locations.Where(l => l.CanGet(pm)).ToList());
            unreachableLocations = locations.Where(l => !l.CanGet(pm)).ToList();

            Sort();
            Place();

            watch.Stop();
            Log($"Time elapsed: {watch.Elapsed}");
            for (int i = 0; i < descent.Count; i++)
            {
                Log($"Pass {i}");
                Log($"   Total items considered: {peak[i]}");
                Log($"   Time adding: {ascent[i].Elapsed}");
                Log($"   Time removing: {descent[i].Elapsed}");
            }
            for (int i = descent.Count; i < ascent.Count; i++)
            {
                Log($"Pass {i}");
                Log($"   Total items considered: {peak[i]}");
                Log($"   Time adding: {ascent[i].Elapsed}");
            }

            TimeSpan z = TimeSpan.Zero;
            for (int i = 0; i < descent.Count; i++)
            {
                z += ascent[i].Elapsed;
                z += descent[i].Elapsed;
            }
            for (int i = descent.Count; i < ascent.Count; i++)
            {
                z += ascent[i].Elapsed;
            }

            Log($"Cumulative time in passes: {z}");
            Log($"Logic evaluations: {LogicDef.logicEvaluations}");
            Log($"PM flag reads: {LogicDef.flagReads}");
            Log($"Logic time: {LogicDef.stopwatch.Elapsed}");
        }


        public void Sort()
        {
            while (items.Any())
            {
                ForceNext(out List<LogicItem> newItems, out List<PriorityLocation> newLocations);
                orderedItems.Add(newItems);
                if (newLocations.Count != 0) orderedLocations.Add(newLocations);
                else if (unreachableLocations.Count != 0)
                {
                    throw new Exception("Unreachable locations detected!");
                }
            }
        }

        public void ForceNext(out List<LogicItem> newItems, out List<PriorityLocation> newLocations)
        {
            int i = 0;
            newItems = new List<LogicItem>();
            newLocations = new List<PriorityLocation>();

            Stopwatch asc = Stopwatch.StartNew();

            Log("Beginning pass...");

            if (unreachableLocations.Count == 0)
            {
                Log($"All locations reachable. Auto adding items: {string.Join(", ", items.Select(item => item.name).ToArray())}");
                pm.Add(items);
                newItems.AddRange(items);
                items.Clear();
                return;
            }

            pm.AddTemp(items.Take(bookmark));
            newItems.AddRange(items.Take(bookmark));
            newLocations.AddRange(unreachableLocations.Where(l => l.CanGet(pm)));
            if (bookmark > 0) Log($"Batch-added items: {string.Join(", ", items.Take(bookmark).Select(item => item.name).ToArray())}");

            if (newLocations.Count == 0)
            {
                for (i = bookmark; i < items.Count; i++)
                {
                    pm.AddTemp(items[i]);
                    newItems.Add(items[i]);
                    newLocations.AddRange(unreachableLocations.Where(l => l.CanGet(pm)));

                    string s = newLocations.Count > 0 ? $"  {items[i].name}: unlocked {newLocations.Count} locations!"
                        : $"  {items[i].name}";

                    Log(s);

                    if (newLocations.Count > 0)
                    {
                        break;
                    }
                }
            }
            else
            {
                for (i = bookmark - 1; i >= 0; i--)
                {
                    pm.Remove(items[i]);
                    if (newLocations.Any(l => l.CanGet(pm)))
                    {
                        newItems.RemoveAt(i);
                        Log($"  removed {items[i].name} from bookmark");
                        newLocations.RemoveAll(l => !l.CanGet(pm));
                    }
                    else
                    {
                        Log($"  kept {items[i].name} with bookmark");
                        pm.Add(items[i]);
                        break;
                    }
                }
            }

            asc.Stop();
            ascent.Add(asc);
            peak.Add(newItems.Count);
            Stopwatch des = Stopwatch.StartNew();

            if (newLocations.Count == 0)
            {
                throw new InvalidOperationException($"Unreachable locations found: {string.Join(", ", unreachableLocations.Select(l => l.name).ToArray())}");
            }


            // Most progression comes as a single item
            // Also, we know the last item was required
            // Thus, check the special case first!
            pm.RestrictTempTo(newItems[i]);
            if (newLocations.Any(l => l.CanGet(pm)))
            {
                Log($"  {newItems[i].name} is solo progression--early search terminate");
                items.RemoveAt(i);
                newItems.RemoveRange(0, newItems.Count - 1);
                newLocations.RemoveAll(l => !l.CanGet(pm));
            }
            else
            {
                pm.AddTemp(newItems.Take(newItems.Count - 1));
                for (; i >= 0; i--)
                {
                    pm.Remove(items[i]);
                    if (newLocations.Any(l => l.CanGet(pm)))
                    {
                        newItems.RemoveAt(i);
                        Log($"  removed {items[i].name} from newitems");
                        newLocations.RemoveAll(l => !l.CanGet(pm));
                    }
                    else
                    {
                        Log($"  kept {items[i].name} with newitems");
                        pm.Add(items[i]);
                        items.RemoveAt(i);
                    }
                }
            }

            unreachableLocations.RemoveAll(l => l.CanGet(pm));

            Log($"Progression chunk: {string.Join(", ", newItems.Select(item => item.name).ToArray())}");
            Log($"Unlocked locations: {string.Join(", ", newLocations.Select(loc => loc.name).ToArray())}");
            Log($"Current Geo: {pm.obtained[pm.Index("GEO")]}");
            Log($"Current Grubs: {pm.obtained[pm.Index("GRUBS")]}");
            Log($"Current Essence: {pm.obtained[pm.Index("ESSENCE")]}");
            pm.SaveTempItems();
            Log();

            des.Stop();
            descent.Add(des);
            bookmark = peak.Last() - newItems.Count;

            return;
        }

        public void Place()
        {
            PriorityQueue<PriorityLocation> queue = new PriorityQueue<PriorityLocation>();
            placements = new List<RandoPlacement>(orderedItems.Sum(l => l.Count));
            StringBuilder log = new StringBuilder();
            log.AppendLine();
            log.AppendLine("Randomization complete");
            log.AppendLine();
            log.AppendLine();

            if (orderedItems.Count != orderedLocations.Count) throw new ArgumentException("Randomizer.Place List counts not aligned!");

            int itemSum = orderedItems.Sum(i => i.Count);
            int locSum = orderedLocations.Sum(l => l.Count);
            if (itemSum != locSum) throw new ArgumentException("Randomizer.Place inner List counts not aligned!");


            int depthPriorityFactor = -5; // TODO: move to settings

            for (int i = 0; i < orderedItems.Count; i++)
            {
                foreach (PriorityLocation loc in orderedLocations[i]) queue.Enqueue(loc.priority + depthPriorityFactor * i, loc);
                foreach (LogicItem item in orderedItems[i])
                {
                    if (queue.Count == 0)
                    {
                        Log(log);
                        throw new Exception("Ran out of locations at placement " + placements.Count);
                    }
                    var loc = queue.ExtractMin();
                    placements.Add(new RandoPlacement(item, loc.location));
                    log.AppendLine($"{loc.priority}: {item.name} at {loc.name}");
                }
                log.AppendLine();
            }

            Log(log);
            //Export(placements);
        }

        /*
        public void Export(List<(LogicItem item, PriorityLocation location)> placements)
        {
            Finder.Load();

            DefaultShopItems defaultShopItems = GetDefaultShopItems();
            Dictionary<string, AbstractPlacement> export = new Dictionary<string, AbstractPlacement>();
            foreach (var (item, location) in placements)
            {
                if (!export.TryGetValue(location.name, out AbstractPlacement p))
                {
                    var l = Finder.GetLocation(location.name);
                    if (l != null)
                    {
                        p = l.Wrap();
                    }
                    else if (location.name == "Grubfather")
                    {
                        var chest = Finder.GetLocation(LocationNames.Grubberflys_Elegy) as ItemChanger.Locations.ContainerLocation;
                        var tablet = Finder.GetLocation(LocationNames.Mask_Shard_5_Grubs) as ItemChanger.Locations.PlaceableLocation;
                        if (chest == null || tablet == null)
                        {
                            Log("Error constructing Grubfather location!");
                            continue;
                        }

                        p = new ItemChanger.Placements.CostChestPlacement
                        {
                            chestLocation = chest,
                            tabletLocation = tablet,
                        };
                    }
                    else if (location.name == "Seer")
                    {
                        var chest = Finder.GetLocation(LocationNames.Awoken_Dream_Nail) as ItemChanger.Locations.ContainerLocation;
                        var tablet = Finder.GetLocation(LocationNames.Hallownest_Seal_Seer) as ItemChanger.Locations.PlaceableLocation;
                        if (chest == null || tablet == null)
                        {
                            Log("Error constructing Grubfather location!");
                            continue;
                        }

                        p = new ItemChanger.Placements.CostChestPlacement
                        {
                            chestLocation = chest,
                            tabletLocation = tablet,
                        };
                    }
                    else
                    {
                        Log($"Location {location.name} did not correspond to any ItemChanger location!");
                        continue;
                    }

                    if (p is ItemChanger.Placements.ShopPlacement sp) sp.defaultShopItems = defaultShopItems;
                    export.Add(p.Name, p);
                }

                var i = Finder.GetItem(item.name);
                if (i == null)
                {
                    Log($"Item {item.name} did not correspond to any ItemChanger item!");
                    continue;
                }
                if (location.location.costs != null)
                {
                    Cost c = location.location.costs.Aggregate<LogicCost, Cost>(null, (d, e) => d + e.ToRealCost());
                    if (location.location.multi) i.AddTag<CostTag>().Cost = c;
                    else if (p is ItemChanger.Placements.ISingleCostPlacement iscp) iscp.Cost += c;
                    else i.AddTag<CostTag>().Cost = c;
                }
            }
        }

        public ItemChanger.DefaultShopItems GetDefaultShopItems()
        {
            DefaultShopItems items = DefaultShopItems.None;

            items |= DefaultShopItems.IseldaMapPins;
            items |= DefaultShopItems.IseldaMapMarkers;
            items |= DefaultShopItems.SalubraBlessing;

            if (!_gs.PoolSettings.Keys)
            {
                items |= DefaultShopItems.SlyLantern;
                items |= DefaultShopItems.SlySimpleKey;
                items |= DefaultShopItems.SlyKeyElegantKey;
            }

            if (!_gs.PoolSettings.Charms)
            {
                items |= DefaultShopItems.SlyCharms;
                items |= DefaultShopItems.SlyKeyCharms;
                items |= DefaultShopItems.IseldaCharms;
                items |= DefaultShopItems.SalubraCharms;
                items |= DefaultShopItems.LegEaterCharms;
                items |= DefaultShopItems.LegEaterRepair;
            }

            if (!_gs.PoolSettings.Maps)
            {
                items |= DefaultShopItems.IseldaQuill;
                items |= DefaultShopItems.IseldaMaps;
            }

            if (!_gs.PoolSettings.MaskShards)
            {
                items |= DefaultShopItems.SlyMaskShards;
            }

            if (!_gs.PoolSettings.VesselFragments)
            {
                items |= DefaultShopItems.SlyVesselFragments;
            }

            if (!_gs.PoolSettings.RancidEggs)
            {
                items |= DefaultShopItems.SlyRancidEgg;
            }

            return items;
        }
        */
    }

    public class PriorityQueue<T> : IEnumerable<T>
    {
        private readonly SortedDictionary<int, Queue<T>> dict;

        public int Count { get; private set; } = 0;

        public PriorityQueue()
        {
            dict = new SortedDictionary<int, Queue<T>>();
        }

        public void Enqueue(int priority, T t)
        {
            if (dict.TryGetValue(priority, out Queue<T> q)) q.Enqueue(t);
            else
            {
                q = new Queue<T>();
                q.Enqueue(t);
                dict.Add(priority, q);
            }

            Count++;
        }

        public T ExtractMin()
        {
            Queue<T> q = dict.Values.First();
            T t = q.Dequeue();
            Count--;
            if (q.Count == 0) dict.Remove(dict.Keys.First());
            return t;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return dict.SelectMany(p => p.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dict.SelectMany(p => p.Value).GetEnumerator();
        }
    }


}
