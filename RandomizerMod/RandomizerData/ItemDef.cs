namespace RandomizerMod.RandomizerData
{
    public class ItemDef
    {
        public string Name { get; init; }
        public string Pool { get; init; } // for items in multiple pools, give the most common pool.
        public int PriceCap { get; init; }

        // Item tier flags
        public bool Progression { get; init; }
        public bool ItemCandidate { get; init; } // progression items which may open new locations in a pinch
        public bool MajorItem { get; init; } // reserved for the most useful items in the randomizer
    }
}
