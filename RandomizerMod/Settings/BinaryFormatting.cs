using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using MenuChanger.Attributes;

namespace RandomizerMod.Settings
{
    public class MinValueAttribute : Attribute
    {
        public MinValueAttribute(int value) { Value = value; }
        public readonly int Value;
    }

    public class MaxValueAttribute : Attribute
    {
        public MaxValueAttribute(int value) { Value = value; }
        public readonly int Value;
    }

    public readonly struct ConstrainedIntField
    {
        public ConstrainedIntField(FieldInfo field)
        {
            this.field = field;
            
            if (field.GetCustomAttribute<MenuRangeAttribute>() is MenuRangeAttribute mr)
            {
                this.minValue = (int)mr.min;
                this.maxValue = (int)mr.max;
            }
            else
            {
                if (field.GetCustomAttribute<MinValueAttribute>() is MinValueAttribute min)
                {
                    this.minValue = min.Value;
                }
                else if (field.FieldType.IsEnum)
                {
                    this.minValue = Enum.GetValues(field.FieldType).Cast<int>().Min();
                }
                else this.minValue = int.MinValue;

                if (field.GetCustomAttribute<MaxValueAttribute>() is MaxValueAttribute max)
                {
                    this.maxValue = max.Value;
                }
                else if (field.FieldType.IsEnum)
                {
                    this.maxValue = Enum.GetValues(field.FieldType).Cast<int>().Max();
                }
                else this.maxValue = int.MaxValue;
            }
        }

        public readonly FieldInfo field;
        public readonly int minValue;
        public readonly int maxValue;
    }

    public static class BinaryFormatting
    {
        public const char CLASS_SEPARATOR = ';';
        public const char STRING_SEPARATOR = '`';

        public class ReflectionData
        {
            static Dictionary<Type, ReflectionData> cache = new Dictionary<Type, ReflectionData>();

            public static ReflectionData GetReflectionData(Type T)
            {
                if (cache.TryGetValue(T, out ReflectionData rd)) return rd;
                else
                {
                    cache[T] = rd = new ReflectionData(T);
                    return rd;
                }
            }

            public FieldInfo[] numericFields;
            public FieldInfo[] boolFields;
            public FieldInfo[] stringFields;
            public ConstrainedIntField[] intFields;

            public ReflectionData(Type T)
            {
                FieldInfo[] fields = T.GetFields(BindingFlags.Public | BindingFlags.Instance);
                boolFields = fields.Where(f => f.FieldType == typeof(bool)).OrderBy(f => f.Name).ToArray();
                stringFields = fields.Where(f => f.FieldType == typeof(string)).OrderBy(f => f.Name).ToArray();
                intFields = fields.Where(f => f.FieldType == typeof(int) || f.FieldType.IsEnum).OrderBy(f => f.Name).Select(f => new ConstrainedIntField(f)).ToArray();
            }
        }

        public static string Serialize(object o)
        {
            Type T = o.GetType();
            ReflectionData rd = ReflectionData.GetReflectionData(T);

            using MemoryStream stream = new();
            BinaryWriter writer = new(stream);
            foreach (ConstrainedIntField f in rd.intFields)
            {
                int range = f.maxValue - f.maxValue;
                int value = (int)f.field.GetValue(o);
                if (range < 0)
                {
                    writer.Write(value);
                }
                else if (range <= byte.MaxValue)
                {
                    writer.Write((byte)(value - f.minValue));
                }
                else if (range <= ushort.MaxValue)
                {
                    writer.Write((ushort)(value - f.minValue));
                }
                else
                {
                    writer.Write(value);
                }
            }

            bool[] boolValues = rd.boolFields.Select(f => (bool)f.GetValue(o)).ToArray();
            foreach (byte b in ConvertBoolArrayToByteArray(boolValues))
            {
                writer.Write(b);
            }

            writer.Close();
            StringBuilder sb = new(Convert.ToBase64String(stream.ToArray()));
            foreach (FieldInfo f in rd.stringFields)
            {
                sb.Append($"{STRING_SEPARATOR}{Convert.ToBase64String(Encoding.ASCII.GetBytes((string)f.GetValue(o)))}");
                // this is less compressed than just adding the string directly, but it avoids the risk of special characters in the string
                // and critically, prevents people from memeing about the start location name being readable from the settings string.
            }
            return sb.ToString();
        }

        public static void Deserialize(string code, object o)
        {
            Type T = o.GetType();
            ReflectionData rd = ReflectionData.GetReflectionData(T);

            string[] pieces = code.Split(STRING_SEPARATOR);
            code = pieces[0];

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(code);
            }
            catch (Exception e)
            {
                LogHelper.LogWarn($"Malformatted Base64 string {{{code}}}\n" + e);
                return;
            }


            using MemoryStream stream = new(bytes);
            using BinaryReader reader = new(stream);
            try
            {
                foreach (ConstrainedIntField field in rd.intFields)
                {
                    int range = field.maxValue - field.maxValue;
                    if (range < 0)
                    {
                        field.field.SetValue(o, reader.ReadInt32());
                    }
                    else if (range <= byte.MaxValue)
                    {
                        field.field.SetValue(o, field.minValue + reader.ReadByte());
                    }
                    else if (range <= ushort.MaxValue)
                    {
                        field.field.SetValue(o, field.minValue + reader.ReadUInt16());
                    }
                    else
                    {
                        field.field.SetValue(o, reader.ReadInt32());
                    }
                }

                bool[] boolValues = ConvertByteArrayToBoolArray(reader.ReadBytes(bytes.Length - (int)stream.Position));
                int cap = Math.Min(boolValues.Length, rd.boolFields.Length);
                for (int i = 0; i < cap; i++)
                {
                    rd.boolFields[i].SetValue(o, boolValues[i]);
                }

                cap = Math.Min(rd.stringFields.Length, pieces.Length - 1);
                for (int i = 0; i < cap; i++)
                {
                    rd.stringFields[i].SetValue(o, Encoding.ASCII.GetString(Convert.FromBase64String(pieces[i + 1])));
                }
            }
            catch (Exception e)
            {
                LogHelper.LogError($"Error in deserializing {T.Name}:\n{e}");
            }
        }

        public static bool[] ConvertByteArrayToBoolArray(byte[] bytes)
        {
            BitArray bits = new(bytes);
            bool[] bools = new bool[bits.Count];
            bits.CopyTo(bools, 0);
            return bools;
        }

        public static byte[] ConvertBoolArrayToByteArray(bool[] boolArr)
        {
            BitArray bits = new(boolArr);
            byte[] bytes = new byte[bits.Length / 8 + 1];
            if (bits.Length > 0)
            {
                bits.CopyTo(bytes, 0);
            }
            
            return bytes;
        }

    }
}
