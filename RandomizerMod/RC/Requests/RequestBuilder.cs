using RandomizerCore;
using RandomizerCore.Extensions;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;
using RandomizerCore.Randomization;
using RandomizerCore.Logic;
using System.Collections.ObjectModel;
using ItemChanger;

namespace RandomizerMod.RC
{
    public delegate bool ItemMatchHandler(string name, out ItemRequestInfo info);
    public delegate bool LocationMatchHandler(string name, out LocationRequestInfo info);

    /// <summary>
    /// Class used to build the request that is passed to the randomizer. The OnUpdate event allows modification of the request as it is built.
    /// </summary>
    public class RequestBuilder
    {
        public void Run(out RandomizationStage[] stages, out List<RandoPlacement> vanilla)
        {
            HandleUpdateEvents();

            RandoFactory factory = new(this);
            stages = Stages.Select(s => s.ToRandomizationStage(factory)).ToArray();
            foreach (RandomizationStage stage in stages) rng.PermuteInPlace(stage.groups); // random tiebreakers between groups

            vanilla = Vanilla.EnumerateWithMultiplicity()
                .Select(v => !Data.IsTransition(v.Item)
                ? new RandoPlacement(factory.MakeItem(v.Item), factory.MakeLocation(v.Location))
                : new RandoPlacement(factory.MakeTransition(v.Item), factory.MakeTransition(v.Location)))
                .ToList();
        }


        private readonly List<StageBuilder> _stages;
        public readonly ReadOnlyCollection<StageBuilder> Stages;
        public readonly StageBuilder MainItemStage;
        public readonly StageBuilder MainTransitionStage;
        public readonly ItemGroupBuilder MainItemGroup;

        /// <summary>
        /// Enumerates the GroupBuilders of type ItemGroupBuilder.
        /// </summary>
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

        /// <summary>
        /// Enumerates the GroupBuilers not of type ItemGroupBuilder.
        /// </summary>
        public IEnumerable<GroupBuilder> EnumerateTransitionGroups()
        {
            foreach (StageBuilder sb in Stages)
            {
                foreach (GroupBuilder gb in sb.Groups)
                {
                    if (gb is not ItemGroupBuilder) yield return gb;
                }
            }
        }


        public readonly Dictionary<string, ItemRequestInfo> ItemDefs = new();
        public readonly List<ItemMatchHandler> ItemMatchers = new();

        public readonly Dictionary<string, LocationRequestInfo> LocationDefs = new();
        public readonly List<LocationMatchHandler> LocationMatchers = new();

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

            MainItemStage = new(RBConsts.MainItemStage);
            MainItemGroup = new()
            {
                label = RBConsts.MainItemGroup,
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

        public void EditItemInfo(string name, Action<ItemRequestInfo> edit)
        {
            if (!ItemDefs.TryGetValue(name, out ItemRequestInfo info))
            {
                info = new();
                ItemDefs.Add(name, info);
            }
            edit(info);
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

        public void EditLocationInfo(string name, Action<LocationRequestInfo> edit)
        {
            if (!LocationDefs.TryGetValue(name, out LocationRequestInfo info))
            {
                info = new();
                LocationDefs.Add(name, info);
            }
            edit(info);
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
            BuiltinRequests.ApplyAll();
        }
    }
}
