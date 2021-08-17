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
    public class RandomizerMod : Mod, IGlobalSettings<GlobalSettings>, IMenuMod
    {
        private readonly string _version = $"PRERELEASE: {GetSHA1()}";
        public override string GetVersion() => _version;

        public static GlobalSettings GS { get; private set; } = new GlobalSettings();

        public bool ToggleButtonInsideMenu => true;

        public static Logic.LogicManager ItemLogicManager; // TODO: move to data


        public RandomizerMod() : base("Randomizer 4") { }

        public override void Initialize()
        {
            base.Initialize();
            LogHelper.OnLog += Log;
            SpriteManager.LoadEmbeddedPngs("RandomizerMod.Resources.");

            try
            {
                ItemLogicManager = RandomizerData.Data.Load();
            }
            catch (Exception e)
            {
                LogError($"Error loading RandomizerData!\n{e}");
            }

            MenuChanger.ModeMenu.AddMode(new Menu.RandomizerMenuConstructor());
            Log("Initialization complete.");
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

        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            return new List<IMenuMod.MenuEntry> { new IMenuMod.MenuEntry("Hello", new string[] { "a", "b" }, "World", (i) => { }, () => 0) };
        }
    }
}
