using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomizerMod.Extensions
{
    public static class CollectionExtensions
    {
        public static bool TryPop<T>(this Stack<T> ts, out T t)
        {
            bool flag = ts.Count > 0;
            t = flag ? ts.Pop() : default;
            return flag;
        }

        public static bool TryPeek<T>(this Stack<T> ts, out T t)
        {
            bool flag = ts.Count > 0;
            t = flag ? ts.Peek() : default;
            return flag;
        }

        public static bool TryDequeue<T>(this Queue<T> ts, out T t)
        {
            bool flag = ts.Count > 0;
            t = flag ? ts.Dequeue() : default;
            return flag;
        }

        public static bool TryPeek<T>(this Queue<T> ts, out T t)
        {
            bool flag = ts.Count > 0;
            t = flag ? ts.Peek() : default;
            return flag;
        }
    }
}
