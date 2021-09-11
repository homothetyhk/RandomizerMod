using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings.Presets
{
    public static class Captions
    {
        public static string Caption(this GrubCostRandomizerSettings gc)
        {
            return $"Grub reward items will be randomized to costs between " +
            $"{gc.MinimumGrubCost} and {gc.MaximumGrubCost}. For each item, the randomizer will guarantee " +
            $"{gc.GrubTolerance} grub(s) beyond the listed cost are accessible before " +
            $"the item is expected in logic.";
        }

        public static string Caption(this EssenceCostRandomizerSettings ec)
        {
            return $"Seer reward items will be randomized to costs between " +
            $"{ec.MinimumEssenceCost} and {ec.MaximumEssenceCost}. For each item, the randomizer will guarantee " +
            $"{ec.EssenceTolerance} essence beyond the listed cost is accessible before " +
            $"the item is expected in logic.";
        }

        public static string Caption(this LongLocationSettings ll, GenerationSettings Settings)
        {
            StringBuilder sb = new StringBuilder();
            switch (ll.RandomizationInWhitePalace)
            {
                case LongLocationSettings.WPSetting.ExcludePathOfPain:
                    sb.Append("Locations (such as soul totems) in Path of Pain will not be randomized. ");
                    break;
                case LongLocationSettings.WPSetting.ExcludeWhitePalace:
                    sb.Append("Locations (such as King Fragment and soul totems) in White Palace will not be randomized. ");
                    break;
            }
            switch (ll.BossEssenceRandomization)
            {
                case LongLocationSettings.BossEssenceSetting.ExcludeAllDreamBosses when Settings.PoolSettings.BossEssence:
                    sb.Append("Dream Boss essence rewards will not be randomized. ");
                    break;
                case LongLocationSettings.BossEssenceSetting.ExcludeAllDreamWarriors when Settings.PoolSettings.BossEssence:
                    sb.Append("Dream Warrior essence rewards will not be randomized. ");
                    break;
                case LongLocationSettings.BossEssenceSetting.ExcludeGreyPrinceZoteAndWhiteDefender when Settings.PoolSettings.BossEssence:
                    sb.Append("Grey Prince Zote and White Defender essence rewards will not be randomized. ");
                    break;
            }
            switch (ll.CostItemHints)
            {
                case LongLocationSettings.CostItemHintSettings.CostOnly:
                    sb.Append("Item dialogue boxes will not display the item name, and will show only the cost of the item. ");
                    break;
                case LongLocationSettings.CostItemHintSettings.NameOnly:
                    sb.Append("Item dialogue boxes will not display the cost, and will show only the name of the item. ");
                    break;
                case LongLocationSettings.CostItemHintSettings.None:
                    sb.Append("Item dialogue boxes will show neither the cost nor the name of the item. ");
                    break;
            }
            switch (ll.LongLocationHints)
            {
                case LongLocationSettings.LongLocationHintSetting.Standard:

                    sb.Append("Hints are provided for the items (if randomized) " +
                        (ll.RandomizationInWhitePalace != LongLocationSettings.WPSetting.ExcludeWhitePalace ?
                        "at King Fragment, " : string.Empty) +
                        (ll.BossEssenceRandomization != LongLocationSettings.BossEssenceSetting.ExcludeAllDreamBosses &&
                        ll.BossEssenceRandomization != LongLocationSettings.BossEssenceSetting.ExcludeGreyPrinceZoteAndWhiteDefender ?
                        "at Grey Prince Zote, " : string.Empty) +
                        "in the colosseum, " +
                        "and behind Flower Quest. ");
                    break;
            }

            return sb.ToString();
        }

        public static string Caption(this CursedSettings cs)
        {
            StringBuilder sb = new StringBuilder();
            if (cs.RandomCurses) sb.Append("A random assortment of curses. ");
            if (cs.RandomizeFocus) sb.Append("The ability to heal is randomized. ");
            if (cs.ReplaceJunkWithOneGeo) sb.Append("Luxury items like mask shards and pale ore and the like are replaced with 1 geo pickups. ");
            if (cs.RemoveSpellUpgrades) sb.Append("Spell upgrades are completely removed. ");
            if (cs.LongerProgressionChains) sb.Append("Progression items are harder to find on average. ");
            if (cs.SplitClaw) sb.Append("The abilities to walljump from left and right slopes are separated. ");
            if (cs.SplitCloak) sb.Append("The abilities to dash left and right are separated. ");
            if (cs.RandomizeNail) sb.Append("The abilities to swing the nail in each direction are randomized. ");

            return sb.ToString();
        }

        public static string Caption(this StartItemSettings si)
        {
            StringBuilder sb = new StringBuilder();
            if (si.MinimumStartGeo == si.MaximumStartGeo)
            {
                sb.Append($"Start with {si.MinimumStartGeo} geo. ");
            }
            else
            {
                sb.Append($"Start with random geo between {si.MinimumStartGeo} and {si.MaximumStartGeo}. ");
            }

            switch (si.VerticalMovement)
            {
                default:
                case StartItemSettings.StartVerticalType.None:
                    break;
                case StartItemSettings.StartVerticalType.ZeroOrMore:
                    sb.Append("May start with random vertical movement items. ");
                    break;
                case StartItemSettings.StartVerticalType.MantisClaw:
                    sb.Append("Start with Mantis Claw. ");
                    break;
                case StartItemSettings.StartVerticalType.MonarchWings:
                    sb.Append("Start with Monarch Wings. ");
                    break;
                case StartItemSettings.StartVerticalType.OneRandomItem:
                    sb.Append("Start with a random vertical movement item. ");
                    break;
                case StartItemSettings.StartVerticalType.All:
                    sb.Append("Start with all vertical movement. ");
                    break;
            }

            switch (si.HorizontalMovement)
            {
                default:
                case StartItemSettings.StartHorizontalType.None:
                    break;
                case StartItemSettings.StartHorizontalType.ZeroOrMore:
                    sb.Append("May start with random horizontal movement items. ");
                    break;
                case StartItemSettings.StartHorizontalType.MothwingCloak:
                    sb.Append("Start with Mothwing Cloak. ");
                    break;
                case StartItemSettings.StartHorizontalType.CrystalHeart:
                    sb.Append("Start with Crystal Heart. ");
                    break;
                case StartItemSettings.StartHorizontalType.OneRandomItem:
                    sb.Append("Start with a random horizontal movement item. ");
                    break;
                case StartItemSettings.StartHorizontalType.All:
                    sb.Append("Start with all horizontal movement. ");
                    break;
            }

            switch (si.Charms)
            {
                default:
                case StartItemSettings.StartCharmType.None:
                    break;
                case StartItemSettings.StartCharmType.ZeroOrMore:
                    sb.Append("May start with random equipped charms. ");
                    break;
                case StartItemSettings.StartCharmType.OneRandomItem:
                    sb.Append("Start with a random equipped charm. ");
                    break;
            }

            switch (si.Stags)
            {
                default:
                case StartItemSettings.StartStagType.None:
                    break;
                case StartItemSettings.StartStagType.DirtmouthStag:
                    sb.Append("Start with Dirtmouth Stag door unlocked. ");
                    break;
                case StartItemSettings.StartStagType.ZeroOrMoreRandomStags:
                    sb.Append("May start with some random stags. ");
                    break;
                case StartItemSettings.StartStagType.OneRandomStag:
                    sb.Append("Start with a random stag. ");
                    break;
                case StartItemSettings.StartStagType.ManyRandomStags:
                    sb.Append("Start with several random stags. ");
                    break;
                case StartItemSettings.StartStagType.AllStags:
                    sb.Append("Start with all stags. ");
                    break;
            }

            switch (si.MiscItems)
            {
                default:
                case StartItemSettings.StartMiscItems.None:
                    break;
                case StartItemSettings.StartMiscItems.DreamNail:
                    sb.Append("Start with Dream Nail. ");
                    break;
                case StartItemSettings.StartMiscItems.DreamNailAndMore:
                    sb.Append("Start with Dream Nail and a random assortment of useful items. ");
                    break;
                case StartItemSettings.StartMiscItems.ZeroOrMore:
                    sb.Append("May start with a random assortment of useful items. ");
                    break;
                case StartItemSettings.StartMiscItems.Many:
                    sb.Append("Start with many random useful items. ");
                    break;
            }

            return sb.ToString();
        }

        public static string Caption(this StartLocationSettings sl)
        {
            switch (sl.StartLocationType)
            {
                default:
                case StartLocationSettings.RandomizeStartLocationType.Fixed:
                    return $"The randomizer will start at {sl.StartLocation}";
                case StartLocationSettings.RandomizeStartLocationType.RandomExcludingKP:
                    return $"The randomizer will start at a random location. " +
                        $"It will not start at King's Pass or any location that requires additional items.";
                case StartLocationSettings.RandomizeStartLocationType.Random:
                    return $"The randomizer will start at a random location.";
            }
        }

        public static string Caption(this TransitionSettings ts)
        {
            switch (ts.Mode)
            {
                default:
                case TransitionSettings.TransitionMode.None:
                    return "";
                case TransitionSettings.TransitionMode.AreaRandomizer:
                    {
                        StringBuilder sb = new();
                        sb.Append("Transitions between areas will be randomized. ");
                        if (ts.Matched) sb.Append("Transition directions will be preserved. ");
                        else sb.Append("Transition directions will be randomized. ");
                        if (ts.Coupled) sb.Append("Transitions will be reversible.");
                        else sb.Append("Transitions may not be reversible.");
                        return sb.ToString();
                    }
                case TransitionSettings.TransitionMode.RoomRandomizer:
                    {
                        StringBuilder sb = new();
                        sb.Append("Transitions between rooms will be randomized. ");
                        if (ts.ConnectAreas) sb.Append("Where possible, transitions will connect to the same area. ");
                        if (ts.Matched) sb.Append("Transition directions will be preserved. ");
                        else sb.Append("Transition directions will be randomized. ");
                        if (ts.Coupled) sb.Append("Transitions will be reversible.");
                        else sb.Append("Transitions may not be reversible.");
                        return sb.ToString();
                    }
            }
        }
    }
}
