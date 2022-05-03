using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemChanger;
using SD = ItemChanger.Util.SceneDataUtil;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;
using static RandomizerMod.LogHelper;
using RandomizerCore;
using RandomizerMod.RC;

namespace RandomizerMod.IC
{
    public static class Export
    {
        public static void BeginExport(GenerationSettings gs, RandoModContext ctx)
        {
            ItemChangerMod.CreateSettingsProfile(overwrite: true);
            ItemChangerMod.Modules.Add<RandomizerModule>();
            ItemChangerMod.Modules.Add<TrackerUpdate>();
            ItemChangerMod.Modules.Add<TrackerLogModule>();
            ItemChangerMod.Modules.Add<HelperLogModule>();
            ItemChangerMod.Modules.Add<ItemChanger.Modules.JijiJinnPassage>();
            var cpo = ItemChangerMod.Modules.Add<ItemChanger.Modules.CompletionPercentOverride>();
            cpo.CoupledTransitions = gs.TransitionSettings.Coupled;
            ItemChangerMod.Modules.Add<ItemChanger.Modules.DisablePalaceMidWarp>();
            if (gs.MiscSettings.RandomizeNotchCosts)
            {
                ItemChangerMod.Modules.Add<ItemChanger.Modules.NotchCostUI>();
            }
            if (!gs.PoolSettings.GrimmkinFlames)
            {
                ItemChangerMod.Modules.Get<ItemChanger.Modules.InventoryTracker>().TrackGrimmkinFlames = false;
            }
            if (gs.MiscSettings.SalubraNotches == MiscSettings.SalubraNotchesSetting.AutoGivenAtCharmThreshold)
            {
                ItemChangerMod.Modules.Add<ItemChanger.Modules.AutoSalubraNotches>();
            }

            HashSet<string> sourceNames = new(ctx.transitionPlacements?.Select(x => x.Source.Name) ?? Enumerable.Empty<string>());
            HashSet<string> targetNames = new(ctx.transitionPlacements?.Select(x => x.Target.Name) ?? Enumerable.Empty<string>());
            if (targetNames.Contains($"{SceneNames.White_Palace_18}[top1]")
                || targetNames.Contains($"{SceneNames.White_Palace_17}[right1]")
                || targetNames.Contains($"{SceneNames.White_Palace_19}[top1]"))
            {
                ItemChangerMod.Modules.Add<ItemChanger.Modules.ReversePathOfPainSaw>();
            }

            var pde = ItemChangerMod.Modules.GetOrAdd<ItemChanger.Modules.PlayerDataEditModule>();
            if (ctx.notchCosts != null)
            {
                for (int i = 0; i < ctx.notchCosts.Count; i++) pde.AddPDEdit($"charmCost_{i + 1}", ctx.notchCosts[i]);
            }
            if (gs.CursedSettings.CursedNotches > 0)
            {
                pde.AddPDEdit(nameof(PlayerData.charmSlots), 3 - gs.CursedSettings.CursedNotches);
            }
            if (gs.CursedSettings.CursedMasks > 0)
            {
                pde.AddPDEdit(nameof(PlayerData.maxHealth), 5 - gs.CursedSettings.CursedMasks);
                pde.AddPDEdit(nameof(PlayerData.maxHealthBase), 5 - gs.CursedSettings.CursedMasks);
            }
        }


        public static void ExportStart(GenerationSettings gs, RandoModContext ctx)
        {
            if (ctx.StartDef?.ToItemChangerStartDef() is ItemChanger.StartDef def)
            {
                ItemChangerMod.ChangeStartGame(def);
            }

            foreach (SmallPlatform p in PlatformList.GetPlatformList(gs, ctx)) ItemChangerMod.AddDeployer(p); 

            switch (gs.StartLocationSettings.StartLocation)
            {
                // Platforms to allow escaping the Hive start regardless of difficulty or initial items
                case "Hive":
                    ItemChangerMod.AddDeployer(new SmallPlatform { SceneName = SceneNames.Hive_03, X = 58.5f, Y = 134f, Test = PlatformList.lacksRightClaw });
                    ItemChangerMod.AddDeployer(new SmallPlatform { SceneName = SceneNames.Hive_03, X = 58.5f, Y = 138.5f, Test = PlatformList.lacksAnyVertical });
                    break;

                // Drop the vine platforms and add small platforms for jumping up.
                case "Far Greenpath":
                    ItemChangerMod.AddDeployer(new SmallPlatform { SceneName = SceneNames.Fungus1_13, X = 45f, Y = 16.5f, Test = PlatformList.lacksLeftClaw });
                    ItemChangerMod.AddDeployer(new SmallPlatform { SceneName = SceneNames.Fungus1_13, X = 64f, Y = 16.5f, Test = PlatformList.lacksRightClaw });
                    SD.Save(SceneNames.Fungus1_13, "Vine Platform (1)");
                    SD.Save(SceneNames.Fungus1_13, "Vine Platform (2)");
                    break;

                // With the Lower Greenpath start, getting to the rest of Greenpath requires
                // cutting the vine to the right of the vessel fragment.
                case "Lower Greenpath":
                    if (gs.NoveltySettings.RandomizeNail) SD.Save(SceneNames.Fungus1_13, "Vine Platform");
                    break;
            }
        }

        public static void ExportItemPlacements(RequestBuilder rb, IReadOnlyList<ItemPlacement> itemPlacements)
        {
            Dictionary<string, AbstractPlacement> export = new();
            ICFactory factory = new(rb, export);

            foreach (ItemPlacement p in itemPlacements)
            {
                factory.HandlePlacement(p.Index, p.Item, p.Location);
            }

            ItemChangerMod.AddPlacements(export.Select(kvp => kvp.Value));
        }

        public static void ExportTransitionPlacements(RequestBuilder rb, IEnumerable<TransitionPlacement> ps)
        {
            foreach ((RandoModTransition target, RandoModTransition source) in ps)
            {
                ITransition realTarget;
                Transition realSource;
                if (target.info?.realTargetCreator != null)
                {
                    realTarget = target.info.realTargetCreator(rb, new(target, source));
                }
                else
                {
                    realTarget = new Transition(target.TransitionDef.SceneName, target.TransitionDef.DoorName);
                }

                if (source.info?.realSourceCreator != null)
                {
                    realSource = source.info.realSourceCreator(rb, new(target, source));
                }
                else
                {
                    realSource = new Transition(source.TransitionDef.SceneName, source.TransitionDef.DoorName);
                }

                ItemChangerMod.AddTransitionOverride(realSource, realTarget);
            }
        }
    }
}
