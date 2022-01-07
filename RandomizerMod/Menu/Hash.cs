namespace RandomizerMod.Menu
{
    public static class Hash
    {
        public const int Length = 4;
        public static string[] Entries;

        public static string[] GetHash(int seed, int length = Length)
        {
            Random rng = new(seed + length);
            string[] arr = new string[length];
            for (int i = 0; i < length; i++)
            {
                arr[i] = Entries[rng.Next(Entries.Length)];
            }

            return arr;
        }

        static Hash()
        {
            using Stream stream = typeof(Hash).Assembly.GetManifestResourceStream("RandomizerMod.Resources.entries.txt");
            using StreamReader sr = new(stream);
            List<string> strs = new();
            while (sr.ReadLine() is string s) strs.Add(s);
            Entries = strs.ToArray();
        }
    }
}
