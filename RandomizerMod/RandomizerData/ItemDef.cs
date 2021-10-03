using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.RandomizerData
{
    public class ItemDef
    {
        public string name;
        public string pool; // for items in multiple pools, give the most common pool.
        public int priceCap;

        // Item tier flags
        public bool progression;
        public bool itemCandidate; // progression items which may open new locations in a pinch
        public bool majorItem; // reserved for the most useful items in the randomizer
    }
}
