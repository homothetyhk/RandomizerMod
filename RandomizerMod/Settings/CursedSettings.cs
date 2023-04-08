using MenuChanger.Attributes;
using RandomizerCore.Extensions;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class CursedSettings : SettingsModule
    {
        public bool LongerProgressionChains;
        public bool ReplaceJunkWithOneGeo;
        public bool RemoveSpellUpgrades;
        public bool Deranged;
        [MenuRange(0, 4)]
        public int CursedMasks;
        [MenuRange(0, 2)]
        public int CursedNotches;
        public bool RandomizeMimics;
        [MinValue(0)]
        public int MaximumGrubsReplacedByMimics;

        public string ToMultiline()
        {
            StringBuilder sb = new("Curses");
            foreach (var field in Util.GetFieldNames(typeof(CursedSettings)))
            {
                sb.AppendLine($"{field.FromCamelCase()}: {Util.Get(this, field)}");
            }

            return sb.ToString();
        }
    }
}
