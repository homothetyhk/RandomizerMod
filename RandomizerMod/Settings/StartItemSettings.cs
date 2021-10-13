using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class StartItemSettings : SettingsModule
    {
        public int MinimumStartGeo;
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
