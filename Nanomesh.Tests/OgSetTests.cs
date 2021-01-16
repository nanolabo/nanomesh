using Nanomesh.Collections;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nanomesh.Tests
{
    class OgSetTests
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
        public void Test()
        {
            //RadixSortedSet set = new RadixSortedSet();

            //for (int i = 0; i < 100000; i++)
            //{
            //    float f1 = RandomFloat(0f, 100000f);
            //    float f2 = RandomFloat(0f, 100000f);

            //    int i1 = set.Add(f1);
            //    int i2 = set.Add(f2);

            //    Assert.True((i1 > i2 && f1 > f2) || (i1 <= i2 && f1 <= f2));
            //}

            var radixCustom = new RadixSortedSet<float>(x => x);

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
