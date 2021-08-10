using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class StartItemSettings : ICloneable
    {
        public int MinimumStartGeo;
        public int MaximumStartGeo;

        public enum StartVerticalType : byte
        {
            None,
            ZeroOrMore,
            OneRandomItem,
            MantisClaw,
            MonarchWings,
            All,
        }
        public StartVerticalType VerticalMovement;

        public enum StartHorizontalType : byte
        {
            None,
            ZeroOrMore,
            OneRandomItem,
            MothwingCloak,
            CrystalHeart,
            All
        }
        public StartHorizontalType HorizontalMovement;

        public enum StartCharmType : byte
        {
            None,
            ZeroOrMore,
            OneRandomItem,
        }
        public StartCharmType Charms;

        public enum StartStagType : byte
        {
            None,
            DirtmouthStag,
            ZeroOrMoreRandomStags,
            OneRandomStag,
            ManyRandomStags,
            AllStags
        }
        public StartStagType Stags;

        public enum StartMiscItems : byte
        {
            None,
            ZeroOrMore,
            Many,
            DreamNail,
            DreamNailAndMore,
        }
        public StartMiscItems MiscItems;


        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
