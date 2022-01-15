using RandomizerCore.Collections;

namespace RandomizerMod
{
    public class PriorityEvent<T>
    {
        public PriorityEvent(out IPriorityEventOwner p)
        {
            p = new PriorityEventOwner(this);
        }

        private readonly SortedArrayList<PriorityEntry> _entries = new(Comparer<PriorityEntry>.Default, EqualityComparer<PriorityEntry>.Default);
        private PriorityEntry[] _cachedSubscriberArray;
        private bool _cacheInvalidated = true;

        public void Subscribe(float key, T value)
        {
            _entries.Add(new(key, value));
            _cacheInvalidated = true;
        }

        public void Unsubscribe(float key, T value)
        {
            if (_entries.Remove(new(key, value)))
            {
                _cacheInvalidated = true;
            }
        }

        public interface IPriorityEventOwner
        {
            /// <summary>
            /// Returns an ordered enumerable of the event's subscribers at the moment of the request. 
            /// </summary>
            IEnumerable<T> GetSubscribers();
            /// <summary>
            /// Returns an ordered enumerable of the event's subscribers in the specified priority range, with inclusive bounds.
            /// </summary>
            IEnumerable<T> GetSubscriberRange(float min, float max);

        }

        private class PriorityEventOwner : IPriorityEventOwner
        {
            public PriorityEventOwner(PriorityEvent<T> _parent) => this._parent = _parent;
            private readonly PriorityEvent<T> _parent;
            public IEnumerable<T> GetSubscribers()
            {
                if (_parent._cacheInvalidated)
                {
                    _parent._cachedSubscriberArray = _parent._entries.ToArray();
                    _parent._cacheInvalidated = false;
                }

                return _parent._cachedSubscriberArray.Select(kvp => kvp.Value);
            }

            public IEnumerable<T> GetSubscriberRange(float min, float max)
            {
                int lb = _parent._entries.FindInclusiveLowerBound(new (min, default));
                int ub = _parent._entries.FindExclusiveUpperBound(new (max, default));

                T[] result = new T[ub - lb];
                for (int i = lb; i < ub; i++) result[i - lb] = _parent._entries[i].Value;
                return result;
            }
        }

        public readonly record struct PriorityEntry(float Key, T Value) : IComparable<PriorityEntry>
        {
            public int CompareTo(PriorityEvent<T>.PriorityEntry other)
            {
                return Key.CompareTo(other.Key);
            }
        }
    }
}
