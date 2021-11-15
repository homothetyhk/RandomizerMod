using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;
using RandomizerCore;
using RandomizerCore.Extensions;
using RandomizerCore.Logic;
using RandomizerCore.Randomizers;
using static RandomizerMod.LogHelper;

namespace RandomizerMod.RC
{
    // cursed settings manipulation code
    // some settings are implemented through RandoController instead
    public class WrappedSettings : IRandomizerSettings, IItemRandomizerSettings, ITransitionRandomizerSettings
    {
        public WrappedSettings(GenerationSettings gs, RandoContext ctx)
        {
            if (!RCData.Loaded) RCData.Load();

            this.gs = gs;
            this.ctx = ctx;
            LM = ctx.LM;
        }

        List<RandoItem> items;
        List<RandoLocation> locations;

        void IRandomizerSettings.Initialize(Random rng)
        {
            GetStartItems(rng);
            SetRandomizedItems(rng);
            SetRandomizedLocations(rng);
            int diff = items.Count - locations.Count;
            if (diff > 0) PadLocations(rng, locations, diff);
            else if (diff < 0) PadItems(rng, items, -diff);
        }


        readonly GenerationSettings gs;
        readonly RandoContext ctx;

        public LogicManager LM { get; }

        int IRandomizerSettings.Seed => gs.Seed;

        bool ITransitionRandomizerSettings.Matched => gs.TransitionSettings.Matched;
        bool ITransitionRandomizerSettings.Coupled => gs.TransitionSettings.Coupled;

        void IRandomizerSettings.ApplySettings(ProgressionManager pm)
        {
            foreach (string setting in Data.GetApplicableLogicSettings(gs))
            {
                pm.Set(LM.GetTerm(setting).Id, 1);
            }

            var mode = gs.TransitionSettings.GetLogicMode();
            StartDef start = Data.GetStartDef(gs.StartLocationSettings.StartLocation);

            if (mode != LogicMode.Room) pm.Set(LM.GetTerm(start.waypoint).Id, 1);
            if (mode == LogicMode.Area) pm.Set(LM.GetTerm(start.areaTransition).Id, 1);
            if (mode == LogicMode.Room) pm.Set(LM.GetTerm(start.roomTransition).Id, 1);

            // use these baseline numbers for cursed settings and add shards/notches as vanilla items at start if necessary
            pm.Set(LM.GetTerm("MASKSHARDS").Id, 4); 
            pm.Set(LM.GetTerm("NOTCHES").Id, 1);
        }

        void GetStartItems(Random rng)
        {
            ctx.itemPlacements ??= new();

            void PlaceAtStart(RandoItem item)
            {
                ctx.itemPlacements.Add(new(item, new() { logic = LM.GetLogicDef("Start") }));
            }

            if (gs.StartItemSettings.MaximumStartGeo > 0)
            {
                PlaceAtStart(new CustomGeoItem(LM, rng.Next(gs.StartItemSettings.MinimumStartGeo, gs.StartItemSettings.MaximumStartGeo + 1)));
            }

            if (gs.NoveltySettings.RandomizeNail)
            {
                PlaceAtStart(new() { item = LM.GetItem("Downslash") });
            }

            // TODO: add remaining start items, and remove corresponding randomized items
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

            if (gs.PoolSettings.GrimmkinFlames && gs.PoolSettings.Charms)
            {
                int grimmchildCount = items.RemoveAll(i => i.Name == "Grimmchild2");
                for (int i = 0; i < grimmchildCount; i++) items.Add(new RandoItem { item = LM.GetItem("Grimmchild1") });
            }

            HashSet<string> removeItems = new();
            if (gs.CursedSettings.RemoveSpellUpgrades)
            {
                removeItems.Add("Abyss_Shriek");
                removeItems.Add("Shade_Soul");
                removeItems.Add("Descending_Dark");
            }

            if (gs.NoveltySettings.SplitClaw)
            {
                removeItems.Add("Mantis_Claw");
            }

            if (gs.NoveltySettings.SplitCloak)
            {
                removeItems.Add("Mothwing_Cloak");
                removeItems.Add("Shade_Cloak");
                bool leftBiased = rng.NextBool();
                if (!leftBiased) // the serialized Split Shade Cloak template is left biased.
                {
                    int index = items.FindIndex(i => i.Name == "Split_Shade_Cloak");
                    items[index].item = ((SplitCloakItem)items[index].item) with { LeftBiased = false };
                }
            }

            switch (gs.LongLocationSettings.RandomizationInWhitePalace)
            {
                case LongLocationSettings.WPSetting.ExcludeWhitePalace:
                    removeItems.Add("Soul_Totem-Palace");
                    removeItems.Add("Lore_Tablet-Palace_Workshop");
                    removeItems.Add("Lore_Tablet-Palace_Throne");
                    goto case LongLocationSettings.WPSetting.ExcludePathOfPain;
                case LongLocationSettings.WPSetting.ExcludePathOfPain:
                    removeItems.Add("Soul_Totem-Path_of_Pain");
                    removeItems.Add("Journal_Entry-Seal_of_Binding");
                    removeItems.Add("Lore_Tablet-Path_of_Pain_Entrance");
                    break;
            }

            if (gs.MiscSettings.AddDuplicateItems)
            {
                // TODO: better dupe settings
                List<string> dupes = new()
                {
                    "Vengeful_Spirit",
                    "Howling_Wraiths",
                    "Desolate_Dive",
                    "Mothwing_Cloak",
                    "Mantis_Claw",
                    "Crystal_Heart",
                    "Isma's_Tear",
                    "Monarch_Wings",
                    "Shade_Cloak",
                    "Dreamer",
                    "Void_Heart",
                    "Dream_Nail",
                };

                if (gs.NoveltySettings.SplitClaw) dupes.Remove("Mantis_Claw");
                if (gs.NoveltySettings.SplitCloak)
                {
                    dupes.Remove("Mothwing_Cloak");
                    dupes.Remove("Shade_Cloak");
                }
                if (gs.CursedSettings.RemoveSpellUpgrades)
                {
                    dupes.Remove("Vengeful_Spirit");
                    dupes.Remove("Howling_Wraiths");
                    dupes.Remove("Desolate_Dive");
                }

                foreach (string dupe in dupes) items.Add(new PlaceholderItem(new RandoItem { item = LM.GetItem(dupe) }));

                // dupe simple keys which are logic readable to increase odds of 4 keys waterways
                items.Add(new RandoItem { item = LM.GetItem("Simple_Key") });
                items.Add(new RandoItem { item = LM.GetItem("Simple_Key") });
            }

            if (gs.MiscSettings.MaskShards != MiscSettings.MaskShardType.FourShardsPerMask)
            {
                int maskShards = items.RemoveAll(i => i.Name == "Mask_Shard");
                if (gs.MiscSettings.MaskShards == MiscSettings.MaskShardType.TwoShardsPerMask)
                {
                    int doubleShards = maskShards / 2;
                    int singleShards = maskShards - 2 * doubleShards;
                    for (int i = 0; i < doubleShards; i++) items.Add(new RandoItem { item = LM.GetItem("Double_Mask_Shard") });
                    for (int i = 0; i < singleShards; i++) items.Add(new RandoItem { item = LM.GetItem("Mask_Shard") });
                }
                else if (gs.MiscSettings.MaskShards == MiscSettings.MaskShardType.OneShardPerMask)
                {
                    int fullmasks = maskShards / 4;
                    int singleShards = maskShards - 4 * fullmasks;
                    for (int i = 0; i < fullmasks; i++) items.Add(new RandoItem { item = LM.GetItem("Full_Mask") });
                    for (int i = 0; i < singleShards; i++) items.Add(new RandoItem { item = LM.GetItem("Mask_Shard") });
                }
            }

            if (gs.MiscSettings.VesselFragments != MiscSettings.VesselFragmentType.ThreeFragmentsPerVessel)
            {
                int vesselFragments = items.RemoveAll(i => i.Name == "Vessel_Fragment");
                if (gs.MiscSettings.VesselFragments == MiscSettings.VesselFragmentType.TwoFragmentsPerVessel)
                {
                    int doubleFragments = vesselFragments / 2;
                    int singleFragments = vesselFragments - 2 * doubleFragments;
                    for (int i = 0; i < doubleFragments; i++) items.Add(new RandoItem { item = LM.GetItem("Double_Vessel_Fragment") });
                    for (int i = 0; i < singleFragments; i++) items.Add(new RandoItem { item = LM.GetItem("Vessel_Fragment") });
                }
                else if (gs.MiscSettings.VesselFragments == MiscSettings.VesselFragmentType.OneFragmentPerVessel)
                {
                    int fullVessels = vesselFragments / 3;
                    int singleFragments = vesselFragments - 3 * fullVessels;
                    for (int i = 0; i < fullVessels; i++) items.Add(new RandoItem { item = LM.GetItem("Full_Soul_Vessel") });
                    for (int i = 0; i < singleFragments; i++) items.Add(new RandoItem { item = LM.GetItem("Vessel_Fragment") });
                }
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

            items.RemoveAll(i => removeItems.Contains(i.Name));
        }

        private void PadItems(Random rng, List<RandoItem> items, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (rng.Next(4) == 0) items.Add(new RandoItem { item = LM.GetItem("Lumafly_Escape") });
                else items.Add(new RandoItem { item = LM.GetItem("One_Geo") });
            }
        }

        List<RandoItem> IItemRandomizerSettings.GetRandomizedItems() => items ?? throw new InvalidOperationException("Randomized items were not initialized.");

        List<string> multiNames;

        private RandoLocation GetLocation(string name, Random rng)
        {
            string lookupName = name switch
            {
                "Salubra_(Requires_Charms)" => "Salubra",
                _ => name
            };

            RandoLocation rl = new()
            {
                logic = LM.GetLogicDef(lookupName) ?? throw new ArgumentException($"No logic found for location {lookupName}!")
            };

            if (Data.TryGetCost(name, out CostDef def))
            {
                switch (def.term)
                {
                    case "ESSENCE":
                    case "GRUBS":
                    case "SIMPLE":
                        rl.AddCost(new SimpleCost(LM.GetTerm("SIMPLE"), 1));
                        break;
                    case "Spore_Shroom":
                        rl.AddCost(new SimpleCost(LM.GetTerm("Spore_Shroom"), 1));
                        break;
                    case "GEO":
                        rl.AddCost(new LogicGeoCost(LM, def.amount));
                        break;
                    default:
                        rl.AddCost(new SimpleCost(LM.GetTerm(def.term), def.amount));
                        break;
                }
            }

            switch (name)
            {
                case "Grubfather":
                    rl.AddCost(new SimpleCost(LM.GetTerm("GRUBS"), rng.Next(gs.CostSettings.MinimumGrubCost, gs.CostSettings.MaximumGrubCost + 1)));
                    break;
                case "Seer":
                    rl.AddCost(new SimpleCost(LM.GetTerm("ESSENCE"), rng.Next(gs.CostSettings.MinimumEssenceCost, gs.CostSettings.MaximumEssenceCost + 1)));
                    break;
                case "Egg_Shop":
                    rl.AddCost(new SimpleCost(LM.GetTerm("RANCIDEGGS"), rng.Next(gs.CostSettings.MinimumEggCost, gs.CostSettings.MaximumEggCost + 1)));
                    break;
                case "Salubra_(Requires_Charms)":
                    rl.AddCost(new SimpleCost(LM.GetTerm("CHARMS"), rng.Next(gs.CostSettings.MinimumCharmCost, gs.CostSettings.MaximumCharmCost + 1)));
                    goto case "Salubra";
                case "Sly":
                case "Sly_(Key)":
                case "Iselda":
                case "Salubra":
                case "Leg_Eater":
                    rl.AddCost(new LogicGeoCost(LM, -1));
                    break;
            }

            return rl;
        }

        void SetRandomizedLocations(Random rng)
        {
            List<string> locationNames = new();

            foreach (var pool in Data.Pools)
            {
                if (pool.IsIncluded(gs))
                {
                    locationNames.AddRange(pool.includeLocations);
                }
            }
            if (gs.NoveltySettings.SplitClaw) locationNames.RemoveAll(l => l == "Mantis_Claw");
            if (gs.LongLocationSettings.RandomizationInWhitePalace == LongLocationSettings.WPSetting.ExcludeWhitePalace)
            {
                locationNames.RemoveAll(l => (Data.GetLocationDef(l).areaName == "White_Palace" || Data.GetLocationDef(l).areaName == "Path_of_Pain") && l != "King_Fragment");
            }
            else if (gs.LongLocationSettings.RandomizationInWhitePalace == LongLocationSettings.WPSetting.ExcludePathOfPain)
            {
                locationNames.RemoveAll(l => Data.GetLocationDef(l).areaName == "Path_of_Pain" && l != "King_Fragment");
            }

            multiNames = locationNames.Where(l => Data.GetLocationDef(l).multi).Distinct().ToList();
            int multiCount = locationNames.RemoveAll(l => Data.GetLocationDef(l).multi);

            locationNames.AddRange(multiNames);
            for (int i = multiNames.Count; i < multiCount; i++)
            {
                locationNames.Add(rng.Next(multiNames));
            }

            locations = locationNames.Select(l => GetLocation(l, rng)).ToList();
        }

        private int GetShopCost(Random rng, string itemName, bool required)
        {
            double pow = 1.2; // setting?

            int cap = Data.GetItemDef(itemName).priceCap;
            if (cap <= 100) return cap;
            if (required) return rng.PowerLaw(pow, 100, Math.Min(cap, 500)).ClampToMultipleOf(5);
            return rng.PowerLaw(pow, 100, cap).ClampToMultipleOf(5);
        }

        public void Finalize(Random rng)
        {
            UnwrapPlaceholders();
            FinalizeLocationCosts(rng);
        }

        private void FinalizeLocationCosts(Random rng)
        {
            int nonrequiredCount = spheredPlacements[spheredPlacements.Count - 1].Item2.Count;
            int threshold = ctx.itemPlacements.Count - nonrequiredCount;

            for (int i = 0; i < ctx.itemPlacements.Count; i++)
            {
                (RandoItem ri, RandoLocation rl) = ctx.itemPlacements[i];
                if (rl.costs == null) continue;
                bool required = i < threshold;
                foreach (LogicGeoCost gc in rl.costs.OfType<LogicGeoCost>())
                {
                    if (gc.GeoAmount < 0) gc.GeoAmount = GetShopCost(rng, ri.Name, required);
                }
            }
        }

        private void UnwrapPlaceholders()
        {
            for (int i = 0; i < ctx.itemPlacements.Count; i++)
            {
                (RandoItem item, RandoLocation loc) = ctx.itemPlacements[i];
                if (item is PlaceholderItem pi)
                {
                    ctx.itemPlacements[i] = new(pi.Unwrap(), loc);
                }
            }
        }

        private void PadLocations(Random rng, List<RandoLocation> locations, int count)
        {
            for (int i = 0; i < count; i++) locations.Add(GetLocation(rng.Next(multiNames), rng));
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

        public List<ItemPlacement> GetVanillaPlacements()
        {
            List<ItemPlacement> placements = new();
            foreach (PoolDef pool in Data.Pools)
            {
                if (pool.IsVanilla(gs))
                {
                    switch (pool.name)
                    {
                        case "Flame" when gs.PoolSettings.Charms:
                            placements.AddRange(pool.vanilla.Skip(6).Select(p => new ItemPlacement(new RandoItem { item = LM.GetItem(p.item) }, new RandoLocation { logic = LM.GetLogicDef(p.location) })));
                            break;
                        default:
                            placements.AddRange(pool.vanilla.Select(p => new ItemPlacement(new RandoItem { item = LM.GetItem(p.item) }, new RandoLocation { logic = LM.GetLogicDef(p.location) })));
                            break;
                    }
                }
            }
            return placements;
        }


        public List<(ItemSphere sphere, IReadOnlyList<ItemPlacement>)> spheredPlacements = new();
        ItemPlacementStrategy IItemRandomizerSettings.GetItemPlacementStrategy()
        {
            return new DefaultItemPlacementStrategy
            {
                depthPriorityTransform = (depth, location) => location.priority - 5 * depth,
                placementRecorder = (sphere, ps) =>
                {
                    LogDebug("SPHERE " + sphere.index);
                    if (sphere.index == 0) spheredPlacements.Clear();
                    spheredPlacements.Add((sphere, ps));
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
            bool dupePenalty = true;

            if (!majorPenalty && !dupePenalty) return;

            for (int i = 0; i < items.Count; i++)
            {
                if (majorPenalty && (Data.GetItemDef(items[i].Name)?.majorItem ?? false))
                {
                    try
                    {
                        checked { items[i].priority += rng.Next(items.Count - i); } // makes major items more likely to be selected late in progression
                    }
                    catch (OverflowException)
                    {
                        items[i].priority = int.MaxValue;
                    }
                }

                if (dupePenalty && items[i] is PlaceholderItem)
                {
                    try
                    {
                        checked { items[i].priority -= 1000; } // makes dupes more likely to be placed immediately after progression, into late locations
                    }
                    catch (OverflowException)
                    {
                        items[i].priority = int.MinValue;
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

        private void PlacementRecorder(TransitionSphere sphere, List<TransitionPlacement> ps)
        {
            LogDebug(sphere != null ? "SPHERE " + sphere.index : "OVERFLOW");
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

                    placementRecorder?.Invoke(spheres[i], current);
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
                    TransitionSphere fakeSphere = new()
                    {
                        index = spheres.Count,
                        directionCounts = spheres[spheres.Count - 1].directionCounts,
                        placedTransitions = new(),
                        reachableTransitions = new(),
                    };
                    placementRecorder.Invoke(fakeSphere, current);
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
