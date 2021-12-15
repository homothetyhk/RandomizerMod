using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomizerMod
{
    public class PriorityEvent<T> where T : Delegate
    {
        public PriorityEvent(out IPriorityEventOwner p)
        {
            p = new PriorityEventOwner(this);
        }

        private readonly SortedDictionary<float, T> _events = new();
        private T[] _cachedSubscriberArray;
        private bool _cacheInvalidated = true;

        public void Subscribe(float key, T subscriber)
        {
            T current = _events.TryGetValue(key, out current) ? current : null;
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
            T[] GetSubscriberList();
        }

        private class PriorityEventOwner : IPriorityEventOwner
        {
            public PriorityEventOwner(PriorityEvent<T> _parent) => this._parent = _parent;
            private readonly PriorityEvent<T> _parent;
            public T[] GetSubscriberList()
            {
                if (_parent._cacheInvalidated)
                {
                    _parent._cachedSubscriberArray = _parent._events.Values.ToArray();
                    _parent._cacheInvalidated = false;
                }

                return _parent._cachedSubscriberArray;
            }
        }
    }
}
