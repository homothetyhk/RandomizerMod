using GlobalEnums;

namespace RandomizerMod.RandomizerData
{
    public record StartDef
    {
        public string Name { get; init; }

        // respawn marker properties
        public string SceneName { get; init; }
        public float X { get; init; }
        public float Y { get; init; }
        public MapZone Zone { get; init; }

        // logic info
        public string Transition { get; init; }

        // Primitive logic -- check SettingsPM
        public string Logic { get; init; }
    }
}
