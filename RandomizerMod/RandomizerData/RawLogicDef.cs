using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.RandomizerData
{
    public struct RawLogicDef
    {
        public string name;
        public string logic;
    }

    public struct ModeLogicDef
    {
        public string name;
        public string itemLogic;
        public string areaLogic;
        public string roomLogic;

        public RawLogicDef ToItemLogic()
        {
            return new RawLogicDef
            {
                name = name,
                logic = itemLogic
            };
        }

        public RawLogicDef ToAreaLogic()
        {
            return new RawLogicDef
            {
                name = name,
                logic = areaLogic
            };
        }

        public RawLogicDef ToRoomLogic()
        {
            return new RawLogicDef
            {
                name = name,
                logic = roomLogic
            };
        }
    }

}
