using Newtonsoft.Json;
using RandomizerCore;
using RandomizerCore.Json;
using RandomizerMod.RC;

namespace RandomizerMod.Settings
{
    public class RandomizerSettings
    {
        public GenerationSettings GenerationSettings;
        public int ProfileID = -1; // LocalSettings load before GameManager.instance.profileId or PlayerData.instance.profileId are loaded.
        public TrackerData TrackerData;
        public TrackerData TrackerDataWithoutSequenceBreaks;
        [JsonIgnore]
        public RandoModContext Context;

        public void Setup()
        {
            if (GenerationSettings != null && Context == null && ProfileID >= 0)
            {
                string rawSpoilerPath = Path.Combine(Logging.LogManager.UserDirectory, "RawSpoiler.json");
                if (!File.Exists(rawSpoilerPath))
                {
                    LogError($"No file found at {rawSpoilerPath}!");
                    return;
                }

                using FileStream fs = File.OpenRead(rawSpoilerPath);
                using StreamReader sr = new(fs);
                using JsonTextReader jtr = new(sr);
                try
                {
                    Context = JsonUtil.Deserialize<RandoModContext>(jtr);
                }
                catch (Exception e)
                {
                    LogError($"Error deserializing raw spoiler from {rawSpoilerPath}\n:{e}");
                }
                try
                {
                    TrackerData?.Setup(GenerationSettings, Context);
                    TrackerDataWithoutSequenceBreaks?.Setup(GenerationSettings, Context);
                }
                catch (Exception e)
                {
                    LogError($"Error setting up tracker data:\n{e}");
                }

                Logging.LogManager.UpdateRecent(ProfileID);
            }
        }
    }
}
