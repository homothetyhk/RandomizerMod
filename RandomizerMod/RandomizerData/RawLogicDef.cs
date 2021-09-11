using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.RandomizerData
{
    public readonly struct RawLogicDef
    {
        public RawLogicDef(string name, string logic)
        {
            this.name = name;
            this.logic = logic;
        }

        public readonly string name;
        public readonly string logic;
    }

    public readonly struct ModeLogicDef
    {
        public ModeLogicDef(string name, string itemLogic, string areaLogic, string roomLogic)
        {
            this.name = name;
            this.itemLogic = itemLogic;
            this.areaLogic = areaLogic;
            this.roomLogic = roomLogic;
        }


        public readonly string name;
        public readonly string itemLogic;
        public readonly string areaLogic;
        public readonly string roomLogic;

        public RawLogicDef ToItemLogic()
        {
            return new RawLogicDef(name, itemLogic);
        }

        public RawLogicDef ToAreaLogic()
        {
            return new RawLogicDef(name, areaLogic);
        }

        public RawLogicDef ToRoomLogic()
        {
            return new RawLogicDef(name, roomLogic);
        }
    }

}
