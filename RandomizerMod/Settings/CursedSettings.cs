using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RandomizerMod.Extensions;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class CursedSettings : ICloneable
    {
        public bool RandomCurses;
        public bool RandomizeFocus;
        public bool RandomizeNail;
        public bool LongerProgressionChains;
        public bool ReplaceJunkWithOneGeo;
        public bool RemoveSpellUpgrades;
        public bool SplitClaw;
        public bool SplitCloak;
        public bool CursedMasks;
        public bool CursedNotches;

        public bool RandomizeSwim;


        public void HandleRandomCurses(Random rng)
        {
            if (RandomCurses)
            {
                foreach (var field in Util.GetFieldNames(typeof(CursedSettings)))
                {
                    if ((bool)Util.Get(this, field) && rng.Next(0, 2) == 0) Util.Set(this, field, false);
                }
            }
        }

        public string ToMultiline()
        {
            StringBuilder sb = new StringBuilder("Curses");
            foreach (var field in Util.GetFieldNames(typeof(CursedSettings)))
            {
                sb.AppendLine($"{field.FromCamelCase()}: {Util.Get(this, field)}");
            }

            return sb.ToString();
        }


        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
