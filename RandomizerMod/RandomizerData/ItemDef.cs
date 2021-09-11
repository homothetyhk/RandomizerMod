using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.RandomizerData
{
    public class ItemDef
    {
        /*
         * An item has a name, progression flags, and a template that can be converted to a LogicItem at runtime
         */
        public string name;

        // Item tier flags
        public bool progression;
        public bool itemCandidate; // progression items which may open new locations in a pinch
        public bool majorItem; // reserved for the most useful items in the randomizer
    }
}
