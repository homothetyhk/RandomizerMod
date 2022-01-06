namespace RandomizerMod.RandomizerData
{
    public record ItemDef
    {
        public string Name { get; init; }
        public string Pool { get; init; } // for items in multiple pools, give the most common pool.
        public int PriceCap { get; init; }
        public bool MajorItem { get; init; } // reserved for the most useful items in the randomizer, used by CursedSettings to penalize progression
    }

    /*
    [Flags]
    public enum ItemPoolFlags : long
    {
        None = 0L,
        IsSkill = 1L << 0,
        IsKey = 1L << 1,
        IsCharm = 1L << 2,
        IsMask = 1L << 3,
        IsVessel = 1L << 4,
        IsNotch = 1L << 5,
        IsOre = 1L << 6,
        IsGeoChest = 1L << 7,
        IsEgg = 1L << 8,
        IsRelic = 1L << 9,
        IsRoot = 1L << 10,
        IsDreamWarriorEssence = 1L << 11,
        IsDreamBossEssence = 1L << 12,
        IsGrub = 1L << 13,
        IsMimic = 1L << 14,
        IsMap = 1L << 15,
        IsStag = 1L << 16,
        IsCocoon = 1L << 17,
        IsFlame = 1L << 18,
        IsHunterJournalEntry = 1L << 19,
        IsRock = 1L << 20,
        IsSoul = 1L << 21,
        IsLore = 1L << 22,
        IsCustomAbility = 1L << 23,
        IsDreamer = 1L << 24, // oops
    }

    [Flags]
    public enum ItemPropertyFlags : long
    {
        None = 0L,
        IsUnique = 1L << 0,
        IsBigItem = 1L << 1,
        IsSpell = 1L << 2,
        IsGeo = 1L << 3,
        // ???
    }
    */
}
