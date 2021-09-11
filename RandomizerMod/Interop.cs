using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemChanger;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;
using static RandomizerMod.LogHelper;

namespace RandomizerMod
{
    public static class Interop
    {
        public static void ExportStart(GenerationSettings gs)
        {
            string startName = gs.StartLocationSettings.StartLocation;
            if (!string.IsNullOrEmpty(startName) && Data.GetStartDef(startName) is RandomizerData.StartDef def)
            {
                ItemChangerMod.ChangeStartGame(new ItemChanger.StartDef
                {
                    startSceneName = def.sceneName,
                    startX = def.x,
                    startY = def.y,
                    mapZone = (int)def.zone,
                });
            }
        }

        public static void ExportSettings(GenerationSettings gs)
        {
            if (gs.CursedSettings.RandomizeFocus)
            {
                ItemChanger.Internal.Ref.Settings.CustomSkills.canFocus = false;
            }

            if (gs.CursedSettings.RandomizeNail)
            {
                ItemChanger.Internal.Ref.Settings.CustomSkills.canSideslashLeft = false;
                ItemChanger.Internal.Ref.Settings.CustomSkills.canSideslashRight = false;
                ItemChanger.Internal.Ref.Settings.CustomSkills.canUpslash = false;
            }

            if (gs.CursedSettings.RandomizeSwim)
            {
                ItemChanger.Internal.Ref.Settings.CustomSkills.canSwim = false;
            }
        }

        public static void ExportItemPlacements(GenerationSettings gs, IEnumerable<RandomizerCore.ItemPlacement> randoPlacements)
        {
            DefaultShopItems defaultShopItems = GetDefaultShopItems(gs);
            Dictionary<string, AbstractPlacement> export = new Dictionary<string, AbstractPlacement>();
            foreach (var (item, location) in randoPlacements)
            {
                if (!export.TryGetValue(location.Name, out AbstractPlacement p))
                {
                    var l = Finder.GetLocation(location.Name);
                    if (l != null)
                    {
                        p = l.Wrap();
                    }
                    else if (location.Name == "Grubfather")
                    {
                        var chest = Finder.GetLocation(LocationNames.Grubberflys_Elegy) as ItemChanger.Locations.ContainerLocation;
                        var tablet = Finder.GetLocation(LocationNames.Mask_Shard_5_Grubs) as ItemChanger.Locations.PlaceableLocation;
                        if (chest == null || tablet == null)
                        {
                            Log("Error constructing Grubfather location!");
                            continue;
                        }

                        chest.name = tablet.name = "Grubfather";
                        p = new ItemChanger.Placements.CostChestPlacement
                        {
                            chestLocation = chest,
                            tabletLocation = tablet,
                        };
                    }
                    else if (location.Name == "Seer")
                    {
                        var chest = Finder.GetLocation(LocationNames.Awoken_Dream_Nail) as ItemChanger.Locations.ContainerLocation;
                        var tablet = Finder.GetLocation(LocationNames.Hallownest_Seal_Seer) as ItemChanger.Locations.PlaceableLocation;
                        if (chest == null || tablet == null)
                        {
                            Log("Error constructing Grubfather location!");
                            continue;
                        }

                        chest.name = tablet.name = "Seer";
                        p = new ItemChanger.Placements.CostChestPlacement
                        {
                            chestLocation = chest,
                            tabletLocation = tablet,
                        };
                    }
                    else
                    {
                        Log($"Location {location.Name} did not correspond to any ItemChanger location!");
                        continue;
                    }

                    if (p is ItemChanger.Placements.ShopPlacement sp) sp.defaultShopItems = defaultShopItems;
                    export.Add(p.Name, p);
                }

                var i = Finder.GetItem(item.Name);
                if (i == null)
                {
                    Log($"Item {item.Name} did not correspond to any ItemChanger item!");
                    continue;
                }
                if (location.costs != null)
                {
                    Cost c = null;
                    foreach (var lc in location.costs)
                    {
                        // TODO: move cost resolution to method
                        if (lc is RandomizerCore.Logic.SimpleCost sc)
                        {
                            switch (sc.term)
                            {
                                case "GRUBS":
                                    c += Cost.NewGrubCost(sc.threshold);
                                    break;
                                case "ESSENCE":
                                    c += Cost.NewEssenceCost(sc.threshold);
                                    break;
                                case "GEO":
                                    c += Cost.NewGeoCost(sc.threshold);
                                    break;
                                // TODO:
                                //case "SIMPLE":
                                //case "Spore_Shroom":
                                default:
                                    throw new ArgumentException($"Unknown term {sc.term} found in simple cost on location {location.Name} during Export.");
                            }
                        }
                        else
                        {
                            throw new ArgumentException($"Unknown cost {lc.GetType().Name} found on location {location.Name} during Export.");
                        }
                    }

                    i.GetOrAddTag<CostTag>().Cost += c;
                }
                p.AddItem(i);
            }

            ItemChangerMod.AddPlacements(export.Select(kvp => kvp.Value));
        }

        public static void ExportTransitionPlacements(IEnumerable<RandomizerCore.TransitionPlacement> ps)
        {
            foreach (var p in ps) ItemChangerMod.AddTransitionOverride(new Transition(p.source.lt.sceneName, p.source.lt.gateName), new Transition(p.target.lt.sceneName, p.target.lt.gateName));
        }


        public static DefaultShopItems GetDefaultShopItems(GenerationSettings gs)
        {
            DefaultShopItems items = DefaultShopItems.None;

            items |= DefaultShopItems.IseldaMapPins;
            items |= DefaultShopItems.IseldaMapMarkers;
            items |= DefaultShopItems.SalubraBlessing;

            if (!gs.PoolSettings.Keys)
            {
                items |= DefaultShopItems.SlyLantern;
                items |= DefaultShopItems.SlySimpleKey;
                items |= DefaultShopItems.SlyKeyElegantKey;
            }

            if (!gs.PoolSettings.Charms)
            {
                items |= DefaultShopItems.SlyCharms;
                items |= DefaultShopItems.SlyKeyCharms;
                items |= DefaultShopItems.IseldaCharms;
                items |= DefaultShopItems.SalubraCharms;
                items |= DefaultShopItems.LegEaterCharms;
                items |= DefaultShopItems.LegEaterRepair;
            }

            if (!gs.PoolSettings.Maps)
            {
                items |= DefaultShopItems.IseldaQuill;
                items |= DefaultShopItems.IseldaMaps;
            }

            if (!gs.PoolSettings.MaskShards)
            {
                items |= DefaultShopItems.SlyMaskShards;
            }

            if (!gs.PoolSettings.VesselFragments)
            {
                items |= DefaultShopItems.SlyVesselFragments;
            }

            if (!gs.PoolSettings.RancidEggs)
            {
                items |= DefaultShopItems.SlyRancidEgg;
            }

            return items;
        }
    }
}
