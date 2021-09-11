using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class TransitionSettings : SettingsModule
    {
        public enum TransitionMode
        {
            None,
            AreaRandomizer,
            RoomRandomizer,
        }
        public TransitionMode Mode;

        public bool ConnectAreas;

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

        public bool Matched = true;
        public bool Coupled = true;

        public LogicMode GetLogicMode()
        {
            return Mode switch
            {
                TransitionMode.AreaRandomizer => LogicMode.Area,
                TransitionMode.RoomRandomizer => LogicMode.Room,
                _ => LogicMode.Item,
            };
        }
    }
}
