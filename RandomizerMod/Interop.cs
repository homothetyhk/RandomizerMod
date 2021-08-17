using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomizerMod.Logic;
using ItemChanger;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;
using static RandomizerMod.LogHelper;

namespace RandomizerMod
{
    public static class Interop
    {
        public static void Export(GenerationSettings gs, IEnumerable<RandoPlacement> randoPlacements)
        {
            DefaultShopItems defaultShopItems = GetDefaultShopItems(gs);
            Dictionary<string, AbstractPlacement> export = new Dictionary<string, AbstractPlacement>();
            foreach (var (item, location) in randoPlacements)
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

                        chest.name = tablet.name = "Grubfather";
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

                        chest.name = tablet.name = "Seer";
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
                if (location.costs != null)
                {
                    Cost c = location.costs.Aggregate<LogicCost, Cost>(null, (d, e) => d + GetCost(e));
                    if (location.multi) i.AddTag<CostTag>().Cost = c;
                    else if (p is ItemChanger.Placements.ISingleCostPlacement iscp) iscp.Cost += c;
                    else i.AddTag<CostTag>().Cost = c;
                }
                p.AddItem(i);
                Log($"Exported item {i.name} at placement {p.Name}");
            }

            ItemChangerMod.AddPlacements(export.Select(kvp => kvp.Value));
            Log("Exported all placements");

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
                Log($"Exported start location {def.name}");
            }
        }

        public static ItemChanger.Cost GetCost(LogicCost lc)
        {
            if (lc is GrubCost gc) return Cost.NewGrubCost(gc.cost);
            else if (lc is EssenceCost ec) return Cost.NewEssenceCost(ec.cost);

            throw new NotImplementedException($"Unknown LogicCost {lc}");
        }

        public static ItemChanger.DefaultShopItems GetDefaultShopItems(GenerationSettings gs)
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
