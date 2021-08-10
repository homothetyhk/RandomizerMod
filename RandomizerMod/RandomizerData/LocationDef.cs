using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.RandomizerData
{
    public class LocationDef
    {
        /*
         A location has a name, scene, area, pool, logic, and possibly a location-wide variable cost.
         */
        public string name;
        public string sceneName;
        public string areaName;
        public bool multi;

        public string itemLogic;
        public string areaLogic;
        public string roomLogic;
    }
}
