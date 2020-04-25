using NUnit.Framework;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using static Nanolabo.DecimateModifier;

namespace Nanolabo
{
    public class SortingTests
    {
        const int SIZE = 100000;
        const int ITERATION = 10;
        const int K = 10;

        HashSet<PairCollapse> pairs;

        [SetUp]
        public void Setup()
        {
            Random rnd = new Random();
            pairs = new HashSet<PairCollapse>();
            for (int i = 0; i < SIZE; i++)
            {
                pairs.Add(new PairCollapse() { pos1 = i, pos2 = i + SIZE, error = (float)rnd.NextDouble() });
            }
        }

        [Test]
        public void SortedSetRemoveMin()
        {
            SortedSet<PairCollapse> sortedSet = new SortedSet<PairCollapse>(pairs);

            var pair = pairs.ElementAt(SIZE / 2);
            Assert.IsTrue(sortedSet.Remove(pair));
        }

        [Test]
        [Category("Benchmark K Smallest")]
        public void Custom()
        {
            long ba = GC.GetAllocatedBytesForCurrentThread();

            int k = 0;

            for (int j = 0; j < ITERATION; j++)
            {
                foreach (var pair in pairs)
                {
                    k++;
                }
            }

            TestContext.Out.WriteLine("Result : " + k);

            TestContext.Out.WriteLine("Allocated : " + (GC.GetAllocatedBytesForCurrentThread() - ba));
        }

        [Test]
        [Category("Benchmark K Smallest")]
        public void SortedSet()
        {
            long ba = GC.GetAllocatedBytesForCurrentThread();

            SortedSet<PairCollapse> sortedSet = new SortedSet<PairCollapse>(pairs);

            float k = 0;
            for (int j = 0; j < ITERATION; j++)
            {
                var min = sortedSet.Min;
                sortedSet.Remove(min);
            }

            TestContext.Out.WriteLine("Mins : " + k);
            TestContext.Out.WriteLine("Allocated : " + (GC.GetAllocatedBytesForCurrentThread() - ba));
        }

        [Test]
        [Category("Benchmark K Smallest")]
        public void OrderBy()
        {
            long ba = GC.GetAllocatedBytesForCurrentThread();

            float k = 0;
            for (int j = 0; j < ITERATION; j++)
            {
                var mins = pairs.OrderBy(x => x).Take(K);
                
                foreach (var min in mins)
                {
                    k += min.error;
                }
            }

            TestContext.Out.WriteLine("Mins : " + k);
            TestContext.Out.WriteLine("Allocated : " + (GC.GetAllocatedBytesForCurrentThread() - ba));
        }

        [Test]
        [Category("Benchmark K Smallest")]
        public void ArraySort()
        {
            long ba = GC.GetAllocatedBytesForCurrentThread();

            for (int j = 0; j < ITERATION; j++)
            {
                var pool = ArrayPool<PairCollapse>.Shared;
                PairCollapse[] array = pool.Rent(pairs.Count);

                pairs.ToArray(ref array);

                Array.Sort(array);

                var min = array.Take(K);

                pool.Return(array);
            }

            TestContext.Out.WriteLine("Allocated : " + (GC.GetAllocatedBytesForCurrentThread() - ba));
        }
    }
}
