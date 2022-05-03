using MenuChanger.Extensions;
using System.Text;

namespace RandomizerMod.Settings.Presets
{
    public static class Captions
    {
        public static string Caption(this PoolSettings ps)
        {
            return string.Join(", ", typeof(PoolSettings).GetFields().Where(f => (bool)f.GetValue(ps)).Select(f => Localize(f.GetMenuName())));
        }

        public static string Caption(this SkipSettings ss)
        {
            return string.Join(", ", typeof(SkipSettings).GetFields().Where(f => (bool)f.GetValue(ss)).Select(f => Localize(f.GetMenuName())));
        }

        public static string Caption(this CostSettings cs)
        {
            StringBuilder sb = new();
            sb.AppendLine(Localize("Grub costs may be randomized in ") + $"[{cs.MinimumGrubCost}, {cs.MaximumGrubCost}] ({Localize("tol")}:{cs.GrubTolerance})");
            sb.AppendLine(Localize("Essence costs may be randomized in ") + $"[{cs.MinimumEssenceCost}, {cs.MaximumEssenceCost}] ({Localize("tol")}:{cs.EssenceTolerance})");
            sb.AppendLine(Localize("Egg shop costs may be randomized in ") + $"[{cs.MinimumEggCost}, {cs.MaximumEggCost}] ({Localize("tol")}:{cs.EggTolerance})");
            sb.AppendLine(Localize("Salubra charm costs may be randomized in ") + $"[{cs.MinimumCharmCost}, {cs.MaximumCharmCost}] ({Localize("tol")}:{cs.CharmTolerance})");
            return sb.ToString();
        }

        public static string Caption(this LongLocationSettings ll, GenerationSettings Settings)
        {
            StringBuilder sb = new();
            switch (ll.WhitePalaceRando)
            {
                case LongLocationSettings.WPSetting.ExcludePathOfPain:
                    sb.Append(Localize("Locations (such as soul totems) in Path of Pain will not be randomized. "));
                    break;
                case LongLocationSettings.WPSetting.ExcludeWhitePalace:
                    sb.Append(Localize("Locations (such as soul totems or lore tablets) in White Palace will not be randomized. "));
                    break;
            }
            switch (ll.BossEssenceRando)
            {
                case LongLocationSettings.BossEssenceSetting.ExcludeAllDreamBosses when Settings.PoolSettings.BossEssence:
                    sb.Append(Localize("Dream Boss essence rewards will not be randomized. "));
                    break;
                case LongLocationSettings.BossEssenceSetting.ExcludeAllDreamWarriors when Settings.PoolSettings.BossEssence:
                    sb.Append(Localize("Dream Warrior essence rewards will not be randomized. "));
                    break;
                case LongLocationSettings.BossEssenceSetting.ExcludeZoteAndWhiteDefender when Settings.PoolSettings.BossEssence:
                    sb.Append(Localize("Grey Prince Zote and White Defender essence rewards will not be randomized. "));
                    break;
            }
            sb.Append(Localize("See Long Location Options for details regarding location previews."));

            return sb.ToString();
        }

        public static string Caption(this NoveltySettings ns)
        {
            StringBuilder sb = new();
            List<string> terms = new();

            if (ns.RandomizeSwim) terms.Add(Localize("swim"));
            if (ns.RandomizeElevatorPass) terms.Add(Localize("ride elevators"));
            if (ns.RandomizeFocus) terms.Add(Localize("heal"));
            if (ns.RandomizeNail) terms.Add(Localize("attack left or right or up"));
            if (terms.Count > 0)
            {
                string ability = terms.Count > 1 ? "abilities" : "ability";
                sb.Append(Localize($"The {ability} to "));
                for (int i = 0; i < terms.Count - 1; i++)
                {
                    sb.Append(terms[i]);
                    sb.Append(", ");
                }
                if (terms.Count == 2) sb.Remove(sb.Length - 2, 1);
                if (terms.Count > 1) sb.Append(Localize("and "));
                sb.Append(terms[terms.Count - 1]);
                sb.Append(Localize(" will be removed and randomized."));
            }
            terms.Clear();
            if (ns.SplitClaw) sb.Append(Localize("The abilities to walljump from left and right slopes will be separated. "));
            if (ns.SplitCloak) sb.Append(Localize("The abilities to dash left and right will be separated. "));
            if (ns.SplitSuperdash) sb.Append(Localize("The abilities to superdash left and right will be separated. "));
            if (ns.EggShop) sb.Append(Localize("Jiji will trade items for rancid eggs. "));

            return sb.ToString();
        }

        public static string Caption(this CursedSettings cs)
        {
            StringBuilder sb = new();
            if (cs.ReplaceJunkWithOneGeo) sb.Append(Localize("Luxury items like mask shards and pale ore and the like are replaced with 1 geo pickups. "));
            if (cs.RemoveSpellUpgrades) sb.Append(Localize("Spell upgrades are completely removed. "));
            if (cs.LongerProgressionChains) sb.Append(Localize("Progression items are harder to find on average. "));
            if (cs.Deranged) sb.Append(Localize("Placements are much less likely to be vanilla. "));
            if (cs.CursedMasks > 0)
            {
                if (cs.CursedMasks == 4) sb.Append(Localize($"Start with only 1 mask. "));
                else sb.Append(Localize($"Start with only {5 - cs.CursedMasks} masks. "));
            }
            if (cs.CursedNotches > 0)
            {
                if (cs.CursedNotches == 2) sb.Append(Localize("Start with only 1 charm notch. "));
                else sb.Append(Localize($"Start with only {3 - cs.CursedNotches} charm notches. "));
            }
            if (cs.RandomizeMimics) sb.Append(Localize("Some grub bottles may contain a surprise..."));

            return sb.ToString();
        }

        public static string Caption(this StartItemSettings si)
        {
            StringBuilder sb = new();
            if (si.MinimumStartGeo == si.MaximumStartGeo)
            {
                sb.Append($"{Localize("Start with")} {si.MinimumStartGeo} {Localize("geo")}. ");
            }
            else
            {
                sb.Append($"{Localize("Start with random geo between")} {si.MinimumStartGeo} {Localize("and")} {si.MaximumStartGeo}. ");
            }

            switch (si.VerticalMovement)
            {
                default:
                case StartItemSettings.StartVerticalType.None:
                    break;
                case StartItemSettings.StartVerticalType.ZeroOrMore:
                    sb.Append(Localize("May start with random vertical movement items. "));
                    break;
                case StartItemSettings.StartVerticalType.MantisClaw:
                    sb.Append(Localize("Start with Mantis Claw. "));
                    break;
                case StartItemSettings.StartVerticalType.MonarchWings:
                    sb.Append(Localize("Start with Monarch Wings. "));
                    break;
                case StartItemSettings.StartVerticalType.OneRandomItem:
                    sb.Append(Localize("Start with a random vertical movement item. "));
                    break;
                case StartItemSettings.StartVerticalType.All:
                    sb.Append(Localize("Start with all vertical movement. "));
                    break;
            }

            switch (si.HorizontalMovement)
            {
                default:
                case StartItemSettings.StartHorizontalType.None:
                    break;
                case StartItemSettings.StartHorizontalType.ZeroOrMore:
                    sb.Append(Localize("May start with random horizontal movement items. "));
                    break;
                case StartItemSettings.StartHorizontalType.MothwingCloak:
                    sb.Append(Localize("Start with Mothwing Cloak. "));
                    break;
                case StartItemSettings.StartHorizontalType.CrystalHeart:
                    sb.Append(Localize("Start with Crystal Heart. "));
                    break;
                case StartItemSettings.StartHorizontalType.OneRandomItem:
                    sb.Append(Localize("Start with a random horizontal movement item. "));
                    break;
                case StartItemSettings.StartHorizontalType.All:
                    sb.Append(Localize("Start with all horizontal movement. "));
                    break;
            }

            switch (si.Charms)
            {
                default:
                case StartItemSettings.StartCharmType.None:
                    break;
                case StartItemSettings.StartCharmType.ZeroOrMore:
                    sb.Append(Localize("May start with random equipped charms. "));
                    break;
                case StartItemSettings.StartCharmType.OneRandomItem:
                    sb.Append(Localize("Start with a random equipped charm. "));
                    break;
            }

            switch (si.Stags)
            {
                default:
                case StartItemSettings.StartStagType.None:
                    break;
                case StartItemSettings.StartStagType.DirtmouthStag:
                    sb.Append(Localize("Start with Dirtmouth Stag door unlocked. "));
                    break;
                case StartItemSettings.StartStagType.ZeroOrMoreRandomStags:
                    sb.Append(Localize("May start with some random stags. "));
                    break;
                case StartItemSettings.StartStagType.OneRandomStag:
                    sb.Append(Localize("Start with a random stag. "));
                    break;
                case StartItemSettings.StartStagType.ManyRandomStags:
                    sb.Append(Localize("Start with several random stags. "));
                    break;
                case StartItemSettings.StartStagType.AllStags:
                    sb.Append(Localize("Start with all stags. "));
                    break;
            }

            switch (si.MiscItems)
            {
                default:
                case StartItemSettings.StartMiscItems.None:
                    break;
                case StartItemSettings.StartMiscItems.DreamNail:
                    sb.Append(Localize("Start with Dream Nail. "));
                    break;
                case StartItemSettings.StartMiscItems.DreamNailAndMore:
                    sb.Append(Localize("Start with Dream Nail and a random assortment of useful items. "));
                    break;
                case StartItemSettings.StartMiscItems.ZeroOrMore:
                    sb.Append(Localize("May start with a random assortment of useful items. "));
                    break;
                case StartItemSettings.StartMiscItems.Many:
                    sb.Append(Localize("Start with many random useful items. "));
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
                    return Localize("The randomizer will start at ") + Localize(sl.StartLocation);
                case StartLocationSettings.RandomizeStartLocationType.RandomExcludingKP:
                    return Localize("The randomizer will start at a random location.") +
                        Localize(" It will not start at King's Pass or any location that requires additional items.");
                case StartLocationSettings.RandomizeStartLocationType.Random:
                    return Localize("The randomizer will start at a random location.");
            }
        }

        public static string Caption(this TransitionSettings ts)
        {
            if (ts.Mode == TransitionSettings.TransitionMode.None) return string.Empty;

            StringBuilder sb = new();
            switch (ts.Mode)
            {
                case TransitionSettings.TransitionMode.MapAreaRandomizer:
                    sb.Append(Localize("Transitions between areas with different maps (e.g. Greenpath, Fog Canyon) will be randomized. "));
                    break;
                case TransitionSettings.TransitionMode.FullAreaRandomizer:
                    sb.Append(Localize("Transitions between areas with different titles (e.g. Greenpath, Lake of Unn) will be randomized. "));
                    break;
                case TransitionSettings.TransitionMode.RoomRandomizer:
                    sb.Append(Localize("Transitions between rooms will be randomized. "));
                    break;
            }
            switch (ts.TransitionMatching)
            {
                case TransitionSettings.TransitionMatchingSetting.MatchingDirections:
                    sb.Append(Localize("Transition directions will be preserved. "));
                    break;
                case TransitionSettings.TransitionMatchingSetting.MatchingDirectionsAndNoDoorToDoor:
                    sb.Append(Localize("Transition directions will be preserved, and doors will not map to doors. "));
                    break;
                default:
                    sb.Append(Localize("Transition directions will be randomized. "));
                    break;
            }
            switch (ts.AreaConstraint)
            {
                case TransitionSettings.AreaConstraintSetting.None:
                    break;
                case TransitionSettings.AreaConstraintSetting.MoreConnectedMapAreas:
                    sb.Append(Localize("Where possible, transitions will connect to the same map area. "));
                    break;
                case TransitionSettings.AreaConstraintSetting.MoreConnectedTitledAreas:
                    sb.Append(Localize("Where possible, transitions will connect to the same titled area. "));
                    break;
            }
            if (ts.Coupled) sb.Append(Localize("Transitions will be reversible."));
            else sb.Append(Localize("Transitions may not be reversible."));
            return sb.ToString();
        }

        public static string Caption(this MiscSettings ms)
        {
            StringBuilder sb = new();
            if (ms.RandomizeNotchCosts)
            {
                sb.Append(Localize("Notch costs of charms will be randomized. "));
            }
            else
            {
                sb.Append(Localize("Notch costs of charms will not be randomized. "));
            }

            if (ms.ExtraPlatforms)
            {
                sb.Append(Localize("Extra platforms will be added in certain places to prevent softlocks. "));
            }
            else
            {
                sb.Append(Localize("Softlock-prevention platforms will not be provided. "));
            }

            switch (ms.SalubraNotches)
            {
                case MiscSettings.SalubraNotchesSetting.GroupedWithCharmNotchesPool:
                    sb.Append(Localize("Salubra notches will behave like other charm notches. "));
                    break;
                case MiscSettings.SalubraNotchesSetting.Vanilla:
                    sb.Append(Localize("Salubra notches will never be randomized. "));
                    break;
                case MiscSettings.SalubraNotchesSetting.Randomized:
                    sb.Append(Localize("Salubra notches will always be randomized. "));
                    break;
                case MiscSettings.SalubraNotchesSetting.AutoGivenAtCharmThreshold:
                    sb.Append(Localize("Salubra notches will be automatically given. "));
                    break;
            }
            switch (ms.MaskShards)
            {
                case MiscSettings.MaskShardType.FourShardsPerMask:
                    break;
                case MiscSettings.MaskShardType.TwoShardsPerMask:
                    sb.Append(Localize("Mask Shards will be consolidated to double shards. "));
                    break;
                case MiscSettings.MaskShardType.OneShardPerMask:
                    sb.Append(Localize("Mask Shards will be consolidated to quadruple shards. "));
                    break;
            }
            switch (ms.VesselFragments)
            {
                case MiscSettings.VesselFragmentType.ThreeFragmentsPerVessel:
                    break;
                case MiscSettings.VesselFragmentType.TwoFragmentsPerVessel:
                    sb.Append(Localize("Vessel Fragments will be consolidated to double fragments. "));
                    break;
                case MiscSettings.VesselFragmentType.OneFragmentPerVessel:
                    sb.Append(Localize("Vessel Fragments will be consolidated to triple fragments. "));
                    break;
            }
            if (ms.SteelSoul)
            {
                sb.Append(Localize("Steel soul mode will be enabled. "));
            }

            return sb.ToString();
        }

        public static string Caption(this DuplicateItemSettings ds)
        {
            return Localize("For more information, see the Duplicate Items page in Advanced Settings.");
        }

        public static string Caption(this SplitGroupSettings sgs)
        {
            var groups = SplitGroupSettings.IntFields.Values.Select(fi => (fi, (int)fi.GetValue(sgs)))
                .Where(p => p.Item2 >= 0)
                .GroupBy(p => p.Item2);

            StringBuilder sb = new();

            if (sgs.RandomizeOnStart)
            {
                sb.Append("Randomized: ");
                sb.Append(string.Join(", ", groups.Where(g => g.Key < 3).SelectMany(g => g.Select(p => Localize(p.fi.GetMenuName())))));
                sb.Append(". ");
                foreach (var g in groups.Where(g => g.Key >= 3))
                {
                    sb.Append(g.Key);
                    sb.Append(": ");
                    sb.Append(string.Join(", ", g.Select(p => Localize(p.fi.GetMenuName()))));
                    sb.Append(". ");
                }
            }
            else
            {
                foreach (var g in groups)
                {
                    sb.Append(g.Key);
                    sb.Append(": ");
                    sb.Append(string.Join(", ", g.Select(p => Localize(p.fi.GetMenuName()))));
                    sb.Append(". ");
                }
            }

            if (sb.Length == 0) sb.Append(Localize("Disabled."));

            return sb.ToString();
        }
    }
}
