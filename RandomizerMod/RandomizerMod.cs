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

        public static GlobalSettings GS { get; private set; } = new GlobalSettings();
        public static Logic.LogicManager ItemLogicManager; // TODO: move to data


        public RandomizerMod() : base("Randomizer 4") { }

        public override void Initialize()
        {
            base.Initialize();
            LogHelper.OnLog += Log;
            SpriteManager.LoadEmbeddedPngs("RandomizerMod.Resources.");
            ItemLogicManager = RandomizerData.Data.Load();
            MenuChanger.ModeMenu.AddMode(new Menu.RandomizerMenuConstructor());
        }


        public static string GetSHA1()
        {
            Assembly a = typeof(RandomizerMod).Assembly;
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            using (var sr = File.OpenRead(a.Location))
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
    }
}
