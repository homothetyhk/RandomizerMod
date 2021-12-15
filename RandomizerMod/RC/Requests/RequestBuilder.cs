using RandomizerCore;
using RandomizerCore.Extensions;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;
using RandomizerCore.Randomization;
using RandomizerCore.Logic;
using System.Collections.ObjectModel;

namespace RandomizerMod.RC
{
    public class RequestBuilder
    {
        public RandomizationStage[] Run()
        {
            HandleUpdateEvents();

            RandoFactory factory = new(this);
            RandomizationStage[] stages = Stages.Select(s => s.ToRandomizationStage(factory)).ToArray();
            foreach (RandomizationStage stage in stages) rng.PermuteInPlace(stage.groups); // random tiebreakers between groups
            return stages;
        }


        /*
        public const string MainItemGroup = "Main Item Group";
        public const string MainItemStage = "Main Item Stage";

        public const string LeftTransitionGroup = "Left";
        public const string RightTransitionGroup = "Right";
        public const string TopTransitionGroup = "Top";
        public const string BotTransitionGroup = "Bot";
        public const string OneWayTransitionGroup = "One Way";
        public const string MainTransitionStage = "Main Transition Stage";
        */

        /*
        public readonly Dictionary<string, StageRequest> StageRequests = new();
        public readonly Dictionary<string, GroupRequest> GroupRequests = new();
        */

        /*
        public readonly Bucket<string> Items = new();
        public readonly Dictionary<string, ItemRequestInfo> ItemDefs = new();
        public readonly Bucket<string> Locations = new();
        public readonly Dictionary<string, LocationRequestInfo> LocationDefs = new();
        public readonly Bucket<string> StartItems = new();
        public readonly Bucket<VanillaRequest> Vanilla = new();
        public readonly Bucket<string> Transitions = new();
        */
        private readonly List<StageBuilder> _stages;
        public readonly ReadOnlyCollection<StageBuilder> Stages;
        public readonly StageBuilder MainItemStage;
        public readonly StageBuilder MainTransitionStage;
        public readonly ItemGroupBuilder MainItemGroup;

        public IEnumerable<ItemGroupBuilder> EnumerateItemGroups()
        {
            foreach (StageBuilder sb in Stages)
            {
                foreach (GroupBuilder gb in sb.Groups)
                {
                    if (gb is ItemGroupBuilder igb) yield return igb;
                }
            }
        }

        public IEnumerable<TransitionGroupBuilder> EnumerateTransitionGroups()
        {
            foreach (StageBuilder sb in Stages)
            {
                foreach (GroupBuilder gb in sb.Groups)
                {
                    if (gb is TransitionGroupBuilder tgb) yield return tgb;
                }
            }
        }


        public readonly Dictionary<string, ItemRequestInfo> ItemDefs = new();
        public readonly Dictionary<string, LocationRequestInfo> LocationDefs = new();
        public readonly Bucket<string> StartItems = new();
        public readonly Bucket<VanillaRequest> Vanilla = new();

        public readonly GenerationSettings gs;
        public readonly LogicManager lm;
        public readonly Random rng;

        private static readonly List<string> _set = new(); // used as a utility for several methods

        public RequestBuilder(GenerationSettings gs, LogicManager lm)
        {
            this.gs = gs;
            this.lm = lm;
            rng = new(gs.Seed + 11);
            _stages = new();
            Stages = new(_stages);

            MainItemStage = new("Main Item Stage");
            MainItemGroup = new()
            {
                label = "Main Item Group",
                stageLabel = "Main Item Stage",
            };
            MainItemStage.Add(MainItemGroup);
            _stages.Add(MainItemStage);
        }

        public enum ElementType
        {
            Unknown,
            Item,
            Location,
            Transition
        }

        public StageBuilder AddStage(string name)
        {
            return InsertStage(_stages.Count, name);
        }

        public StageBuilder InsertStage(int index, string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (_stages.Any(s => s.label == name)) throw new ArgumentException(nameof(name));
            if (index < 0 || index > _stages.Count) throw new ArgumentOutOfRangeException(nameof(index));

            StageBuilder sb = new(name);
            _stages.Insert(index, sb);
            return sb;
        }

        public GroupBuilder GetGroupFor(string name, ElementType type = ElementType.Unknown)
        {
            foreach (GroupResolver resolver in _onGetGroupForOwner.GetSubscriberList())
            {
                if (resolver(this, name, type, out GroupBuilder gb)) return gb;
            }

            if (Data.IsItem(name) || Data.IsLocation(name))
            {
                return MainItemGroup;
            }

            return null;
        }


        public ItemGroupBuilder GetItemGroupFor(string name)
        {
            if (GetGroupFor(name, ElementType.Item) is ItemGroupBuilder igb) return igb;
            return MainItemGroup;
        }

        public ItemGroupBuilder GetLocationGroupFor(string name)
        {
            if (GetGroupFor(name, ElementType.Location) is ItemGroupBuilder igb) return igb;
            return MainItemGroup;
        }

        public void AddItemByName(string name)
        {
            GetItemGroupFor(name).Items.Add(name);
        }

        public void AddItemByName(string name, int count)
        {
            GetItemGroupFor(name).Items.Increment(name, count);
        }

        public void RemoveItemByName(string name)
        {
            foreach (ItemGroupBuilder gb in EnumerateItemGroups()) gb.Items.RemoveAll(name);
        }

        public bool TryGetItemDef(string name, out ItemRequestInfo def)
        {
            return ItemDefs.TryGetValue(name, out def);
        }

        public void SetItemDef(string name, ItemRequestInfo def)
        {
            ItemDefs[name] = def;
        }

        public void AddLocationByName(string name)
        {
            GetLocationGroupFor(name).Locations.Add(name);
        }

        public void AddLocationByName(string name, int count)
        {
            GetLocationGroupFor(name).Locations.Increment(name, count);
        }

        public void RemoveLocationByName(string name)
        {
            foreach (ItemGroupBuilder gb in EnumerateItemGroups()) gb.Locations.RemoveAll(name);
        }

        public void RemoveLocationByName(string name, int count)
        {
            GetLocationGroupFor(name).Locations.Remove(name, count);
        }

        public bool TryGetLocationDef(string name, out LocationRequestInfo info)
        {
            return LocationDefs.TryGetValue(name, out info);
        }

        public void SetLocationDef(string name, LocationRequestInfo info)
        {
            LocationDefs[name] = info;
        }

        public void RemoveItemsWhere(Predicate<string> selector)
        {
            foreach (ItemGroupBuilder gb in EnumerateItemGroups())
            {
                if (_set.Count != 0) _set.Clear();
                foreach (string s in gb.Items.EnumerateDistinct())
                {
                    if (selector(s)) _set.Add(s);
                }
                foreach (string s in _set) gb.Items.RemoveAll(s);
            }
            _set.Clear();
        }

        public void ReplaceItem(string name, string replaceWith)
        {
            foreach (ItemGroupBuilder gb in EnumerateItemGroups())
            {
                gb.Items.Replace(name, replaceWith);
            }
        }

        public void ReplaceItem(string name, Func<int, IEnumerable<string>> replacer)
        {
            foreach (ItemGroupBuilder gb in EnumerateItemGroups())
            {
                gb.Items.Replace(name, replacer);
            }
        }

        public void ReplaceItem(Predicate<string> selector, Func<string, int, IEnumerable<string>> replacer)
        {
            foreach (ItemGroupBuilder gb in EnumerateItemGroups())
            {
                if (_set.Count != 0) _set.Clear();

                foreach (string s in gb.Items.EnumerateDistinct())
                {
                    if (selector(s)) _set.Add(s);
                }
                foreach (string s in _set)
                {
                    int count = gb.Items.GetCount(s);
                    gb.Items.RemoveAll(s);
                    gb.Items.AddRange(replacer(s, count));
                }
            }
            _set.Clear();
        }

        public void RemoveLocationsWhere(Predicate<string> selector)
        {
            foreach (ItemGroupBuilder gb in EnumerateItemGroups())
            {
                if (_set.Count != 0) _set.Clear();
                foreach (string s in gb.Locations.EnumerateDistinct())
                {
                    if (selector(s)) _set.Add(s);
                }
                foreach (string s in _set) gb.Locations.RemoveAll(s);
            }
            _set.Clear();
        }

        public void ReplaceLocation(string name, string replaceWith)
        {
            foreach (ItemGroupBuilder gb in EnumerateItemGroups())
            {
                gb.Locations.Replace(name, replaceWith);
            }
        }

        public void ReplaceLocation(string name, Func<int, IEnumerable<string>> replacer)
        {
            foreach (ItemGroupBuilder gb in EnumerateItemGroups())
            {
                gb.Locations.Replace(name, replacer);
            }
        }

        public void ReplaceLocation(Predicate<string> selector, Func<string, int, IEnumerable<string>> replacer)
        {
            foreach (ItemGroupBuilder gb in EnumerateItemGroups())
            {
                if (_set.Count != 0) _set.Clear();

                foreach (string s in gb.Locations.EnumerateDistinct())
                {
                    if (selector(s)) _set.Add(s);
                }
                foreach (string s in _set)
                {
                    int count = gb.Locations.GetCount(s);
                    gb.Locations.RemoveAll(s);
                    gb.Locations.AddRange(replacer(s, count));
                }
            }
            _set.Clear();
        }

        public void AddToVanilla(string item, string location)
        {
            Vanilla.Add(new(item, location));
        }

        public void AddToStart(string item)
        {
            StartItems.Add(item);
        }

        public void AddToStart(string item, int count)
        {
            StartItems.Increment(item, count);
        }


        public static void ApplyTransitionSettings(RequestBuilder rb)
        {
            TransitionSettings ts = rb.gs.TransitionSettings;

            if (ts.Mode == TransitionSettings.TransitionMode.None) return;
            string stageLabel = "Main Transition Stage";
            StageBuilder sb = rb.AddStage(stageLabel);
            IEnumerable<TransitionDef> transitions = (ts.Mode == TransitionSettings.TransitionMode.AreaRandomizer ? Data.GetAreaTransitionNames() : Data.GetRoomTransitionNames())
                .Select(t => Data.GetTransitionDef(t));

            TransitionGroupBuilder oneWays = new()
            {
                label = "One Way Transitions",
                stageLabel = stageLabel,
            };

            if (ts.Matched)
            {
                SymmetricTransitionGroupBuilder horizontal = new()
                {
                    label = "Left -> Right",
                    reverseLabel = "Right -> Left",
                    coupled = ts.Coupled,
                    stageLabel = stageLabel
                };
                SymmetricTransitionGroupBuilder vertical = new()
                {
                    label = "Top -> Bot",
                    reverseLabel = "Bot -> Top",
                    coupled = ts.Coupled,
                    stageLabel = stageLabel
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
                foreach (TransitionDef def in tops) vertical.Group1.Add(def.Name);
                foreach (TransitionDef def in bots) vertical.Group2.Add(def.Name);

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
                    label = "Two Way Transitions",
                    stageLabel = stageLabel,
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


        public static void ApplyGrubfatherDef(RequestBuilder rb)
        {
            rb.SetLocationDef("Grubfather", new LocationRequestInfo
            {
                randoLocationCreator = factory =>
                {
                    LogicManager lm = factory.lm;
                    Random rng = factory.rng;
                    GenerationSettings gs = factory.gs;
                    RandoLocation rl = factory.MakeLocationInternal("Grubfather");
                    rl.AddCost(new SimpleCost(lm.GetTerm("GRUBS"), rng.Next(gs.CostSettings.MinimumGrubCost, gs.CostSettings.MaximumGrubCost + 1)));
                    return rl;
                },
                realPlacementCreator = args =>
                {
                    
                },
            });
        }

        public static void ApplySeerDef(RequestBuilder rb)
        {
            rb.SetLocationDef("Seer", new LocationRequestInfo
            {
                randoLocationCreator = factory =>
                {
                    LogicManager lm = factory.lm;
                    Random rng = factory.rng;
                    GenerationSettings gs = factory.gs;
                    RandoLocation rl = factory.MakeLocationInternal("Grubfather");
                    rl.AddCost(new SimpleCost(lm.GetTerm("ESSENCE"), rng.Next(gs.CostSettings.MinimumEssenceCost, gs.CostSettings.MaximumEssenceCost + 1)));
                    return rl;
                },
                realPlacementCreator = args =>
                {

                },
            });
        }

        public static void ApplySalubraCharmDef(RequestBuilder rb)
        {
            rb.SetLocationDef("Seer", new LocationRequestInfo
            {
                randoLocationCreator = factory =>
                {
                    LogicManager lm = factory.lm;
                    Random rng = factory.rng;
                    GenerationSettings gs = factory.gs;
                    RandoLocation rl = factory.MakeLocationInternal("Grubfather");
                    rl.AddCost(new SimpleCost(lm.GetTerm("CHARMS"), rng.Next(gs.CostSettings.MinimumCharmCost, gs.CostSettings.MaximumCharmCost + 1)));
                    return rl;
                },
                realPlacementCreator = args =>
                {

                },
            });
        }

        public static void ApplyEggShopDef(RequestBuilder rb)
        {
            rb.SetLocationDef("Seer", new LocationRequestInfo
            {
                randoLocationCreator = factory =>
                {
                    LogicManager lm = factory.lm;
                    Random rng = factory.rng;
                    GenerationSettings gs = factory.gs;
                    RandoLocation rl = factory.MakeLocationInternal("Grubfather");
                    rl.AddCost(new SimpleCost(lm.GetTerm("RANCIDEGGS"), rng.Next(gs.CostSettings.MinimumEggCost, gs.CostSettings.MaximumEggCost + 1)));
                    return rl;
                },
                realPlacementCreator = args =>
                {

                },
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
                    foreach (PoolDef.StringILP p in pool.vanilla) rb.AddToVanilla(p.item, p.location);
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
                    string name = $"Dupe[{dupe}]";
                    rb.SetItemDef(name, new ItemRequestInfo
                    {
                        randoItemCreator = (args) => new PlaceholderItem(new RandoItem { item = args.lm.GetItem(dupe) }),
                        realItemCreator = (args) => args.GetDefault(dupe),
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
                if (leftBiased) return; // the serialized Split Shade Cloak template is left biased.

                rb.SetItemDef("Split_Shade_Cloak", new ItemRequestInfo
                {
                    randoItemCreator = (args) => new RandoItem { item = ((SplitCloakItem)args.lm.GetItem("Split_Shade_Cloak")) with { LeftBiased = false } },
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
                // TODO: this doesn't handle double/full mask shards/vessel fragments. Can this be done in a better way?
            }
        }

        public delegate bool GroupResolver(RequestBuilder rb, string item, ElementType type, out GroupBuilder gb);
        public static readonly PriorityEvent<GroupResolver> OnGetGroupFor = new(out _onGetGroupForOwner);
        private static readonly PriorityEvent<GroupResolver>.IPriorityEventOwner _onGetGroupForOwner;

        public delegate void RequestBuilderUpdateHandler(RequestBuilder rb);
        public static readonly PriorityEvent<RequestBuilderUpdateHandler> OnUpdate = new(out _onUpdateOwner);
        private static readonly PriorityEvent<RequestBuilderUpdateHandler>.IPriorityEventOwner _onUpdateOwner;
        protected void HandleUpdateEvents()
        {
            foreach (var d in _onUpdateOwner?.GetSubscriberList())
            {
                d?.Invoke(this);
            }
        }

        static RequestBuilder()
        {
            OnUpdate.Subscribe(-1000f, ApplyTransitionSettings);

            OnUpdate.Subscribe(0f, ApplyPoolSettings);
            OnUpdate.Subscribe(0f, ApplyGrimmchildSetting);
            OnUpdate.Subscribe(0f, ApplySplitCloakShadeCloakRandomize);

            OnUpdate.Subscribe(10f, ApplyMaskShardCountSetting);
            OnUpdate.Subscribe(10f, ApplyVesselFragmentCountSetting);

            OnUpdate.Subscribe(15f, ApplySplitClawFullClawRemove);
            OnUpdate.Subscribe(15f, ApplySplitCloakFullCloakRemove);
            OnUpdate.Subscribe(15f, ApplySpellRemove);

            OnUpdate.Subscribe(16f, ApplyJunkItemRemove);

            OnUpdate.Subscribe(20f, ApplyDuplicateItemSettings);

            OnUpdate.Subscribe(30f, ApplyLongLocationSettings);
        }
    }
}
