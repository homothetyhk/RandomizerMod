using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings.Presets
{
    public static class Hash
    {
        public const int Length = 4;
        public static string[] Entries;

        public static string[] GetHash(int seed)
        {
            Random rng = new Random(seed + Entries.Length);
            string[] arr = new string[Length];
            for (int i = 0; i < Length; i++)
            {
                arr[i] = Entries[rng.Next(Entries.Length)];
            }

            return arr;
        }

        static Hash()
        {
            using (Stream stream = typeof(Hash).Assembly.GetManifestResourceStream("RandomizerMod.Resources.entries.txt"))
            using (StreamReader sr = new StreamReader(stream))
            {
                List<string> strs = new List<string>();
                while (sr.ReadLine() is string s) strs.Add(s);
                Entries = strs.ToArray();
            }
        }

    }
}
