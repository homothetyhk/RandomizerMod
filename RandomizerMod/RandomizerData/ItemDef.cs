using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.Logic;

namespace RandomizerMod.RandomizerData
{
    public readonly struct ItemEffect
    {
        [Newtonsoft.Json.JsonConstructor]
        public ItemEffect(string id, int incr)
        {
            this.id = id;
            this.incr = incr;
        }

        public readonly string id;
        public readonly int incr;
    }

    

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

        public LogicItemTemplate itemTemplate;

        public ItemDef Clone()
        {
            return MemberwiseClone() as ItemDef;
        }
    }
}
