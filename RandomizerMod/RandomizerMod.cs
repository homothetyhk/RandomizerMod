using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Modding;
using RandomizerMod.Settings;

namespace RandomizerMod
{
    public class RandomizerMod : Mod, IGlobalSettings<GlobalSettings>
    {
        private readonly string _version = $"PRERELEASE: {GetSHA1()}";
        public override string GetVersion() => _version;

        public static GlobalSettings GS { get; private set; } = new();


        public RandomizerMod() : base("Randomizer 4") { }

        public override void Initialize()
        {
            base.Initialize();
            LogHelper.OnLog += Log;
            RandomizerCore.LogHelper.OnLog += Log;
            SpriteManager.LoadEmbeddedPngs("RandomizerMod.Resources.");

            foreach (var s in typeof(RandomizerMod).Assembly.GetManifestResourceNames()) Log(s);

            try
            {
                RandomizerData.Data.Load();
            }
            catch (Exception e)
            {
                LogError($"Error loading RandomizerData!\n{e}");
                throw;
            }

            try
            {
                RCData.Load();
            }
            catch (Exception e)
            {
                LogError($"Error loading RCData!\n{e}");
                throw;
            }

            MenuChanger.ModeMenu.AddMode(new Menu.RandomizerMenuConstructor());
            Log("Initialization complete.");
        }


        public static string GetSHA1()
        {
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            using (var sr = File.OpenRead(Location))
            {
                return Convert.ToBase64String(sha1.ComputeHash(sr));
            }
        }

        public void OnLoadGlobal(GlobalSettings s)
        {
            GS = s;
        }

        public GlobalSettings OnSaveGlobal()
        {
            return GS;
        }

        public static string Folder { get; }
        public static string Location { get; }
        public static Assembly Assembly { get; }

        static RandomizerMod()
        {
            Assembly = typeof(RandomizerMod).Assembly;
            Location = Assembly.Location;
            Folder = Path.GetDirectoryName(Location);
        }
    }
}
