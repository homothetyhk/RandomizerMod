using System.Text;
using RandomizerCore.Extensions;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class CursedSettings : SettingsModule
    {
        public bool LongerProgressionChains;
        public bool ReplaceJunkWithOneGeo;
        public bool RemoveSpellUpgrades;
        public bool CursedMasks;
        public bool CursedNotches;
        public bool RandomizeMimics;
        public int MaximumGrubsReplacedByMimics;

        public string ToMultiline()
        {
            StringBuilder sb = new StringBuilder("Curses");
            foreach (var field in Util.GetFieldNames(typeof(CursedSettings)))
            {
                sb.AppendLine($"{field.FromCamelCase()}: {Util.Get(this, field)}");
            }

            return sb.ToString();
        }
    }
}
