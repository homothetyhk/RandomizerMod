using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Extensions
{
    public static class RandomExtensions
    {
        public static T Pop<T>(this IList<T> list, int index = 0)
        {
            T val = list[index];
            list.RemoveAt(index);
            return val;
        }

        public static T Pop<T>(this List<T> list, Predicate<T> TSelector)
        {
            int i = list.FindIndex(TSelector);
            return list.Pop(i);
        }

        public static IEnumerable<T> Slice<T>(this List<T> list, int start, int count)
        {
            for (int i = start; i < start + count; i++) yield return list[i];
        }

        public static IEnumerable<T> Slice<T>(this T[] list, int start, int count)
        {
            for (int i = start; i < start + count; i++) yield return list[i];
        }

        public static bool TryPop<T>(this List<T> list, Predicate<T> TSelector, out T val)
        {
            int i = list.FindIndex(TSelector);
            if (i < 0)
            {
                val = default;
                return false;
            }
            else
            {
                val = list.Pop(i);
                return true;
            }
        }

        public static T Next<T>(this Random rand, IList<T> ts)
        {
            return ts[rand.Next(ts.Count())];
        }

        public static T PopNext<T>(this Random rand, IList<T> ts)
        {
            return ts.Pop(rand.Next(ts.Count));
        }

        public static int[] Permute(this Random rand, int n)
        {
            int[] arr = new int[n];
            for (int i = 0; i < n; i++) arr[i] = i;
            rand.PermuteInPlace(arr);
            return arr;
        }

        public static T[] Permute<T>(this Random rand, T[] input)
        {
            T[] output = input.Clone() as T[];
            rand.PermuteInPlace(output);
            return output;
        }

        public static void PermuteInPlace<T>(this Random rand, T[] input)
        {
            for (int i = input.Length - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                T temp = input[i];
                input[i] = input[j];
                input[j] = temp;
            }
        }

        public static List<T> Permute<T>(this Random rand, IEnumerable<T> input)
        {
            List<T> output = new List<T>(input);
            rand.PermuteInPlace(output);
            return output;
        }

        public static void PermuteInPlace<T>(this Random rand, List<T> input)
        {
            for (int i = input.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                T temp = input[i];
                input[i] = input[j];
                input[j] = temp;
            }
        }

        public static void Swap<T>(this T[] arr, int i, int j)
        {
            T t = arr[i];
            arr[i] = arr[j];
            arr[j] = t;
        }


    }
}
