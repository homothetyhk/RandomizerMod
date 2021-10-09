using RandomizerCore;
using RandomizerCore.Logic;

namespace RandomizerMod.RC
{
    public static class RCData
    {
        public static LogicManager ItemLM;
        public static LogicManager AreaLM;
        public static LogicManager RoomLM;
        public static bool Loaded = false;

        public static void Load()
        {
            if (Loaded) return;
            Loaded = true;
            ItemLM = Loader.LoadFromAssembly(RandomizerMod.Assembly, "RandomizerMod.Resources.Item.");
            AreaLM = Loader.LoadFromAssembly(RandomizerMod.Assembly, "RandomizerMod.Resources.Area.");
            RoomLM = Loader.LoadFromAssembly(RandomizerMod.Assembly, "RandomizerMod.Resources.Room.");
        }

        public static LogicManager GetLM(LogicMode mode) => mode switch
        {
            LogicMode.Room => RoomLM,
            LogicMode.Area => AreaLM,
            _ => ItemLM,
        };
    }
}
