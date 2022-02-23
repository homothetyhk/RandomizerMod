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
            OnSelectStart.Subscribe(float.PositiveInfinity, SelectStart);

            OnUpdate.Subscribe(-10000f, AssignNotchCosts);

            OnUpdate.Subscribe(-2000f, ApplyPlaceholderMatch);
            OnUpdate.Subscribe(-2000f, ApplyCustomGeoMatch);

            OnUpdate.Subscribe(-1000f, ApplyTransitionSettings);
            OnUpdate.Subscribe(-800f, PlaceUnrandomizedTransitions);

            OnUpdate.Subscribe(-500f, ApplySplitGroupResolver);

            OnUpdate.Subscribe(-100f, ApplyShopDefs);
            OnUpdate.Subscribe(-100f, ApplyGrubfatherDef);
            OnUpdate.Subscribe(-100f, ApplySeerDef);
            OnUpdate.Subscribe(-100f, ApplyEggShopDef);
            OnUpdate.Subscribe(-100f, ApplySalubraCharmDef);

            OnUpdate.Subscribe(-80f, ApplyWorldSenseDef);
            OnUpdate.Subscribe(-80f, ApplyFocusDef);

            OnUpdate.Subscribe(0f, ApplyPoolSettings);
            OnUpdate.Subscribe(0f, ApplyGrimmchildSetting);
            OnUpdate.Subscribe(0f, ApplySplitCloakShadeCloakRandomize);
            OnUpdate.Subscribe(0f, ApplyProgressiveSplitClaw);
            OnUpdate.Subscribe(0f, ApplySalubraNotchesSetting);
            OnUpdate.Subscribe(1f, ApplyMultiLocationRebalancing);
            OnUpdate.Subscribe(2f, ApplyGrubMimicRando);

            OnUpdate.Subscribe(4f, ApplyRandomizeCursedMasks);
            OnUpdate.Subscribe(4f, ApplyRandomizeCursedNotches);

            OnUpdate.Subscribe(5f, ApplyStartGeo);
            OnUpdate.Subscribe(5f, ApplyDownslashStart);
            OnUpdate.Subscribe(5f, ApplyStartItemSettings);

            OnUpdate.Subscribe(15f, ApplySplitClawFullClawRemove);
            OnUpdate.Subscribe(15f, ApplySplitCloakFullCloakRemove);
            OnUpdate.Subscribe(15f, ApplySplitSuperdashFullCrystalHeartRemove);
            OnUpdate.Subscribe(15f, ApplySpellRemove);

            OnUpdate.Subscribe(16f, ApplyJunkItemRemove);

            // These must run after junk remove, because junk remove cannot detect modified items.
            OnUpdate.Subscribe(18f, ApplyMaskShardCountSetting);
            OnUpdate.Subscribe(18f, ApplyVesselFragmentCountSetting);

            OnUpdate.Subscribe(20f, ApplyDuplicateItemSettings);

            OnUpdate.Subscribe(30f, ApplyPalaceLongLocationSetting);
            OnUpdate.Subscribe(30f, ApplyBossEssenceLongLocationSetting);
            OnUpdate.Subscribe(30f, ApplyLongLocationPreviewSettings);

            OnUpdate.Subscribe(99f, ApplyItemPlacementStrategy);
            OnUpdate.Subscribe(99f, ApplyTransitionPlacementStrategy);

            OnUpdate.Subscribe(100f, ApplyItemPostPermuteEvents);
            OnUpdate.Subscribe(100f, ApplyDefaultItemPadding);
            OnUpdate.Subscribe(100f, ApplyConnectAreasPostPermuteEvent);
            OnUpdate.Subscribe(100f, ApplyAreaConstraint);
            OnUpdate.Subscribe(100f, ApplyDerangedConstraint);
        }

        public static bool SelectStart(Random rng, GenerationSettings gs, SettingsPM pm, out RandomizerData.StartDef def)
        {
            Dictionary<string, RandomizerData.StartDef> startDefs = Menu.RandomizerMenuAPI.GenerateStartLocationDict();

            StartLocationSettings.RandomizeStartLocationType type = gs.StartLocationSettings.StartLocationType;
            if (type != StartLocationSettings.RandomizeStartLocationType.Fixed)
            {
                List<string> startNames = new(startDefs.Values.Where(s => s.CanBeRandomized(pm)).Select(s => s.Name));
                if (type == StartLocationSettings.RandomizeStartLocationType.RandomExcludingKP) startNames.Remove("King's Pass");
                gs.StartLocationSettings.StartLocation = rng.Next(startNames);
            }

            if (gs.StartLocationSettings.StartLocation == null || !startDefs.TryGetValue(gs.StartLocationSettings.StartLocation, out def))
            {
                LogWarn($"Unknown start location {gs.StartLocationSettings.StartLocation} selected in BuiltinRequests.SelectStart, falling back to King's Pass");
                gs.StartLocationSettings.StartLocation = Data.GetStartNames().First();
                def = Data.GetStartDef(gs.StartLocationSettings.StartLocation);
            }

            return true;
        }

        public static void AssignNotchCosts(RequestBuilder rb)
        {
            if (!rb.gs.MiscSettings.RandomizeNotchCosts)
            {
                rb.ctx.notchCosts = Enumerable.Range(1, 40).Select(i => CharmNotchCosts.GetVanillaCost(i)).ToList();
            }
            else
            {
                rb.ctx.notchCosts = CharmNotchCosts.GetUniformlyRandomCosts(rb.rng, 70, 110).ToList();
            }
        }

        public static void ApplyPlaceholderMatch(RequestBuilder rb)
        {
            bool TryMatch(string name, out ItemRequestInfo info)
            {
                if (name.StartsWith(PlaceholderItem.Prefix))
                {
                    info = new ItemRequestInfo
                    {
                        randoItemCreator = factory => factory.MakeWrappedItem(name.Substring(PlaceholderItem.Prefix.Length)),
                        onRandomizerFinish = (placement) => ((PlaceholderItem)placement.Item).Unwrap(),
                        getItemDef = () => rb.TryGetItemDef(name.Substring(PlaceholderItem.Prefix.Length), out ItemDef def) ? def : null,
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

            bool TryResolveGroup(RequestBuilder rb, string item, ElementType type, out GroupBuilder gb)
            {
                if ((type == ElementType.Item || type == ElementType.Unknown) && item.StartsWith(PlaceholderItem.Prefix))
                {
                    gb = rb.GetItemGroupFor(item.Substring(PlaceholderItem.Prefix.Length));
                    return true;
                }
                gb = default;
                return false;
            }
            rb.OnGetGroupFor.Subscribe(100f, TryResolveGroup);
        }

        public static void ApplyCustomGeoMatch(RequestBuilder rb)
        {
            bool TryMatch(string name, out ItemRequestInfo info)
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
                        },
                        getItemDef = () => Data.GetItemDef(ItemNames.One_Geo) with { Name = name },
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
            IEnumerable<TransitionDef> transitions = (ts.Mode switch
            {
                TransitionSettings.TransitionMode.RoomRandomizer => Data.GetRoomTransitionNames(),
                TransitionSettings.TransitionMode.FullAreaRandomizer => Data.GetAreaTransitionNames(),
                TransitionSettings.TransitionMode.MapAreaRandomizer => Data.GetMapAreaTransitionNames(),
                _ => Enumerable.Empty<string>(),
            }).Select(t => Data.GetTransitionDef(t));

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
                    bool isTransition = type == ElementType.Transition || Data.IsTransition(item);
                    if (isTransition)
                    {
                        bool isModeTransition = rb.gs.TransitionSettings.Mode switch
                        {
                            TransitionSettings.TransitionMode.MapAreaRandomizer => Data.IsMapAreaTransition(item),
                            TransitionSettings.TransitionMode.FullAreaRandomizer => Data.IsAreaTransition(item),
                            TransitionSettings.TransitionMode.RoomRandomizer => true,
                            _ => false,
                        };
                        if (isModeTransition)
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
                    }
                    gb = null;
                    return false;
                }
                rb.OnGetGroupFor.Subscribe(-1000f, MatchedTryResolveGroup);
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
                    bool isTransition = type == ElementType.Transition || Data.IsTransition(item);
                    if (isTransition)
                    {
                        bool isModeTransition = rb.gs.TransitionSettings.Mode switch
                        {
                            TransitionSettings.TransitionMode.MapAreaRandomizer => Data.IsMapAreaTransition(item),
                            TransitionSettings.TransitionMode.FullAreaRandomizer => Data.IsAreaTransition(item),
                            TransitionSettings.TransitionMode.RoomRandomizer => true,
                            _ => false,
                        };
                        if (isModeTransition)
                        {
                            TransitionDef def = Data.GetTransitionDef(item);
                            gb = def.Sides != TransitionSides.Both ? oneWays : twoWays;
                            return true;
                        }
                    }
                    gb = default;
                    return false;
                }
                rb.OnGetGroupFor.Subscribe(-1000f, NonMatchedTryResolveGroup);
            }
        }

        public static void PlaceUnrandomizedTransitions(RequestBuilder rb)
        {
            var mode = rb.gs.TransitionSettings.Mode;
            bool IsVanillaInMode(string s)
            {
                TransitionDef def = Data.GetTransitionDef(s);
                return mode switch
                {
                    TransitionSettings.TransitionMode.MapAreaRandomizer => !def.IsMapAreaTransition,
                    TransitionSettings.TransitionMode.FullAreaRandomizer => !def.IsTitledAreaTransition,
                    TransitionSettings.TransitionMode.RoomRandomizer => false,
                    _ => true,
                };
            }

            foreach (string t in Data.GetRoomTransitionNames())
            {
                if (IsVanillaInMode(t)) rb.EnsureVanillaSourceTransition(t);
            }
        }

        public static void ApplySplitGroupResolver(RequestBuilder rb)
        {
            if (rb.gs.SplitGroupSettings.RandomizeOnStart) rb.gs.SplitGroupSettings.Randomize(rb.rng);

            Dictionary<int, ItemGroupBuilder> splitGroups = new();
            Dictionary<string, Bucket<int>> itemWeightBuilder = new();
            Dictionary<string, Bucket<int>> locationWeightBuilder = new();
            splitGroups.Add(0, rb.MainItemGroup);
            foreach (ItemGroupBuilder igb in rb.EnumerateItemGroups()) // compatibility in case something else already made split groups
            {
                if (igb.label.StartsWith(RBConsts.SplitGroupPrefix) && int.TryParse(igb.label.Substring(RBConsts.SplitGroupPrefix.Length), out int splitGroupIndex) && splitGroupIndex > 0)
                {
                    splitGroups[splitGroupIndex] = igb;
                }
            }

            foreach (PoolDef def in Data.Pools)
            {
                if (rb.gs.SplitGroupSettings.TryGetValue(def, out int value))
                {
                    if (!splitGroups.ContainsKey(value))
                    {
                        splitGroups.Add(value, rb.MainItemStage.AddItemGroup(RBConsts.SplitGroupPrefix + value));
                    }
                    foreach (string s in def.IncludeItems)
                    {
                        if (!itemWeightBuilder.TryGetValue(s, out Bucket<int> b)) itemWeightBuilder.Add(s, b = new());
                        b.Add(value);
                    }
                    foreach (string s in def.IncludeLocations)
                    {
                        if (!locationWeightBuilder.TryGetValue(s, out Bucket<int> b)) locationWeightBuilder.Add(s, b = new());
                        b.Add(value);
                    }
                }
            }

            Dictionary<string, CDFWeightedArray<int>> itemGroups = itemWeightBuilder.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToWeightedArray());
            Dictionary<string, CDFWeightedArray<int>> locationGroups = locationWeightBuilder.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToWeightedArray());

            bool TryGetSplitGroup(RequestBuilder rb, string item, ElementType type, out GroupBuilder gb)
            {
                if (type == ElementType.Unknown)
                {
                    if (locationGroups.ContainsKey(item)) type = ElementType.Location;
                    else if (itemGroups.ContainsKey(item)) type = ElementType.Item;
                }
                if (type == ElementType.Item)
                {
                    if (itemGroups.TryGetValue(item, out CDFWeightedArray<int> arr) && splitGroups.TryGetValue(arr.Next(rb.rng), out ItemGroupBuilder igb))
                    {
                        gb = igb;
                        return true;
                    }
                }
                if (type == ElementType.Location)
                {
                    if (locationGroups.TryGetValue(item, out CDFWeightedArray<int> arr) && splitGroups.TryGetValue(arr.Next(rb.rng), out ItemGroupBuilder igb))
                    {
                        gb = igb;
                        return true;
                    }
                }
                gb = null;
                return false;
            }
            rb.OnGetGroupFor.Subscribe(0f, TryGetSplitGroup);
        }

        public static void ApplyShopDefs(RequestBuilder rb)
        {
            string[] shops = new[]
            {
                LocationNames.Sly, LocationNames.Sly_Key, LocationNames.Iselda, LocationNames.Salubra, LocationNames.Leg_Eater,
            };

            foreach (string s in shops)
            {
                rb.EditLocationRequest(s, info =>
                {
                    info.customPlacementFetch = (factory, placement) =>
                    {
                        if (factory.TryFetchPlacement(s, out AbstractPlacement ap)) return ap;

                        ItemChanger.Placements.ShopPlacement sp = (ItemChanger.Placements.ShopPlacement)factory.FetchOrMakePlacement(s);
                        sp.defaultShopItems = IC.Shops.GetDefaultShopItems(factory.gs);
                        return sp;
                    };
                    info.onRandomizerFinish += placement =>
                    {
                        if (placement.Location is not RandoModLocation rl || placement.Item is not RandoModItem ri || rl.costs == null) return;
                        foreach (LogicGeoCost gc in rl.costs.OfType<LogicGeoCost>())
                        {
                            if (gc.GeoAmount < 0) gc.GeoAmount = GetShopCost(rb.rng, ri);
                        }
                    };
                });
            }

            static int GetShopCost(Random rng, RandoModItem item)
            {
                double pow = 1.2; // setting?

                int cap = item.ItemDef is not null ? item.ItemDef.PriceCap : 500;
                if (cap <= 100) return cap;
                if (item.Required) return rng.PowerLaw(pow, 100, Math.Min(cap, 500)).ClampToMultipleOf(5);
                return rng.PowerLaw(pow, 100, cap).ClampToMultipleOf(5);
            }
        }

        public static void ApplyGrubfatherDef(RequestBuilder rb)
        {
            rb.EditLocationRequest(LocationNames.Grubfather, info =>
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
                    if (factory.TryFetchPlacement(LocationNames.Grubfather, out AbstractPlacement p)) return p;
                    return IC.Grubfather.GetGrubfatherPlacement(factory);
                };
            });
        }

        public static void ApplySeerDef(RequestBuilder rb)
        {
            rb.EditLocationRequest(LocationNames.Seer, info =>
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
                    if (factory.TryFetchPlacement(LocationNames.Seer, out AbstractPlacement p)) return p;
                    return IC.Seer.GetSeerPlacement(factory);
                };
            });
        }

        public static void ApplySalubraCharmDef(RequestBuilder rb)
        {
            rb.EditLocationRequest("Salubra_(Requires_Charms)", info =>
            {
                info.randoLocationCreator += factory => factory.MakeLocation(LocationNames.Salubra);
                info.onRandoLocationCreation += (factory, rl) =>
                {
                    LogicManager lm = factory.lm;
                    Random rng = factory.rng;
                    GenerationSettings gs = factory.gs;
                    rl.AddCost(new SimpleCost(lm.GetTerm("CHARMS"), rng.Next(gs.CostSettings.MinimumCharmCost, gs.CostSettings.MaximumCharmCost + 1)));
                };
                info.customPlacementFetch = (factory, placement) =>
                {
                    return factory.FetchOrMakePlacementWithEvents(LocationNames.Salubra, placement);
                };
            });
        }

        public static void ApplyEggShopDef(RequestBuilder rb)
        {
            rb.EditLocationRequest(LocationNames.Egg_Shop, info =>
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

        public static void ApplyWorldSenseDef(RequestBuilder rb)
        {
            if (rb.gs.PoolSettings.LoreTablets)
            {
                rb.EditLocationRequest(LocationNames.World_Sense, info => info.randoLocationCreator = (factory) => factory.MakeLocation(LocationNames.Lore_Tablet_World_Sense));
                /*
                rb.EditLocationRequest(LocationNames.World_Sense, info => info.customPlacementFetch =
                    (factory, next) => factory.FetchOrMakePlacementWithEvents(LocationNames.Lore_Tablet_World_Sense, next));
                */
            }
        }

        public static void ApplyFocusDef(RequestBuilder rb)
        {
            if (rb.gs.PoolSettings.LoreTablets)
            {
                rb.EditLocationRequest(LocationNames.Focus, info => info.randoLocationCreator = (factory) => factory.MakeLocation(LocationNames.Lore_Tablet_Kings_Pass_Focus));
                /*
                rb.EditLocationRequest(LocationNames.Focus, info => info.customPlacementFetch =
                    (factory, next) => factory.FetchOrMakePlacementWithEvents(LocationNames.Lore_Tablet_Kings_Pass_Focus, next));
                */
            }
        }

        public static void ApplyStartGeo(RequestBuilder rb)
        {
            int minGeo = rb.gs.StartItemSettings.MinimumStartGeo;
            int maxGeo = rb.gs.StartItemSettings.MaximumStartGeo;
            int geo = minGeo < maxGeo ? rb.rng.Next(minGeo, maxGeo) : minGeo;
            if (geo > 0) rb.AddToStart($"{geo}_Geo");
        }

        public static void ApplyStartItemSettings(RequestBuilder rb)
        {
            List<string> startItems = new();
            StartItemSettings ss = rb.gs.StartItemSettings;
            NoveltySettings ns = rb.gs.NoveltySettings;

            void AddUniformlyFrom(IList<string> items, double rate, int cap)
            {
                rb.rng.PermuteInPlace(items);

                int toAdd = 0;
                for (int i = 0; i < items.Count; i++)
                {
                    if (rb.rng.NextDouble() < rate)
                    {
                        // Cursed way to make the distribution a bit more smooth than simply
                        // flipping a coin for each until we reach the cap
                        toAdd = (toAdd + 1) % (cap + 1);
                    }
                }

                for (int i = 0; i < toAdd; i++)
                {
                    startItems.Add(items[i]);
                }
            }

            {
                string[] horizontal = new[] { ItemNames.Mothwing_Cloak, ItemNames.Crystal_Heart };
                switch (ss.HorizontalMovement)
                {
                    case StartItemSettings.StartHorizontalType.None:
                        break;
                    case StartItemSettings.StartHorizontalType.MothwingCloak:
                        startItems.Add(ItemNames.Mothwing_Cloak);
                        break;
                    case StartItemSettings.StartHorizontalType.CrystalHeart:
                        startItems.Add(ItemNames.Crystal_Heart);
                        break;
                    case StartItemSettings.StartHorizontalType.OneRandomItem:
                        startItems.Add(rb.rng.Next(horizontal));
                        break;
                    case StartItemSettings.StartHorizontalType.ZeroOrMore:
                        AddUniformlyFrom(horizontal, 0.5, 2);
                        break;
                    case StartItemSettings.StartHorizontalType.All:
                        startItems.AddRange(horizontal);
                        break;
                }
            }
            {
                string[] vertical = new[] { ItemNames.Mantis_Claw, ItemNames.Monarch_Wings };
                switch (ss.VerticalMovement)
                {
                    case StartItemSettings.StartVerticalType.None:
                        break;
                    case StartItemSettings.StartVerticalType.MantisClaw:
                        startItems.Add(ItemNames.Mantis_Claw);
                        break;
                    case StartItemSettings.StartVerticalType.MonarchWings:
                        startItems.Add(ItemNames.Monarch_Wings);
                        break;
                    case StartItemSettings.StartVerticalType.OneRandomItem:
                        startItems.Add(rb.rng.Next(vertical));
                        break;
                    case StartItemSettings.StartVerticalType.ZeroOrMore:
                        AddUniformlyFrom(vertical, 0.5, 2);
                        break;
                    case StartItemSettings.StartVerticalType.All:
                        startItems.AddRange(vertical);
                        break;
                }
            }
            {
                List<string> charms = Data.GetPoolDef(PoolNames.Charm)
                    .IncludeItems
                    .Except(new[] { ItemNames.Queen_Fragment, ItemNames.King_Fragment, ItemNames.Void_Heart, ItemNames.Grimmchild2 })
                    .ToList();
                charms.Add(rb.gs.PoolSettings.GrimmkinFlames ? ItemNames.Grimmchild1 : ItemNames.Grimmchild2);
                switch (ss.Charms)
                {
                    case StartItemSettings.StartCharmType.None:
                        break;
                    case StartItemSettings.StartCharmType.ZeroOrMore:
                        AddUniformlyFrom(charms, 2.0 / (3.0d * charms.Count), 4);
                        break;
                    case StartItemSettings.StartCharmType.OneRandomItem:
                        startItems.Add(rb.rng.Next(charms));
                        break;
                }
            }
            {
                string[] stags = Data.GetPoolDef(PoolNames.Stag).IncludeItems;
                switch (ss.Stags)
                {
                    case StartItemSettings.StartStagType.None:
                        break;
                    case StartItemSettings.StartStagType.DirtmouthStag:
                        startItems.Add(ItemNames.Dirtmouth_Stag);
                        break;
                    case StartItemSettings.StartStagType.OneRandomStag:
                        startItems.Add(rb.rng.Next(stags));
                        break;
                    case StartItemSettings.StartStagType.ZeroOrMoreRandomStags:
                        AddUniformlyFrom(stags, (double)1 / (double)stags.Length, 3);
                        break;
                    case StartItemSettings.StartStagType.ManyRandomStags:
                        AddUniformlyFrom(stags, 0.4, stags.Length);
                        break;
                    case StartItemSettings.StartStagType.AllStags:
                        startItems.AddRange(stags);
                        break;
                }
            }
            {
                List<string> miscSkill = new()
                {
                    ItemNames.Dream_Nail,
                    ItemNames.Ismas_Tear,
                    ItemNames.Vengeful_Spirit,
                    ItemNames.Desolate_Dive,
                    ItemNames.Howling_Wraiths,
                    ItemNames.Cyclone_Slash,
                    ItemNames.Great_Slash,
                    ItemNames.Dash_Slash
                };
                List<string> miscKey = new()
                {
                    ItemNames.Simple_Key,
                    ItemNames.Elegant_Key,
                    ItemNames.Tram_Pass,
                    ItemNames.Kings_Brand,
                    ItemNames.Love_Key,
                    ItemNames.Lumafly_Lantern,
                    ItemNames.Shopkeepers_Key,
                    ItemNames.City_Crest
                };
                List<string> miscAll = Enumerable.Concat(miscSkill, miscKey).ToList();

                switch (ss.MiscItems)
                {
                    case StartItemSettings.StartMiscItems.None:
                        break;
                    case StartItemSettings.StartMiscItems.DreamNail:
                        startItems.Add(ItemNames.Dream_Nail);
                        break;
                    case StartItemSettings.StartMiscItems.ZeroOrMore:
                        AddUniformlyFrom(miscAll, 1.0 / (double)miscAll.Count, 3);
                        break;
                    case StartItemSettings.StartMiscItems.Many:
                        AddUniformlyFrom(miscAll, 3.0 / (double)miscAll.Count, 8);
                        break;
                    case StartItemSettings.StartMiscItems.DreamNailAndMore:
                        startItems.Add(ItemNames.Dream_Nail);
                        miscAll.Remove(ItemNames.Dream_Nail);
                        miscAll.Add(ItemNames.Dream_Gate);
                        AddUniformlyFrom(miscAll, 1.0 / (double)miscAll.Count, 3);
                        break;
                }
            }

            // If they wanted both sides of the skill they shouldn't have split it
            if (ns.SplitCloak)
            {
                if (startItems.Remove(ItemNames.Mothwing_Cloak))
                {
                    startItems.Add(rb.rng.Next(new[] { ItemNames.Left_Mothwing_Cloak, ItemNames.Right_Mothwing_Cloak }));
                }
            }
            if (ns.SplitClaw)
            {
                if (startItems.Remove(ItemNames.Mantis_Claw))
                {
                    startItems.Add(rb.rng.Next(new[] { ItemNames.Left_Mantis_Claw, ItemNames.Right_Mantis_Claw }));
                }
            }
            if (ns.SplitSuperdash)
            {
                if (startItems.Remove(ItemNames.Crystal_Heart))
                {
                    startItems.Add(rb.rng.Next(new[] { ItemNames.Left_Crystal_Heart, ItemNames.Right_Crystal_Heart }));
                }
            }

            foreach (string item in startItems)
            {
                rb.AddToStart(item);
                rb.GetItemGroupFor(item).Items.Remove(item, 1);
                if (Data.GetItemDef(item).Pool == PoolNames.Charm && rb.rng.NextBool())
                {
                    rb.EditItemRequest(item, info =>
                    {
                        info.realItemCreator = (factory, placement) =>
                        {
                            AbstractItem realItem = factory.MakeItem(item);
                            realItem.AddTag<ItemChanger.Tags.EquipCharmOnGiveTag>();
                            return realItem;
                        };
                    });
                }
            }
        }

        public static void ApplyDownslashStart(RequestBuilder rb)
        {
            if (rb.gs.NoveltySettings.RandomizeNail)
            {
                rb.AddToStart(ItemNames.Downslash);
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
                    if (pool.Name == PoolNames.Flame && rb.gs.PoolSettings.Charms)
                    {
                        foreach (VanillaDef def in pool.Vanilla.Skip(6)) rb.AddToVanilla(def.Item, def.Location);
                    }
                    else foreach (VanillaDef def in pool.Vanilla) rb.AddToVanilla(def.Item, def.Location);
                }
            }
        }

        public static void ApplyPalaceLongLocationSetting(RequestBuilder rb)
        {
            switch (rb.gs.LongLocationSettings.WhitePalaceRando)
            {
                case LongLocationSettings.WPSetting.ExcludeWhitePalace:
                    rb.RemoveItemByName(ItemNames.Soul_Totem_Palace);
                    rb.RemoveItemByName(ItemNames.Lore_Tablet_Palace_Workshop);
                    rb.RemoveItemByName(ItemNames.Lore_Tablet_Palace_Throne);
                    rb.RemoveItemByName(ItemNames.Soul_Totem_Path_of_Pain);
                    rb.RemoveItemByName(ItemNames.Journal_Entry_Seal_of_Binding);
                    rb.RemoveItemByName(ItemNames.Lore_Tablet_Path_of_Pain_Entrance);
                    rb.RemoveLocationsWhere(s => s != LocationNames.King_Fragment && rb.TryGetLocationDef(s, out LocationDef def) && def?.MapArea == "White Palace");
                    break;
                case LongLocationSettings.WPSetting.ExcludePathOfPain:
                    rb.RemoveItemByName(ItemNames.Soul_Totem_Path_of_Pain);
                    rb.RemoveItemByName(ItemNames.Journal_Entry_Seal_of_Binding);
                    rb.RemoveItemByName(ItemNames.Lore_Tablet_Path_of_Pain_Entrance);
                    rb.RemoveLocationsWhere(s => s != LocationNames.King_Fragment && rb.TryGetLocationDef(s, out LocationDef def) && def?.TitledArea == "Path of Pain");
                    break;
            }

            if (rb.gs.LongLocationSettings.WhitePalaceRando == LongLocationSettings.WPSetting.ExcludeWhitePalace)
            {
                rb.UnrandomizeTransitionsWhere(t => Data.GetTransitionDef(t)?.MapArea == "White Palace");
            }
            else if (rb.gs.LongLocationSettings.WhitePalaceRando == LongLocationSettings.WPSetting.ExcludePathOfPain)
            {
                rb.UnrandomizeTransitionsWhere(t => Data.GetTransitionDef(t)?.TitledArea == "Path of Pain");
                rb.UnrandomizeTransitionByName("White_Palace_06[left1]");
            }
        }

        public static void ApplyBossEssenceLongLocationSetting(RequestBuilder rb)
        {
            if (!rb.gs.PoolSettings.BossEssence)
            {
                return;
            }

            switch (rb.gs.LongLocationSettings.BossEssenceRando)
            {
                case LongLocationSettings.BossEssenceSetting.ExcludeAllDreamWarriors:
                    PoolDef warriors = Data.GetPoolDef(PoolNames.DreamWarrior);
                    foreach (string item in warriors.IncludeItems) rb.RemoveItemByName(item);
                    foreach (string loc in warriors.IncludeLocations) rb.RemoveLocationByName(loc);
                    foreach (VanillaDef def in warriors.Vanilla) rb.AddToVanilla(def.Item, def.Location);
                    break;
                case LongLocationSettings.BossEssenceSetting.ExcludeAllDreamBosses:
                    PoolDef bosses = Data.GetPoolDef(PoolNames.DreamBoss);
                    foreach (string item in bosses.IncludeItems) rb.RemoveItemByName(item);
                    foreach (string loc in bosses.IncludeLocations) rb.RemoveLocationByName(loc);
                    foreach (VanillaDef def in bosses.Vanilla) rb.AddToVanilla(def.Item, def.Location);
                    break;
                case LongLocationSettings.BossEssenceSetting.ExcludeZoteAndWhiteDefender:
                    rb.RemoveItemByName(ItemNames.Boss_Essence_White_Defender);
                    rb.RemoveItemByName(ItemNames.Boss_Essence_Grey_Prince_Zote);
                    rb.RemoveLocationByName(LocationNames.Boss_Essence_White_Defender);
                    rb.RemoveLocationByName(LocationNames.Boss_Essence_Grey_Prince_Zote);
                    rb.AddToVanilla(ItemNames.Boss_Essence_White_Defender, LocationNames.Boss_Essence_White_Defender);
                    rb.AddToVanilla(ItemNames.Boss_Essence_Grey_Prince_Zote, LocationNames.Boss_Essence_Grey_Prince_Zote);
                    break;
            }    
        }

        public static void ApplyLongLocationPreviewSettings(RequestBuilder rb)
        {
            LongLocationSettings lls = rb.gs.LongLocationSettings;

            /*
            static void RemoveLocalHint(ICFactory factory, RandoPlacement next, AbstractPlacement placement)
            {
                if (placement is ItemChanger.Placements.IPrimaryLocationPlacement iplp
                && iplp.Location is ItemChanger.Locations.ILocalHintLocation ilhl)
                {
                    ilhl.HintActive = false;
                }
                else
                {
                    LogWarn($"Unable to disable hint on placement {placement.Name}");
                }
            }
            */

            static void RemoveNamePreview(ICFactory factory, RandoPlacement next, AbstractPlacement placement)
            {
                placement.GetOrAddTag<ItemChanger.Tags.DisableItemPreviewTag>();
            }

            static void RemoveCostPreview(ICFactory factory, RandoPlacement next, AbstractPlacement placement)
            {
                placement.GetOrAddTag<ItemChanger.Tags.DisableCostPreviewTag>();
            }

            static void RemoveNameAndCostPreview(ICFactory factory, RandoPlacement next, AbstractPlacement placement)
            {
                placement.GetOrAddTag<ItemChanger.Tags.DisableItemPreviewTag>();
                placement.GetOrAddTag<ItemChanger.Tags.DisableCostPreviewTag>();
            }

            List<string> hintRemoveLocations = new();

            if (!lls.ColosseumPreview)
            {
                hintRemoveLocations.Add(LocationNames.Charm_Notch_Colosseum);
                hintRemoveLocations.Add(LocationNames.Pale_Ore_Colosseum);
            }
            if (!lls.KingFragmentPreview)
            {
                hintRemoveLocations.Add(LocationNames.King_Fragment);
            }
            if (!lls.FlowerQuestPreview)
            {
                hintRemoveLocations.Add(LocationNames.Mask_Shard_Grey_Mourner);
            }
            if (!lls.GreyPrinceZotePreview)
            {
                hintRemoveLocations.Add(LocationNames.Boss_Essence_Grey_Prince_Zote);
            }
            if (!lls.WhisperingRootPreview)
            {
                hintRemoveLocations.AddRange(Data.GetPoolDef(PoolNames.Root).IncludeLocations);
            }
            if (!lls.AbyssShriekPreview)
            {
                hintRemoveLocations.Add(LocationNames.Abyss_Shriek);
            }
            if (!lls.VoidHeartPreview)
            {
                hintRemoveLocations.Add(LocationNames.Void_Heart);
            }
            if (!lls.DreamerPreview)
            {
                hintRemoveLocations.Add(LocationNames.Lurien);
                hintRemoveLocations.Add(LocationNames.Monomon);
                hintRemoveLocations.Add(LocationNames.Herrah);
            }
            if (!lls.GodtunerPreview)
            {
                hintRemoveLocations.Add(LocationNames.Godtuner);
            }
            if (!lls.BasinFountainPreview)
            {
                hintRemoveLocations.Add(LocationNames.Vessel_Fragment_Basin);
            }
            if (!lls.NailmasterPreview)
            {
                hintRemoveLocations.Add(LocationNames.Cyclone_Slash);
                hintRemoveLocations.Add(LocationNames.Great_Slash);
                hintRemoveLocations.Add(LocationNames.Dash_Slash);
            }
            if (!lls.StagPreview)
            {
                hintRemoveLocations.AddRange(Data.GetPoolDef(PoolNames.Stag).IncludeLocations);
                hintRemoveLocations.Add(LocationNames.Elevator_Pass);
            }
            if (!lls.MapPreview)
            {
                hintRemoveLocations.AddRange(Data.GetPoolDef(PoolNames.Map).IncludeLocations);
            }
            if (!lls.LoreTabletPreview)
            {
                // only the spore shroom tablets are technically needed
                // but might as well block previews on all of them
                hintRemoveLocations.AddRange(Data.GetPoolDef(PoolNames.Lore).IncludeLocations);
            }
            if (!lls.DivinePreview)
            {
                hintRemoveLocations.Add(LocationNames.Unbreakable_Heart);
                hintRemoveLocations.Add(LocationNames.Unbreakable_Greed);
                hintRemoveLocations.Add(LocationNames.Unbreakable_Strength);
            }


            foreach (string s in hintRemoveLocations)
            {
                rb.EditLocationRequest(s, info => info.onPlacementFetch += RemoveNamePreview);
            }

            (string, LongLocationSettings.CostItemHintSettings)[] costHints = new[]
            {
                (LocationNames.Sly, lls.GeoShopPreview),
                (LocationNames.Sly_Key, lls.GeoShopPreview),
                (LocationNames.Iselda, lls.GeoShopPreview),
                (LocationNames.Salubra, lls.GeoShopPreview),
                (LocationNames.Leg_Eater, lls.GeoShopPreview),
                (LocationNames.Grubfather, lls.GrubfatherPreview),
                (LocationNames.Seer, lls.SeerPreview),
                (LocationNames.Egg_Shop, lls.EggShopPreview),
            };

            foreach ((string loc, LongLocationSettings.CostItemHintSettings cs) in costHints)
            {
                switch (cs)
                {
                    case LongLocationSettings.CostItemHintSettings.CostAndName:
                        break;
                    case LongLocationSettings.CostItemHintSettings.CostOnly:
                        rb.EditLocationRequest(loc, info => info.onPlacementFetch += RemoveNamePreview);
                        break;
                    case LongLocationSettings.CostItemHintSettings.NameOnly:
                        rb.EditLocationRequest(loc, info => info.onPlacementFetch += RemoveCostPreview);
                        break;
                    case LongLocationSettings.CostItemHintSettings.None:
                        rb.EditLocationRequest(loc, info => info.onPlacementFetch += RemoveNameAndCostPreview);
                        break;
                }
            }
        }

        public static void ApplyDuplicateItemSettings(RequestBuilder rb)
        {
            DuplicateItemSettings ds = rb.gs.DuplicateItemSettings;
            NoveltySettings ns = rb.gs.NoveltySettings;
            CursedSettings cs = rb.gs.CursedSettings;
            PoolSettings ps = rb.gs.PoolSettings;
            List<string> dupes = new();

            if (ds.MothwingCloak && !ns.SplitCloak && !rb.IsAtStart(ItemNames.Mothwing_Cloak))
            {
                dupes.Add(ItemNames.Mothwing_Cloak);
            }
            if (ds.MantisClaw && !ns.SplitClaw && !rb.IsAtStart(ItemNames.Mantis_Claw))
            {
                dupes.Add(ItemNames.Mantis_Claw);
            }
            if (ds.CrystalHeart && !ns.SplitSuperdash && !rb.IsAtStart(ItemNames.Crystal_Heart))
            {
                dupes.Add(ItemNames.Crystal_Heart);
            }
            if (ds.MonarchWings && !rb.IsAtStart(ItemNames.Monarch_Wings))
            {
                dupes.Add(ItemNames.Monarch_Wings);
            }
            if (ds.ShadeCloak && !rb.IsAtStart(ItemNames.Shade_Cloak) && !rb.IsAtStart(ItemNames.Split_Shade_Cloak))
            {
                dupes.Add(ns.SplitCloak ? ItemNames.Split_Shade_Cloak : ItemNames.Shade_Cloak);
            }
            if (ds.DreamNail && !rb.IsAtStart(ItemNames.Dream_Nail))
            {
                dupes.Add(ItemNames.Dream_Nail);
            }
            if (ds.VoidHeart)
            {
                dupes.Add(ItemNames.Void_Heart);
            }
            if (ds.Dreamer && !rb.IsAtStart(ItemNames.Dreamer))
            {
                dupes.Add(ItemNames.Dreamer);
            }
            if (ds.SwimmingItems)
            {
                if (!rb.IsAtStart(ItemNames.Ismas_Tear))
                {
                    dupes.Add(ItemNames.Ismas_Tear);
                }
                if (ns.RandomizeSwim && !rb.IsAtStart(ItemNames.Swim))
                {
                    dupes.Add(ItemNames.Swim);
                }
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
            if (ds.Grimmchild && !rb.IsAtStart(ItemNames.Grimmchild1) && !rb.IsAtStart(ItemNames.Grimmchild2))
            {
                dupes.Add(ps.GrimmkinFlames ? ItemNames.Grimmchild1 : ItemNames.Grimmchild2);
            }
            if (ds.NailArts)
            {
                if (!rb.IsAtStart(ItemNames.Cyclone_Slash)) dupes.Add(ItemNames.Cyclone_Slash);
                if (!rb.IsAtStart(ItemNames.Great_Slash)) dupes.Add(ItemNames.Great_Slash);
                if (!rb.IsAtStart(ItemNames.Dash_Slash)) dupes.Add(ItemNames.Dash_Slash);
            }
            if (ds.CursedNailItems && ns.RandomizeNail)
            {
                if (!rb.IsAtStart(ItemNames.Upslash)) dupes.Add(ItemNames.Upslash);
                if (!rb.IsAtStart(ItemNames.Leftslash)) dupes.Add(ItemNames.Leftslash);
                if (!rb.IsAtStart(ItemNames.Rightslash)) dupes.Add(ItemNames.Rightslash);
            }
            if (ds.DuplicateUniqueKeys)
            {
                if (!rb.IsAtStart(ItemNames.City_Crest)) dupes.Add(ItemNames.City_Crest);
                if (!rb.IsAtStart(ItemNames.Lumafly_Lantern)) dupes.Add(ItemNames.Lumafly_Lantern);
                if (!rb.IsAtStart(ItemNames.Tram_Pass)) dupes.Add(ItemNames.Tram_Pass);
                if (!rb.IsAtStart(ItemNames.Shopkeepers_Key)) dupes.Add(ItemNames.Shopkeepers_Key);
                if (!rb.IsAtStart(ItemNames.Elegant_Key)) dupes.Add(ItemNames.Elegant_Key);
                if (!rb.IsAtStart(ItemNames.Love_Key)) dupes.Add(ItemNames.Love_Key);
                if (!rb.IsAtStart(ItemNames.Kings_Brand)) dupes.Add(ItemNames.Kings_Brand);
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
            if (ns.SplitClaw && !rb.IsAtStart(ItemNames.Mantis_Claw))
            {
                switch (ds.SplitClawHandling)
                {
                    default:
                    case DuplicateItemSettings.SplitItemSetting.NoDupe:
                        break;
                    case DuplicateItemSettings.SplitItemSetting.DupeLeft:
                        if (!rb.IsAtStart(ItemNames.Left_Mantis_Claw)) dupes.Add(ItemNames.Left_Mantis_Claw);
                        break;
                    case DuplicateItemSettings.SplitItemSetting.DupeRight:
                        if (!rb.IsAtStart(ItemNames.Right_Mantis_Claw)) dupes.Add(ItemNames.Right_Mantis_Claw);
                        break;
                    case DuplicateItemSettings.SplitItemSetting.DupeRandom:
                        if (rb.rng.NextBool()) goto case DuplicateItemSettings.SplitItemSetting.DupeLeft;
                        else goto case DuplicateItemSettings.SplitItemSetting.DupeRight;
                    case DuplicateItemSettings.SplitItemSetting.DupeBoth:
                        if (!rb.IsAtStart(ItemNames.Left_Mantis_Claw)) dupes.Add(ItemNames.Left_Mantis_Claw);
                        if (!rb.IsAtStart(ItemNames.Right_Mantis_Claw)) dupes.Add(ItemNames.Right_Mantis_Claw);
                        break;
                }
            }
            if (ns.SplitCloak && !rb.IsAtStart(ItemNames.Mothwing_Cloak))
            {
                switch (ds.SplitCloakHandling)
                {
                    default:
                    case DuplicateItemSettings.SplitItemSetting.NoDupe:
                        break;
                    case DuplicateItemSettings.SplitItemSetting.DupeLeft:
                        if (!rb.IsAtStart(ItemNames.Left_Mothwing_Cloak)) dupes.Add(ItemNames.Left_Mothwing_Cloak);
                        break;
                    case DuplicateItemSettings.SplitItemSetting.DupeRight:
                        if (!rb.IsAtStart(ItemNames.Right_Mothwing_Cloak)) dupes.Add(ItemNames.Right_Mothwing_Cloak);
                        break;
                    case DuplicateItemSettings.SplitItemSetting.DupeRandom:
                        if (rb.rng.NextBool()) goto case DuplicateItemSettings.SplitItemSetting.DupeLeft;
                        else goto case DuplicateItemSettings.SplitItemSetting.DupeRight;
                    case DuplicateItemSettings.SplitItemSetting.DupeBoth:
                        if (!rb.IsAtStart(ItemNames.Left_Mothwing_Cloak)) dupes.Add(ItemNames.Left_Mothwing_Cloak);
                        if (!rb.IsAtStart(ItemNames.Right_Mothwing_Cloak)) dupes.Add(ItemNames.Right_Mothwing_Cloak);
                        break;
                }
            }
            if (ns.SplitSuperdash && !rb.IsAtStart(ItemNames.Crystal_Heart))
            {
                switch (ds.SplitSuperdashHandling)
                {
                    default:
                    case DuplicateItemSettings.SplitItemSetting.NoDupe:
                        break;
                    case DuplicateItemSettings.SplitItemSetting.DupeLeft:
                        if (!rb.IsAtStart(ItemNames.Left_Crystal_Heart)) dupes.Add(ItemNames.Left_Crystal_Heart);
                        break;
                    case DuplicateItemSettings.SplitItemSetting.DupeRight:
                        if (!rb.IsAtStart(ItemNames.Right_Crystal_Heart)) dupes.Add(ItemNames.Right_Crystal_Heart);
                        break;
                    case DuplicateItemSettings.SplitItemSetting.DupeRandom:
                        if (rb.rng.NextBool()) goto case DuplicateItemSettings.SplitItemSetting.DupeLeft;
                        else goto case DuplicateItemSettings.SplitItemSetting.DupeRight;
                    case DuplicateItemSettings.SplitItemSetting.DupeBoth:
                        if (!rb.IsAtStart(ItemNames.Left_Crystal_Heart)) dupes.Add(ItemNames.Left_Crystal_Heart);
                        if (!rb.IsAtStart(ItemNames.Right_Crystal_Heart)) dupes.Add(ItemNames.Right_Crystal_Heart);
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
                rb.ReplaceItem(ItemNames.Grimmchild2, ItemNames.Grimmchild1);
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
                        ItemGroupBuilder gb = rb.GetItemGroupFor(ItemNames.Charm_Notch);
                        gb.Items.Remove(ItemNames.Charm_Notch, 4);
                        gb.Locations.Remove("Salubra_(Requires_Charms)", 4);
                    }
                    break;
                case MiscSettings.SalubraNotchesSetting.Randomized when !rb.gs.PoolSettings.CharmNotches:
                    {
                        ItemGroupBuilder gb = rb.GetItemGroupFor(ItemNames.Charm_Notch);
                        gb.Items.Increment(ItemNames.Charm_Notch, 4);
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
                rb.EditItemRequest(ItemNames.Vengeful_Spirit, info =>
                {
                    info.realItemCreator = (factory, placement) =>
                    {
                        AbstractItem item = factory.MakeItem(ItemNames.Vengeful_Spirit);
                        item.RemoveTags<ItemChanger.Tags.ItemChainTag>();
                        return item;
                    };
                });
                rb.EditItemRequest(ItemNames.Howling_Wraiths, info =>
                {
                    info.realItemCreator = (factory, placement) =>
                    {
                        AbstractItem item = factory.MakeItem(ItemNames.Howling_Wraiths);
                        item.RemoveTags<ItemChanger.Tags.ItemChainTag>();
                        return item;
                    };
                });
                rb.EditItemRequest(ItemNames.Desolate_Dive, info =>
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
                rb.RemoveItemByName(ItemNames.Mantis_Claw);
                rb.RemoveLocationByName(LocationNames.Mantis_Claw);
            }
        }

        public static void ApplySplitCloakFullCloakRemove(RequestBuilder rb)
        {
            if (rb.gs.NoveltySettings.SplitCloak)
            {
                rb.RemoveItemByName(ItemNames.Mothwing_Cloak);
                rb.RemoveItemByName(ItemNames.Shade_Cloak);
            }
        }

        public static void ApplySplitSuperdashFullCrystalHeartRemove(RequestBuilder rb)
        {
            if (rb.gs.NoveltySettings.SplitSuperdash)
            {
                rb.RemoveItemByName(ItemNames.Crystal_Heart);
            }
        }

        public static void ApplySplitCloakShadeCloakRandomize(RequestBuilder rb)
        {
            if (rb.gs.NoveltySettings.SplitCloak)
            {
                rb.EditItemRequest(ItemNames.Split_Shade_Cloak, info =>
                {
                    info.randoItemCreator = factory =>
                    {
                        return new RandoModItem
                        {
                            item = ((SplitCloakItem)factory.lm.GetItem(ItemNames.Split_Shade_Cloak)) with { LeftBiased = rb.rng.NextBool() }
                        };
                    };
                    info.realItemCreator = (factory, next) =>
                    {
                        AbstractItem item = factory.MakeItem(ItemNames.Split_Shade_Cloak);

                        RandoModItem ri = (RandoModItem)next.Item;
                        if (ri is PlaceholderItem pi) ri = pi.innerItem;

                        bool leftBiased;
                        if (ri.item is SplitCloakItem cloak) leftBiased = cloak.LeftBiased;
                        else leftBiased = factory.rb.rng.NextBool();

                        item.GetTag<ItemChanger.Tags.ItemChainTag>().predecessor = leftBiased ? ItemNames.Left_Mothwing_Cloak : ItemNames.Right_Mothwing_Cloak;
                        return item;
                    };
                });
            }
        }

        public static void ApplyProgressiveSplitClaw(RequestBuilder rb)
        {
            rb.EditItemRequest(ItemNames.Left_Mantis_Claw, info => info.realItemCreator = (factory, placement) =>
            {
                AbstractItem item = factory.MakeItem(ItemNames.Left_Mantis_Claw);
                item.AddTag<ItemChanger.Tags.ItemTreeTag>().successors = new[] { ItemNames.Right_Mantis_Claw };
                return item;
            });
            rb.EditItemRequest(ItemNames.Right_Mantis_Claw, info => info.realItemCreator = (factory, placement) =>
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
                rb.ReplaceItem(ItemNames.Mask_Shard, (i) =>
                {
                    int doubleShards = i / 2;
                    int singleShards = i - 2 * doubleShards;

                    return Enumerable.Repeat(ItemNames.Double_Mask_Shard, doubleShards).Concat(Enumerable.Repeat(ItemNames.Mask_Shard, singleShards));
                });
            }
            else if (rb.gs.MiscSettings.MaskShards == MiscSettings.MaskShardType.OneShardPerMask)
            {
                rb.ReplaceItem(ItemNames.Mask_Shard, (i) =>
                {
                    int fullMasks = i / 4;
                    int singleShards = i - 4 * fullMasks;

                    return Enumerable.Repeat(ItemNames.Full_Mask, fullMasks).Concat(Enumerable.Repeat(ItemNames.Mask_Shard, singleShards));
                });
            }
        }

        public static void ApplyVesselFragmentCountSetting(RequestBuilder rb)
        {
            if (rb.gs.MiscSettings.VesselFragments == MiscSettings.VesselFragmentType.TwoFragmentsPerVessel)
            {
                rb.ReplaceItem(ItemNames.Vessel_Fragment, i =>
                {
                    int doubleFragments = i / 2;
                    int singleFragments = i - 2 * doubleFragments;
                    return Enumerable.Repeat(ItemNames.Double_Vessel_Fragment, doubleFragments).Concat(Enumerable.Repeat(ItemNames.Vessel_Fragment, singleFragments));
                });
            }
            else if (rb.gs.MiscSettings.VesselFragments == MiscSettings.VesselFragmentType.OneFragmentPerVessel)
            {
                rb.ReplaceItem(ItemNames.Vessel_Fragment, i =>
                {
                    int fullVessels = i / 3;
                    int singleFragments = i - 3 * fullVessels;
                    return Enumerable.Repeat(ItemNames.Full_Soul_Vessel, fullVessels).Concat(Enumerable.Repeat(ItemNames.Vessel_Fragment, singleFragments));
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
                    if (rb.TryGetLocationDef(l, out LocationDef def) && def.FlexibleCount)
                    {
                        multiSet.Add(l);
                        count += gb.Locations.GetCount(l) - 1;
                    }
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
                PoolDef mimicPool = Data.GetPoolDef(PoolNames.Mimic);
                PoolDef grubPool = Data.GetPoolDef(PoolNames.Grub);
                
                rb.RemoveItemByName(ItemNames.Mimic_Grub);
                foreach (string loc in mimicPool.IncludeLocations) rb.RemoveLocationByName(loc);
                foreach (VanillaDef def in grubPool.Vanilla) rb.RemoveFromVanilla(def.Item, def.Location);

                StageBuilder sb = rb.AddStage(RBConsts.GrubMimicStage);
                ItemGroupBuilder gb = sb.AddItemGroup(RBConsts.GrubMimicGroup);
                int extraMimics = rb.rng.Next(rb.gs.CursedSettings.MaximumGrubsReplacedByMimics + 1);
                extraMimics = Math.Min(extraMimics, 46 - rb.gs.CostSettings.MaximumGrubCost - rb.gs.CostSettings.GrubTolerance);
                gb.Items.Set(ItemNames.Grub, 46 - extraMimics);
                gb.Items.Set(ItemNames.Mimic_Grub, 4 + extraMimics);
                gb.Locations.AddRange(grubPool.IncludeLocations);
                gb.Locations.AddRange(mimicPool.IncludeLocations);

                bool TryMatchGroup(RequestBuilder builder, string item, ElementType type, out GroupBuilder group)
                {
                    if (item == ItemNames.Grub || item == ItemNames.Mimic_Grub)
                    {
                        group = gb;
                        return true;
                    }

                    group = null;
                    return false;
                }
                rb.OnGetGroupFor.Subscribe(-2f, TryMatchGroup);
            }
            else if (rb.gs.CursedSettings.RandomizeMimics)
            {
                ItemGroupBuilder gb = rb.GetItemGroupFor(ItemNames.Grub);
                int availableGrubs = gb.Items.GetCount(ItemNames.Grub);
                int extraMimics = rb.rng.Next(rb.gs.CursedSettings.MaximumGrubsReplacedByMimics + 1);
                extraMimics = Math.Min(extraMimics, 46 - rb.gs.CostSettings.MaximumGrubCost - rb.gs.CostSettings.GrubTolerance);
                extraMimics = Math.Min(extraMimics, availableGrubs);
                gb.Items.Remove(ItemNames.Grub, extraMimics);
                gb.Items.Increment(ItemNames.Mimic_Grub, extraMimics);
            }
        }

        public static void ApplyRandomizeCursedMasks(RequestBuilder rb)
        {
            for (int i = 0; i < rb.gs.CursedSettings.CursedMasks * 4; i++) rb.AddItemByName(ItemNames.Mask_Shard);
        }

        private static void ApplyRandomizeCursedNotches(RequestBuilder rb)
        {
            for (int i = 0; i < rb.gs.CursedSettings.CursedNotches; i++) rb.AddItemByName(ItemNames.Charm_Notch);
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
                        if (majorPenalty && rb.TryGetItemDef(items[i].Name, out ItemDef def) && def.MajorItem)
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
                        if (locations[i] is not RandoModLocation rl) continue;
                        if (rl.LocationDef is not null && rl.LocationDef.AdditionalProgressionPenalty)
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
                gb.LocationPadder ??= (factory, count) => LocationPadder(gb, factory, count);
            }

            IEnumerable<RandoModItem> ItemPadder(RandoFactory factory, int count)
            {
                for (int i = 0; i < count; i++) yield return factory.MakeItem(ItemNames.One_Geo);
            }
            IEnumerable<RandoModLocation> LocationPadder(ItemGroupBuilder gb, RandoFactory factory, int count)
            {
                HashSet<string> multiSet = new();
                foreach (string l in gb.Locations.EnumerateDistinct())
                {
                    if (Data.GetLocationDef(l) is LocationDef def && def.FlexibleCount) multiSet.Add(l);
                }
                if (multiSet.Count == 0) multiSet.Add(LocationNames.Sly);
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

        public static void ApplyTransitionPlacementStrategy(RequestBuilder rb)
        {
            foreach (GroupBuilder gb in rb.EnumerateTransitionGroups())
            {
                gb.strategy ??= rb.gs.ProgressionDepthSettings.GetTransitionPlacementStrategy();
            }
        }

        public static void ApplyConnectAreasPostPermuteEvent(RequestBuilder rb)
        {
            if (rb.gs.TransitionSettings.AreaConstraint != TransitionSettings.AreaConstraintSetting.None)
            {
                Dictionary<string, int> areaOrder = new();
                TransitionSettings.AreaConstraintSetting areaConstraint = rb.gs.TransitionSettings.AreaConstraint;

                foreach (GroupBuilder gb in rb.EnumerateTransitionGroups())
                {
                    if (gb.onPermute == null) gb.onPermute += PostPermute;
                }
                rb.rm.OnNewAttempt += areaOrder.Clear;

                string GetAreaName(string transition)
                {
                    return areaConstraint switch
                    {
                        TransitionSettings.AreaConstraintSetting.MoreConnectedMapAreas => Data.GetTransitionDef(transition)?.MapArea,
                        TransitionSettings.AreaConstraintSetting.MoreConnectedTitledAreas => Data.GetTransitionDef(transition)?.TitledArea,
                        _ => throw new NotImplementedException(),
                    };
                }

                // weakly group transitions by area in the order
                // so that selector eliminates one area before moving onto the next
                // "weakly", since we ideally do not want to prevent bottlenecked layouts like vanilla city
                // note that areaOrder is captured, to synchronize across groups
                void PostPermute(Random rng, RandomizationGroup group)
                {
                    foreach (IRandoItem t in group.Items)
                    {
                        string area = GetAreaName(t.Name);
                        if (string.IsNullOrEmpty(area)) continue;
                        if (!areaOrder.TryGetValue(area, out int modifier)) areaOrder.Add(area, modifier = areaOrder.Count);
                        t.Priority += modifier * 0.8f;
                    }
                    foreach (IRandoLocation t in group.Locations)
                    {
                        string area = GetAreaName(t.Name);
                        if (string.IsNullOrEmpty(area)) continue;
                        if (!areaOrder.TryGetValue(area, out int modifier)) areaOrder.Add(area, modifier = areaOrder.Count);
                        t.Priority += modifier * 0.8f;
                    }
                }
            }
        }

        public static void ApplyAreaConstraint(RequestBuilder rb)
        {
            if (rb.gs.TransitionSettings.AreaConstraint != TransitionSettings.AreaConstraintSetting.None)
            {
                TransitionSettings.AreaConstraintSetting areaConstraint = rb.gs.TransitionSettings.AreaConstraint;
                bool AreasMatch(IRandoItem item, IRandoLocation location)
                {
                    if (Data.GetTransitionDef(item.Name) is not TransitionDef t1
                        || Data.GetTransitionDef(location.Name) is not TransitionDef t2)
                    {
                        return true;
                    }

                    return areaConstraint switch
                    {
                        TransitionSettings.AreaConstraintSetting.MoreConnectedTitledAreas => t1.TitledArea == t2.TitledArea,
                        TransitionSettings.AreaConstraintSetting.MoreConnectedMapAreas => t1.MapArea == t2.MapArea,
                        _ => throw new NotImplementedException(),
                    };
                }

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
        }

        public static void ApplyDerangedConstraint(RequestBuilder rb)
        {
            if (!rb.gs.CursedSettings.Deranged) return;

            static bool NotVanillaTransition(IRandoItem item, IRandoLocation location)
            {
                if (Data.GetTransitionDef(location.Name) is not TransitionDef source)
                {
                    return true;
                }

                return source.VanillaTarget != item.Name;
            }

            Dictionary<string, HashSet<string>> vanillaLookup = Data.Pools
                .SelectMany(p => p.Vanilla)
                .GroupBy(v => v.Location, v => v.Item)
                .ToDictionary(g => g.Key, g => new HashSet<string>(g));
            bool NotVanillaLocation(IRandoItem item, IRandoLocation location)
            {
                return !vanillaLookup.TryGetValue(location.Name, out HashSet<string> items)
                    || !items.Contains(item.Name);
            }

            foreach (GroupBuilder gb in rb.EnumerateTransitionGroups())
            {
                if (gb.strategy is DefaultGroupPlacementStrategy dgps)
                {
                    dgps.Constraints += NotVanillaTransition;
                }
            }
            foreach (ItemGroupBuilder gb in rb.EnumerateItemGroups())
            {
                if (gb.strategy is DefaultGroupPlacementStrategy dgps)
                {
                    dgps.Constraints += NotVanillaLocation;
                }
            }
        }
    }
}
