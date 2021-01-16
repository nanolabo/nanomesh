using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Nanomesh.Collections
{
    public class RadixSortedSet : IEnumerable<float>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe int GetHash(float value)
        {
            float* fRef = &value;
            return *(int*)fRef;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBitSet(int num, int bit)
        {
            return 1 == ((num >> bit) & 1);
        }

        private BitNode _root = new BitNode();

        public int Count { get; private set; }

        public void Add(float value)
        {
            Debug.Assert(value >= 0f, "Only works with positive numbers !");

            int hash = GetHash(value);

            Count++;

            AddInternal(value, hash, 31, _root);
        }

        private void AddInternal(float value, int hash, int bitStart, BitNode current)
        {
            for (int bit = bitStart; bit >= 0; bit--)
            {
                bool is1 = IsBitSet(hash, bit);

                if (is1)
                {
                    if (current.node1 == null)
                    {
                        current = current.node1 = new BitNode();
                        break;
                    }
                    else
                    {
                        current = current.node1;
                        if (current.value > 0 && current.value != value)
                        {
                            AddInternal(value, hash, bit, current);
                            current.values = 0;
                            current.value = 0;
                        }
                    }
                }
                else
                {
                    if (current.node0 == null)
                    {
                        current = current.node0 = new BitNode();
                        break;
                    }
                    else
                    {
                        current = current.node0;
                        if (current.values > 0 && current.value != value)
                        {
                            AddInternal(value, hash, bit, current);
                            current.values = 0;
                            current.value = 0;
                        }
                    }
                }
            }

            current.value = value;
            current.values++;
        }

        public IEnumerator<float> GetEnumerator()
        {
            int currentCount = 0;
            int depth = 0;
            int depthLastSplit = -1;
            var history = new BitNode[33];

            history[0] = _root;

            while (currentCount < Count)
            {
                while (depth < 32)
                {
                    if (history[depth].node0 == null)
                    {
                        history[depth + 1] = history[depth].node1;
                    }
                    else
                    {
                        if (history[depth].node1 == null)
                        {
                            history[depth + 1] = history[depth].node0;
                        }
                        else
                        {
                            // Tree splits here
                            if (history[depth].node0 == history[depth + 1])
                            {
                                // node0 was just browsed, so now we can go to node1
                                history[depth + 1] = history[depth].node1;
                            }
                            else
                            {
                                if (history[depth].node1 == history[depth + 1])
                                {
                                    do
                                    {
                                        history[depth + 1] = null;
                                        depth--;
                                    }
                                    while (depth >=0 && !(history[depth].node0 != null
                                        && history[depth].node1 != null
                                        && history[depth].node0 == history[depth + 1]));

                                    depthLastSplit = depth;

                                    continue;
                                }
                                else
                                {
                                    history[depth + 1] = history[depth].node0;
                                    depthLastSplit = depth;
                                }
                            }
                        }
                    }

                    if (history[depth].value > 0)
                    {
                        depthLastSplit = depth;
                        break;
                    }

                    depth++;
                }

                for (int k = 0; k < history[depth].values; k++)
                {
                    yield return history[depth].value;
                    currentCount++;
                }

                depth = depthLastSplit;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        //public bool Contains(float value)
        //{

        //}
    }

    public class BitNode
    {
        public BitNode node0;
        public BitNode node1;
        public int values;
        public float value;
    }
}
