using System.Collections.Generic;

namespace Nanolabo
{
    public static class CollectionUtils
    {
        public static T[] ToArray<T>(this HashSet<T> items, ref T[] array)
        {
            int i = 0;
            foreach (T item in items)
            {
                array[i++] = item;
            }

            return array;
        }
    }
}
