using RandomizerMod.Settings;

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
        private static Dictionary<string, ItemDef> _items;

        // Locations
        private static Dictionary<string, LocationDef> _locations;

        // Transitions
        private static Dictionary<string, TransitionDef> _transitions;

        // Rooms
        private static Dictionary<string, RoomDef> _rooms;

        // Starts
        private static Dictionary<string, StartDef> _starts;

        // Logic Settings
        private static Dictionary<string, string> _logicSettings; // name in logic --> settings path

        // Costs
        private static Dictionary<string, CostDef> _costs;

        private static Dictionary<string, PoolDef> _pools;
        private static PoolDef[] __pools;
        public static IEnumerable<PoolDef> Pools => __pools;

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

            LogWarn($"Unable to find LocationDef for {name}.");
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
        public static void Load()
        {
            _items = JsonUtil.Deserialize<Dictionary<string, ItemDef>>("RandomizerMod.Resources.Data.items.json");
            _locations = JsonUtil.Deserialize<Dictionary<string, LocationDef>>("RandomizerMod.Resources.Data.locations.json");
            _logicSettings = JsonUtil.Deserialize<Dictionary<string, string>>("RandomizerMod.Resources.Data.logic_settings.json");
            __pools = JsonUtil.Deserialize<PoolDef[]>("RandomizerMod.Resources.Data.pools.json");
            _pools = __pools.ToDictionary(def => def.Name);
            _starts = JsonUtil.Deserialize<Dictionary<string, StartDef>>("RandomizerMod.Resources.Data.starts.json");
            _transitions = JsonUtil.Deserialize<Dictionary<string, TransitionDef>>("RandomizerMod.Resources.Data.transitions.json");
            _rooms = JsonUtil.Deserialize<Dictionary<string, RoomDef>>("RandomizerMod.Resources.Data.rooms.json");
            _costs = JsonUtil.Deserialize<Dictionary<string, CostDef>>("RandomizerMod.Resources.Data.costs.json");
        }

    }
}
