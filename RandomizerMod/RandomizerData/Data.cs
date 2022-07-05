using RandomizerMod.Settings;
using System.Collections.ObjectModel;

namespace RandomizerMod.RandomizerData
{
    public struct WaypointDef
    {
        public string name;
        public string itemLogic;
        public string areaLogic;
    }

    public static class Data
    {
        // Items
        public static ReadOnlyDictionary<string, ItemDef> Items { get; private set; }
        private static Dictionary<string, ItemDef> _items;

        // Locations
        public static ReadOnlyDictionary<string, LocationDef> Locations { get; private set; }
        private static Dictionary<string, LocationDef> _locations;

        // Transitions
        public static ReadOnlyDictionary<string, TransitionDef> Transitions { get; private set; }
        private static Dictionary<string, TransitionDef> _transitions;

        // Rooms
        public static ReadOnlyDictionary<string, RoomDef> Rooms { get; private set; }
        private static Dictionary<string, RoomDef> _rooms;

        // Starts
        public static ReadOnlyDictionary<string, StartDef> Starts { get; private set; }
        private static Dictionary<string, StartDef> _starts;

        // Logic Settings
        public static ReadOnlyDictionary<string, string> LogicSettings { get; private set; }
        private static Dictionary<string, string> _logicSettings; // name in logic --> settings path

        // Costs
        public static ReadOnlyDictionary<string, CostDef> Costs { get; private set; }
        private static Dictionary<string, CostDef> _costs;

        public static ReadOnlyDictionary<string, PoolDef> PoolLookup { get; private set; }
        private static Dictionary<string, PoolDef> _pools;
        public static ReadOnlyCollection<PoolDef> PoolList { get; private set; }
        private static PoolDef[] __pools;
        public static IEnumerable<PoolDef> Pools => PoolList;

        #region Item Methods

        public static ItemDef GetItemDef(string name)
        {
            if (_items.TryGetValue(name, out var def)) return def;
            return null;
        }

        public static ItemDef[] GetItemArray()
        {
            return _items.Values.ToArray();
        }

        public static bool IsItem(string item)
        {
            return _items.ContainsKey(item);
        }

        #endregion
        #region Location Methods

        public static LocationDef GetLocationDef(string name)
        {
            if (_locations.TryGetValue(name, out var def)) return def;
            return null;
        }


        public static LocationDef[] GetLocationArray()
        {
            return _locations.Values.ToArray();
        }

        public static bool IsLocation(string location)
        {
            return _locations.ContainsKey(location);
        }

        #endregion
        #region Transition Methods
        public static TransitionDef GetTransitionDef(string name)
        {
            if (_transitions.TryGetValue(name, out TransitionDef def)) return def;
            return null;
        }

        public static IEnumerable<string> GetMapAreaTransitionNames()
        {
            return _transitions.Where(kvp => kvp.Value.IsMapAreaTransition).Select(kvp => kvp.Key);
        }

        public static IEnumerable<string> GetAreaTransitionNames()
        {
            return _transitions.Where(kvp => kvp.Value.IsTitledAreaTransition).Select(kvp => kvp.Key);
        }

        public static IEnumerable<string> GetRoomTransitionNames()
        {
            return _transitions.Keys;
        }

        public static bool IsMapAreaTransition(string str)
        {
            return _transitions.TryGetValue(str, out TransitionDef def) && def.IsMapAreaTransition;
        }

        public static bool IsAreaTransition(string str)
        {
            return _transitions.TryGetValue(str, out TransitionDef def) && def.IsTitledAreaTransition;
        }

        public static bool IsTransition(string str)
        {
            return _transitions.ContainsKey(str);
        }

        public static bool IsTransitionWithEntry(string str)
        {
            return _transitions.TryGetValue(str, out var def) && def.Sides != TransitionSides.OneWayOut;
        }

        public static bool IsExitOnlyTransition(string str)
        {
            return _transitions.TryGetValue(str, out var def) && def.Sides == TransitionSides.OneWayOut;
        }

        public static bool IsEnterOnlyTransition(string str)
        {
            return _transitions.TryGetValue(str, out var def) && def.Sides == TransitionSides.OneWayIn;
        }
        #endregion

        #region Room Methods

        public static RoomDef GetRoomDef(string name)
        {
            if (name is null)
            {
                return null;
            }
            if (!_rooms.TryGetValue(name, out RoomDef def))
            {
                return null;
            }
            return def;
        }

        public static bool IsRoom(string str)
        {
            return str is not null && _rooms.ContainsKey(str);
        }

        #endregion

        #region Start Methods

        public static bool IsStart(string str)
        {
            return _starts.ContainsKey(str);
        }

        public static StartDef GetStartDef(string str)
        {
            if (_starts.TryGetValue(str, out StartDef def)) return def;
            return null;
        }

        public static IEnumerable<string> GetStartNames()
        {
            return _starts.Keys;
        }

        #endregion

        #region Logic Settings Methods

        public static bool IsLogicSetting(string str)
        {
            return _logicSettings.ContainsKey(str);
        }

        public static string[] GetLogicNames()
        {
            return _logicSettings.Keys.ToArray();
        }

        public static IEnumerable<string> GetApplicableLogicSettings(GenerationSettings settings)
        {
            return _logicSettings.Where(kvp => (bool)Util.Get(settings, kvp.Value)).Select(kvp => kvp.Key);
        }

        #endregion

        #region Cost Methods

        public static bool TryGetCost(string name, out CostDef def) => _costs.TryGetValue(name, out def);

        #endregion

        public static PoolDef GetPoolDef(string name)
        {
            if (_pools.TryGetValue(name, out var def)) return def;
            return null;
        }

        private static bool _loaded;
        public static void Load()
        {
            if (_loaded) return;

            _items = JsonUtil.Deserialize<Dictionary<string, ItemDef>>("RandomizerMod.Resources.Data.items.json");
            Items = new(_items);

            _locations = JsonUtil.Deserialize<Dictionary<string, LocationDef>>("RandomizerMod.Resources.Data.locations.json");
            Locations = new(_locations);

            _logicSettings = JsonUtil.Deserialize<Dictionary<string, string>>("RandomizerMod.Resources.Data.logic_settings.json");
            LogicSettings = new(_logicSettings);

            __pools = JsonUtil.Deserialize<PoolDef[]>("RandomizerMod.Resources.Data.pools.json");
            PoolList = new(__pools);

            _pools = __pools.ToDictionary(def => def.Name);
            PoolLookup = new(_pools);

            _starts = JsonUtil.Deserialize<Dictionary<string, StartDef>>("RandomizerMod.Resources.Data.starts.json");
            Starts = new(_starts);

            _transitions = JsonUtil.Deserialize<Dictionary<string, TransitionDef>>("RandomizerMod.Resources.Data.transitions.json");
            Transitions = new(_transitions);

            _rooms = JsonUtil.Deserialize<Dictionary<string, RoomDef>>("RandomizerMod.Resources.Data.rooms.json");
            Rooms = new(_rooms);

            _costs = JsonUtil.Deserialize<Dictionary<string, CostDef>>("RandomizerMod.Resources.Data.costs.json");
            Costs = new(_costs);

            _loaded = true;
        }

    }
}
