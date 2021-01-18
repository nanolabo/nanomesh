using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Nanomesh.Collections
{
    /// <summary>
    /// A very special collection featuring :
    /// - Persistent Sorting (smallest to largest)
    /// - Add(T) in constant time
    /// - Remove(T) in constant time
    /// - Contains(T) in constant time
    /// - GetFirst() & GetLast() in constant time (thanks to ordering)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SortedSetRadix32<T> : IEnumerable<T>
    {
        private Func<T, float> _valueGetter;
        private ConcurrentBag<HashSet<T>> _hashSetPool = new ConcurrentBag<HashSet<T>>();

        public SortedSetRadix32(Func<T, float> valueGetter)
        {
            _valueGetter = valueGetter;
        }

        private HashSet<T> RentHashSet()
        {
            return _hashSetPool.TryTake(out HashSet<T> item) ? item : new HashSet<T>();
        }

        private void ReturnHashSet(HashSet<T> item)
        {
            item.Clear();
            _hashSetPool.Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe int GetHash(float value)
        {
            Debug.Assert(value >= 0f, "Only works with positive numbers !");

            float* fRef = &value;
            return *(int*)fRef;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsBitSet(int num, int bit)
        {
            return 1 == ((num >> bit) & 1);
        }

        private BitNode _root = new BitNode();

        public int Count { get; private set; }

        public void Add(T item)
        {
            if (AddInternal(item, 31, _root))
                Count++;
        }

        private bool AddInternal(T item, int bitStart, BitNode current, HashSet<T> move = null)
        {
            float value = _valueGetter(item);
            int hash = GetHash(value);

            for (int bit = bitStart; bit >= 0; bit--)
            {
                bool is1 = IsBitSet(hash, bit);

                if (is1)
                {
                    if (current.node1 == null)
                    {
                        current = current.node1 = new BitNode();
                        if (move != null)
                            current.values = move;
                        break;
                    }
                    else
                    {
                        current = current.node1;
                        if (current.values?.Count > 0)
                        {
                            var inPlaceItem = current.values.First();
                            if (_valueGetter(inPlaceItem) == value)
                            {
                                break;
                            }
                            else
                            {
                                AddInternal(inPlaceItem, bit - 1, current, current.values);
                                current.values = null;
                            }
                        }
                    }
                }
                else
                {
                    if (current.node0 == null)
                    {
                        current = current.node0 = new BitNode();
                        if (move != null)
                            current.values = move;
                        break;
                    }
                    else
                    {
                        current = current.node0;
                        if (current.values?.Count > 0)
                        {
                            var inPlaceItem = current.values.First();
                            if (_valueGetter(inPlaceItem) == value)
                            {
                                break;
                            }
                            else
                            {
                                AddInternal(inPlaceItem, bit - 1, current, current.values);
                                current.values = null;
                            }
                        }
                    }
                }
            }

            if (current.values == null)
                current.values = RentHashSet();

            return current.values.Add(item);
        }

        public bool Remove(T item)
        {
            if (item == null)
                return false;

            int hash = GetHash(_valueGetter(item));
            BitNode current = _root;

            for (int bit = 31; bit >= 0; bit--)
            {
                bool is1 = IsBitSet(hash, bit);

                if (is1)
                {
                    if (current.node1 == null)
                    {
                        break;
                    }
                    else
                    {
                        current = current.node1;
                    }
                }
                else
                {
                    if (current.node0 == null)
                    {
                        break;
                    }
                    else
                    {
                        current = current.node0;
                    }
                }
            }

            if (current.values == null)
                return false;

            bool removed = current.values.Remove(item);

            if (removed)
                Count--;

            if (current.values.Count == 0)
            {
                ReturnHashSet(current.values);
                current.values = null;
            }

            return removed;
        }

        public IEnumerator<T> GetEnumerator()
        {
            int currentCount = 0;
            int depth = 0;
            var history = new BitNode[34];

            history[0] = _root;

            while (currentCount < Count)
            {
                int previous = history[depth + 1] != null ? (history[depth + 1] == history[depth].node1 ? 1 : (history[depth + 1] == history[depth].node0 ? 0 : -1)) : -1;

                if (history[depth].node0 != null && previous == -1)
                {
                    history[depth + 1] = history[depth].node0;
                    depth++;
                }
                else
                {
                    if (history[depth].node1 != null && previous != 1)
                    {
                        history[depth + 1] = history[depth].node1;
                        depth++;
                    }
                    else
                    {
                        if (history[depth].values?.Count > 0)
                        {
                            foreach (var item in history[depth].values)
                            {
                                yield return item;
                                currentCount++;
                            }
                        }

                        do
                        {
                            depth--;
                        }
                        while (depth >= 0
                           && (history[depth].node0 == null || history[depth].node1 == null)
                           && history[depth].values == null);
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(T item)
        {
            if (item == null)
                return false;

            int hash = GetHash(_valueGetter(item));
            BitNode current = _root;

            for (int bit = 31; bit >= 0; bit--)
            {
                bool is1 = IsBitSet(hash, bit);

                if (is1)
                {
                    if (current.node1 == null)
                    {
                        break;
                    }
                    else
                    {
                        current = current.node1;
                    }
                }
                else
                {
                    if (current.node0 == null)
                    {
                        break;
                    }
                    else
                    {
                        current = current.node0;
                    }
                }
            }

            if (current.values == null)
                return false;

            return current.values.Contains(item);
        }

        public T GetFirst()
        {
            if (Count <= 0)
                return default(T);

            BitNode current = _root;
            while (true)
            {
                if (current.node0 != null)
                {
                    current = current.node0;
                }
                else if (current.node1 != null)
                {
                    current = current.node1;
                }
                else
                {
                    Debug.Assert(current.values?.Count > 0, "It shouldn't be empty !");
                    return current.values.First();
                }
            }
        }

        public T GetLast()
        {
            if (Count <= 0)
                return default(T);

            BitNode current = _root;
            while (true)
            {
                if (current.node1 != null)
                {
                    current = current.node1;
                }
                else if (current.node0 != null)
                {
                    current = current.node0;
                }
                else
                {
                    Debug.Assert(current.values?.Count > 0, "It shouldn't be empty !");
                    return current.values.First();
                }
            }
        }

        public class BitNode
        {
            public BitNode node0;
            public BitNode node1;
            public HashSet<T> values;

            public override string ToString()
            {
                //return $"{(node0 != null ? "0" : null)}{node0?.ToString()} {(node1 != null ? "0" : null)}{node1?.ToString()}";
                if (values?.Count > 0)
                {
                    return $"{values.Count} * {values.First()}";
                }
                else
                {
                    return "-";
                }
            }
        }
    }
}
