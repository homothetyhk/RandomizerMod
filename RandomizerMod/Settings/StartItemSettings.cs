using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MenuChanger.Attributes;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class StartItemSettings : SettingsModule
    {
        [DynamicBound(nameof(MaximumStartGeo), true)]
        [MenuRange(0, 50000)]
        public int MinimumStartGeo;

        [DynamicBound(nameof(MinimumStartGeo), false)]
        [MenuRange(0, 50000)]
        public int MaximumStartGeo;

        public enum StartVerticalType
        {
            None,
            ZeroOrMore,
            OneRandomItem,
            MantisClaw,
            MonarchWings,
            All,
        }
        public StartVerticalType VerticalMovement;

        public enum StartHorizontalType
        {
            None,
            ZeroOrMore,
            OneRandomItem,
            MothwingCloak,
            CrystalHeart,
            All
        }
        public StartHorizontalType HorizontalMovement;

        public enum StartCharmType
        {
            None,
            ZeroOrMore,
            OneRandomItem,
        }
        public StartCharmType Charms;

        public enum StartStagType
        {
            None,
            DirtmouthStag,
            ZeroOrMoreRandomStags,
            OneRandomStag,
            ManyRandomStags,
            AllStags
        }
        public StartStagType Stags;

        public enum StartMiscItems
        {
            None,
            ZeroOrMore,
            Many,
            DreamNail,
            DreamNailAndMore,
        }
        public StartMiscItems MiscItems;
    }
}
