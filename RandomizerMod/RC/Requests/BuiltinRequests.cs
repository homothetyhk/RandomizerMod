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
            OnUpdate.Subscribe(-800f, PlaceUnrandomizedTransitions);

            OnUpdate.Subscribe(-100f, ApplyShopDefs);
            OnUpdate.Subscribe(-100f, ApplyGrubfatherDef);
            OnUpdate.Subscribe(-100f, ApplySeerDef);
            OnUpdate.Subscribe(-100f, ApplyEggShopDef);
            OnUpdate.Subscribe(-100f, ApplySalubraCharmDef);

            OnUpdate.Subscribe(0f, ApplyPoolSettings);
            OnUpdate.Subscribe(0f, ApplyGrimmchildSetting);
            OnUpdate.Subscribe(0f, ApplySplitCloakShadeCloakRandomize);
            OnUpdate.Subscribe(0f, ApplyProgressiveSplitClaw);
            OnUpdate.Subscribe(0f, ApplySalubraNotchesSetting);
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

            OnUpdate.Subscribe(30f, ApplyPalaceLongLocationSetting);
            OnUpdate.Subscribe(30f, ApplyBossEssenceLongLocationSetting);


            OnUpdate.Subscribe(100f, ApplyItemPostPermuteEvents);
            OnUpdate.Subscribe(100f, ApplyDefaultItemPadding);
            OnUpdate.Subscribe(100f, ApplyTransitionPostPermuteEvents);
            OnUpdate.Subscribe(100f, ApplyTransitionPlacementStrategy);
        }


        public static void ApplyPlaceholderMatch(RequestBuilder rb)
        {
            static bool TryMatch(string name, out ItemRequestInfo info)
            {
                if (name.StartsWith(PlaceholderItem.Prefix))
                {
                    info = new ItemRequestInfo
                    {
                        randoItemCreator = factory => factory.MakeWrappedItem(name.Substring(PlaceholderItem.Prefix.Length)),
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

            if (ts.TransitionMatching == TransitionSettings.TransitionMatchingSetting.MatchingDirections 
                || ts.TransitionMatching == TransitionSettings.TransitionMatchingSetting.MatchingDirectionsAndNoDoorToDoor)
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
                    { TransitionDirection.Door, doors },
                };

                foreach (TransitionDef def in transitions)
                {
                    switch (def.Sides)
                    {
                        case TransitionSides.OneWayOut:
                            oneWays.Targets.Add(def.Name);
                            break;
                        case TransitionSides.OneWayIn:
                            oneWays.Sources.Add(def.Name);
                            break;
                        default:
                            directedTransitions[def.Direction].Add(def);
                            break;
                    }
                }

                rb.rng.PermuteInPlace(doors);
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

                oneWays.strategy = rb.gs.ProgressionDepthSettings.GetTransitionPlacementStrategy();
                horizontal.strategy = rb.gs.ProgressionDepthSettings.GetTransitionPlacementStrategy();
                vertical.strategy = rb.gs.ProgressionDepthSettings.GetTransitionPlacementStrategy();

                if (ts.TransitionMatching == TransitionSettings.TransitionMatchingSetting.MatchingDirectionsAndNoDoorToDoor)
                {
                    static bool NotDoorToDoor(IRandoItem item, IRandoLocation location)
                    {
                        if (Data.GetTransitionDef(item.Name) is not TransitionDef t1
                            || Data.GetTransitionDef(location.Name) is not TransitionDef t2)
                        {
                            return true;
                        }

                        return t1.Direction != TransitionDirection.Door || t2.Direction != TransitionDirection.Door;
                    }
                    ((DefaultGroupPlacementStrategy)horizontal.strategy).Constraints += NotDoorToDoor;
                }

                sb.Add(oneWays);
                sb.Add(horizontal);
                sb.Add(vertical);

                bool MatchedTryResolveGroup(RequestBuilder rb, string item, ElementType type, out GroupBuilder gb)
                {
                    if ((type == ElementType.Transition || Data.IsTransition(item))
                        && (rb.gs.TransitionSettings.Mode != TransitionSettings.TransitionMode.AreaRandomizer || Data.IsAreaTransition(item)))
                    {
                        TransitionDef def = Data.GetTransitionDef(item);
                        gb = def.Sides != TransitionSides.Both
                            ? oneWays
                            : Data.GetTransitionDef(item).Direction switch
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
                    switch (def.Sides)
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

                oneWays.strategy = rb.gs.ProgressionDepthSettings.GetTransitionPlacementStrategy();
                twoWays.strategy = rb.gs.ProgressionDepthSettings.GetTransitionPlacementStrategy();

                sb.Add(oneWays);
                sb.Add(twoWays);

                bool NonMatchedTryResolveGroup(RequestBuilder rb, string item, ElementType type, out GroupBuilder gb)
                {
                    if ((type == ElementType.Transition || Data.IsTransition(item))
                        && (rb.gs.TransitionSettings.Mode != TransitionSettings.TransitionMode.AreaRandomizer || Data.IsAreaTransition(item)))
                    {
                        TransitionDef def = Data.GetTransitionDef(item);
                        gb = def.Sides != TransitionSides.Both ? oneWays : twoWays;
                        return true;
                    }
                    gb = default;
                    return false;
                }
                OnGetGroupFor.Subscribe(-1000f, NonMatchedTryResolveGroup);
            }
        }

        public static void PlaceUnrandomizedTransitions(RequestBuilder rb)
        {
            HashSet<string> RandomizedTransitions = rb.gs.TransitionSettings.Mode switch
            {
                TransitionSettings.TransitionMode.RoomRandomizer => new(Data.GetRoomTransitionNames()),
                TransitionSettings.TransitionMode.AreaRandomizer => new(Data.GetAreaTransitionNames()),
                _ => new()
            };

            foreach (string trans in Data.GetRoomTransitionNames().Except(RandomizedTransitions))
            {
                string target = Data.GetTransitionDef(trans).VanillaTarget;
                if (!string.IsNullOrEmpty(target))
                {
                    rb.AddToVanilla(target, trans);
                }
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

                if (itemName.StartsWith(PlaceholderItem.Prefix))
                {
                    itemName = itemName.Substring(PlaceholderItem.Prefix.Length);
                }

                int cap = Data.GetItemDef(itemName) is ItemDef def ? def.PriceCap : 500;
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
                    if (factory.TryFetchPlacement("Grubfather", out AbstractPlacement p)) return p;
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
                    if (factory.TryFetchPlacement("Seer", out AbstractPlacement p)) return p;
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
                    return factory.FetchOrMakePlacementWithEvents("Salubra", placement);
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
                    foreach (string item in pool.IncludeItems) rb.AddItemByName(item);
                    foreach (string location in pool.IncludeLocations) rb.AddLocationByName(location);
                }
                if (pool.IsVanilla(rb.gs))
                {
                    if (pool.Name == "Flame" && rb.gs.PoolSettings.Charms)
                    {
                        foreach (PoolDef.StringILP p in pool.Vanilla.Skip(6)) rb.AddToVanilla(p.item, p.location);
                    }
                    else foreach (PoolDef.StringILP p in pool.Vanilla) rb.AddToVanilla(p.item, p.location);
                }
            }
        }

        public static void ApplyPalaceLongLocationSetting(RequestBuilder rb)
        {
            switch (rb.gs.LongLocationSettings.RandomizationInWhitePalace)
            {
                case LongLocationSettings.WPSetting.ExcludeWhitePalace:
                    rb.RemoveItemByName("Soul_Totem-Palace");
                    rb.RemoveItemByName("Lore_Tablet-Palace_Workshop");
                    rb.RemoveItemByName("Lore_Tablet-Palace_Throne");
                    rb.RemoveLocationsWhere(s => s != "King_Fragment" && Data.GetLocationDef(s)?.AreaName == "White_Palace");
                    goto case LongLocationSettings.WPSetting.ExcludePathOfPain;
                case LongLocationSettings.WPSetting.ExcludePathOfPain:
                    rb.RemoveItemByName("Soul_Totem-Path_of_Pain");
                    rb.RemoveItemByName("Journal_Entry-Seal_of_Binding");
                    rb.RemoveItemByName("Lore_Tablet-Path_of_Pain_Entrance");
                    rb.RemoveLocationsWhere(s => s != "King_Fragment" && Data.GetLocationDef(s)?.AreaName == "Path_of_Pain");
                    break;
            }
        }

        public static void ApplyBossEssenceLongLocationSetting(RequestBuilder rb)
        {
            if (!rb.gs.PoolSettings.BossEssence)
            {
                return;
            }

            switch (rb.gs.LongLocationSettings.BossEssenceRandomization)
            {
                case LongLocationSettings.BossEssenceSetting.ExcludeAllDreamWarriors:
                    PoolDef warriors = Data.Pools.First(p => p.Name == "DreamWarrior");
                    foreach (string item in warriors.IncludeItems) rb.RemoveItemByName(item);
                    foreach (string loc in warriors.IncludeLocations) rb.RemoveLocationByName(loc);
                    foreach (PoolDef.StringILP p in warriors.Vanilla) rb.AddToVanilla(p.item, p.location);
                    break;
                case LongLocationSettings.BossEssenceSetting.ExcludeAllDreamBosses:
                    PoolDef bosses = Data.Pools.First(p => p.Name == "DreamBoss");
                    foreach (string item in bosses.IncludeItems) rb.RemoveItemByName(item);
                    foreach (string loc in bosses.IncludeLocations) rb.RemoveLocationByName(loc);
                    foreach (PoolDef.StringILP p in bosses.Vanilla) rb.AddToVanilla(p.item, p.location);
                    break;
                case LongLocationSettings.BossEssenceSetting.ExcludeGreyPrinceZoteAndWhiteDefender:
                    rb.RemoveItemByName(ItemNames.Boss_Essence_White_Defender);
                    rb.RemoveItemByName(ItemNames.Boss_Essence_Grey_Prince_Zote);
                    rb.RemoveLocationByName(LocationNames.Boss_Essence_White_Defender);
                    rb.RemoveLocationByName(LocationNames.Boss_Essence_Grey_Prince_Zote);
                    rb.AddToVanilla(ItemNames.Boss_Essence_White_Defender, LocationNames.Boss_Essence_White_Defender);
                    rb.AddToVanilla(ItemNames.Boss_Essence_Grey_Prince_Zote, LocationNames.Boss_Essence_Grey_Prince_Zote);
                    break;
            }    
        }

        public static void ApplyDuplicateItemSettings(RequestBuilder rb)
        {
            DuplicateItemSettings ds = rb.gs.DuplicateItemSettings;
            NoveltySettings ns = rb.gs.NoveltySettings;
            CursedSettings cs = rb.gs.CursedSettings;
            PoolSettings ps = rb.gs.PoolSettings;
            List<string> dupes = new();

            if (ds.MothwingCloak && !ns.SplitCloak) dupes.Add(ItemNames.Mothwing_Cloak);
            if (ds.MantisClaw && !ns.SplitClaw) dupes.Add(ItemNames.Mantis_Claw);
            if (ds.CrystalHeart) dupes.Add(ItemNames.Crystal_Heart);
            if (ds.MonarchWings) dupes.Add(ItemNames.Monarch_Wings);
            if (ds.ShadeCloak) dupes.Add(ns.SplitCloak ? ItemNames.Split_Shade_Cloak : ItemNames.Shade_Cloak);
            if (ds.DreamNail) dupes.Add(ItemNames.Dream_Nail);
            if (ds.Dreamer) dupes.Add(ItemNames.Dreamer);
            if (ds.SwimmingItems)
            {
                dupes.Add(ItemNames.Ismas_Tear);
                if (ns.RandomizeSwim) dupes.Add(ItemNames.Swim);
            }
            if (ds.LevelOneSpells)
            {
                dupes.Add(ItemNames.Vengeful_Spirit);
                dupes.Add(ItemNames.Desolate_Dive);
                dupes.Add(ItemNames.Howling_Wraiths);
            }
            if (ds.LevelTwoSpells && !cs.RemoveSpellUpgrades)
            {
                dupes.Add(ItemNames.Shade_Soul);
                dupes.Add(ItemNames.Descending_Dark);
                dupes.Add(ItemNames.Abyss_Shriek);
            }
            if (ds.Grimmchild)
            {
                dupes.Add(ps.GrimmkinFlames ? ItemNames.Grimmchild1 : ItemNames.Grimmchild2);
            }
            if (ds.NailArts)
            {
                dupes.Add(ItemNames.Cyclone_Slash);
                dupes.Add(ItemNames.Great_Slash);
                dupes.Add(ItemNames.Dash_Slash);
            }
            if (ds.CursedNailItems && ns.RandomizeNail)
            {
                dupes.Add(ItemNames.Upslash);
                dupes.Add(ItemNames.Leftslash);
                dupes.Add(ItemNames.Rightslash);
            }
            if (ds.DuplicateUniqueKeys)
            {
                dupes.Add(ItemNames.City_Crest);
                dupes.Add(ItemNames.Lumafly_Lantern);
                dupes.Add(ItemNames.Tram_Pass);
                dupes.Add(ItemNames.Shopkeepers_Key);
                dupes.Add(ItemNames.Elegant_Key);
                dupes.Add(ItemNames.Love_Key);
                dupes.Add(ItemNames.Kings_Brand);
            }
            switch (ds.SimpleKeyHandling)
            {
                default:
                case DuplicateItemSettings.SimpleKeySetting.NoDupe:
                    break;
                case DuplicateItemSettings.SimpleKeySetting.TwoDupeKeys:
                    dupes.Add(ItemNames.Simple_Key);
                    dupes.Add(ItemNames.Simple_Key);
                    break;
                case DuplicateItemSettings.SimpleKeySetting.TwoExtraKeysInLogic:
                    rb.AddItemByName(ItemNames.Simple_Key);
                    rb.AddItemByName(ItemNames.Simple_Key);
                    break;
            }
            if (ns.SplitClaw)
            {
                switch (ds.SplitClawHandling)
                {
                    default:
                    case DuplicateItemSettings.SplitItemSetting.NoDupe:
                        break;
                    case DuplicateItemSettings.SplitItemSetting.DupeLeft:
                        dupes.Add(ItemNames.Left_Mantis_Claw);
                        break;
                    case DuplicateItemSettings.SplitItemSetting.DupeRight:
                        dupes.Add(ItemNames.Right_Mantis_Claw);
                        break;
                    case DuplicateItemSettings.SplitItemSetting.DupeRandom:
                        if (rb.rng.NextBool()) goto case DuplicateItemSettings.SplitItemSetting.DupeLeft;
                        else goto case DuplicateItemSettings.SplitItemSetting.DupeRight;
                    case DuplicateItemSettings.SplitItemSetting.DupeBoth:
                        dupes.Add(ItemNames.Left_Mantis_Claw);
                        dupes.Add(ItemNames.Right_Mantis_Claw);
                        break;
                }
            }
            if (ns.SplitCloak)
            {
                switch (ds.SplitCloakHandling)
                {
                    default:
                    case DuplicateItemSettings.SplitItemSetting.NoDupe:
                        break;
                    case DuplicateItemSettings.SplitItemSetting.DupeLeft:
                        dupes.Add(ItemNames.Left_Mothwing_Cloak);
                        break;
                    case DuplicateItemSettings.SplitItemSetting.DupeRight:
                        dupes.Add(ItemNames.Right_Mothwing_Cloak);
                        break;
                    case DuplicateItemSettings.SplitItemSetting.DupeRandom:
                        if (rb.rng.NextBool()) goto case DuplicateItemSettings.SplitItemSetting.DupeLeft;
                        else goto case DuplicateItemSettings.SplitItemSetting.DupeRight;
                    case DuplicateItemSettings.SplitItemSetting.DupeBoth:
                        dupes.Add(ItemNames.Left_Mothwing_Cloak);
                        dupes.Add(ItemNames.Right_Mothwing_Cloak);
                        break;
                }
            }

            // non-logic-readable dupes
            foreach (string dupe in dupes)
            {
                string name = $"{PlaceholderItem.Prefix}{dupe}";
                rb.AddItemByName(name);
            }
        }

        public static void ApplyGrimmchildSetting(RequestBuilder rb)
        {
            if (rb.gs.PoolSettings.GrimmkinFlames && rb.gs.PoolSettings.Charms)
            {
                rb.ReplaceItem("Grimmchild2", "Grimmchild1");
            }
        }

        public static void ApplySalubraNotchesSetting(RequestBuilder rb)
        {
            switch (rb.gs.MiscSettings.SalubraNotches)
            {
                default:
                case MiscSettings.SalubraNotchesSetting.GroupedWithCharmNotchesPool: 
                    return;
                case MiscSettings.SalubraNotchesSetting.AutoGivenAtCharmThreshold when rb.gs.PoolSettings.CharmNotches:
                case MiscSettings.SalubraNotchesSetting.Vanilla when rb.gs.PoolSettings.CharmNotches:
                    {
                        ItemGroupBuilder gb = rb.GetItemGroupFor("Charm_Notch");
                        gb.Items.Remove("Charm_Notch", 4);
                        gb.Locations.Remove("Salubra_(Requires_Charms)", 4);
                    }
                    break;
                case MiscSettings.SalubraNotchesSetting.Randomized when !rb.gs.PoolSettings.CharmNotches:
                    {
                        ItemGroupBuilder gb = rb.GetItemGroupFor("Charm_Notch");
                        gb.Items.Increment("Charm_Notch", 4);
                        gb.Locations.Increment("Salubra_(Requires_Charms)", 4);
                    }
                    break;
            }
        }

        public static void ApplySpellRemove(RequestBuilder rb)
        {
            if (rb.gs.CursedSettings.RemoveSpellUpgrades)
            {
                rb.RemoveItemByName(ItemNames.Shade_Soul);
                rb.RemoveItemByName(ItemNames.Abyss_Shriek);
                rb.RemoveItemByName(ItemNames.Descending_Dark);
                rb.EditItemInfo(ItemNames.Vengeful_Spirit, info =>
                {
                    info.realItemCreator = (factory, placement) =>
                    {
                        AbstractItem item = factory.MakeItem(ItemNames.Vengeful_Spirit);
                        item.RemoveTags<ItemChanger.Tags.ItemChainTag>();
                        return item;
                    };
                });
                rb.EditItemInfo(ItemNames.Howling_Wraiths, info =>
                {
                    info.realItemCreator = (factory, placement) =>
                    {
                        AbstractItem item = factory.MakeItem(ItemNames.Howling_Wraiths);
                        item.RemoveTags<ItemChanger.Tags.ItemChainTag>();
                        return item;
                    };
                });
                rb.EditItemInfo(ItemNames.Desolate_Dive, info =>
                {
                    info.realItemCreator = (factory, placement) =>
                    {
                        AbstractItem item = factory.MakeItem(ItemNames.Desolate_Dive);
                        item.RemoveTags<ItemChanger.Tags.ItemChainTag>();
                        return item;
                    };
                });
            }
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

        public static void ApplyProgressiveSplitClaw(RequestBuilder rb)
        {
            rb.EditItemInfo(ItemNames.Left_Mantis_Claw, info => info.realItemCreator = (factory, placement) =>
            {
                AbstractItem item = factory.MakeItem(ItemNames.Left_Mantis_Claw);
                item.AddTag<ItemChanger.Tags.ItemTreeTag>().successors = new[] { ItemNames.Right_Mantis_Claw };
                return item;
            });
            rb.EditItemInfo(ItemNames.Right_Mantis_Claw, info => info.realItemCreator = (factory, placement) =>
            {
                AbstractItem item = factory.MakeItem(ItemNames.Right_Mantis_Claw);
                item.AddTag<ItemChanger.Tags.ItemTreeTag>().successors = new[] { ItemNames.Left_Mantis_Claw };
                return item;
            });
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
                    switch (pool.Name)
                    {
                        case "Mask":
                        case "CursedMask":
                        case "Vessel":
                        case "Ore":
                        case "Notch":
                        case "CursedNotch":
                        case "Geo":
                        case "Egg" when !rb.gs.NoveltySettings.EggShop:
                        case "Relic":
                        case "Rock":
                        case "Soul":
                        case "PalaceSoul":
                        case "Boss_Geo":
                            foreach (string i in pool.IncludeItems) rb.RemoveItemByName(i);
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
                int count = 0;
                foreach (string l in gb.Locations.EnumerateDistinct())
                {
                    if (Data.GetLocationDef(l) is LocationDef def && def.Multi)
                    {
                        multiSet.Add(l);
                        count += gb.Locations.GetCount(l) - 1;
                    }
                    // TODO: move multi to LocationRequestInfo?
                }
                string[] multi = multiSet.OrderBy(s => s).ToArray();
                foreach (string l in multi)
                {
                    gb.Locations.Set(l, 1);
                }
                while (count-- > 0)
                {
                    gb.Locations.Add(rb.rng.Next(multi));
                }
                multiSet.Clear();
            }
        }

        public static void ApplyGrubMimicRando(RequestBuilder rb)
        {
            if (rb.gs.CursedSettings.RandomizeMimics && !rb.gs.PoolSettings.Grubs)
            {
                PoolDef mimicPool = Data.Pools.First(p => p.Name == "Mimic");
                PoolDef grubPool = Data.Pools.First(p => p.Name == "Grub");

                rb.RemoveItemByName(ItemNames.Mimic_Grub);
                foreach (string loc in mimicPool.IncludeLocations) rb.RemoveLocationByName(loc);
                foreach (PoolDef.StringILP ilp in grubPool.Vanilla) rb.Vanilla.RemoveAll(new(ilp.item, ilp.location));

                StageBuilder sb = rb.AddStage("Grub Mimic Stage");
                ItemGroupBuilder gb = sb.AddItemGroup("Grub Mimic Group");
                int num_mimics = rb.rng.Next(RBConsts.MIN_MIMIC_COUNT, RBConsts.MAX_MIMIC_COUNT + 1);
                gb.Items.Set(ItemNames.Grub, 50 - num_mimics);
                gb.Items.Set(ItemNames.Mimic_Grub, num_mimics);
                gb.Locations.AddRange(grubPool.IncludeLocations);
                gb.Locations.AddRange(mimicPool.IncludeLocations);
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
                bool dupePenalty = rb.gs.ProgressionDepthSettings.DuplicateItemPenalty;

                if (majorPenalty || dupePenalty)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (majorPenalty && (Data.GetItemDef(items[i].Name)?.MajorItem ?? false))
                        {
                            
                            items[i].Priority += rng.Next(items.Count - i) / (float)items.Count;
                        }

                        if (dupePenalty && items[i] is PlaceholderItem)
                        {
                            // make dupes more likely to be placed immediately after progression, into late locations
                            items[i].Priority -= 0.8f;
                        }
                    }
                }

                bool shopPenalty = rb.gs.ProgressionDepthSettings.MultiLocationPenalty;

                if (shopPenalty)
                {
                    HashSet<string> shops = new();
                    for (int i = 0; i < locations.Count; i++)
                    {
                        if (shopPenalty && (Data.GetLocationDef(locations[i].Name)?.Multi ?? false))
                        {
                            // shops keep their lowest priority slot, but all other slots are moved to the end.
                            if (!shops.Add(locations[i].Name)) locations[i].Priority = Math.Max(locations[i].Priority, 1f);
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
                        if (Data.GetLocationDef(l) is LocationDef def && def.Multi) multiSet.Add(l);
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
                gb.strategy ??= rb.gs.ProgressionDepthSettings.GetItemPlacementStrategy();
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
                        string area = Data.GetTransitionDef(t.Name).AreaName ?? string.Empty;
                        if (!areaOrder.TryGetValue(area, out int modifier)) areaOrder.Add(area, modifier = areaOrder.Count);
                        t.Priority += modifier * 0.8f;
                        
                    }
                    foreach (IRandoLocation t in group.Locations)
                    {
                        string area = Data.GetTransitionDef(t.Name).AreaName ?? string.Empty;
                        if (!areaOrder.TryGetValue(area, out int modifier)) areaOrder.Add(area, modifier = areaOrder.Count);
                        t.Priority += modifier * 0.8f;
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
                    gb.strategy ??= rb.gs.ProgressionDepthSettings.GetTransitionPlacementStrategy();
                    if (gb.strategy is DefaultGroupPlacementStrategy s)
                    {
                        s.Constraints += AreasMatch;
                    }
                    else throw new InvalidOperationException("Connected areas conflict with transition group placement strategy!");
                }
            }

            static bool AreasMatch(IRandoItem item, IRandoLocation location)
            {
                if (Data.GetTransitionDef(item.Name) is not TransitionDef t1
                    || Data.GetTransitionDef(location.Name) is not TransitionDef t2)
                {
                    return true;
                }

                return t1.AreaName == t2.AreaName;
            }
        }
    }
}
