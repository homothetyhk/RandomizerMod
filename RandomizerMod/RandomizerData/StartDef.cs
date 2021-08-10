using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalEnums;

namespace RandomizerMod.RandomizerData
{
    public class StartDef
    {
        public string name;

        // respawn marker properties
        public string sceneName;
        public float x;
        public float y;
        public MapZone zone;

        // logic info
        public string waypoint;
        public string areaTransition;
        public string roomTransition;

        // Primitive logic -- check MenuChanger / PreRandomizer for supported flags
        public string logic;
    }
}
