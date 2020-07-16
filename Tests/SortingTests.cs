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
        const int SIZE = 1000;
        const int ITERATION = 10;
        const int K = 10;

        HashSet<EdgeCollapse> pairs;

        [SetUp]
        public void Setup()
        {
            Random rnd = new Random();
            pairs = new HashSet<EdgeCollapse>();
            for (int i = 0; i < SIZE; i++)
            {
                pairs.Add(new EdgeCollapse(i, i + SIZE) { error = rnd.NextDouble() });
            }
        }

        [Test]
        public void SortedSetRemoveMin()
        {
            SortedSet<EdgeCollapse> sortedSet = new SortedSet<EdgeCollapse>(pairs);

            //var pair = pairs.ElementAt(SIZE / 2);
            //Assert.IsTrue(sortedSet.Remove(pair));
            //Assert.IsFalse(sortedSet.Remove(pair));

            //pair = pairs.Min();
            //Assert.IsTrue(sortedSet.Remove(pair));
            //Assert.IsFalse(sortedSet.Remove(pair));

            for (int i = 0; i < SIZE; i++)
            {
                TestContext.Out.WriteLine(pairs.Min().error);
                //Assert.IsTrue(sortedSet.Remove(pairs.Min()));
                Assert.IsTrue(sortedSet.Remove(pairs.Min()));
            }

            TestContext.Out.WriteLine(sortedSet.Count);
            Assert.IsTrue(sortedSet.Count == 0);
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

            SortedSet<EdgeCollapse> sortedSet = new SortedSet<EdgeCollapse>(pairs);

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

            double k = 0;
            for (int j = 0; j < ITERATION; j++)
            {
                var mins = pairs.OrderByCustom(x => x).Take(K);
                
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
                var pool = ArrayPool<EdgeCollapse>.Shared;
                EdgeCollapse[] array = pool.Rent(pairs.Count);

                pairs.ToArray(ref array);

                Array.Sort(array);

                var min = array.Take(K);

                pool.Return(array);
            }

            TestContext.Out.WriteLine("Allocated : " + (GC.GetAllocatedBytesForCurrentThread() - ba));
        }
    }
}
