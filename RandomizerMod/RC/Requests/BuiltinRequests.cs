using RandomizerCore;
using ItemChanger;
using static RandomizerMod.RC.RequestBuilder;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;
using RandomizerCore.Extensions;
using RandomizerCore.Logic;
using System.Text.RegularExpressions;
using RandomizerCore.Randomization;

namespace RandomizerMod.RC
{
    public static class BuiltinRequests
    {
        public static void ApplyAll()
        {
            OnUpdate.Subscribe(-2000f, ApplyPlaceholderMatch);
            OnUpdate.Subscribe(-2000f, ApplyCustomGeoMatch);

            OnUpdate.Subscribe(-1000f, ApplyTransitionSettings);

            OnUpdate.Subscribe(-100f, ApplyShopDefs);
            OnUpdate.Subscribe(-100f, ApplyGrubfatherDef);
            OnUpdate.Subscribe(-100f, ApplySeerDef);
            OnUpdate.Subscribe(-100f, ApplyEggShopDef);
            OnUpdate.Subscribe(-100f, ApplySalubraCharmDef);

            OnUpdate.Subscribe(0f, ApplyPoolSettings);
            OnUpdate.Subscribe(0f, ApplyGrimmchildSetting);
            OnUpdate.Subscribe(0f, ApplySplitCloakShadeCloakRandomize);
            OnUpdate.Subscribe(1f, ApplyMultiLocationRebalancing);
            OnUpdate.Subscribe(2f, ApplyGrubMimicRando);

            OnUpdate.Subscribe(5f, ApplyStartGeo);
            OnUpdate.Subscribe(5f, ApplyDownslashStart);

            OnUpdate.Subscribe(15f, ApplySplitClawFullClawRemove);
            OnUpdate.Subscribe(15f, ApplySplitCloakFullCloakRemove);
            OnUpdate.Subscribe(15f, ApplySpellRemove);

            OnUpdate.Subscribe(16f, ApplyJunkItemRemove);

            // These must run after junk remove, because junk remove cannot detect modified items.
            OnUpdate.Subscribe(18f, ApplyMaskShardCountSetting);
            OnUpdate.Subscribe(18f, ApplyVesselFragmentCountSetting);

            OnUpdate.Subscribe(20f, ApplyDuplicateItemSettings);

            OnUpdate.Subscribe(30f, ApplyLongLocationSettings);


            OnUpdate.Subscribe(100f, ApplyItemPostPermuteEvents);
            OnUpdate.Subscribe(100f, ApplyDefaultItemPadding);
            OnUpdate.Subscribe(100f, ApplyTransitionPostPermuteEvents);
            OnUpdate.Subscribe(100f, ApplyTransitionPlacementStrategy);
        }

        public static void ApplyPlaceholderMatch(RequestBuilder rb)
        {
            const string prefix = "Placeholder-";
            static bool TryMatch(string name, out ItemRequestInfo info)
            {
                if (name.StartsWith(prefix))
                {
                    info = new ItemRequestInfo
                    {
                        randoItemCreator = factory => factory.MakeWrappedItem(name.Substring(prefix.Length))
                    };
                    return true;
                }
                else
                {
                    info = default;
                    return false;
                }
            }
            rb.ItemMatchers.Add(TryMatch);
        }

        public static void ApplyCustomGeoMatch(RequestBuilder rb)
        {
            static bool TryMatch(string name, out ItemRequestInfo info)
            {
                if (name.EndsWith("_Geo") && int.TryParse(name.Substring(0, name.IndexOf('_')), out int value))
                {
                    info = new ItemRequestInfo
                    {
                        randoItemCreator = factory => new CustomGeoItem(factory.lm, value),
                        realItemCreator = (factory, placement) =>
                        {
                            CustomGeoItem geoItem = (CustomGeoItem)placement.Item;
                            return ItemChanger.Items.SpawnGeoItem.MakeGeoItem(geoItem.geo);
                        }
                    };
                    return true;
                }
                else
                {
                    info = default;
                    return false;
                }
            }
            rb.ItemMatchers.Add(TryMatch);
        }

        public static void ApplyTransitionSettings(RequestBuilder rb)
        {
            TransitionSettings ts = rb.gs.TransitionSettings;

            if (ts.Mode == TransitionSettings.TransitionMode.None) return;
            StageBuilder sb = rb.AddStage(RBConsts.MainTransitionStage);
            IEnumerable<TransitionDef> transitions = (ts.Mode == TransitionSettings.TransitionMode.AreaRandomizer ? Data.GetAreaTransitionNames() : Data.GetRoomTransitionNames())
                .Select(t => Data.GetTransitionDef(t));

            TransitionGroupBuilder oneWays = new()
            {
                label = RBConsts.OneWayGroup,
                stageLabel = RBConsts.MainTransitionStage,
            };

            if (ts.Matched)
            {
                SymmetricTransitionGroupBuilder horizontal = new()
                {
                    label = RBConsts.InLeftOutRightGroup,
                    reverseLabel = RBConsts.InRightOutLeftGroup,
                    coupled = ts.Coupled,
                    stageLabel = RBConsts.MainTransitionStage
                };
                SymmetricTransitionGroupBuilder vertical = new()
                {
                    label = RBConsts.InTopOutBotGroup,
                    reverseLabel = RBConsts.InBotOutTopGroup,
                    coupled = ts.Coupled,
                    stageLabel = RBConsts.MainTransitionStage
                };

                List<TransitionDef> lefts = new();
                List<TransitionDef> rights = new();
                List<TransitionDef> tops = new();
                List<TransitionDef> bots = new();
                List<TransitionDef> doors = new();

                Dictionary<TransitionDirection, List<TransitionDef>> directedTransitions = new()
                {
                    { TransitionDirection.Left, lefts },
                    { TransitionDirection.Right, rights },
                    { TransitionDirection.Top, tops },
                    { TransitionDirection.Bot, bots },
                    { TransitionDirection.Unknown, doors },
                };

                foreach (TransitionDef def in transitions)
                {
                    switch (def.sides)
                    {
                        case TransitionSides.OneWayOut:
                            oneWays.Targets.Add(def.Name);
                            break;
                        case TransitionSides.OneWayIn:
                            oneWays.Sources.Add(def.Name);
                            break;
                        default:
                            directedTransitions[def.GetDirection()].Add(def);
                            break;
                    }
                }


                for (int i = 0; i < doors.Count; i++)
                {
                    switch (rights.Count - lefts.Count)
                    {
                        case > 0:
                            lefts.Add(doors[i]);
                            break;
                        case < 0:
                            rights.Add(doors[i]);
                            break;
                        case 0:
                            switch (tops.Count - bots.Count)
                            {
                                case > 0:
                                    bots.Add(doors[i]);
                                    break;
                                case < 0:
                                    tops.Add(doors[i]);
                                    break;
                                case 0:
                                    if (rb.rng.NextBool())
                                    {
                                        rights.Add(doors[i]);
                                    }
                                    else
                                    {
                                        lefts.Add(doors[i]);
                                    }
                                    break;
                            }
                            break;
                    }
                }

                foreach (TransitionDef def in rights) horizontal.Group1.Add(def.Name);
                foreach (TransitionDef def in lefts) horizontal.Group2.Add(def.Name);
                foreach (TransitionDef def in bots) vertical.Group1.Add(def.Name);
                foreach (TransitionDef def in tops) vertical.Group2.Add(def.Name);

                sb.Add(oneWays);
                sb.Add(horizontal);
                sb.Add(vertical);

                bool MatchedTryResolveGroup(RequestBuilder rb, string item, ElementType type, out GroupBuilder gb)
                {
                    if ((type == ElementType.Transition || Data.IsTransition(item))
                        && (rb.gs.TransitionSettings.Mode != TransitionSettings.TransitionMode.AreaRandomizer || Data.IsAreaTransition(item)))
                    {
                        TransitionDef def = Data.GetTransitionDef(item);
                        gb = def.sides != TransitionSides.Both
                            ? oneWays
                            : Data.GetTransitionDef(item).GetDirection() switch
                            {
                                TransitionDirection.Top or
                                TransitionDirection.Bot => vertical,
                                _ => horizontal,
                            };
                        return true;
                    }
                    gb = default;
                    return false;
                }
                OnGetGroupFor.Subscribe(-1000f, MatchedTryResolveGroup);
            }
            else
            {
                SelfDualTransitionGroupBuilder twoWays = new()
                {
                    label = RBConsts.TwoWayGroup,
                    stageLabel = RBConsts.MainTransitionStage,
                    coupled = ts.Coupled,
                };
                foreach (TransitionDef def in transitions)
                {
                    switch (def.sides)
                    {
                        case TransitionSides.OneWayOut:
                            oneWays.Targets.Add(def.Name);
                            break;
                        case TransitionSides.OneWayIn:
                            oneWays.Sources.Add(def.Name);
                            break;
                        default:
                            twoWays.Transitions.Add(def.Name);
                            break;
                    }
                }

                sb.Add(oneWays);
                sb.Add(twoWays);

                bool NonMatchedTryResolveGroup(RequestBuilder rb, string item, ElementType type, out GroupBuilder gb)
                {
                    if ((type == ElementType.Transition || Data.IsTransition(item))
                        && (rb.gs.TransitionSettings.Mode != TransitionSettings.TransitionMode.AreaRandomizer || Data.IsAreaTransition(item)))
                    {
                        TransitionDef def = Data.GetTransitionDef(item);
                        gb = def.sides != TransitionSides.Both ? oneWays : twoWays;
                        return true;
                    }
                    gb = default;
                    return false;
                }
                OnGetGroupFor.Subscribe(-1000f, NonMatchedTryResolveGroup);
            }
        }

        public static void ApplyShopDefs(RequestBuilder rb)
        {
            string[] shops = new[]
            {
                "Sly", "Sly_(Key)", "Iselda", "Salubra", "Leg_Eater"
            };

            foreach (string s in shops)
            {
                rb.EditLocationInfo(s, info =>
                {
                    info.onRandoLocationCreation += (factory, rl) =>
                    {
                        rl.AddCost(new LogicGeoCost(factory.lm, -1));
                    };
                    info.customPlacementFetch = (factory, placement) =>
                    {
                        if (factory.TryFetchPlacement(s, out AbstractPlacement ap)) return ap;

                        ItemChanger.Placements.ShopPlacement sp = (ItemChanger.Placements.ShopPlacement)factory.FetchOrMakePlacement(s);
                        sp.defaultShopItems = IC.Shops.GetDefaultShopItems(factory.gs);
                        return sp;
                    };
                    info.onRandomizerFinish += placement =>
                    {
                        IRandoLocation location = placement.Location;
                        if (location is not RandoModLocation rl || rl.costs == null) return;
                        foreach (LogicGeoCost gc in rl.costs.OfType<LogicGeoCost>())
                        {
                            if (gc.GeoAmount < 0) gc.GeoAmount = GetShopCost(rb.rng, placement.Item.Name, placement.Item.Required);
                        }
                    };
                });
            }

            static int GetShopCost(Random rng, string itemName, bool required)
            {
                double pow = 1.2; // setting?

                int cap = Data.GetItemDef(itemName).priceCap;
                if (cap <= 100) return cap;
                if (required) return rng.PowerLaw(pow, 100, Math.Min(cap, 500)).ClampToMultipleOf(5);
                return rng.PowerLaw(pow, 100, cap).ClampToMultipleOf(5);
            }
        }

        public static void ApplyGrubfatherDef(RequestBuilder rb)
        {
            rb.EditLocationInfo("Grubfather", info =>
            {
                info.onRandoLocationCreation += (factory, rl) =>
                {
                    LogicManager lm = factory.lm;
                    Random rng = factory.rng;
                    GenerationSettings gs = factory.gs;
                    rl.AddCost(new SimpleCost(lm.GetTerm("GRUBS"), rng.Next(gs.CostSettings.MinimumGrubCost, gs.CostSettings.MaximumGrubCost + 1)));
                };
                info.customPlacementFetch = (factory, placement) =>
                {
                    return IC.Grubfather.GetGrubfatherPlacement(factory);
                };
            });
        }

        public static void ApplySeerDef(RequestBuilder rb)
        {
            rb.EditLocationInfo("Seer", info =>
            {
                info.onRandoLocationCreation += (factory, rl) =>
                {
                    LogicManager lm = factory.lm;
                    Random rng = factory.rng;
                    GenerationSettings gs = factory.gs;
                    rl.AddCost(new SimpleCost(lm.GetTerm("ESSENCE"), rng.Next(gs.CostSettings.MinimumEssenceCost, gs.CostSettings.MaximumEssenceCost + 1)));
                };
                info.customPlacementFetch = (factory, placement) =>
                {
                    return IC.Seer.GetSeerPlacement(factory);
                };
            });
        }

        public static void ApplySalubraCharmDef(RequestBuilder rb)
        {
            rb.EditLocationInfo("Salubra_(Requires_Charms)", info =>
            {
                info.randoLocationCreator += factory => factory.MakeLocation("Salubra");
                info.onRandoLocationCreation += (factory, rl) =>
                {
                    LogicManager lm = factory.lm;
                    Random rng = factory.rng;
                    GenerationSettings gs = factory.gs;
                    rl.AddCost(new SimpleCost(lm.GetTerm("CHARMS"), rng.Next(gs.CostSettings.MinimumCharmCost, gs.CostSettings.MaximumCharmCost + 1)));
                };
                info.customPlacementFetch = (factory, placement) =>
                {
                    return factory.FetchOrMakePlacement("Salubra");
                };
            });
        }

        public static void ApplyEggShopDef(RequestBuilder rb)
        {
            rb.EditLocationInfo("Egg_Shop", info =>
            {
                info.onRandoLocationCreation += (factory, rl) =>
                {
                    LogicManager lm = factory.lm;
                    Random rng = factory.rng;
                    GenerationSettings gs = factory.gs;
                    rl.AddCost(new SimpleCost(lm.GetTerm("RANCIDEGGS"), rng.Next(gs.CostSettings.MinimumEggCost, gs.CostSettings.MaximumEggCost + 1)));
                };
            });
        }

        public static void ApplyStartGeo(RequestBuilder rb)
        {
            int minGeo = rb.gs.StartItemSettings.MinimumStartGeo;
            int maxGeo = rb.gs.StartItemSettings.MaximumStartGeo;
            if (minGeo < maxGeo)
            {
                int geo = rb.rng.Next(minGeo, maxGeo);
                rb.AddToStart($"{geo}_Geo");
                // TODO: can it resolve this item pattern?
            }
        }

        public static void ApplyDownslashStart(RequestBuilder rb)
        {
            if (rb.gs.NoveltySettings.RandomizeNail)
            {
                rb.AddToStart("Downslash");
            }
        }

        public static void ApplyPoolSettings(RequestBuilder rb)
        {
            foreach (PoolDef pool in Data.Pools)
            {
                if (pool.IsIncluded(rb.gs))
                {
                    foreach (string item in pool.includeItems) rb.AddItemByName(item);
                    foreach (string location in pool.includeLocations) rb.AddLocationByName(location);
                }
                if (pool.IsVanilla(rb.gs))
                {
                    if (pool.name == "Flame" && rb.gs.PoolSettings.Charms)
                    {
                        foreach (PoolDef.StringILP p in pool.vanilla.Skip(6)) rb.AddToVanilla(p.item, p.location);
                    }
                    else foreach (PoolDef.StringILP p in pool.vanilla) rb.AddToVanilla(p.item, p.location);
                }
            }
        }

        public static void ApplyLongLocationSettings(RequestBuilder rb)
        {
            switch (rb.gs.LongLocationSettings.RandomizationInWhitePalace)
            {
                case LongLocationSettings.WPSetting.ExcludeWhitePalace:
                    rb.RemoveItemByName("Soul_Totem-Palace");
                    rb.RemoveItemByName("Lore_Tablet-Palace_Workshop");
                    rb.RemoveItemByName("Lore_Tablet-Palace_Throne");
                    rb.RemoveLocationsWhere(s => s != "King_Fragment" && Data.GetLocationDef(s)?.areaName == "White_Palace");
                    goto case LongLocationSettings.WPSetting.ExcludePathOfPain;
                case LongLocationSettings.WPSetting.ExcludePathOfPain:
                    rb.RemoveItemByName("Soul_Totem-Path_of_Pain");
                    rb.RemoveItemByName("Journal_Entry-Seal_of_Binding");
                    rb.RemoveItemByName("Lore_Tablet-Path_of_Pain_Entrance");
                    rb.RemoveLocationsWhere(s => s != "King_Fragment" && Data.GetLocationDef(s)?.areaName == "Path_of_Pain");
                    break;
            }
        }

        public static void ApplyDuplicateItemSettings(RequestBuilder rb)
        {
            if (rb.gs.MiscSettings.AddDuplicateItems)
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

                if (rb.gs.NoveltySettings.SplitClaw) dupes.Remove("Mantis_Claw");
                if (rb.gs.NoveltySettings.SplitCloak)
                {
                    dupes.Remove("Mothwing_Cloak");
                    dupes.Remove("Shade_Cloak");
                }
                if (rb.gs.CursedSettings.RemoveSpellUpgrades)
                {
                    dupes.Remove("Vengeful_Spirit");
                    dupes.Remove("Howling_Wraiths");
                    dupes.Remove("Desolate_Dive");
                }

                // non-logic-readable dupes
                foreach (string dupe in dupes)
                {
                    string name = $"Placeholder-{dupe}";
                    rb.EditItemInfo(name, info =>
                    {
                        info.randoItemCreator = (args) => args.MakeWrappedItem(dupe);
                    });
                    rb.AddItemByName(name);
                }

                // dupe simple keys which are logic readable to increase odds of 4 keys waterways
                rb.AddItemByName("Simple_Key");
                rb.AddItemByName("Simple_Key");
            }
        }

        public static void ApplyGrimmchildSetting(RequestBuilder rb)
        {
            if (rb.gs.PoolSettings.GrimmkinFlames && rb.gs.PoolSettings.Charms)
            {
                rb.ReplaceItem("Grimmchild2", "Grimmchild1");
            }
        }

        public static void ApplySpellRemove(RequestBuilder rb)
        {
            rb.RemoveItemByName("Shade_Soul");
            rb.RemoveItemByName("Abyss_Shriek");
            rb.RemoveItemByName("Descending_Dark");
        }

        public static void ApplySplitClawFullClawRemove(RequestBuilder rb)
        {
            if (rb.gs.NoveltySettings.SplitClaw)
            {
                rb.RemoveItemByName("Mantis_Claw");
                rb.RemoveLocationByName("Mantis_Claw");
            }
        }

        public static void ApplySplitCloakFullCloakRemove(RequestBuilder rb)
        {
            if (rb.gs.NoveltySettings.SplitCloak)
            {
                rb.RemoveItemByName("Mothwing_Cloak");
                rb.RemoveItemByName("Shade_Cloak");
            }
        }

        public static void ApplySplitCloakShadeCloakRandomize(RequestBuilder rb)
        {
            if (rb.gs.NoveltySettings.SplitCloak)
            {
                bool leftBiased = rb.rng.NextBool();
                // note -- this means all Split_Shade_Cloak items have the same bias.
                rb.EditItemInfo(ItemNames.Split_Shade_Cloak, info =>
                {
                    info.randoItemCreator = factory =>
                    {
                        return new RandoModItem
                        {
                            item = ((SplitCloakItem)factory.lm.GetItem(ItemNames.Split_Shade_Cloak)) with { LeftBiased = leftBiased }
                        };
                    };
                    info.realItemCreator = (factory, next) =>
                    {
                        AbstractItem item = factory.MakeItem(ItemNames.Split_Shade_Cloak);
                        item.GetTag<ItemChanger.Tags.ItemChainTag>().predecessor = leftBiased ? ItemNames.Left_Mothwing_Cloak : ItemNames.Right_Mothwing_Cloak;
                        return item;
                    };
                });
            }
        }

        public static void ApplyMaskShardCountSetting(RequestBuilder rb)
        {
            if (rb.gs.MiscSettings.MaskShards == MiscSettings.MaskShardType.TwoShardsPerMask)
            {
                rb.ReplaceItem("Mask_Shard", (i) =>
                {
                    int doubleShards = i / 2;
                    int singleShards = i - 2 * doubleShards;

                    return Enumerable.Repeat("Double_Mask_Shard", doubleShards).Concat(Enumerable.Repeat("Mask_Shard", singleShards));
                });
            }
            else if (rb.gs.MiscSettings.MaskShards == MiscSettings.MaskShardType.OneShardPerMask)
            {
                rb.ReplaceItem("Mask_Shard", (i) =>
                {
                    int fullMasks = i / 4;
                    int singleShards = i - 4 * fullMasks;

                    return Enumerable.Repeat("Full_Mask", fullMasks).Concat(Enumerable.Repeat("Mask_Shard", singleShards));
                });
            }
        }

        public static void ApplyVesselFragmentCountSetting(RequestBuilder rb)
        {
            if (rb.gs.MiscSettings.VesselFragments == MiscSettings.VesselFragmentType.TwoFragmentsPerVessel)
            {
                rb.ReplaceItem("Vessel_Fragment", i =>
                {
                    int doubleFragments = i / 2;
                    int singleFragments = i - 2 * doubleFragments;
                    return Enumerable.Repeat("Double_Vessel_Fragment", doubleFragments).Concat(Enumerable.Repeat("Vessel_Fragment", singleFragments));
                });
            }
            else if (rb.gs.MiscSettings.VesselFragments == MiscSettings.VesselFragmentType.OneFragmentPerVessel)
            {
                rb.ReplaceItem("Vessel_Fragment", i =>
                {
                    int fullVessels = i / 3;
                    int singleFragments = i - 3 * fullVessels;
                    return Enumerable.Repeat("Full_Soul_Vessel", fullVessels).Concat(Enumerable.Repeat("Vessel_Fragment", singleFragments));
                });
            }
        }

        public static void ApplyJunkItemRemove(RequestBuilder rb)
        {
            if (rb.gs.CursedSettings.ReplaceJunkWithOneGeo)
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
                            foreach (string i in pool.includeItems) rb.RemoveItemByName(i);
                            break;
                    }
                }
            }
        }

        public static void ApplyMultiLocationRebalancing(RequestBuilder rb)
        {
            // replace existing multi locations with random multi locations from the same set
            HashSet<string> multiSet = new();
            foreach (ItemGroupBuilder gb in rb.EnumerateItemGroups())
            {
                foreach (string l in gb.Locations.EnumerateDistinct())
                {
                    if (Data.GetLocationDef(l) is LocationDef def && def.multi) multiSet.Add(l);
                    // TODO: move multi to LocationRequestInfo?
                }
            }

            string[] multi = multiSet.OrderBy(s => s).ToArray();
            IEnumerable<string> Select(int i)
            {
                for (int j = 0; j < i; j++) yield return rb.rng.Next(multi);
            }

            rb.ReplaceLocation(l => multiSet.Contains(l), (_, i) => Select(i));
        }

        public static void ApplyGrubMimicRando(RequestBuilder rb)
        {
            if (rb.gs.CursedSettings.RandomizeMimics && !rb.gs.PoolSettings.Grubs)
            {
                PoolDef mimicPool = Data.Pools.First(p => p.name == "Mimic");
                PoolDef grubPool = Data.Pools.First(p => p.name == "Grub");

                rb.RemoveItemByName(ItemNames.Mimic_Grub);
                foreach (string loc in mimicPool.includeLocations) rb.RemoveLocationByName(loc);
                foreach (PoolDef.StringILP ilp in grubPool.vanilla) rb.Vanilla.RemoveAll(new(ilp.item, ilp.location));

                ItemGroupBuilder gb = rb.MainItemStage.AddItemGroup("Grub Mimic Group");
                int num_mimics = rb.rng.Next(RBConsts.MIN_MIMIC_COUNT, RBConsts.MAX_MIMIC_COUNT + 1);
                gb.Items.Set(ItemNames.Grub, 50 - num_mimics);
                gb.Items.Set(ItemNames.Mimic_Grub, num_mimics);
                gb.Locations.AddRange(grubPool.includeLocations);
                gb.Locations.AddRange(mimicPool.includeLocations);
            }
        }

        public static void ApplyItemPostPermuteEvents(RequestBuilder rb)
        {
            foreach (ItemGroupBuilder gb in rb.EnumerateItemGroups())
            {
                if (gb.onPermute == null) gb.onPermute = PostPermute;
                // is it a good idea to put this on every item group?
            } 

            void PostPermute(Random rng, RandomizationGroup group)
            {
                IReadOnlyList<IRandoItem> items = group.Items;
                IReadOnlyList<IRandoLocation> locations = group.Locations;
                bool majorPenalty = rb.gs.CursedSettings.LongerProgressionChains;
                bool dupePenalty = true;

                if (majorPenalty || dupePenalty)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (majorPenalty && (Data.GetItemDef(items[i].Name)?.majorItem ?? false))
                        {
                            try
                            {
                                checked { items[i].Priority += rng.Next(items.Count - i); } // makes major items more likely to be selected late in progression
                            }
                            catch (OverflowException)
                            {
                                items[i].Priority = int.MaxValue;
                            }
                        }

                        if (dupePenalty && items[i] is PlaceholderItem)
                        {
                            try
                            {
                                checked { items[i].Priority -= 1000; } // makes dupes more likely to be placed immediately after progression, into late locations
                            }
                            catch (OverflowException)
                            {
                                items[i].Priority = int.MinValue;
                            }
                        }
                    }
                }

                bool shopPenalty = true;

                if (shopPenalty)
                {
                    HashSet<string> shops = new();
                    for (int i = 0; i < locations.Count; i++)
                    {
                        if (shopPenalty && (Data.GetLocationDef(locations[i].Name)?.multi ?? false))
                        {
                            // shops keep their lowest priority slot, but all other slots are moved to the end.
                            if (!shops.Add(locations[i].Name)) locations[i].Priority = Math.Max(locations[i].Priority, locations.Count);
                        }
                    }
                }
            }
        }

        public static void ApplyDefaultItemPadding(RequestBuilder rb)
        {
            foreach (ItemGroupBuilder gb in rb.EnumerateItemGroups())
            {
                gb.ItemPadder ??= ItemPadder;
                gb.LocationPadder ??= LocationPadder;
            }

            IEnumerable<RandoModItem> ItemPadder(RandoFactory factory, int count)
            {
                for (int i = 0; i < count; i++) yield return factory.MakeItem(ItemNames.One_Geo);
            }
            IEnumerable<RandoModLocation> LocationPadder(RandoFactory factory, int count)
            {
                HashSet<string> multiSet = new();
                foreach (ItemGroupBuilder gb in rb.EnumerateItemGroups())
                {
                    foreach (string l in gb.Locations.EnumerateDistinct())
                    {
                        if (Data.GetLocationDef(l) is LocationDef def && def.multi) multiSet.Add(l);
                    }
                }
                // TODO: this method of padding does not consider group!
                if (multiSet.Count == 0) multiSet.Add("Sly");
                string[] multi = multiSet.OrderBy(s => s).ToArray();
                for (int i = 0; i < count; i++) yield return factory.MakeLocation(rb.rng.Next(multi));
            }
        }

        public static void ApplyItemPlacementStrategy(RequestBuilder rb)
        {
            foreach (ItemGroupBuilder gb in rb.EnumerateItemGroups())
            {
                gb.strategy ??= new DefaultGroupPlacementStrategy(5); // TODO: depth priority transform setting
            }
        }

        public static void ApplyTransitionPostPermuteEvents(RequestBuilder rb)
        {
            Dictionary<string, int> areaOrder = new(); // TODO: how to manage this effectively?
            // reset event through RandoMonitor maybe?

            foreach (GroupBuilder gb in rb.EnumerateTransitionGroups())
            {
                if (gb.onPermute == null) gb.onPermute = PostPermute;
            }

            // weakly group transitions by area in the order
            // so that selector eliminates one area before moving onto the next
            // "weakly", since we ideally do not want to prevent bottlenecked layouts like vanilla city
            // note that areaOrder is captured, to synchronize across groups
            void PostPermute(Random rng, RandomizationGroup group)
            {
                if (rb.gs.TransitionSettings.ConnectAreas)
                {
                    foreach (IRandoItem t in group.Items)
                    {
                        string area = Data.GetTransitionDef(t.Name).areaName ?? string.Empty;
                        if (!areaOrder.TryGetValue(area, out int modifier)) areaOrder.Add(area, modifier = areaOrder.Count);
                        t.Priority += modifier * 10;
                        
                    }
                    foreach (IRandoLocation t in group.Locations)
                    {
                        string area = Data.GetTransitionDef(t.Name).areaName ?? string.Empty;
                        if (!areaOrder.TryGetValue(area, out int modifier)) areaOrder.Add(area, modifier = areaOrder.Count);
                        t.Priority += modifier * 10;
                    }
                }
            }
        }

        public static void ApplyTransitionPlacementStrategy(RequestBuilder rb)
        {
            if (rb.gs.TransitionSettings.ConnectAreas)
            {
                foreach (GroupBuilder gb in rb.EnumerateTransitionGroups())
                {
                    gb.strategy ??= new DefaultGroupPlacementStrategy(1);
                    if (gb.strategy is DefaultGroupPlacementStrategy s)
                    {
                        s.Constraints += AreasMatch;
                    }
                    else throw new InvalidOperationException("Connected areas conflict with transition group placement strategy!");
                }
            }
            if (false) // ban door-door transitions
            {
                foreach (GroupBuilder gb in rb.EnumerateTransitionGroups())
                {
                    if (gb.label == "Left -> Right" || gb.label == "Right -> Left")
                    {
                        gb.strategy ??= new DefaultGroupPlacementStrategy(1);
                        if (gb.strategy is DefaultGroupPlacementStrategy s)
                        {
                            s.Constraints += NotDoorToDoor;
                        }
                        else throw new InvalidOperationException("Avoid Door-Door transitions conflicts with transition group placement strategy!");
                    }
                }
            }

            static bool AreasMatch(IRandoItem item, IRandoLocation location)
            {
                if (Data.GetTransitionDef(item.Name) is not TransitionDef t1
                    || Data.GetTransitionDef(location.Name) is not TransitionDef t2)
                {
                    return true;
                }

                return t1.areaName == t2.areaName;
            }

            static bool NotDoorToDoor(IRandoItem item, IRandoLocation location)
            {
                if (Data.GetTransitionDef(item.Name) is not TransitionDef t1
                    || Data.GetTransitionDef(location.Name) is not TransitionDef t2)
                {
                    return true;
                }

                return t1.GetDirection() != TransitionDirection.Unknown || t2.GetDirection() != TransitionDirection.Unknown;
            }
        }
    }
}
