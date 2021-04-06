using BenchmarkDotNet.Attributes;

namespace Nanomesh.Benchmarks
{
    [MemoryDiagnoser]
    public class CastingBenchmark
    {
        private unsafe struct TestStruct<T0, T1>
            where T0 : unmanaged
            where T1 : unmanaged
        {
            public static int[] Positions;

            static TestStruct()
            {
                Positions = new[] { sizeof(T0), sizeof(T0) + sizeof(T1) };
            }

            public readonly T0 attr0;
            public readonly T1 attr1;

            public object GetObject(int index)
            {
                switch (index)
                {
                    case 0:
                        return attr0;
                    case 1:
                        return attr1;
                    default:
                        throw new System.Exception();
                }
            }

            public K GetWithPosition<K>(int index) where K : unmanaged
            {
                fixed (void* v = &this) {
                    byte* b = (byte*)v;
                    b += Positions[index];
                    return ((K*)b)[0];
                };
            }

            public K GetWithSwitch<K>(int index) where K : unmanaged
            {
                switch (index)
                {
                    case 0:
                        fixed (T0* b = &attr0) { return ((K*)b)[0]; };
                    case 1:
                        fixed (T1* b = &attr1) { return ((K*)b)[0]; };
                    default:
                        throw new System.Exception();
                }
            }

            public TestStruct(T0 value0, T1 value1)
            {
                attr0 = value0;
                attr1 = value1;
            }
        }


        [Benchmark]
        public long NoCasting()
        {
            long total = 0;
            for (int i = 0; i < 1_000_000; i++)
            {
                var x = new TestStruct<int, int>(i, i);
                total += x.attr1;
            }

            return total;
        }

        [Benchmark]
        public long CastingIs()
        {
            long total = 0;
            for (int i = 0; i < 1_000_000; i++)
            {
                var x = new TestStruct<int, int>(i, i);
                if (x.GetObject(1) is int value)
                    total += value;
            }

            return total;
        }

        [Benchmark]
        public long CastingNoCheck()
        {
            long total = 0;
            for (int i = 0; i < 1_000_000; i++)
            {
                var x = new TestStruct<int, int>(i, i);
                total += (int)x.GetObject(1);
            }

            return total;
        }

        [Benchmark]
        public long CastingReinterpretPointerSwitch()
        {
            long total = 0;
            for (int i = 0; i < 1_000_000; i++)
            {
                var x = new TestStruct<int, int>(i, i);
                total += x.GetWithSwitch<int>(1);
            }

            return total;
        }

        [Benchmark]
        public long CastingReinterpretPointerPosition()
        {
            long total = 0;
            for (int i = 0; i < 1_000_000; i++)
            {
                var x = new TestStruct<int, int>(i, i);
                total += x.GetWithPosition<int>(1);
            }

            return total;
        }
    }
}