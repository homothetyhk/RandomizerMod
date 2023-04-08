using static RandomizerMod.Settings.TransitionSettings;

namespace RandomizerMod.Settings.Presets
{
    public static class TransitionPresetData
    {
        public static TransitionSettings None;
        public static TransitionSettings MapArea;
        public static TransitionSettings Area;
        public static TransitionSettings Room;
        public static TransitionSettings ConnectedAreaRoom;
        public static TransitionSettings Chaos;
        public static Dictionary<string, TransitionSettings> TransitionPresets;

        static TransitionPresetData()
        {
            None = new TransitionSettings
            {
                Mode = TransitionMode.None,
                AreaConstraint = AreaConstraintSetting.None,
                TransitionMatching = TransitionMatchingSetting.MatchingDirections,
                Coupled = true,
            };

            MapArea = new TransitionSettings
            {
                Mode = TransitionMode.MapAreaRandomizer,
                AreaConstraint = AreaConstraintSetting.None,
                TransitionMatching = TransitionMatchingSetting.MatchingDirections,
                Coupled = true,
            };

            Area = new TransitionSettings
            {
                Mode = TransitionMode.FullAreaRandomizer,
                AreaConstraint = AreaConstraintSetting.None,
                TransitionMatching = TransitionMatchingSetting.MatchingDirections,
                Coupled = true,
            };

            ConnectedAreaRoom = new TransitionSettings
            {
                Mode = TransitionMode.RoomRandomizer,
                AreaConstraint = AreaConstraintSetting.MoreConnectedMapAreas,
                TransitionMatching = TransitionMatchingSetting.MatchingDirections,
                Coupled = true,
            };

            Room = new TransitionSettings
            {
                Mode = TransitionMode.RoomRandomizer,
                AreaConstraint = AreaConstraintSetting.None,
                TransitionMatching = TransitionMatchingSetting.MatchingDirections,
                Coupled = true,
            };

            Chaos = new TransitionSettings
            {
                Mode = TransitionMode.RoomRandomizer,
                AreaConstraint = AreaConstraintSetting.None,
                TransitionMatching = TransitionMatchingSetting.NonmatchingDirections,
                Coupled = false,
            };

            TransitionPresets = new Dictionary<string, TransitionSettings>
            {
                { "None", None },
                { "Map Area Rando", MapArea },
                { "Full Area Rando", Area },
                { "Connected-Area Room Rando", ConnectedAreaRoom },
                { "Room Rando", Room },
                { "Chaos Room Rando", Chaos },
            };
        }
    }
}
