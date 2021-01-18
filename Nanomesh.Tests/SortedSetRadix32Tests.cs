using Nanomesh.Collections;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nanomesh.Tests
{
    class SortedSetRadix32Tests
    {
        Random _random;

        [SetUp]
        public void Setup()
        {
            _random = new Random();
        }

        private float RandomFloat(float minValue, float maxValue)
        {
            float next = (float)_random.NextDouble();
            return minValue + (next * (maxValue - minValue));
        }

        [Test]
        public void Contains()
        {
            var radixCustom = new SortedSetRadix32<float>(x => x);

            radixCustom.Add(12354f);
            radixCustom.Add(1f);
            radixCustom.Add(0.0016554f);
            radixCustom.Add(9f);

            var array = radixCustom.ToArray();

            Assert.IsTrue(radixCustom.Contains(12354f));
            Assert.IsTrue(radixCustom.Contains(9f));
            Assert.IsFalse(radixCustom.Contains(5f));
        }

        [Test]
        public void GetFirst()
        {
            var radixCustom = new SortedSetRadix32<float>(x => x);

            radixCustom.Add(0.0651f);
            radixCustom.Add(10f);
            radixCustom.Add(1f);
            radixCustom.Add(1f);
            radixCustom.Add(0.0001f);
            radixCustom.Add(999f);

            var array = radixCustom.ToArray();

            Assert.IsTrue(radixCustom.GetFirst() == 0.0001f);
        }

        [Test]
        public void GetLast()
        {
            var radixCustom = new SortedSetRadix32<float>(x => x);

            radixCustom.Add(5f);
            radixCustom.Add(6f);
            radixCustom.Add(100000f);
            radixCustom.Add(5f);
            radixCustom.Add(0.0001f);
            radixCustom.Add(999f);

            var array = radixCustom.ToArray();

            Assert.IsTrue(radixCustom.GetLast() == 100000f);
        }

        public class TestObject
        {
            public float error;
            public override string ToString() => $"{error}f";
        }

        [Test]
        public void Add_SameObject()
        {
            var radixCustom = new SortedSetRadix32<float>(x => x);

            radixCustom.Add(1f);
            radixCustom.Add(1f);
            radixCustom.Add(1f);

            Assert.AreEqual(1, radixCustom.Count);
            Assert.IsTrue(radixCustom.Contains(1f));
        }

        [Test]
        public void Add_SameValue()
        {
            var radixCustom = new SortedSetRadix32<TestObject>(x => x.error);

            radixCustom.Add(new TestObject { error = 1f });
            radixCustom.Add(new TestObject { error = 1f });
            radixCustom.Add(new TestObject { error = 1f });

            Assert.AreEqual(3, radixCustom.Count);
            var array = radixCustom.ToArray();
        }

        [Test]
        public void Remove_SameValue()
        {
            var radixCustom = new SortedSetRadix32<TestObject>(x => x.error);

            radixCustom.Add(new TestObject { error = 1f });
            radixCustom.Add(new TestObject { error = 1f });
            radixCustom.Add(new TestObject { error = 1f });

            Assert.AreEqual(3, radixCustom.Count);

            Assert.IsTrue(radixCustom.Remove(radixCustom.GetFirst()));

            Assert.AreEqual(2, radixCustom.Count);

            Assert.IsTrue(radixCustom.Remove(radixCustom.GetFirst()));

            Assert.AreEqual(1, radixCustom.Count);

            Assert.IsTrue(radixCustom.Remove(radixCustom.GetFirst()));

            Assert.AreEqual(0, radixCustom.Count);

            Assert.IsFalse(radixCustom.Remove(radixCustom.GetFirst()));
        }

        [Test]
        public void ToArray()
        {
            var radixCustom = new SortedSetRadix32<TestObject>(x => x.error);

            radixCustom.Add(new TestObject { error = 1f });
            radixCustom.Add(new TestObject { error = 2f });
            radixCustom.Add(new TestObject { error = 3f });
            radixCustom.Add(new TestObject { error = 956423323f });
            radixCustom.Add(new TestObject { error = 0.12356f });

            Assert.AreEqual(5, radixCustom.Count);
            var array = radixCustom.Select(x => x.error).ToArray();
            var set = new HashSet<float>(array);

            Assert.IsTrue(set.Contains(1f));
            Assert.IsTrue(set.Contains(2f));
            Assert.IsTrue(set.Contains(3f));
            Assert.IsTrue(set.Contains(956423323f));
            Assert.IsTrue(set.Contains(0.12356f));
            Assert.IsFalse(set.Contains(10f));
        }

        [Test]
        public void IsBitSet()
        {
            var radixCustom = new SortedSetRadix32<float>(x => x);
            Assert.AreEqual(false, SortedSetRadix32<float>.IsBitSet(1065353216, 0));
            Assert.AreEqual(false, SortedSetRadix32<float>.IsBitSet(1065353216, 1));
            Assert.AreEqual(false, SortedSetRadix32<float>.IsBitSet(1065353216, 2));
            Assert.AreEqual(false, SortedSetRadix32<float>.IsBitSet(1065353216, 3));
            Assert.AreEqual(true, SortedSetRadix32<float>.IsBitSet(1065353216, 28));
            Assert.AreEqual(true, SortedSetRadix32<float>.IsBitSet(1065353216, 29));
            Assert.AreEqual(false, SortedSetRadix32<float>.IsBitSet(1065353216, 30));
            Assert.AreEqual(false, SortedSetRadix32<float>.IsBitSet(1065353216, 31));
        }

        [Test]
        public void Hashing()
        {
            for (int i = 0; i < 100000; i++)
            {
                float f1 = RandomFloat(0f, 2f);
                float f2 = RandomFloat(0f, 2f);

                int i1 = SortedSetRadix32<float>.GetHash(f1);
                int i2 = SortedSetRadix32<float>.GetHash(f2);

                Assert.True((i1 > i2 && f1 > f2) || (i1 <= i2 && f1 <= f2));
            }
        }

        [Test]
        public void Sorting()
        {
            var radixCustom = new SortedSetRadix32<float>(x => x);

            for (int i = 0; i < 100000; i++)
            {
                radixCustom.Add(RandomFloat(0f, 1000000f));
            }

            var array = radixCustom.ToArray();

            for (int i = 0; i < array.Length - 1; i++)
            {
                Assert.LessOrEqual(array[i], array[i + 1]);
            }
        }

        [Test]
        public void Performance()
        {
            var radixCustom = new SortedSetRadix32<float>(x => x);

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++)
            {
                radixCustom.Add(RandomFloat(0f, 10000f));
            }
            sw.Stop();
            Console.WriteLine($"{sw.ElapsedMilliseconds}ms {GC.GetTotalMemory(false) * 0.001 * 0.001}");
            GC.Collect();

            sw.Restart();
            for (int i = 0; i < 100000; i++)
            {
                radixCustom.Add(RandomFloat(0f, 10000f));
            }
            sw.Stop();
            Console.WriteLine($"{sw.ElapsedMilliseconds}ms {GC.GetTotalMemory(false) * 0.001 * 0.001}");
            GC.Collect();

            sw.Restart();
            for (int i = 0; i < 100000; i++)
            {
                radixCustom.Add(RandomFloat(0f, 10000f));
            }
            sw.Stop();
            Console.WriteLine($"{sw.ElapsedMilliseconds}ms {GC.GetTotalMemory(false) * 0.001 * 0.001}");
            GC.Collect();

            sw.Restart();
            for (int i = 0; i < 100000; i++)
            {
                radixCustom.Add(RandomFloat(0f, 10000f));
            }
            sw.Stop();
            Console.WriteLine($"{sw.ElapsedMilliseconds}ms {GC.GetTotalMemory(false) * 0.001 * 0.001}");
            GC.Collect();

            sw.Restart();
            for (int i = 0; i < 100000; i++)
            {
                radixCustom.Add(RandomFloat(0f, 10000f));
            }
            sw.Stop();
            Console.WriteLine($"{sw.ElapsedMilliseconds}ms {GC.GetTotalMemory(false) * 0.001 * 0.001}");
            GC.Collect();

            var array = radixCustom.ToArray();

            for (int i = 0; i < array.Length - 1; i++)
            {
                Assert.LessOrEqual(array[i], array[i + 1]);
            }
        }
    }
}