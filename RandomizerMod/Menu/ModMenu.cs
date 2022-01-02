using ItemChanger.Internal.Menu;
using DirectoryOptions = RandomizerMod.Menu.RandomizerMenu.DirectoryOptions;

namespace RandomizerMod.Menu
{
    public static class ModMenu
    {
        public static MenuScreen GetRandomizerMenuScreen(MenuScreen modListMenu)
        {
            ModMenuScreenBuilder builder = new("Randomizer 4", modListMenu);
            builder.AddButton("Open Log Folder", null, () => RandomizerMenu.OpenFile(null, String.Empty, DirectoryOptions.RecentLogFolder));
            builder.AddButton("Open Helper Log", null, () => RandomizerMenu.OpenFile(null, "HelperLog.txt", DirectoryOptions.RecentLogFolder));
            builder.AddButton("Open Tracker Log", null, () => RandomizerMenu.OpenFile(null, "TrackerLog.txt", DirectoryOptions.RecentLogFolder));
            return builder.CreateMenuScreen();
        }
    }
}
