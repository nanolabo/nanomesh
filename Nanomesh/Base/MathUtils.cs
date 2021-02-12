using System.Runtime.CompilerServices;

namespace Nanomesh
{
    public static class MathUtils
    {
        public const float εf = 1e-15f;
        public const double εd = 1e-40f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DivideSafe(float numerator, float denominator)
        {
            return (denominator > -εf && denominator < εf) ? 0f : numerator / denominator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DivideSafe(double numerator, double denominator)
        {
            return (denominator > -εd && denominator < εd) ? 0d : numerator / denominator;
        }

        public static void SelectMin<T>(double e1, double e2, double e3, in T v1, in T v2, in T v3, out double e, out T v)
        {
            if (e1 < e2)
            {
                if (e1 < e3)
                {
                    e = e1;
                    v = v1;
                }
                else
                {
                    e = e3;
                    v = v3;
                }
            }
            else
            {
                if (e2 < e3)
                {
                    e = e2;
                    v = v2;
                }
                else
                {
                    e = e3;
                    v = v3;
                }
            }
        }

        public static void SelectMin<T>(double e1, double e2, double e3, double e4, in T v1, in T v2, in T v3, in T v4, out double e, out T v)
        {
            if (e1 < e2)
            {
                if (e1 < e3)
                {
                    if (e1 < e4)
                    {
                        e = e1;
                        v = v1;
                    }
                    else
                    {
                        e = e4;
                        v = v4;
                    }
                }
                else
                {
                    if (e3 < e4)
                    {
                        e = e3;
                        v = v3;
                    }
                    else
                    {
                        e = e4;
                        v = v4;
                    }
                }
            }
            else
            {
                if (e2 < e3)
                {
                    if (e2 < e4)
                    {
                        e = e2;
                        v = v2;
                    }
                    else
                    {
                        e = e4;
                        v = v4;
                    }
                }
                else
                {
                    if (e3 < e4)
                    {
                        e = e3;
                        v = v3;
                    }
                    else
                    {
                        e = e4;
                        v = v4;
                    }
                }
            }
        }
    }
}