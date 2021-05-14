using System;
using System.Runtime.InteropServices;

namespace Nanomesh
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct Color32 : IEquatable<Color32>, IInterpolable<Color32>
    {
        [FieldOffset(0)]
        internal readonly int rgba;

        [FieldOffset(0)]
        public readonly byte r;

        [FieldOffset(1)]
        public readonly byte g;

        [FieldOffset(2)]
        public readonly byte b;

        [FieldOffset(3)]
        public readonly byte a;

        public Color32(byte r, byte g, byte b, byte a)
        {
            rgba = 0;
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Color32(float r, float g, float b, float a)
        {
            rgba = 0;
            this.r = (byte)MathF.Round(r);
            this.g = (byte)MathF.Round(g);
            this.b = (byte)MathF.Round(b);
            this.a = (byte)MathF.Round(a);
        }

        public Color32(double r, double g, double b, double a)
        {
            rgba = 0;
            this.r = (byte)Math.Round(r);
            this.g = (byte)Math.Round(g);
            this.b = (byte)Math.Round(b);
            this.a = (byte)Math.Round(a);
        }

        public bool Equals(Color32 other)
        {
            return other.rgba == rgba;
        }

        public Color32 Interpolate(Color32 other, double ratio)
        {
            return ratio * this + (1 - ratio) * other;
        }

        /// <summary>
        /// Adds two colors.
        /// </summary>
        /// <returns></returns>
        public static Color32 operator +(Color32 a, Color32 b) { return new Color32(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a); }

        /// <summary>
        /// Subtracts one color from another.
        /// </summary>
        /// <returns></returns>
        public static Color32 operator -(Color32 a, Color32 b) { return new Color32(1f * a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a); }

        /// <summary>
        /// Multiplies one color by another.
        /// </summary>
        /// <returns></returns>
        public static Color32 operator *(Color32 a, Color32 b) { return new Color32(1f * a.r * b.r, 1f * a.g * b.g, 1f * a.b * b.b, 1f * a.a * b.a); }

        /// <summary>
        /// Divides one color over another.
        /// </summary>
        /// <returns></returns>
        public static Color32 operator /(Color32 a, Color32 b) { return new Color32(1f * a.r / b.r, 1f * a.g / b.g, 1f * a.b / b.b, 1f * a.a / b.a); }


        /// <summary>
        /// Multiplies a color by a number.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Color32 operator *(Color32 a, float d) { return new Color32(d * a.r, d * a.g, d * a.b, d * a.a); }

        public static Color32 operator *(Color32 a, double d) { return new Color32(d * a.r, d * a.g, d * a.b, d * a.a); }

        /// <summary>
        /// Multiplies a color by a number.
        /// </summary>
        /// <returns></returns>
        public static Color32 operator *(float d, Color32 a) { return new Color32(d * a.r, d * a.g, d * a.b, d * a.a); }

        public static Color32 operator *(double d, Color32 a) { return new Color32(d * a.r, d * a.g, d * a.b, d * a.a); }

        /// <summary>
        /// Divides a color by a number.
        /// </summary>
        /// <returns></returns>
        public static Color32 operator /(Color32 a, float d) { return new Color32(1f * a.r / d, 1f * a.g / d, 1f * a.b / d, 1f * a.a / d); }
    }
}