using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings.Presets
{
    public static class LongLocationPresetData
    {
        public static LongLocationSettings Standard;
        public static LongLocationSettings Easier;
        public static LongLocationSettings FewerHints;
        public static LongLocationSettings DAB;


        public static Dictionary<string, LongLocationSettings> LongLocationPresets;

        static LongLocationPresetData()
        {
            Standard = new LongLocationSettings
            {
                RandomizationInWhitePalace = LongLocationSettings.WPSetting.Allowed,
                BossEssenceRandomization = LongLocationSettings.BossEssenceSetting.All,
                CostItemHints = LongLocationSettings.CostItemHintSettings.CostAndName,
                LongLocationHints = LongLocationSettings.LongLocationHintSetting.Standard
            };

            Easier = new LongLocationSettings
            {
                RandomizationInWhitePalace = LongLocationSettings.WPSetting.ExcludePathOfPain,
                BossEssenceRandomization = LongLocationSettings.BossEssenceSetting.ExcludeGreyPrinceZoteAndWhiteDefender,
                CostItemHints = LongLocationSettings.CostItemHintSettings.CostAndName,
                LongLocationHints = LongLocationSettings.LongLocationHintSetting.MoreHints
            };

            FewerHints = new LongLocationSettings
            {
                RandomizationInWhitePalace = LongLocationSettings.WPSetting.Allowed,
                BossEssenceRandomization = LongLocationSettings.BossEssenceSetting.All,
                CostItemHints = LongLocationSettings.CostItemHintSettings.CostOnly,
                LongLocationHints = LongLocationSettings.LongLocationHintSetting.None
            };

            DAB = new LongLocationSettings
            {
                RandomizationInWhitePalace = LongLocationSettings.WPSetting.ExcludeWhitePalace,
                BossEssenceRandomization = LongLocationSettings.BossEssenceSetting.ExcludeAllDreamBosses,
                CostItemHints = LongLocationSettings.CostItemHintSettings.CostAndName,
                LongLocationHints = LongLocationSettings.LongLocationHintSetting.Standard
            };




            LongLocationPresets = new Dictionary<string, LongLocationSettings>
            {
                { "Standard", Standard },
                { "Easier", Easier },
                { "Fewer Hints", FewerHints },
                { "DAB", DAB },
            };
        }


    }
}
