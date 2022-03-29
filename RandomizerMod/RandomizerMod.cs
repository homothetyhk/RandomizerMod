using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Modding;
using RandomizerMod.Extensions;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace RandomizerMod
{
    public class RandomizerMod : Mod, IGlobalSettings<GlobalSettings>, ILocalSettings<RandomizerSettings>, ICustomMenuMod
    {
        /// <summary>
        /// Returns true if the currently loaded settings contain valid RandomizerMod data.
        /// </summary>
        public static bool IsRandoSave { get => RS != null && RS.GenerationSettings != null && RS.ProfileID >= 0; }
        public override string GetVersion() => Version;

        public static GlobalSettings GS { get; private set; } = new();
        public static RandomizerSettings RS { get; internal set; } = new();

        public static ItemChanger.Internal.SpriteManager SpriteManager { get; private set; }

        public RandomizerMod() : base("Randomizer 4") { }

        public override void Initialize()
        {
            base.Initialize();
            LogHelper.OnLog += Log;
            RandomizerCore.LogHelper.OnLog += Log;
            Logging.LogManager.Initialize();
            LocalizationData.Load();
            SpriteManager = new(Assembly, "RandomizerMod.Resources.");

            try
            {
                RandomizerData.Data.Load();
            }
            catch (Exception e)
            {
                LogError($"Error loading RandomizerData!\n{e}");
                throw;
            }

            MenuChanger.ModeMenu.AddMode(new Menu.RandomizerMenuConstructor());
            Log("Initialization complete.");
        }


        public static string GetSHA1()
        {
            using var sha1 = System.Security.Cryptography.SHA1.Create();
            using var sr = File.OpenRead(Location);
            return Convert.ToBase64String(sha1.ComputeHash(sr));
        }

        public void OnLoadGlobal(GlobalSettings s)
        {
            GS = !GlobalSettings.IsInvalid(s) ? s : new();
        }

        public GlobalSettings OnSaveGlobal()
        {
            return GS;
        }

        public void OnLoadLocal(RandomizerSettings s)
        {
            RS = s;
            RS?.Setup();
        }

        public RandomizerSettings OnSaveLocal()
        {
            return RS;
        }

        public static string Folder { get; }
        public static string Location { get; }
        public static Assembly Assembly { get; }
        public static string SHA1 { get; }
        public static string Version { get; }

        static RandomizerMod()
        {
            Assembly = typeof(RandomizerMod).Assembly;
            Location = Assembly.Location;
            Folder = Path.GetDirectoryName(Location);
            SHA1 = GetSHA1();
            System.Version v = Assembly.GetName().Version;
            int buildHash = Math.Abs(SHA1.GetStableHashCode()) % 997;
            Version = $"{v.Major}.{v.Minor}.{v.Build}+{buildHash.ToString().PadLeft(3, '0')}";
        }

        public bool ToggleButtonInsideMenu => false;
        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? _)
        {
            return Menu.ModMenu.GetRandomizerMenuScreen(modListMenu);
        }
    }
}