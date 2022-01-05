using System.Text;
using System.Reflection;
using RandomizerCore.Extensions;

namespace RandomizerMod.Settings
{
    public class SkipSettings : SettingsModule
    {
        public bool PreciseMovement;
        public bool ProficientCombat;
        public bool BackgroundObjectPogos;
        public bool EnemyPogos;
        public bool ObscureSkips;
        public bool ShadeSkips;
        public bool InfectionSkips;
        public bool FireballSkips;
        public bool SpikeTunnels;
        public bool AcidSkips;
        public bool DamageBoosts;
        public bool DangerousSkips;
        public bool DarkRooms;
        public bool ComplexSkips;
        public bool DifficultSkips;

        private static Dictionary<string, FieldInfo> fields = typeof(SkipSettings)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(f => f.Name, f => f);
        public static string[] FieldNames => fields.Keys.ToArray();

        public void SetFieldByName(string fieldName, object value)
        {
            if (fields.TryGetValue(fieldName, out FieldInfo field))
            {
                field.SetValue(this, value);
            }
        }

        public bool GetFieldByName(string fieldName)
        {
            return fields.TryGetValue(fieldName, out FieldInfo field)
                && field.GetValue(this) is bool value
                && value;
        }

        public string ToMultiline()
        {
            StringBuilder sb = new StringBuilder("Skip Settings");
            foreach (var kvp in fields)
            {
                sb.AppendLine($"{kvp.Key.FromCamelCase()}: {kvp.Value.GetValue(this)}");
            }

            return sb.ToString();
        }
    }
}
