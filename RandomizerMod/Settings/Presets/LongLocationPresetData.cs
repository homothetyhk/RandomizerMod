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
        public static LongLocationSettings NoPreviews;

        public static Dictionary<string, LongLocationSettings> LongLocationPresets;

        static LongLocationPresetData()
        {
            Standard = new LongLocationSettings
            {
                WhitePalaceRando = LongLocationSettings.WPSetting.Allowed,
                BossEssenceRando = LongLocationSettings.BossEssenceSetting.All,
                ColosseumPreview = true,
                KingFragmentPreview = true,
                FlowerQuestPreview = true,
                GreyPrinceZotePreview = true,
                WhisperingRootPreview = true,
                AbyssShriekPreview = true,
                VoidHeartPreview = true,
                DreamerPreview = true,
                GodtunerPreview = true,
                BasinFountainPreview = true,
                NailmasterPreview = true,
                MapPreview = true,
                StagPreview = true,
                LoreTabletPreview = true,
                DivinePreview = true,
                GeoShopPreview = LongLocationSettings.CostItemHintSettings.CostAndName,
                GrubfatherPreview = LongLocationSettings.CostItemHintSettings.CostAndName,
                SeerPreview = LongLocationSettings.CostItemHintSettings.CostAndName,
                EggShopPreview = LongLocationSettings.CostItemHintSettings.CostAndName,
            };

            Easier = new LongLocationSettings
            {
                WhitePalaceRando = LongLocationSettings.WPSetting.ExcludePathOfPain,
                BossEssenceRando = LongLocationSettings.BossEssenceSetting.ExcludeZoteAndWhiteDefender,
                ColosseumPreview = true,
                KingFragmentPreview = true,
                FlowerQuestPreview = true,
                GreyPrinceZotePreview = true,
                WhisperingRootPreview = true,
                AbyssShriekPreview = true,
                VoidHeartPreview = true,
                DreamerPreview = true,
                GodtunerPreview = true,
                BasinFountainPreview = true,
                NailmasterPreview = true,
                StagPreview = true,
                MapPreview = true,
                LoreTabletPreview = true,
                DivinePreview = true,
                GeoShopPreview = LongLocationSettings.CostItemHintSettings.CostAndName,
                GrubfatherPreview = LongLocationSettings.CostItemHintSettings.CostAndName,
                SeerPreview = LongLocationSettings.CostItemHintSettings.CostAndName,
                EggShopPreview = LongLocationSettings.CostItemHintSettings.CostAndName,
            };

            FewerHints = new LongLocationSettings
            {
                WhitePalaceRando = LongLocationSettings.WPSetting.Allowed,
                BossEssenceRando = LongLocationSettings.BossEssenceSetting.All,
                ColosseumPreview = true,
                KingFragmentPreview = true,
                FlowerQuestPreview = true,
                GreyPrinceZotePreview = true,
                WhisperingRootPreview = false,
                AbyssShriekPreview = false,
                VoidHeartPreview = false,
                DreamerPreview = false,
                GodtunerPreview = false,
                BasinFountainPreview = false,
                NailmasterPreview = false,
                StagPreview = false,
                MapPreview = false,
                LoreTabletPreview = false,
                DivinePreview = false,
                GeoShopPreview = LongLocationSettings.CostItemHintSettings.CostAndName,
                GrubfatherPreview = LongLocationSettings.CostItemHintSettings.CostOnly,
                SeerPreview = LongLocationSettings.CostItemHintSettings.CostOnly,
                EggShopPreview = LongLocationSettings.CostItemHintSettings.CostOnly,
            };

            DAB = new LongLocationSettings
            {
                WhitePalaceRando = LongLocationSettings.WPSetting.ExcludeWhitePalace,
                BossEssenceRando = LongLocationSettings.BossEssenceSetting.ExcludeAllDreamBosses,
                ColosseumPreview = true,
                KingFragmentPreview = true,
                FlowerQuestPreview = true,
                GreyPrinceZotePreview = true,
                WhisperingRootPreview = true,
                AbyssShriekPreview = true,
                VoidHeartPreview = true,
                DreamerPreview = true,
                GodtunerPreview = true,
                BasinFountainPreview = true,
                NailmasterPreview = true,
                StagPreview = true,
                MapPreview = true,
                LoreTabletPreview = true,
                DivinePreview = true,
                GeoShopPreview = LongLocationSettings.CostItemHintSettings.CostAndName,
                GrubfatherPreview = LongLocationSettings.CostItemHintSettings.CostAndName,
                SeerPreview = LongLocationSettings.CostItemHintSettings.CostAndName,
                EggShopPreview = LongLocationSettings.CostItemHintSettings.CostAndName,
            };

            NoPreviews = new LongLocationSettings
            {
                WhitePalaceRando = LongLocationSettings.WPSetting.Allowed,
                BossEssenceRando = LongLocationSettings.BossEssenceSetting.All,
                ColosseumPreview = false,
                KingFragmentPreview = false,
                FlowerQuestPreview = false,
                GreyPrinceZotePreview = false,
                WhisperingRootPreview = false,
                AbyssShriekPreview = false,
                VoidHeartPreview = false,
                DreamerPreview = false,
                GodtunerPreview = false,
                BasinFountainPreview = false,
                NailmasterPreview = false,
                StagPreview = false,
                MapPreview = false,
                LoreTabletPreview = false,
                DivinePreview = false,
                GeoShopPreview = LongLocationSettings.CostItemHintSettings.None,
                GrubfatherPreview = LongLocationSettings.CostItemHintSettings.None,
                SeerPreview = LongLocationSettings.CostItemHintSettings.None,
                EggShopPreview = LongLocationSettings.CostItemHintSettings.None,
            };


            LongLocationPresets = new Dictionary<string, LongLocationSettings>
            {
                { "Standard", Standard },
                { "Easier", Easier },
                { "Fewer Hints", FewerHints },
                { "DAB", DAB },
                { "No Previews", NoPreviews },
            };
        }


    }
}
