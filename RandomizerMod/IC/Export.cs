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

            HashSet<string> sourceNames = new(ctx.transitionPlacements?.Select(x => x.source.Name) ?? Enumerable.Empty<string>());
            HashSet<string> targetNames = new(ctx.transitionPlacements?.Select(x => x.target.Name) ?? Enumerable.Empty<string>());
            if (targetNames.Contains($"{SceneNames.White_Palace_18}[top1]")
                || targetNames.Contains($"{SceneNames.White_Palace_17}[right1]")
                || targetNames.Contains($"{SceneNames.White_Palace_19}[top1]"))
            {
                ItemChangerMod.Modules.Add<ItemChanger.Modules.ReversePathOfPainSaw>();
            }
        }


        public static void ExportStart(GenerationSettings gs, RandoModContext ctx)
        {
            string startName = gs.StartLocationSettings.StartLocation;
            if (!string.IsNullOrEmpty(startName) && Data.GetStartDef(startName) is RandomizerData.StartDef def)
            {
                ItemChangerMod.ChangeStartGame(new ItemChanger.StartDef
                {
                    SceneName = def.SceneName,
                    X = def.X,
                    Y = def.Y,
                    MapZone = (int)def.Zone,
                    SpecialEffects = SpecialStartEffects.Default | SpecialStartEffects.SlowSoulRefill,
                    RespawnFacingRight = true,
                });
            }

            foreach (SmallPlatform p in PlatformList.GetPlatformList(gs, ctx)) ItemChangerMod.AddDeployer(p); 

            switch (startName)
            {
                // Platforms to allow escaping the Hive start regardless of difficulty or initial items
                case "Hive":
                    ItemChangerMod.AddDeployer(new SmallPlatform { SceneName = SceneNames.Hive_03, X = 58.5f, Y = 134f, Test = PlatformList.lacksRightClaw });
                    ItemChangerMod.AddDeployer(new SmallPlatform { SceneName = SceneNames.Hive_03, X = 58.5f, Y = 138.5f, Test = PlatformList.lacksAnyVertical });
                    break;

                // Drop the vine platforms and add small platforms for jumping up.
                case "Far Greenpath":
                    ItemChangerMod.AddDeployer(new SmallPlatform { SceneName = SceneNames.Fungus1_13, X = 45f, Y = 16.5f, Test = PlatformList.lacksLeftVertical });
                    ItemChangerMod.AddDeployer(new SmallPlatform { SceneName = SceneNames.Fungus1_13, X = 64f, Y = 16.5f, Test = PlatformList.lacksRightVertical });
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

        public static void ExportItemPlacements(RequestBuilder rb, IReadOnlyList<ItemPlacement> randoPlacements)
        {
            Dictionary<string, AbstractPlacement> export = new();
            ICFactory factory = new(rb, export);

            for(int j = 0; j < randoPlacements.Count; j++)
            {
                RandoModItem item = (RandoModItem)randoPlacements[j].item;
                RandoModLocation location = (RandoModLocation)randoPlacements[j].location;
                factory.HandlePlacement(j, item, location);
            }

            ItemChangerMod.AddPlacements(export.Select(kvp => kvp.Value));
        }

        public static void ExportTransitionPlacements(IEnumerable<TransitionPlacement> ps)
        {
            foreach (var p in ps) ItemChangerMod.AddTransitionOverride(new Transition(p.source.lt.data.SceneName, p.source.lt.data.GateName), new Transition(p.target.lt.data.SceneName, p.target.lt.data.GateName));
        }
    }
}
