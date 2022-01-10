using RandomizerCore.Collections;

namespace RandomizerMod
{
    public class PriorityEvent<T> where T : Delegate
    {
        public PriorityEvent(out IPriorityEventOwner p)
        {
            p = new PriorityEventOwner(this);
        }

        private readonly SortedArrayList<float> _keys = new();
        private readonly SortedDictionary<float, T> _events = new();
        private KeyValuePair<float, T>[] _cachedSubscriberArray;
        private bool _cacheInvalidated = true;

        public void Subscribe(float key, T subscriber)
        {
            if (!_events.TryGetValue(key, out T current))
            {
                _keys.Add(key);
            }

            _events[key] = (T)Delegate.Combine(current, subscriber);
            _cacheInvalidated = true;
        }

        public void Unsubscribe(float key, T subscriber)
        {
            if (_events.TryGetValue(key, out T current))
            {
                _events[key] = (T)Delegate.Remove(current, subscriber);
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
            /// Returns an ordered enumerable of the event's subscribers and their priority keys at the moment of the request. 
            /// </summary>
            IEnumerable<KeyValuePair<float, T>> GetKeyedSubscribers();
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
                    _parent._cachedSubscriberArray = _parent._events.ToArray();
                    _parent._cacheInvalidated = false;
                }

                return _parent._cachedSubscriberArray.Select(kvp => kvp.Value);
            }

            public IEnumerable<T> GetSubscriberRange(float min, float max)
            {
                int lb = _parent._keys.FindInclusiveLowerBound(min);
                int ub = _parent._keys.FindExclusiveUpperBound(max);

                return Enumerable.Range(lb, ub - lb).Select(i => _parent._events[_parent._keys[i]]).ToList();
            }

            public IEnumerable<KeyValuePair<float, T>> GetKeyedSubscribers()
            {
                if (_parent._cacheInvalidated)
                {
                    _parent._cachedSubscriberArray = _parent._events.ToArray();
                    _parent._cacheInvalidated = false;
                }

                return _parent._cachedSubscriberArray;
            }
        }
    }
}
