using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemChanger;
using SD = ItemChanger.Util.SceneDataUtil;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;
using static RandomizerMod.LogHelper;
using RandomizerCore;

namespace RandomizerMod.IC
{
    public static class Export
    {
        public static void BeginExport(GenerationSettings gs)
        {
            ItemChangerMod.CreateSettingsProfile(overwrite: true);
            ItemChangerMod.Modules.Add<RandomizerModule>();
            ItemChangerMod.Modules.Add<TrackerUpdate>();
            ItemChangerMod.Modules.Add<TrackerLog>();
            if (gs.MiscSettings.RandomizeNotchCosts) ItemChangerMod.Modules.Add<ItemChanger.Modules.NotchCostUI>();
        }


        public static void ExportStart(GenerationSettings gs)
        {
            string startName = gs.StartLocationSettings.StartLocation;
            if (!string.IsNullOrEmpty(startName) && Data.GetStartDef(startName) is RandomizerData.StartDef def)
            {
                ItemChangerMod.ChangeStartGame(new ItemChanger.StartDef
                {
                    SceneName = def.sceneName,
                    X = def.x,
                    Y = def.y,
                    MapZone = (int)def.zone,
                    SpecialEffects = SpecialStartEffects.Default | SpecialStartEffects.SlowSoulRefill, // TODO: identify which starts+modes don't need soul refill
                    RespawnFacingRight = true, // TODO: are there any starts which should face left?
                });
            }

            foreach (SmallPlatform p in PlatformList.GetPlatformList(gs)) ItemChangerMod.AddDeployer(p); 

            switch (startName)
            {
                // Platforms to allow escaping the Hive start regardless of difficulty or initial items
                case "Hive":
                    ItemChangerMod.AddDeployer(new SmallPlatform { SceneName = SceneNames.Hive_03, X = 58.5f, Y = 134f, });
                    ItemChangerMod.AddDeployer(new SmallPlatform { SceneName = SceneNames.Hive_03, X = 58.5f, Y = 138.5f, });
                    break;

                // Drop the vine platforms and add small platforms for jumping up.
                case "Far Greenpath":
                    ItemChangerMod.AddDeployer(new SmallPlatform { SceneName = SceneNames.Fungus1_13, X = 45f, Y = 16.5f });
                    ItemChangerMod.AddDeployer(new SmallPlatform { SceneName = SceneNames.Fungus1_13, X = 64f, Y = 16.5f });
                    SD.Save(SceneNames.Fungus1_13, "Vine Platform (1)");
                    SD.Save(SceneNames.Fungus1_13, "Vine Platform (2)");
                    break;

                // With the Lower Greenpath start, getting to the rest of Greenpath requires
                // cutting the vine to the right of the vessel fragment.
                case "Lower Greenpath":
                    if (gs.NoveltySettings.RandomizeNail) SD.Save(SceneNames.Fungus1_13, "Vine Platform");
                    break;
            }
        }

        public static void ExportItemPlacements(GenerationSettings gs, IReadOnlyList<ItemPlacement> randoPlacements)
        {
            DefaultShopItems defaultShopItems = GetDefaultShopItems(gs);
            Dictionary<string, AbstractPlacement> export = new();

            for(int j = 0; j < randoPlacements.Count; j++)
            {
                var (item, location) = randoPlacements[j];
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
                        p = new ItemChanger.Placements.CostChestPlacement("Grubfather")
                        {
                            chestLocation = chest,
                            tabletLocation = tablet,
                        };
                        p.AddTag<ItemChanger.Tags.DestroyGrubRewardTag>().destroyRewards = GetRandomizedGrubRewards(gs);
                    }
                    else if (location.Name == "Seer")
                    {
                        var chest = Finder.GetLocation(LocationNames.Vessel_Fragment_Seer) as ItemChanger.Locations.ContainerLocation;
                        var tablet = Finder.GetLocation(LocationNames.Hallownest_Seal_Seer) as ItemChanger.Locations.PlaceableLocation;
                        if (chest == null || tablet == null)
                        {
                            Log("Error constructing Seer location!");
                            continue;
                        }

                        chest.name = tablet.name = "Seer";
                        p = new ItemChanger.Placements.CostChestPlacement("Seer")
                        {
                            chestLocation = chest,
                            tabletLocation = tablet,
                        };
                        p.AddTag<ItemChanger.Tags.DestroySeerRewardTag>().destroyRewards = GetRandomizedSeerRewards(gs);
                    }
                    else
                    {
                        throw new ArgumentException($"Location {location.Name} did not correspond to any ItemChanger location!");
                    }

                    if (p is ItemChanger.Placements.ShopPlacement sp) sp.defaultShopItems = defaultShopItems;
                    p.AddTag<RandoPlacementTag>();
                    export.Add(p.Name, p);
                }

                var i = GetItem(item);
                if (i == null)
                {
                    throw new ArgumentException($"Item {item.Name} did not correspond to any ItemChanger item!");
                }
                if (item.Name == "Split_Shade_Cloak" && !((RC.SplitCloakItem)item.item).LeftBiased) // default is left biased
                {
                    i.GetTag<ItemChanger.Tags.ItemChainTag>().predecessor = ItemNames.Right_Mothwing_Cloak;
                }

                if (location.costs != null)
                {
                    CostConversion.HandleCosts(location.costs, i, p);
                }

                i.AddTag<RandoItemTag>().id = j;
                p.Add(i);
            }

            ItemChangerMod.AddPlacements(export.Select(kvp => kvp.Value));
        }

        private static AbstractItem GetItem(RandoItem item)
        {
            if (item is RC.CustomGeoItem geo)
            {
                return new ItemChanger.Items.AddGeoItem
                {
                    amount = geo.geo,
                    name = item.Name,
                    UIDef = null,
                };
            }
            else return Finder.GetItem(item.Name);
        }


        public static void ExportTransitionPlacements(IEnumerable<RandomizerCore.TransitionPlacement> ps)
        {
            foreach (var p in ps) ItemChangerMod.AddTransitionOverride(new Transition(p.source.lt.data.SceneName, p.source.lt.data.GateName), new Transition(p.target.lt.data.SceneName, p.target.lt.data.GateName));
        }

        public static GrubfatherRewards GetRandomizedGrubRewards(GenerationSettings gs)
        {
            GrubfatherRewards gr = GrubfatherRewards.None;
            if (gs.PoolSettings.Charms)
            {
                gr |= GrubfatherRewards.Grubsong | GrubfatherRewards.GrubberflysElegy;
            }
            if (gs.PoolSettings.MaskShards)
            {
                gr |= GrubfatherRewards.MaskShard;
            }
            if (gs.PoolSettings.PaleOre)
            {
                gr |= GrubfatherRewards.PaleOre;
            }
            if (gs.PoolSettings.Relics)
            {
                gr |= GrubfatherRewards.HallownestSeal | GrubfatherRewards.KingsIdol;
            }
            if (gs.PoolSettings.RancidEggs)
            {
                gr |= GrubfatherRewards.RancidEgg;
            }
            
            return gr;
        }

        public static SeerRewards GetRandomizedSeerRewards(GenerationSettings gs)
        {
            SeerRewards sr = SeerRewards.None;
            if (gs.PoolSettings.Relics)
            {
                sr |= SeerRewards.HallownestSeal | SeerRewards.ArcaneEgg;
            }
            if (gs.PoolSettings.PaleOre)
            {
                sr |= SeerRewards.PaleOre;
            }
            if (gs.PoolSettings.Charms)
            {
                sr |= SeerRewards.DreamWielder;
            }
            if (gs.PoolSettings.VesselFragments)
            {
                sr |= SeerRewards.VesselFragment;
            }
            if (gs.PoolSettings.Skills)
            {
                sr |= SeerRewards.DreamGate | SeerRewards.AwokenDreamNail;
            }
            if (gs.PoolSettings.MaskShards)
            {
                sr |= SeerRewards.MaskShard;
            }

            return sr;
        }

        public static DefaultShopItems GetDefaultShopItems(GenerationSettings gs)
        {
            DefaultShopItems items = DefaultShopItems.None;

            items |= DefaultShopItems.IseldaMapPins;
            items |= DefaultShopItems.IseldaMapMarkers;
            items |= DefaultShopItems.SalubraBlessing;
            items |= DefaultShopItems.LegEaterRepair;

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
