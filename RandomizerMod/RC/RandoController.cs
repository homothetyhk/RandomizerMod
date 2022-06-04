using System.Text;
using Newtonsoft.Json;
using RandomizerCore;
using RandomizerCore.Extensions;
using RandomizerCore.Randomization;
using RandomizerMod.IC;
using RandomizerMod.Logging;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;

namespace RandomizerMod.RC
{
    public class RandoController
    {
        public RandoModContext ctx;
        public RequestBuilder rb;
        public readonly GenerationSettings gs;
        public readonly RandoMonitor rm;
        public readonly SettingsPM pm;
        public readonly Random rng;
        public LogArguments args;
        public RandomizationStage[] stages;
        public List<List<RandoPlacement>[]> stagedPlacements;
        public Randomizer randomizer;

        /// <summary>
        /// Event invoked on the RandoController immediately after sending all data to ItemChanger, but before printing logs and activating tracker data.
        /// </summary>
        public static event Action<RandoController> OnExportCompleted;
        /// <summary>
        /// Event which allows external subscribers to modify the hash. Each subscriber is invoked separately, and the results are combined into the hash seed.
        /// <br/>Return values of 0 are ignored, and do not modify the hash.
        /// <br/>The second argument is the base hash seed, depending only on the generation settings and the placement data.
        /// </summary>
        public static event Func<RandoController, int, int> OnCalculateHash;


        public RandoController(GenerationSettings gs, SettingsPM pm, RandoMonitor rm)
        {
            this.gs = gs.Clone() as GenerationSettings;
            this.rm = rm;
            this.pm = pm;
            rng = new Random(gs.Seed + 4);
        }

        public void Run()
        {
            gs.Clamp();
            rb = new(gs, pm, rm);
            rb.Run(out stages, out ctx);
            randomizer = new(new Random(gs.Seed), ctx, stages, rm);
            stagedPlacements = randomizer.Run();
            for (int i = 0; i < stagedPlacements.Count; i++)
            {
                for (int j = 0; j < stagedPlacements[i].Length; j++)
                {
                    foreach (RandoPlacement placement in stagedPlacements[i][j])
                    {
                        if (placement.Item is RandoModItem item && placement.Location is RandoModLocation location)
                        {
                            item.info?.onRandomizerFinish?.Invoke(placement);
                            location.info?.onRandomizerFinish?.Invoke(placement);
                            ctx.itemPlacements.Add(new(item, location));
                        }
                        else if (placement.Item is RandoModTransition target && placement.Location is RandoModTransition source)
                        {
                            ctx.transitionPlacements ??= new();
                            target.info?.onRandomizerFinish?.Invoke(placement);
                            source.info?.onRandomizerFinish?.Invoke(placement);
                            ctx.transitionPlacements.Add(new(target, source));
                        }
                        else
                        {
                            throw new InvalidOperationException($"Unknown placement type found in randomizer result with {placement.Item} at {placement.Location}");
                        }
                    }
                }
            }
            args = new()
            {
                ctx = new RandoModContext(ctx.LM) // we clone the context for the loggers so that we can obfuscate progression on the ctx used for Export
                {
                    notchCosts = ctx.notchCosts?.ToList(),
                    itemPlacements = ctx.itemPlacements?.ToList(),
                    transitionPlacements = ctx.transitionPlacements?.ToList(),
                    StartDef = ctx.StartDef,
                    InitialProgression = ctx.InitialProgression,
                    Vanilla = ctx.Vanilla.ToList(),
                    GenerationSettings = ctx.GenerationSettings,
                },
                gs = gs,
                randomizer = randomizer,
            };
        }

        public int Hash()
        {
            using System.Security.Cryptography.SHA256Managed sHA256 = new();
            using StringWriter sw = new();
            JsonSerializer js = new()
            {
                DefaultValueHandling = DefaultValueHandling.Include,
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.Auto,
            };
            js.Converters.Add(new RandomizerCore.Json.TermConverter() { LM = ctx.LM });
            js.Converters.Add(new RandomizerCore.Json.LogicDefConverter() { LM = ctx.LM });

            js.Serialize(sw, gs);

            if (ctx.notchCosts != null) js.Serialize(sw, ctx.notchCosts);
            else sw.Write("\"notchCosts\": null,");

            if (ctx.itemPlacements != null) js.Serialize(sw, ctx.itemPlacements);
            else sw.Write("\"itemPlacements\": null,");

            if (ctx.transitionPlacements != null) js.Serialize(sw, ctx.transitionPlacements);
            else sw.Write("\"transitionPlacements\": null,");

            StringBuilder sb = sw.GetStringBuilder();
            sb.Replace("\r", string.Empty);
            sb.Replace("\n", string.Empty);

            byte[] sha = sHA256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));

            int seed = 17;
            for (int i = 0; i < sha.Length; i++) seed = 31 * seed ^ sha[i];

            if (OnCalculateHash != null)
            {
                int modSeed = 0;
                foreach (Func<RandoController, int, int> f in OnCalculateHash.GetInvocationList())
                {
                    try
                    {
                        int result = f(this, seed);
                        if (result != 0)
                        {
                            modSeed = modSeed * 1566083941 + result;
                        }
                    }
                    catch (Exception e)
                    {
                        LogError($"Error invoking delegate {f.Method.Name} in OnCalculateHash:\n{e}");
                        continue;
                    }
                }
                seed += modSeed << 16; // preserve the lower 16 bits from the original hash. If all results are 0, then modSeed is 0 and seed is unmodified.
            }

            return seed;
        }

        public void Save()
        {
            if (ctx.itemPlacements != null)
            {
                rng.PermuteInPlace(ctx.itemPlacements);
                for (int i = 0; i < ctx.itemPlacements.Count; i++) ctx.itemPlacements[i] = ctx.itemPlacements[i] with { Index = i };
            }
            if (ctx.transitionPlacements != null)
            {
                rng.PermuteInPlace(ctx.transitionPlacements);
            }

            RandomizerMod.RS = new()
            {
                GenerationSettings = gs,
                Context = ctx,
                ProfileID = GameManager.instance.profileID,
                TrackerData = new() { AllowSequenceBreaks = true, logFileName = "TrackerDataDebugHistory.txt", },
                TrackerDataWithoutSequenceBreaks = new() { AllowSequenceBreaks = false, logFileName = "TrackerDataWithoutSequenceBreaksDebugHistory.txt", }
            };

            Export.BeginExport(gs, ctx);
            Export.ExportStart(gs, ctx);
            if (ctx.itemPlacements != null) Export.ExportItemPlacements(rb, ctx.itemPlacements);
            if (ctx.transitionPlacements != null) Export.ExportTransitionPlacements(rb, ctx.transitionPlacements);

            try
            {
                OnExportCompleted?.Invoke(this);
            }
            catch (Exception e)
            {
                LogError($"Error invoking OnExportCompleted:\n{e}");
            }

            LogManager.WriteLogs(args);
            WriteRawSpoiler(gs, ctx); // write it here and not in LogManager so that it uses the permuted context // write it after LogManager so it doesn't get deleted
            RandomizerMod.RS.TrackerData.Setup(gs, ctx);
            RandomizerMod.RS.TrackerDataWithoutSequenceBreaks.Setup(gs, ctx);
        }

        private static void WriteRawSpoiler(GenerationSettings gs, RandoContext ctx)
        {
            using StringWriter sw = new();
            LogManager.Write((tw) =>
            {
                using JsonTextWriter jtr = new(tw);
                JsonUtil._js.Serialize(jtr, ctx);
            }, "RawSpoiler.json");
        }
    }
}
