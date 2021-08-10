using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.RandomizerData
{
    public class TransitionDef
    {
        public string sceneName;
        public string doorName;
        public string areaName;

        public string destinationScene;
        public string destinationGate;

        public bool isolated;
        public bool deadEnd;
        public int oneWay; // 0 == 2-way, 1 == can only go in, 2 == can only come out
    }
}
