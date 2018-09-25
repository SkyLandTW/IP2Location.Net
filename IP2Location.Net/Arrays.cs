using System;
using System.Diagnostics.Contracts;

namespace IP2Location.Net
{
    internal static class Arrays
    {
        [Pure]
        public static BinarySearchResult BinarySearch<T>(this T[] array, Func<T, uint> getKey, uint targetKey)
        {
            if (array.Length == 0)
                return new BinarySearchResult(false, 0);
            var low = 0;
            var high = array.Length - 1;
            while (low <= high)
            {
                var mid = unchecked((low + high) >> 1);
                var midVal = array[mid];
                var midKey = getKey(midVal);
                if (midKey < targetKey)
                {
                    low = mid + 1;
                }
                else if (midKey > targetKey)
                {
                    high = mid - 1;
                }
                else
                {
                    return new BinarySearchResult(true, mid);
                }
            }
            return new BinarySearchResult(false, low);
        }
    }
}