using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class LongLocationSettings : SettingsModule
    {
        public enum WPSetting
        {
            Allowed,
            ExcludePathOfPain,
            ExcludeWhitePalace
        }

        public enum BossEssenceSetting
        {
            All,
            ExcludeGreyPrinceZoteAndWhiteDefender,
            ExcludeAllDreamBosses,
            ExcludeAllDreamWarriors
        }

        public enum CostItemHintSettings
        {
            CostAndName,
            CostOnly,
            NameOnly,
            None
        }

        public enum LongLocationHintSetting
        {
            Standard,
            MoreHints,
            None
        }

        public WPSetting RandomizationInWhitePalace;
        public BossEssenceSetting BossEssenceRandomization;
        public CostItemHintSettings CostItemHints;
        public LongLocationHintSetting LongLocationHints;
    }
}
