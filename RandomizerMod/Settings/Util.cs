using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using static RandomizerMod.LogHelper;

namespace RandomizerMod.Settings
{
    public static class Util
    {
        public static Dictionary<Type, Dictionary<string, FieldInfo>> TypeSortedFields;

        static Util()
        {
            TypeSortedFields = new Dictionary<Type, Dictionary<string, FieldInfo>>();
            Cache(typeof(GenerationSettings));
            Cache(typeof(PoolSettings));
            Cache(typeof(SkipSettings));
            Cache(typeof(CursedSettings));
            Cache(typeof(MiscSettings));
        }

        public static void Cache(Type T)
        {
            TypeSortedFields[T] = T.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .ToDictionary(f => f.Name, f => f);
        }

        public static IEnumerable<FieldInfo> GetOrderedFields(Type T)
        {
            if (!TypeSortedFields.TryGetValue(T, out Dictionary<string, FieldInfo> Fields))
            {
                Cache(T);
                Fields = TypeSortedFields[T];
            }

            return Fields.Values.OrderBy(f => f.Name);
        }

        public static FieldInfo GetField(Type T, string fieldName)
        {
            if (!TypeSortedFields.TryGetValue(T, out Dictionary<string, FieldInfo> Fields))
            {
                Cache(T);
                Fields = TypeSortedFields[T];
            }

            if (Fields.TryGetValue(fieldName, out FieldInfo field))
            {
                return field;
            }

            throw new KeyNotFoundException($"Unable to find field {fieldName} of type {T.Name}.");
        }

        public static object Get(this GenerationSettings GS, string path)
        {
            return Get((object)GS, path);
        }

        public static void Set(this GenerationSettings GS, string path, object value)
        {
            Set((object)GS, path, value);
        }

        public static IEnumerable<string> GetFieldNames(Type T)
        {
            if (!TypeSortedFields.TryGetValue(T, out Dictionary<string, FieldInfo> Fields))
            {
                Cache(T);
                Fields = TypeSortedFields[T];
            }

            return Fields.Keys;
        }

        public static object Get(object o, string path)
        {
            try
            {
                foreach (string piece in path.Split('.'))
                {
                    o = GetField(o.GetType(), piece).GetValue(o);
                }
                return o;
            }
            catch (Exception e)
            {
                LogError($"Error retrieving field at {path}:\n{e}");
                return null;
            }
        }

        public static void Set(object o, string path, object value)
        {
            try
            {
                string[] pieces = path.Split('.');
                for (int i = 0; i < pieces.Length - 1; i++)
                {
                    o = GetField(o.GetType(), pieces[i]).GetValue(o);
                }
                GetField(o.GetType(), pieces.Last()).SetValue(o, value);
            }
            catch (Exception e)
            {
                LogError($"Error retrieving field at {path}:\n{e}");
            }
        }

        /// <summary>
        /// Returns the first path to a field with the matching name, for a GenerationSettings object.
        /// <br/> e.g. GetPath("MildSkips") => "SkipSettings.MildSkips"
        /// </summary>
        public static string GetPath(string fieldName)
        {
            StringBuilder sb = new();
            Type T = typeof(GenerationSettings);
            foreach (FieldInfo settings in GetOrderedFields(T).Where(f => f.FieldType.IsSubclassOf(typeof(SettingsModule))))
            {
                foreach (FieldInfo fi in GetOrderedFields(settings.FieldType))
                {
                    if (fi.Name == fieldName) return $"{settings.Name}.{fieldName}";
                }
            }

            throw new ArgumentException("No corresponding field found.", nameof(fieldName));
        }
    }
}
