using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RandomizerMod.Extensions;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class SkipSettings : SettingsModule
    {
        public bool MildSkips;
        public bool ShadeSkips;
        public bool FireballSkips;
        public bool AcidSkips;
        public bool SpikeTunnels;
        public bool DarkRooms;
        public bool SpicySkips;

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
