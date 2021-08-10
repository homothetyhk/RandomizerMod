using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace RandomizerMod.Settings
{
    public static class BinaryFormatting
    {
        public const char CLASS_SEPARATOR = ';';
        public const char STRING_SEPARATOR = '`';


        static Dictionary<Type, (FieldInfo[] numerics, FieldInfo[] bools)> FieldCache = 
            new Dictionary<Type, (FieldInfo[] numerics, FieldInfo[] bools)>();

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

            public ReflectionData(Type T)
            {
                FieldInfo[] fields = T.GetFields(BindingFlags.Public | BindingFlags.Instance);
                numericFields = fields.Where(f => NumericTypes.Contains(f.FieldType) || f.FieldType.IsEnum).OrderBy(f => f.Name).ToArray();
                boolFields = fields.Where(f => f.FieldType == typeof(bool)).OrderBy(f => f.Name).ToArray();
                stringFields = fields.Where(f => f.FieldType == typeof(string)).OrderBy(f => f.Name).ToArray();
            }
        }

        static Type[] NumericTypes = new Type[]
        {
            typeof(int),
            typeof(long),
            typeof(short),
            typeof(byte),
        };

        public static void WriteNumeric(this BinaryWriter writer, Type F, object box)
        {
            if (F == typeof(int))
            {
                writer.Write((int)box);
            }
            else if (F == typeof(long))
            {
                writer.Write((long)box);
            }
            else if (F == typeof(short))
            {
                writer.Write((short)box);
            }
            else if (F == typeof(byte))
            {
                writer.Write((byte)box);
            }
        }

        public static object ReadNumeric(this BinaryReader reader, Type F)
        {
            if (F == typeof(int))
            {
                return reader.ReadInt32();
            }
            else if (F == typeof(long))
            {
                return reader.ReadInt64();
            }
            else if (F == typeof(short))
            {
                return reader.ReadInt16();
            }
            else if (F == typeof(byte))
            {
                return reader.ReadByte();
            }
            return null;
        }

        public static string Serialize(object o)
        {
            Type T = o.GetType();
            ReflectionData rd = ReflectionData.GetReflectionData(T);

            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);
                foreach (FieldInfo f in rd.numericFields)
                {
                    Type F = f.FieldType;
                    if (F.IsEnum)
                    {
                        F = Enum.GetUnderlyingType(F);
                    }
                    object box = f.GetValue(o);
                    writer.WriteNumeric(F, box);
                }

                bool[] boolValues = rd.boolFields.Select(f => (bool)f.GetValue(o)).ToArray();
                foreach (byte b in ConvertBoolArrayToByteArray(boolValues))
                {
                    writer.Write(b);
                }

                writer.Close();
                StringBuilder sb = new StringBuilder(Convert.ToBase64String(stream.ToArray()));
                foreach (FieldInfo f in rd.stringFields)
                {
                    sb.Append($"{STRING_SEPARATOR}{(string)f.GetValue(o)}");
                }
                return sb.ToString();
            }
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


            using (MemoryStream stream = new MemoryStream(bytes))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                try
                {
                    foreach (FieldInfo field in rd.numericFields)
                    {
                        Type F = field.FieldType;
                        if (F.IsEnum)
                        {
                            F = Enum.GetUnderlyingType(F);
                        }
                        field.SetValue(o, reader.ReadNumeric(F));
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
                        rd.stringFields[i].SetValue(o, pieces[i + 1]);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.LogError($"Error in deserializing {T.Name}:\n{e}");
                }
            }
        }

        public static bool[] ConvertByteArrayToBoolArray(byte[] bytes)
        {
            BitArray bits = new BitArray(bytes);
            bool[] bools = new bool[bits.Count];
            bits.CopyTo(bools, 0);
            return bools;
        }

        public static byte[] ConvertBoolArrayToByteArray(bool[] boolArr)
        {
            BitArray bits = new BitArray(boolArr);
            byte[] bytes = new byte[bits.Length / 8 + 1];
            if (bits.Length > 0)
            {
                bits.CopyTo(bytes, 0);
            }
            
            return bytes;
        }

    }
}
