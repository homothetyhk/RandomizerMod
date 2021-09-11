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
        public static TransitionSettings Chaos;
        public static Dictionary<string, TransitionSettings> TransitionPresets;

        static TransitionPresetData()
        {
            None = new TransitionSettings
            {
                Mode = TransitionMode.None,
                ConnectAreas = false,
                Matched = true,
                Coupled = true,
            };

            Area = new TransitionSettings
            {
                Mode = TransitionMode.AreaRandomizer,
                ConnectAreas = false,
                Matched = true,
                Coupled = true,
            };

            ConnectedAreaRoom = new TransitionSettings
            {
                Mode = TransitionMode.RoomRandomizer,
                ConnectAreas = true,
                Matched = true,
                Coupled = true,
            };

            Room = new TransitionSettings
            {
                Mode = TransitionMode.RoomRandomizer,
                ConnectAreas = false,
                Matched = true,
                Coupled = true,
            };

            Chaos = new TransitionSettings
            {
                Mode = TransitionMode.RoomRandomizer,
                ConnectAreas = false,
                Matched = false,
                Coupled = false,
            };

            TransitionPresets = new Dictionary<string, TransitionSettings>
            {
                { "None", None },
                { "Area Rando", Area },
                { "Connected-Area Room Rando", ConnectedAreaRoom },
                { "Room Rando", Room },
                { "Chaos Room Rando", Chaos },
            };
        }
    }
}
