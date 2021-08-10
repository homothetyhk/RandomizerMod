using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class TransitionSettings : ICloneable
    {
        public enum TransitionMode : byte
        {
            None,
            AreaRandomizer,
            RoomRandomizer,
        }
        public TransitionMode Mode;

        public enum MapRuleSetting : byte
        {
            None,
            InternallyConnectedAreas,
            // Vanilla area transitions?
        }
        public MapRuleSetting MapRules;

        public enum RemoveRoomsSetting : byte
        {
            None,
            RemoveEmptyHallways,
            AggressivelyRemoveRooms,
        }
        public RemoveRoomsSetting Remove;

        public LogicMode GetLogicMode()
        {
            switch (Mode)
            {
                default:
                case TransitionMode.None:
                    return LogicMode.Item;
                case TransitionMode.AreaRandomizer:
                    return LogicMode.Area;
                case TransitionMode.RoomRandomizer:
                    return LogicMode.Room;
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
