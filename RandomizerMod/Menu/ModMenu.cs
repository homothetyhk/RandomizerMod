using ItemChanger.Internal.Menu;
using DirectoryOptions = RandomizerMod.Menu.RandomizerMenu.DirectoryOptions;

namespace RandomizerMod.Menu
{
    public static class ModMenu
    {
        public static MenuScreen GetRandomizerMenuScreen(MenuScreen modListMenu)
        {
            ModMenuScreenBuilder builder = new(Localize("Randomizer 4"), modListMenu);
            builder.AddButton(Localize("Open Log Folder"), null, () => RandomizerMenu.OpenFile(null, string.Empty, DirectoryOptions.RecentLogFolder));
            builder.AddButton(Localize("Open Helper Log"), null, () => RandomizerMenu.OpenFile(null, "HelperLog.txt", DirectoryOptions.RecentLogFolder));
            builder.AddButton(Localize("Open Tracker Log"), null, () => RandomizerMenu.OpenFile(null, "TrackerLog.txt", DirectoryOptions.RecentLogFolder));
#if DEBUG
            builder.AddButton(Localize("Reset Profiling Data"), null, () => RandomizerCore.Profiling.Reset());
#endif
            return builder.CreateMenuScreen();
        }
    }
}
