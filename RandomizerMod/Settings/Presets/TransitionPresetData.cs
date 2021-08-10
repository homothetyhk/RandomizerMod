using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RandomizerMod.Settings.TransitionSettings;

namespace RandomizerMod.Settings.Presets
{
    public static class TransitionPresetData
    {
        public static TransitionSettings None;
        public static TransitionSettings Area;
        public static TransitionSettings Room;
        public static TransitionSettings ConnectedAreaRoom;
        public static TransitionSettings ReducedRoom;
        public static Dictionary<string, TransitionSettings> TransitionPresets;

        static TransitionPresetData()
        {
            None = new TransitionSettings
            {
                Mode = TransitionMode.None,
                MapRules = MapRuleSetting.None,
                Remove = RemoveRoomsSetting.None,
            };

            Area = new TransitionSettings
            {
                Mode = TransitionMode.AreaRandomizer,
                MapRules = MapRuleSetting.None,
                Remove = RemoveRoomsSetting.None,
            }; 

            Room = new TransitionSettings
            {
                Mode = TransitionMode.RoomRandomizer,
                MapRules = MapRuleSetting.None,
                Remove = RemoveRoomsSetting.None,
            };

            ConnectedAreaRoom = new TransitionSettings
            {
                Mode = TransitionMode.RoomRandomizer,
                MapRules = MapRuleSetting.InternallyConnectedAreas,
                Remove = RemoveRoomsSetting.None,
            };

            ReducedRoom = new TransitionSettings
            {
                Mode = TransitionMode.RoomRandomizer,
                MapRules = MapRuleSetting.None,
                Remove = RemoveRoomsSetting.RemoveEmptyHallways,
            };

            TransitionPresets = new Dictionary<string, TransitionSettings>
            {
                { "None", None },
                { "Area Rando", Area },
                { "Room Rando", Room },
                { "Connected-Area Room Rando", ConnectedAreaRoom },
                { "Reduced Room Rando", ReducedRoom },
            };
        }
    }
}
