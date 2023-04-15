using System.Diagnostics;
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
        public RandoModContext? ctx;
        public RequestBuilder? rb;
        public readonly GenerationSettings gs;
        public readonly RandoMonitor rm;
        public readonly SettingsPM pm;
        public readonly Random rng;
        public LogArguments? args;
        public RandomizationStage[]? stages;
        public List<List<RandoPlacement>[]>? stagedPlacements;
        public Randomizer? randomizer;

        /// <summary>
        /// Event invoked on the RandoController immediately after sending all data to ItemChanger, but before printing logs and activating tracker data.
        /// </summary>
        public static event Action<RandoController>? OnExportCompleted;
        /// <summary>
        /// Event which allows external subscribers to modify the hash. Each subscriber is invoked separately, and the results are combined into the hash seed.
        /// <br/>Return values of 0 are ignored, and do not modify the hash.
        /// <br/>The second argument is the base hash seed, depending only on the generation settings and the placement data.
        /// </summary>
        public static event Func<RandoController, int, int>? OnCalculateHash;
        /// <summary>
        /// Event invoked at the start of <see cref="Run"/>.
        /// </summary>
        public static event Action<RandoController>? OnBeginRun;
        /// <summary>
        /// Event invoked if the RandoController is discarded after beginning a <see cref="Run"/>.
        /// </summary>
        public static event Action<RandoController>? OnAbort;
        /// <summary>
        /// Event invoked after running the RequestBuilder.
        /// </summary>
        public static event Action<RandoController>? OnRequestBuilt;
        /// <summary>
        /// Event for adding properties to log arguments before export and logging.
        /// </summary>
        public static event Action<LogArguments>? OnCreateLogArguments;


        public RandoController(GenerationSettings gs, SettingsPM pm, RandoMonitor rm)
        {
            this.gs = gs.Clone() as GenerationSettings;
            this.gs.Clamp();
            this.rm = rm;
            this.pm = pm;
            rng = new Random(gs.Seed + 4);
        }

        public void Run()
        {
            try
            {
                OnBeginRun?.Invoke(this);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error invoking OnBeginRun", e);
            }

            rm.OnNewAttempt += () => Profiling.Revert();

            Profiling.Revert();
            rb = new(gs, pm, rm);
            rb.Run(out stages, out ctx);
            Profiling.Commit();

            try
            {
                OnRequestBuilt?.Invoke(this);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error invoking OnRequestBuilt", e);
            }

            randomizer = new(new Random(gs.Seed), ctx, stages, rm);
            try
            {
                stagedPlacements = randomizer.Run();
            }
            catch
            {
                Profiling.Revert();
                LogDebug("Randomization failed. Profiling results for all attempts:");
                Profiling.Log(true);
                throw;
            }

            Profiling.Commit();
            LogDebug("Randomization completed successfully. Profiling results for succeeded attempts:");
            Profiling.Log();
            LogDebug("Profiling results for all attempts:");
            Profiling.Log(true);
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
            js.Converters.Add(new RandomizerCore.Json.TermConverter() { Terms = ctx.LM.Terms });

            js.Serialize(sw, gs);

            js.Serialize(sw, ctx.notchCosts);
            js.Serialize(sw, ctx.itemPlacements);
            js.Serialize(sw, ctx.transitionPlacements);

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
                        throw new InvalidOperationException($"Error invoking delegate {f.Method.Name} in OnCalculateHash", e);
                    }
                }
                seed += modSeed << 16; // preserve the lower 16 bits from the original hash. If all results are 0, then modSeed is 0 and seed is unmodified.
            }

            return seed;
        }

        public void Save()
        {
            args = new()
            {
                ctx = new RandoModContext(ctx), // we clone the context for the loggers so that we can obfuscate progression on the ctx used for Export,
                gs = gs,
                randomizer = randomizer,
            };
            try
            {
                OnCreateLogArguments?.Invoke(args);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error invoking OnCreateLogArguments", e);
            }

            rng.PermuteInPlace(ctx.itemPlacements);
            for (int i = 0; i < ctx.itemPlacements.Count; i++) ctx.itemPlacements[i] = ctx.itemPlacements[i] with { Index = i };
            rng.PermuteInPlace(ctx.transitionPlacements);

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
            Export.ExportItemPlacements(rb, ctx.itemPlacements);
            Export.ExportTransitionPlacements(rb, ctx.transitionPlacements);

            try
            {
                OnExportCompleted?.Invoke(this);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error invoking OnExportCompleted", e);
            }

            LogManager.InitDirectory();
            WriteRawSpoiler(gs, ctx); // write it here and not in LogManager so that it uses the permuted context
            LogManager.WriteLogs(args);
            
            RandomizerMod.RS.TrackerData.Setup(gs, ctx);
            RandomizerMod.RS.TrackerDataWithoutSequenceBreaks.Setup(gs, ctx);
        }

        private static void WriteRawSpoiler(GenerationSettings gs, RandoContext ctx)
        {
            Stopwatch sw = Stopwatch.StartNew();
            LogManager.Write((tw) =>
            {
                using JsonTextWriter jtr = new(tw);
                JsonUtil._js.Serialize(jtr, ctx);
                Log($"Printed raw spoiler in {sw.Elapsed.TotalSeconds} seconds.");
            }, "RawSpoiler.json");
        }

        public void Abort()
        {
            try
            {
                OnAbort?.Invoke(this);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error invoking OnAbort", e);
            }
        }
    }
}
