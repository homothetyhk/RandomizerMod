using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.RC;
using static RandomizerMod.LogHelper;

namespace RandomizerMod.Settings
{
    public class RandomizerSettings
    {
        public GenerationSettings GenerationSettings;
        public int ProfileID = -1; // LocalSettings load before GameManager.instance.profileId or PlayerData.instance.profileId are loaded.
        public TrackerData TrackerData;
        [JsonIgnore]
        public RandoContext Context;

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

                var lm = RCData.GetLM(GenerationSettings.TransitionSettings.GetLogicMode());
                var js = RandomizerCore.Json.JsonUtil.GetLogicSerializer(lm);

                using FileStream fs = File.OpenRead(rawSpoilerPath);
                using StreamReader sr = new(fs);
                using JsonTextReader jtr = new(sr);
                try
                {
                    Context = js.Deserialize<RandoContext>(jtr);
                }
                catch (Exception e)
                {
                    LogError($"Error deserializing raw spoiler from {rawSpoilerPath}\n:{e}");
                }
                try
                {
                    TrackerData.Setup(GenerationSettings, Context);
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
