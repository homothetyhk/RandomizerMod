using Newtonsoft.Json;

namespace RandomizerMod.RandomizerData
{
    public record TransitionDef
    {
        [JsonIgnore] public string Name => $"{SceneName}[{DoorName}]";
        public string SceneName { get; init; }
        public string DoorName { get; init; }
        [JsonIgnore] public virtual string TitledArea { get => Data.GetRoomDef(SceneName)?.TitledArea; }
        [JsonIgnore] public virtual string MapArea { get => Data.GetRoomDef(SceneName)?.MapArea; }
        public string VanillaTarget { get; init; }
        public TransitionDirection Direction { get; init; }
        public bool IsTitledAreaTransition { get; init; }
        public bool IsMapAreaTransition { get; init; }
        public TransitionSides Sides { get; init; }
    }

    public enum TransitionSides
    {
        Both = 0,
        /// <summary>
        /// A one way transition exiting a scene.
        /// </summary>
        OneWayIn = 1,
        /// <summary>
        /// A one way transition entering a scene.
        /// </summary>
        OneWayOut = 2,
    }

    public enum TransitionDirection
    {
        Door,
        Left,
        Right,
        Top,
        Bot,
    }

}
