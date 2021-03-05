using System;

namespace Nanomesh
{
    public static partial class MathF
    {
        // Returns the sine of angle /f/ in radians.
        public static float Sin(float f) { return (float)Math.Sin(f); }

        // Returns the cosine of angle /f/ in radians.
        public static float Cos(float f) { return (float)Math.Cos(f); }

        // Returns the tangent of angle /f/ in radians.
        public static float Tan(float f) { return (float)Math.Tan(f); }

        // Returns the arc-sine of /f/ - the angle in radians whose sine is /f/.
        public static float Asin(float f) { return (float)Math.Asin(f); }

        // Returns the arc-cosine of /f/ - the angle in radians whose cosine is /f/.
        public static float Acos(float f) { return (float)Math.Acos(f); }

        // Returns the arc-tangent of /f/ - the angle in radians whose tangent is /f/.
        public static float Atan(float f) { return (float)Math.Atan(f); }

        // Returns the angle in radians whose ::ref::Tan is @@y/x@@.
        public static float Atan2(float y, float x) { return (float)Math.Atan2(y, x); }

        // Returns square root of /f/.
        public static float Sqrt(float f) { return (float)Math.Sqrt(f); }

        // Returns the absolute value of /f/.
        public static float Abs(float f) { return (float)Math.Abs(f); }

        // Returns the absolute value of /value/.
        public static int Abs(int value) { return Math.Abs(value); }

        /// *listonly*
        public static float Min(float a, float b) { return a < b ? a : b; }
        // Returns the smallest of two or more values.
        public static float Min(params float[] values)
        {
            int len = values.Length;
            if (len == 0)
            {
                return 0;
            }

            float m = values[0];
            for (int i = 1; i < len; i++)
            {
                if (values[i] < m)
                {
                    m = values[i];
                }
            }
            return m;
        }

        /// *listonly*
        public static int Min(int a, int b) { return a < b ? a : b; }
        // Returns the smallest of two or more values.
        public static int Min(params int[] values)
        {
            int len = values.Length;
            if (len == 0)
            {
                return 0;
            }

            int m = values[0];
            for (int i = 1; i < len; i++)
            {
                if (values[i] < m)
                {
                    m = values[i];
                }
            }
            return m;
        }

        /// *listonly*
        public static float Max(float a, float b) { return a > b ? a : b; }
        // Returns largest of two or more values.
        public static float Max(params float[] values)
        {
            int len = values.Length;
            if (len == 0)
            {
                return 0;
            }

            float m = values[0];
            for (int i = 1; i < len; i++)
            {
                if (values[i] > m)
                {
                    m = values[i];
                }
            }
            return m;
        }

        /// *listonly*
        public static int Max(int a, int b) { return a > b ? a : b; }
        // Returns the largest of two or more values.
        public static int Max(params int[] values)
        {
            int len = values.Length;
            if (len == 0)
            {
                return 0;
            }

            int m = values[0];
            for (int i = 1; i < len; i++)
            {
                if (values[i] > m)
                {
                    m = values[i];
                }
            }
            return m;
        }

        // Returns /f/ raised to power /p/.
        public static float Pow(float f, float p) { return (float)Math.Pow(f, p); }

        // Returns e raised to the specified power.
        public static float Exp(float power) { return (float)Math.Exp(power); }

        // Returns the logarithm of a specified number in a specified base.
        public static float Log(float f, float p) { return (float)Math.Log(f, p); }

        // Returns the natural (base e) logarithm of a specified number.
        public static float Log(float f) { return (float)Math.Log(f); }

        // Returns the base 10 logarithm of a specified number.
        public static float Log10(float f) { return (float)Math.Log10(f); }

        // Returns the smallest integer greater to or equal to /f/.
        public static float Ceil(float f) { return (float)Math.Ceiling(f); }

        // Returns the largest integer smaller to or equal to /f/.
        public static float Floor(float f) { return (float)Math.Floor(f); }

        // Returns /f/ rounded to the nearest integer.
        public static float Round(float f) { return (float)Math.Round(f); }

        // Returns the smallest integer greater to or equal to /f/.
        public static int CeilToInt(float f) { return (int)Math.Ceiling(f); }

        // Returns the largest integer smaller to or equal to /f/.
        public static int FloorToInt(float f) { return (int)Math.Floor(f); }

        // Returns /f/ rounded to the nearest integer.
        public static int RoundToInt(float f) { return (int)Math.Round(f); }

        // Returns the sign of /f/.
        public static float Sign(float f) { return f >= 0F ? 1F : -1F; }

        // The infamous ''3.14159265358979...'' value (RO).
        public const float PI = (float)Math.PI;

        // A representation of positive infinity (RO).
        public const float Infinity = float.PositiveInfinity;

        // A representation of negative infinity (RO).
        public const float NegativeInfinity = float.NegativeInfinity;

        // Degrees-to-radians conversion constant (RO).
        public const float Deg2Rad = PI * 2F / 360F;

        // Radians-to-degrees conversion constant (RO).
        public const float Rad2Deg = 1F / Deg2Rad;

        // Clamps a value between a minimum float and maximum float value.
        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            return value;
        }

        // Clamps a value between a minimum float and maximum float value.
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            return value;
        }

        // Clamps value between min and max and returns value.
        // Set the position of the transform to be that of the time
        // but never less than 1 or more than 3
        //
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            return value;
        }

        // Clamps value between 0 and 1 and returns value
        public static float Clamp01(float value)
        {
            if (value < 0F)
            {
                return 0F;
            }
            else if (value > 1F)
            {
                return 1F;
            }
            else
            {
                return value;
            }
        }

        // Interpolates between /a/ and /b/ by /t/. /t/ is clamped between 0 and 1.
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp01(t);
        }

        // Interpolates between /a/ and /b/ by /t/ without clamping the interpolant.
        public static float LerpUnclamped(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        // Same as ::ref::Lerp but makes sure the values interpolate correctly when they wrap around 360 degrees.
        public static float LerpAngle(float a, float b, float t)
        {
            float delta = Repeat((b - a), 360);
            if (delta > 180)
            {
                delta -= 360;
            }

            return a + delta * Clamp01(t);
        }

        // Moves a value /current/ towards /target/.
        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (MathF.Abs(target - current) <= maxDelta)
            {
                return target;
            }

            return current + MathF.Sign(target - current) * maxDelta;
        }

        // Same as ::ref::MoveTowards but makes sure the values interpolate correctly when they wrap around 360 degrees.
        public static float MoveTowardsAngle(float current, float target, float maxDelta)
        {
            float deltaAngle = DeltaAngle(current, target);
            if (-maxDelta < deltaAngle && deltaAngle < maxDelta)
            {
                return target;
            }

            target = current + deltaAngle;
            return MoveTowards(current, target, maxDelta);
        }

        // Interpolates between /min/ and /max/ with smoothing at the limits.
        public static float SmoothStep(float from, float to, float t)
        {
            t = MathF.Clamp01(t);
            t = -2.0F * t * t * t + 3.0F * t * t;
            return to * t + from * (1F - t);
        }

        //*undocumented
        public static float Gamma(float value, float absmax, float gamma)
        {
            bool negative = value < 0F;
            float absval = Abs(value);
            if (absval > absmax)
            {
                return negative ? -absval : absval;
            }

            float result = Pow(absval / absmax, gamma) * absmax;
            return negative ? -result : result;
        }

        // Loops the value t, so that it is never larger than length and never smaller than 0.
        public static float Repeat(float t, float length)
        {
            return Clamp(t - MathF.Floor(t / length) * length, 0.0f, length);
        }

        // PingPongs the value t, so that it is never larger than length and never smaller than 0.
        public static float PingPong(float t, float length)
        {
            t = Repeat(t, length * 2F);
            return length - MathF.Abs(t - length);
        }

        // Calculates the ::ref::Lerp parameter between of two values.
        public static float InverseLerp(float a, float b, float value)
        {
            if (a != b)
            {
                return Clamp01((value - a) / (b - a));
            }
            else
            {
                return 0.0f;
            }
        }

        // Calculates the shortest difference between two given angles.
        public static float DeltaAngle(float current, float target)
        {
            float delta = MathF.Repeat((target - current), 360.0F);
            if (delta > 180.0F)
            {
                delta -= 360.0F;
            }

            return delta;
        }

        internal static long RandomToLong(System.Random r)
        {
            byte[] buffer = new byte[8];
            r.NextBytes(buffer);
            return (long)(System.BitConverter.ToUInt64(buffer, 0) & long.MaxValue);
        }
    }
}
