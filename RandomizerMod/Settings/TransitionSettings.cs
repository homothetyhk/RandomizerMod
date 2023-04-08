namespace RandomizerMod.Settings
{
    [Serializable]
    public class TransitionSettings : SettingsModule
    {
        public enum TransitionMode
        {
            None,
            MapAreaRandomizer,
            FullAreaRandomizer,
            RoomRandomizer,
        }
        public TransitionMode Mode;

        public enum AreaConstraintSetting
        {
            None,
            MoreConnectedMapAreas,
            MoreConnectedTitledAreas,
        }

        public AreaConstraintSetting AreaConstraint;

        /*
        // This will likely be difficult to implement -- not many rooms which don't have items or npcs or events
        // and then even fewer combinations which give matching transition counts
        public enum RemoveRoomsSetting
        {
            None,
            RemoveEmptyHallways,
            AggressivelyRemoveRooms,
        }
        public RemoveRoomsSetting Remove;
        */

        public enum TransitionMatchingSetting
        {
            MatchingDirections,
            MatchingDirectionsAndNoDoorToDoor,
            NonmatchingDirections
        }
        public TransitionMatchingSetting TransitionMatching;
        public bool Coupled = true;
    }
}
