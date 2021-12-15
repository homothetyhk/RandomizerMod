namespace RandomizerMod.RC
{
    public class Bucket<T>
    {
        public Bucket()
        {
            _buckets = new();
        }

        public Bucket(IEqualityComparer<T> equalityComparer)
        {
            _buckets = new(equalityComparer);
        }

        private readonly Dictionary<T, int> _buckets;

        public void Increment(T t, int value)
        {
            _buckets.TryGetValue(t, out int current);
            _buckets[t] = current + value;
        }

        public int GetCount(T t)
        {
            _buckets.TryGetValue(t, out int value);
            return value;
        }

        public void AddRange(IEnumerable<T> ts)
        {
            foreach (T t in ts) Increment(t, 1);
        }

        public void Add(T t)
        {
            Increment(t, 1);
        }

        public void RemoveAll(T t)
        {
            _buckets.Remove(t);
        }

        public void Remove(T t, int count)
        {
            if (_buckets.TryGetValue(t, out int value))
            {
                if (value > count) _buckets[t] = value - count;
                else _buckets.Remove(t); 
            }
        }

        public void Replace(T old, T replaceWith)
        {
            if (_buckets.TryGetValue(old, out int value))
            {
                _buckets.Remove(old);
                Increment(replaceWith, value);
            }
        }

        public void Replace(T old, Func<int, IEnumerable<T>> replacer)
        {
            if (_buckets.TryGetValue(old, out int value))
            {
                _buckets.Remove(old);
                AddRange(replacer(value));
            }
        }

        public void Set(T t, int value)
        {
            _buckets[t] = value;
        }

        public IEnumerable<T> EnumerateDistinct()
        {
            foreach (KeyValuePair<T, int> kvp in _buckets)
            {
                if (kvp.Value > 0) yield return kvp.Key;
            }
        }

        public IEnumerable<T> EnumerateWithMultiplicity()
        {
            foreach (KeyValuePair<T, int> kvp in _buckets)
            {
                for (int i = 0; i < kvp.Value; i++) yield return kvp.Key;
            }
        }

        public int GetTotal() => _buckets.Values.Sum();
    }
}
