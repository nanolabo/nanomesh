using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Nanomesh.Collections
{
    public class SortedSetBuckets16<T>
    {
        private Func<T, Half> _valueGetter;
        private HashSet<T>[] _buckets = new HashSet<T>[65536];
        private HashSet<T> _allItems = new HashSet<T>();

        public SortedSetBuckets16(Func<T, Half> valueGetter)
        {
            _valueGetter = valueGetter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ushort GetHash(Half value)
        {
            Half* fRef = &value;
            return *(ushort*)fRef;
        }

        public int Count { get; private set; }

        private int _firstIndex = int.MaxValue;

        public bool Add(T item)
        {
            if (!_allItems.Add(item))
                return false;

            ushort index = GetHash(_valueGetter(item));

            if (index < _firstIndex)
                _firstIndex = index;

            var bucket = _buckets[index] ??= new HashSet<T>();

            if (bucket.Add(item))
            {
                Count++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Remove(T item)
        {
            if (!_allItems.TryGetValue(item, out item))
                return false;

            _allItems.Remove(item);

            ushort index = GetHash(_valueGetter(item));

            if (_buckets[index].Remove(item))
            {
                Count--;
                if (Count > 0 && _firstIndex == index)
                {
                    while (_buckets[_firstIndex] == null || _buckets[_firstIndex].Count == 0)
                    {
                        _firstIndex++;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public T GetFirst()
        {
            return _buckets[_firstIndex].First();
        }
    }
}