using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings.Presets
{
    public static class Captions
    {
        public static string Caption(this CostSettings cs)
        {
            StringBuilder sb = new();
            sb.AppendLine($"Grub costs may be randomized in [{cs.MinimumGrubCost}, {cs.MaximumGrubCost}] (tol:{cs.GrubTolerance})");
            sb.AppendLine($"Essence costs may be randomized in [{cs.MinimumEssenceCost}, {cs.MaximumEssenceCost}] (tol:{cs.EssenceTolerance})");
            sb.AppendLine($"Egg shop costs may be randomized in [{cs.MinimumEggCost}, {cs.MaximumEggCost}] (tol:{cs.EggTolerance})");
            sb.AppendLine($"Salubra charm costs may be randomized in [{cs.MinimumCharmCost}, {cs.MaximumCharmCost}] (tol:{cs.CharmTolerance})");
            return sb.ToString();
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
            sb.Append("See Long Location Options for details regarding location previews.");

            return sb.ToString();
        }

        public static string Caption(this NoveltySettings ns)
        {
            StringBuilder sb = new();
            List<string> terms = new();

            if (ns.RandomizeSwim) terms.Add("swim");
            if (ns.RandomizeElevatorPass) terms.Add("ride elevators");
            if (ns.RandomizeFocus) terms.Add("heal");
            if (ns.RandomizeNail) terms.Add("attack left or right or up");
            if (terms.Count > 0)
            {
                string ability = terms.Count > 1 ? "abilities" : "ability";
                sb.Append($"The {ability} to ");
                for (int i = 0; i < terms.Count - 1; i++)
                {
                    sb.Append(terms[i]);
                    sb.Append(", ");
                }
                if (terms.Count == 2) sb.Remove(sb.Length - 2, 1);
                if (terms.Count > 1) sb.Append("and ");
                sb.Append(terms[terms.Count - 1]);
                sb.Append(" will be removed and randomized.");
            }
            terms.Clear();
            if (ns.SplitClaw) sb.Append("The abilities to walljump from left and right slopes will be separated. ");
            if (ns.SplitCloak) sb.Append("The abilities to dash left and right will be separated. ");
            if (ns.EggShop) sb.Append("Jiji will trade items for rancid eggs. ");

            return sb.ToString();
        }

        public static string Caption(this CursedSettings cs)
        {
            StringBuilder sb = new StringBuilder();
            if (cs.ReplaceJunkWithOneGeo) sb.Append("Luxury items like mask shards and pale ore and the like are replaced with 1 geo pickups. ");
            if (cs.RemoveSpellUpgrades) sb.Append("Spell upgrades are completely removed. ");
            if (cs.LongerProgressionChains) sb.Append("Progression items are harder to find on average. ");
            if (cs.CursedMasks) sb.Append("Start with only 1 mask. ");
            if (cs.CursedNotches) sb.Append("Start with only 1 charm notch. ");
            if (cs.RandomizeMimics) sb.Append("Some grub bottles may contain a surprise...");

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
            if (ts.Mode == TransitionSettings.TransitionMode.None) return string.Empty;

            StringBuilder sb = new();
            switch (ts.Mode)
            {
                case TransitionSettings.TransitionMode.MapAreaRandomizer:
                    sb.Append("Transitions between areas with different maps (e.g. Greenpath, Fog Canyon) will be randomized. ");
                    break;
                case TransitionSettings.TransitionMode.FullAreaRandomizer:
                    sb.Append("Transitions between areas with different titles (e.g. Greenpath, Lake of Unn) will be randomized. ");
                    break;
                case TransitionSettings.TransitionMode.RoomRandomizer:
                    sb.Append("Transitions between rooms will be randomized. ");
                    break;
            }
            switch (ts.TransitionMatching)
            {
                case TransitionSettings.TransitionMatchingSetting.MatchingDirections:
                    sb.Append("Transition directions will be preserved. ");
                    break;
                case TransitionSettings.TransitionMatchingSetting.MatchingDirectionsAndNoDoorToDoor:
                    sb.Append("Transition directions will be preserved, and doors will not map to doors. ");
                    break;
                default:
                    sb.Append("Transition directions will be randomized. ");
                    break;
            }
            switch (ts.AreaConstraint)
            {
                case TransitionSettings.AreaConstraintSetting.None:
                    break;
                case TransitionSettings.AreaConstraintSetting.MoreConnectedMapAreas:
                    sb.Append("Where possible, transitions will connect to the same map area. ");
                    break;
                case TransitionSettings.AreaConstraintSetting.MoreConnectedTitledAreas:
                    sb.Append("Where possible, transitions will connect to the same titled area. ");
                    break;
            }
            if (ts.Coupled) sb.Append("Transitions will be reversible.");
            else sb.Append("Transitions may not be reversible.");
            return sb.ToString();
        }

        public static string Caption(this MiscSettings ms)
        {
            StringBuilder sb = new();
            if (ms.RandomizeNotchCosts)
            {
                sb.Append("Notch costs of charms will be randomized. ");
            }
            else
            {
                sb.Append("Notch costs of charms will not be randomized. ");
            }

            if (ms.ExtraPlatforms)
            {
                sb.Append("Extra platforms will be added in certain places to prevent softlocks. ");
            }
            else
            {
                sb.Append("Softlock-prevention platforms will not be provided. ");
            }

            switch (ms.SalubraNotches)
            {
                case MiscSettings.SalubraNotchesSetting.GroupedWithCharmNotchesPool:
                    sb.Append("Salubra notches will behave like other charm notches. ");
                    break;
                case MiscSettings.SalubraNotchesSetting.Vanilla:
                    sb.Append("Salubra notches will never be randomized. ");
                    break;
                case MiscSettings.SalubraNotchesSetting.Randomized:
                    sb.Append("Salubra notches will always be randomized. ");
                    break;
                case MiscSettings.SalubraNotchesSetting.AutoGivenAtCharmThreshold:
                    sb.Append("Salubra notches will be automatically given. ");
                    break;
            }
            switch (ms.MaskShards)
            {
                case MiscSettings.MaskShardType.FourShardsPerMask:
                    break;
                case MiscSettings.MaskShardType.TwoShardsPerMask:
                    sb.Append("Mask Shards will be consolidated to double shards. ");
                    break;
                case MiscSettings.MaskShardType.OneShardPerMask:
                    sb.Append("Mask Shards will be consolidated to quadruple shards. ");
                    break;
            }
            switch (ms.VesselFragments)
            {
                case MiscSettings.VesselFragmentType.ThreeFragmentsPerVessel:
                    break;
                case MiscSettings.VesselFragmentType.TwoFragmentsPerVessel:
                    sb.Append("Vessel Fragments will be consolidated to double fragments. ");
                    break;
                case MiscSettings.VesselFragmentType.OneFragmentPerVessel:
                    sb.Append("Vessel Fragments will be consolidated to triple fragments. ");
                    break;
            }

            return sb.ToString();
        }

        public static string Caption(this DuplicateItemSettings ds)
        {
            return "For more information, see the Duplicate Items page in Advanced Settings.";
        }

    }
}
