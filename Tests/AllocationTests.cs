using Nanolabo;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    class AllocationTests
    {
        private ConnectedMesh mesh;
        private const int iterations = 10000000;

        [SetUp]
        public void Setup()
        {
            mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(10, 10));
        }

        [Test]
        [Category("Benchmark Iterate")]
        public unsafe void IterateUsingIEnumerator()
        {
            unchecked
            {
                long ba = GC.GetAllocatedBytesForCurrentThread();

                int k = 0;
                for (int i = 0; i < iterations; i++)
                {
                    int n = 25 + i % 20;

                    /// 771ms - 480000000
                    foreach (var x in GetSiblings_IEnumerator(n))
                    {
                        k += x;
                    }
                }

                TestContext.Out.WriteLine("Allocated : " + (GC.GetAllocatedBytesForCurrentThread() - ba));

                Assert.AreEqual(2053000000, k);
            }
        }

        [Test]
        [Category("Benchmark Iterate")]
        public unsafe void IterateUsingUnsafe()
        {
            unchecked
            {
                long ba = GC.GetAllocatedBytesForCurrentThread();

                int k = 0;
                for (int i = 0; i < iterations; i++)
                {
                    int n = 25 + i % 20;

                    /// 150ms - 0 heap alloc
                    int* p = GetSiblings_Unsafe(n, out int size);
                    for (int j = 0; j < size; j++)
                    {
                        k += p[j];
                    }
                }

                TestContext.Out.WriteLine("Allocated : " + (GC.GetAllocatedBytesForCurrentThread() - ba));

                Assert.AreEqual(2053000000, k);
            }
        }

        [Test]
        [Category("Benchmark Iterate")]
        public unsafe void IterateUsingArray()
        {
            unchecked
            {
                long ba = GC.GetAllocatedBytesForCurrentThread();

                int k = 0;
                for (int i = 0; i < iterations; i++)
                {
                    int n = 25 + i % 20;

                    /// 293ms - 440000000
                    foreach (var x in GetSiblings_Array(n))
                    {
                        k += x;
                    }
                }

                TestContext.Out.WriteLine("Allocated : " + (GC.GetAllocatedBytesForCurrentThread() - ba));

                Assert.AreEqual(2053000000, k);
            }
        }

        [Test]
        [Category("Benchmark Iterate")]
        public unsafe void IterateUsingAction()
        {
            unchecked
            {
                long ba = GC.GetAllocatedBytesForCurrentThread();

                int k = 0;
                for (int i = 0; i < iterations; i++)
                {
                    int n = 25 + i % 20;

                    /// 152sms - 64
                    GetSiblings_Action(n, (x) => { k += x; });
                }

                TestContext.Out.WriteLine("Allocated : " + (GC.GetAllocatedBytesForCurrentThread() - ba));

                Assert.AreEqual(2053000000, k);
            }
        }

        [Test]
        [Category("Benchmark Iterate")]
        public unsafe void IterateUsingInline()
        {
            unchecked
            {
                long ba = GC.GetAllocatedBytesForCurrentThread();

                int k = 0;
                for (int i = 0; i < iterations; i++)
                {
                    int n = 25 + i % 20;

                    /// 64ms - 0 heap alloc
                    int nextNodeIndex = n;
                    while ((nextNodeIndex = mesh.nodes[nextNodeIndex].sibling) != n)
                        k += nextNodeIndex;
                }

                TestContext.Out.WriteLine("Allocated : " + (GC.GetAllocatedBytesForCurrentThread() - ba));

                Assert.AreEqual(2053000000, k);
            }
        }

        private IEnumerable<int> GetSiblings_IEnumerator(int nodeIndex)
        {
            int nextNodeIndex = nodeIndex;
            while ((nextNodeIndex = mesh.nodes[nextNodeIndex].sibling) != nodeIndex)
                yield return nextNodeIndex;
        }

        private int[] GetSiblings_Array(int nodeIndex)
        {
            // Make room
            int k = 0;
            int nextNodeIndex = nodeIndex;
            while ((nextNodeIndex = mesh.nodes[nextNodeIndex].sibling) != nodeIndex)
                k++;

            // Fill
            int[] res = new int[k];
            k = 0;
            while ((nextNodeIndex = mesh.nodes[nextNodeIndex].sibling) != nodeIndex)
                res[k++] = nextNodeIndex;

            return res;
        }

        private void GetSiblings_Action(int nodeIndex, Action<int> action)
        {
            int nextNodeIndex = nodeIndex;
            while ((nextNodeIndex = mesh.nodes[nextNodeIndex].sibling) != nodeIndex)
                action.Invoke(nextNodeIndex);
        }

        private unsafe int* GetSiblings_Unsafe(int nodeIndex, out int size)
        {
            // Make room
            int k = 0;
            int nextNodeIndex = nodeIndex;
            while ((nextNodeIndex = mesh.nodes[nextNodeIndex].sibling) != nodeIndex)
                k++;

            // Make fill
            int* res = stackalloc int[k];
            k = 0;
            while ((nextNodeIndex = mesh.nodes[nextNodeIndex].sibling) != nodeIndex)
                res[k++] = nextNodeIndex;

            size = k;
            return res;
        }
    }
}