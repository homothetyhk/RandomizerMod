namespace RandomizerMod.Settings
{
    public class DuplicateItemSettings : SettingsModule
    {
        public bool MothwingCloak;
        public bool MantisClaw;
        public bool CrystalHeart;
        public bool MonarchWings;
        public bool ShadeCloak;
        public bool DreamNail;
        public bool VoidHeart;
        public bool Dreamer;
        public bool SwimmingItems;
        public bool LevelOneSpells;

        public bool LevelTwoSpells;
        public bool Grimmchild;
        public bool NailArts;
        public bool CursedNailItems;
        public bool DuplicateUniqueKeys;
        public SimpleKeySetting SimpleKeyHandling;
        public SplitItemSetting SplitClawHandling;
        public SplitItemSetting SplitCloakHandling;

        public enum SimpleKeySetting
        {
            NoDupe,
            TwoExtraKeysInLogic,
            TwoDupeKeys,
        }

        public enum SplitItemSetting
        {
            NoDupe,
            DupeLeft,
            DupeRight,
            DupeRandom,
            DupeBoth,
        }
    }
}
