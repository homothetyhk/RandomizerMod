using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomizerCore;
using RandomizerCore.Randomizers;
using RandomizerMod.Extensions;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;

namespace RandomizerMod
{
    public class RandoController
    {
        public readonly RandoContext ctx = new();
        public readonly GenerationSettings gs;
        public readonly RandoMonitor rm;
        public readonly MiniPM mpm;
        public readonly Random rng;
        private LogArguments args;

        public RandoController(GenerationSettings gs, MiniPM mpm, RandoMonitor rm)
        {
            this.gs = gs;
            this.rm = rm;
            this.mpm = mpm;
            this.rng = new Random(gs.Seed + 4);
        }

        public void Run()
        {
            gs.Clamp();
            if (gs.CursedSettings.RandomCurses) gs.CursedSettings.Randomize(rng);

            SelectStart();
            AssignNotchCosts();

            if (gs.TransitionSettings.Mode == TransitionSettings.TransitionMode.None)
            {
                WrappedSettings ws = new(gs);
                ItemRandomizer r = new(ws, ctx, rm);
                r.Run();
                args = new()
                {
                    ctx = ctx,
                    gs = gs,
                    randomizer = r,
                };
            }
            else
            {
                WrappedSettings ws = new(gs);
                TransitionRandomizer r = new(ws, ctx, rm);
                r.Run();
                args = new()
                {
                    ctx = ctx,
                    gs = gs,
                    randomizer = r,
                };
            }
        }

        public int Hash()
        {
            using System.Security.Cryptography.SHA256Managed sHA256 = new();
            StringBuilder sb = new();
            sb.Append(RandomizerData.JsonUtil.Serialize(gs));

            if (ctx.notchCosts != null) sb.Append(string.Join(";", ctx.notchCosts));

            if (ctx.itemPlacements != null)
            {
                foreach (var p in ctx.itemPlacements)
                {
                    sb.Append(p.item.Name);
                    sb.Append(p.location.Name);
                    sb.Append(';');
                }
            }

            if (ctx.transitionPlacements != null)
            {
                foreach (var p in ctx.transitionPlacements)
                {
                    sb.Append(p.source.Name);
                    sb.Append(p.target.Name);
                    sb.Append(';');
                }
            }

            byte[] sha = sHA256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));

            int seed = 17;
            for (int i = 0; i < sha.Length; i++) seed = (31 * seed) ^ sha[i];
            return seed;
        }

        public void Save()
        {
            Interop.BeginExport();
            Interop.ExportStart(gs);
            Interop.ExportSettings(gs);
            if (ctx.itemPlacements != null) Interop.ExportItemPlacements(gs, ctx.itemPlacements);
            if (ctx.transitionPlacements != null) Interop.ExportTransitionPlacements(ctx.transitionPlacements);
            if (ctx.notchCosts != null)
            {
                for (int i = 0; i < ctx.notchCosts.Count; i++) PlayerData.instance.SetInt($"charmCost_{i + 1}", ctx.notchCosts[i]);
            }
            if (gs.CursedSettings.CursedNotches) PlayerData.instance.SetInt(nameof(PlayerData.charmSlots), 1);
            if (gs.CursedSettings.CursedMasks)
            {
                PlayerData.instance.SetInt(nameof(PlayerData.maxHealth), 1);
                PlayerData.instance.SetInt(nameof(PlayerData.maxHealthBase), 1);
            }


            LogManager lm = new(GameManager.instance.profileID);
            lm.WriteLogs(args);
        }


        private void SelectStart()
        {
            var type = gs.StartLocationSettings.StartLocationType;
            if (type != StartLocationSettings.RandomizeStartLocationType.Fixed)
            {
                List<string> startNames = new(Data.GetStartNames().Where(s => mpm.Evaluate(Data.GetStartDef(s).logic)));
                if (type == StartLocationSettings.RandomizeStartLocationType.RandomExcludingKP) startNames.Remove("King's Pass");
                gs.StartLocationSettings.StartLocation = rng.Next(startNames);
            }

            LogHelper.LogDebug(gs.StartLocationSettings.StartLocation);
        }

        private void AssignNotchCosts()
        {
            if (!gs.MiscSettings.RandomizeNotchCosts)
            {
                ctx.notchCosts = Enumerable.Range(1, 40).Select(i => CharmNotchCosts.GetVanillaCost(i)).ToList();
            }
            else
            {
                ctx.notchCosts = CharmNotchCosts.GetUniformlyRandomCosts(rng, 70, 110).ToList();
            }
        }


    }
}
