using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nanomesh.Collections
{
    public class RadixSortedSet : IEnumerable<float>
    {
        private unsafe int GetHash(float value)
        {
            float* fRef = &value;
            return *(int*)fRef;
        }

        private bool IsBitSet(int num, int bit)
        {
            return 1 == ((num >> bit) & 1);
        }

        private BitNode _root = new BitNode();

        public int Count { get; private set; }

        public void Add(float value)
        {
            Debug.Assert(value >= 0f, "Only works with positive numbers !");

            int i = GetHash(value);

            BitNode current = _root;
            for (int b = 0; b < 32; b++)
            {
                bool is1 = IsBitSet(i, 31 - b);
                if (is1)
                {
                    current = current.node1 ??= new BitNode();
                }
                else
                {
                    current = current.node0 ??= new BitNode();
                }
            }

            current.value = value;
            current.values++;

            Count++;
        }

        public IEnumerator<float> GetEnumerator()
        {
            int c = 0;
            int depth = 0;
            var history = new BitNode[33];
            history[0] = _root;

            int depthLastSplit = -1;

            while (c < Count)
            {
                while (depth < 32)
                {
                    Debug.Assert(history[depth] != history[depth + 1]);
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

                    depth++;
                }

                for (int k = 0; k < history[32].values; k++)
                {
                    yield return history[32].value;
                    c++;
                }

                depth = depthLastSplit;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class BitNode
    {
        public BitNode node0;
        public BitNode node1;
        public int values;
        public float value;
    }
}
