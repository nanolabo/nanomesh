using System.Collections.Generic;

namespace Nanomesh
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

        public static bool TryAdd<K, V>(this Dictionary<K, V> dictionary, K key, V value)
        {
            if (dictionary.ContainsKey(key))
            {
                return false;
            }

            dictionary.Add(key, value);
            return true;
        }

        public static V GetOrAdd<K, V>(this Dictionary<K, V> dictionary, K key, V value)
        {
            if (dictionary.TryGetValue(key, out V existingValue))
            {
                return existingValue;
            }

            dictionary.Add(key, value);
            return value;
        }
    }
}
