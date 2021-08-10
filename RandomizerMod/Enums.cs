using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod
{
    public enum LogicOperators
    {
        NONE = -1,
        ANY = -2,
        OR = -3,
        AND = -4,
        ESSENCECOUNT = -5,
        GRUBCOUNT = -6,
        ESSENCE200 = -7,
        FLAME3 = -8,
        FLAME6 = -9,
        GTR = -10,
        NOT = -11,
    }

    public enum LogicMode
    {
        Item,
        Area,
        Room
    }
}
