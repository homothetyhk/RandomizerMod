namespace RandomizerModTests
{
    public static class Extensions
    {
        public static IEnumerable<object[]> Product(this IEnumerable<IEnumerable<object>> eeos)
        {
            IEnumerator<object>[] eos = eeos
            .Select(x => x.GetEnumerator())
            .Where(x => x.MoveNext())
            .ToArray();

            while (true)
            {
                object[] os = new object[eos.Length];
                for (int i = 0; i < eos.Length; i++) os[i] = eos[i].Current;
                yield return os;

                for (int i = 0; i < eos.Length; i++)
                {
                    IEnumerator<object> eo = eos[i];
                    if (!eo.MoveNext())
                    {
                        if (i + 1 == eos.Length) { yield break; }
                        eo.Reset();
                        eo.MoveNext();
                        break;
                    }
                }
            }
        }
    }
}