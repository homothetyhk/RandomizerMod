using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.RandomizerData
{
    public class TransitionDef
    {
        [Newtonsoft.Json.JsonIgnore]
        public string Name => $"{sceneName}[{doorName}]";

        public string sceneName;
        public string doorName;
        public string areaName;

        public bool isAreaTransition;
        public bool isolated;
        public bool deadEnd;
        public TransitionSides sides;
    }

    public enum TransitionSides
    {
        Both = 0,
        /// <summary>
        /// A one way transition exiting a scene.
        /// </summary>
        OneWayIn = 1,
        /// <summary>
        /// A one way transition entering a scene.
        /// </summary>
        OneWayOut = 2,
    }
}
