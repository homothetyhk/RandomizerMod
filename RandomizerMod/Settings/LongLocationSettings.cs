using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class LongLocationSettings : ICloneable
    {
        public enum WPSetting : byte
        {
            Allowed,
            ExcludePathOfPain,
            ExcludeWhitePalace
        }

        public enum BossEssenceSetting : byte
        {
            All,
            ExcludeGreyPrinceZoteAndWhiteDefender,
            ExcludeAllDreamBosses,
            ExcludeAllDreamWarriors
        }

        public enum CostItemHintSettings : byte
        {
            CostAndName,
            CostOnly,
            NameOnly,
            None
        }

        public enum LongLocationHintSetting : byte
        {
            Standard,
            MoreHints,
            None
        }

        public WPSetting RandomizationInWhitePalace;
        public BossEssenceSetting BossEssenceRandomization;
        public CostItemHintSettings CostItemHints;
        public LongLocationHintSetting LongLocationHints;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
