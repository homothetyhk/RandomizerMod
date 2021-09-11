using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using static RandomizerMod.LogHelper;
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

        // Starts
        private static Dictionary<string, StartDef> _starts;

        // Logic Settings
        private static Dictionary<string, string> _logicSettings; // name in logic --> settings path

        private static PoolDef[] __pools;
        public static IEnumerable<PoolDef> Pools => __pools;

        #region Item Methods

        public static ItemDef GetItemDef(string name)
        {
            if (_items.TryGetValue(name, out var def)) return def;
            
            LogWarn($"Unable to find ItemDef for {name}.");
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

            LogWarn($"Unable to find TransitionDef for {name}.");
            return null;
        }

        public static IEnumerable<string> GetAreaTransitionNames()
        {
            return _transitions.Where(kvp => kvp.Value.isAreaTransition).Select(kvp => kvp.Key);
        }

        public static IEnumerable<string> GetRoomTransitionNames()
        {
            return _transitions.Keys;
        }

        public static bool IsAreaTransition(string str)
        {
            return _transitions.TryGetValue(str, out TransitionDef def) && def.isAreaTransition;
        }

        public static bool IsTransition(string str)
        {
            return _transitions.ContainsKey(str);
        }

        public static bool IsTransitionWithEntry(string str)
        {
            return _transitions.TryGetValue(str, out var def) && def.sides != TransitionSides.OneWayOut;
        }

        public static bool IsExitOnlyTransition(string str)
        {
            return _transitions.TryGetValue(str, out var def) && def.sides == TransitionSides.OneWayOut;
        }

        public static bool IsEnterOnlyTransition(string str)
        {
            return _transitions.TryGetValue(str, out var def) && def.sides == TransitionSides.OneWayIn;
        }
        #endregion
        #region Start Methods

        public static bool IsStart(string str)
        {
            return _starts.ContainsKey(str);
        }

        public static StartDef GetStartDef(string str)
        {
            if (_starts.TryGetValue(str, out var def) && def is StartDef) return def;
            else if (_starts.ContainsKey(str)) LogWarn("Null start " + str);

            LogWarn($"Unable to find StartDef for {str}.");
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

        public static IEnumerable<string> GetApplicableLogicSettings(Settings.GenerationSettings settings)
        {
            return _logicSettings.Where(kvp => (bool)Settings.Util.Get(settings, kvp.Value)).Select(kvp => kvp.Key);
        }

        #endregion

        public static void Load()
        {
            _items = JsonUtil.Deserialize<Dictionary<string, ItemDef>>("RandomizerMod.Resources.items.json");
            _locations = JsonUtil.Deserialize<Dictionary<string, LocationDef>>("RandomizerMod.Resources.locations.json");
            _logicSettings = JsonUtil.Deserialize<Dictionary<string, string>>("RandomizerMod.Resources.logic_settings.json");
            __pools = JsonUtil.Deserialize<PoolDef[]>("RandomizerMod.Resources.pools.json");
            _starts = JsonUtil.Deserialize<Dictionary<string, StartDef>>("RandomizerMod.Resources.starts.json");
            _transitions = JsonUtil.Deserialize<Dictionary<string, TransitionDef>>("RandomizerMod.Resources.transitions.json");

            return;
        }

    }
}
