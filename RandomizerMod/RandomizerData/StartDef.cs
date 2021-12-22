using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalEnums;

namespace RandomizerMod.RandomizerData
{
    public class StartDef
    {
        public string Name { get; init; }

        // respawn marker properties
        public string SceneName { get; init; }
        public float X { get; init; }
        public float Y { get; init; }
        public MapZone Zone { get; init; }

        // logic info
        public string Waypoint { get; init; }
        public string AreaTransition { get; init; }
        public string RoomTransition { get; init; }

        // Primitive logic -- check SettingsPM
        public string Logic { get; init; }
    }
}
