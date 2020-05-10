using System.Runtime.CompilerServices;

namespace Nanolabo
{
    public static class MathUtils
    {
        public const float εf = 1e-15f;
        public const double εd = 1e-15f;

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
    }
}
