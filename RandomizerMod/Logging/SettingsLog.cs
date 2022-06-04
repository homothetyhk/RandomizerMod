using Newtonsoft.Json;
using RandomizerMod.RandomizerData;

namespace RandomizerMod.Logging
{
    public class SettingsLog : RandoLogger
    {
        public static event Action<LogArguments, TextWriter> AfterLogSettings;

        public override void Log(LogArguments args)
        {
            LogManager.Write(tw =>
            {
                tw.WriteLine("Logging RandomizerMod GenerationSettings:");
                using JsonTextWriter jtw = new(tw);
                JsonUtil._js.Serialize(jtw, args.gs);
                tw.WriteLine();
                tw.WriteLine("Logging menu GenerationSettings code:");
                tw.WriteLine(RandomizerMod.GS.DefaultMenuSettings.Serialize());
                tw.WriteLine("Logging final GenerationSettings code:");
                tw.WriteLine(args.gs.Serialize());
                try
                {
                    AfterLogSettings?.Invoke(args, tw);
                }
                catch (Exception e)
                {
                    LogError($"Error invoking AfterLogSettings:\n{e}");
                }
            }, "settings.txt");
        }
    }
}
