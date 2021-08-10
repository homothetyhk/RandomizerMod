using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Logic
{
    public class LogicWaypoint
    {
        public LogicWaypoint(int id, LogicDef logic)
        {
            this.logic = logic;
            this.id = id;
            this.item = new SingleItem
            {
                name = logic.name,
                effect = new LogicItemEffect(id: id, incr: 1),
            };
        }

        public readonly LogicDef logic;
        public readonly LogicItem item;
        public readonly int id;
    }
}
