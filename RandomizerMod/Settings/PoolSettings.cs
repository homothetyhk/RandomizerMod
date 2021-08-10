using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RandomizerMod.Extensions;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class PoolSettings : ICloneable
    {
        public bool Dreamers;
        public bool Skills;
        public bool Charms;
        public bool Keys;
        public bool MaskShards;
        public bool VesselFragments;
        public bool PaleOre;
        public bool CharmNotches;
        public bool GeoChests;
        public bool Relics;
        public bool RancidEggs;
        public bool Stags;
        public bool Maps;
        public bool WhisperingRoots;
        public bool Grubs;
        public bool LifebloodCocoons;
        public bool SoulTotems;
        public bool GrimmkinFlames;
        public bool GeoRocks;
        public bool BossEssence;
        public bool BossGeo;
        public bool LoreTablets;

        // TODO: replace?
        public bool PalaceTotems;
        public bool PalaceLore;


        private static Dictionary<string, FieldInfo> fields = typeof(PoolSettings)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(f => f.Name, f => f);
        public static string[] FieldNames => fields.Keys.ToArray();

        public bool GetFieldByName(string fieldName)
        {
            if (fields.TryGetValue(fieldName, out FieldInfo field))
            {
                return (bool)field.GetValue(this);
            }
            return false;
        }

        public void SetFieldByName(string fieldName, object value)
        {
            if (fields.TryGetValue(fieldName, out FieldInfo field))
            {
                field.SetValue(this, value);
            }
        }

        public string ToMultiline()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Pool Settings");
            foreach (var kvp in fields)
            {
                sb.AppendLine($"{kvp.Key.FromCamelCase()}: {kvp.Value.GetValue(this)}");
            }

            return sb.ToString();
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
