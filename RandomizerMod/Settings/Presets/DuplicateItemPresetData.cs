namespace RandomizerMod.Settings.Presets
{
    public static class DuplicateItemPresetData
    {
        public static DuplicateItemSettings DuplicateMajorItems;
        public static DuplicateItemSettings None;
        public static Dictionary<string, DuplicateItemSettings> Presets;


        static DuplicateItemPresetData()
        {
            DuplicateMajorItems = new()
            {
                MothwingCloak = true,
                MantisClaw = true,
                CrystalHeart = true,
                MonarchWings = true,
                ShadeCloak = true,
                DreamNail = true,
                VoidHeart = true,
                Dreamer = true,
                SwimmingItems = true,
                LevelOneSpells = true,
                LevelTwoSpells = false,
                Grimmchild = false,
                NailArts = false,
                CursedNailItems = false,
                DuplicateUniqueKeys = false,
                SimpleKeyHandling = DuplicateItemSettings.SimpleKeySetting.TwoExtraKeysInLogic,
                SplitClawHandling = DuplicateItemSettings.SplitItemSetting.NoDupe,
                SplitCloakHandling = DuplicateItemSettings.SplitItemSetting.DupeBoth,
            };
            None = new()
            {
                MothwingCloak = false,
                MantisClaw = false,
                CrystalHeart = false,
                MonarchWings = false,
                ShadeCloak = false,
                DreamNail = false,
                VoidHeart = false,
                Dreamer = false,
                SwimmingItems = false,
                LevelOneSpells = false,
                LevelTwoSpells = false,
                Grimmchild = false,
                NailArts = false,
                CursedNailItems = false,
                DuplicateUniqueKeys = false,
                SimpleKeyHandling = DuplicateItemSettings.SimpleKeySetting.NoDupe,
                SplitClawHandling = DuplicateItemSettings.SplitItemSetting.NoDupe,
                SplitCloakHandling = DuplicateItemSettings.SplitItemSetting.NoDupe,
            };

            Presets = new()
            {
                { "Duplicate Major Items", DuplicateMajorItems },
                { "None", None },
            };
        }

    }
}
