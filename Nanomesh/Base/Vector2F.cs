using System;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Nanolabo
{
    static class MethodImplOptionsEx
    {
        public const short AggressiveInlining = 256;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2F : IEquatable<Vector2F>
    {
        public float x;
        public float y;

        // Access the /x/ or /y/ component using [0] or [1] respectively.
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector2 index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector2 index!");
                }
            }
        }

        // Constructs a new vector with given x, y components.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public Vector2F(float x, float y) { this.x = x; this.y = y; }

        // Linearly interpolates between two vectors.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static Vector2F Lerp(Vector2F a, Vector2F b, float t)
        {
            t = Math.Clamp(t, 0, 1);
            return new Vector2F(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t
            );
        }

        // Linearly interpolates between two vectors without clamping the interpolant
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static Vector2F LerpUnclamped(Vector2F a, Vector2F b, float t)
        {
            return new Vector2F(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t
            );
        }

        // Moves a point /current/ towards /target/.
        public static Vector2F MoveTowards(Vector2F current, Vector2F target, float maxDistanceDelta)
        {
            // avoid vector ops because current scripting backends are terrible at inlining
            float toVector_x = target.x - current.x;
            float toVector_y = target.y - current.y;

            float sqDist = toVector_x * toVector_x + toVector_y * toVector_y;

            if (sqDist == 0 || (maxDistanceDelta >= 0 && sqDist <= maxDistanceDelta * maxDistanceDelta))
                return target;

            float dist = (float)Math.Sqrt(sqDist);

            return new Vector2F(current.x + toVector_x / dist * maxDistanceDelta,
                current.y + toVector_y / dist * maxDistanceDelta);
        }

        // Multiplies two vectors component-wise.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static Vector2F Scale(Vector2F a, Vector2F b) { return new Vector2F(a.x * b.x, a.y * b.y); }

        // Multiplies every component of this vector by the same component of /scale/.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public void Scale(Vector2F scale) { x *= scale.x; y *= scale.y; }

        // Makes this vector have a ::ref::magnitude of 1.
        public void Normalize()
        {
            float mag = magnitude;
            if (mag > kEpsilon)
                this = this / mag;
            else
                this = zero;
        }

        // Returns this vector with a ::ref::magnitude of 1 (RO).
        public Vector2F normalized
        {
            get
            {
                Vector2F v = new Vector2F(x, y);
                v.Normalize();
                return v;
            }
        }

        // used to allow Vector2s to be used as keys in hash tables
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2);
        }

        // also required for being able to use Vector2s as keys in hash tables
        public override bool Equals(object other)
        {
            if (!(other is Vector2F)) return false;

            return Equals((Vector2F)other);
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public bool Equals(Vector2F other)
        {
            return x == other.x && y == other.y;
        }

        public static Vector2F Reflect(Vector2F inDirection, Vector2F inNormal)
        {
            float factor = -2F * Dot(inNormal, inDirection);
            return new Vector2F(factor * inNormal.x + inDirection.x, factor * inNormal.y + inDirection.y);
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static Vector2F Perpendicular(Vector2F inDirection)
        {
            return new Vector2F(-inDirection.y, inDirection.x);
        }

        // Dot Product of two vectors.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static float Dot(Vector2F lhs, Vector2F rhs) { return lhs.x * rhs.x + lhs.y * rhs.y; }

        // Returns the length of this vector (RO).
        public float magnitude { get { return (float)Math.Sqrt(x * x + y * y); } }
        // Returns the squared length of this vector (RO).
        public float sqrMagnitude { get { return x * x + y * y; } }

        // Returns the angle in degrees between /from/ and /to/.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static float Angle(Vector2F from, Vector2F to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            float denominator = (float)Math.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
            if (denominator < kEpsilonNormalSqrt)
                return 0F;

            float dot = Math.Clamp(Dot(from, to) / denominator, -1F, 1F);
            return (float)Math.Acos(dot) / MathF.PI * 180f;
        }

        // Returns the signed angle in degrees between /from/ and /to/. Always returns the smallest possible angle
        public static float SignedAngle(Vector2F from, Vector2F to)
        {
            float unsigned_angle = Angle(from, to);
            float sign = MathF.Sign(from.x * to.y - from.y * to.x);
            return unsigned_angle * sign;
        }

        // Returns the distance between /a/ and /b/.
        public static float Distance(Vector2F a, Vector2F b)
        {
            float diff_x = a.x - b.x;
            float diff_y = a.y - b.y;
            return (float)Math.Sqrt(diff_x * diff_x + diff_y * diff_y);
        }

        // Returns a copy of /vector/ with its magnitude clamped to /maxLength/.
        public static Vector2F ClampMagnitude(Vector2F vector, float maxLength)
        {
            float sqrMagnitude = vector.sqrMagnitude;
            if (sqrMagnitude > maxLength * maxLength)
            {
                float mag = (float)Math.Sqrt(sqrMagnitude);

                //these intermediate variables force the intermediate result to be
                //of float precision. without this, the intermediate result can be of higher
                //precision, which changes behavior.
                float normalized_x = vector.x / mag;
                float normalized_y = vector.y / mag;
                return new Vector2F(normalized_x * maxLength,
                    normalized_y * maxLength);
            }
            return vector;
        }

        // [Obsolete("Use Vector2.sqrMagnitude")]
        public static float SqrMagnitude(Vector2F a) { return a.x * a.x + a.y * a.y; }

        // [Obsolete("Use Vector2.sqrMagnitude")]
        public float SqrMagnitude() { return x * x + y * y; }

        // Returns a vector that is made from the smallest components of two vectors.
        public static Vector2F Min(Vector2F lhs, Vector2F rhs) { return new Vector2F(MathF.Min(lhs.x, rhs.x), MathF.Min(lhs.y, rhs.y)); }

        // Returns a vector that is made from the largest components of two vectors.
        public static Vector2F Max(Vector2F lhs, Vector2F rhs) { return new Vector2F(MathF.Max(lhs.x, rhs.x), MathF.Max(lhs.y, rhs.y)); }

        // Adds two vectors.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static Vector2F operator+(Vector2F a, Vector2F b) { return new Vector2F(a.x + b.x, a.y + b.y); }

        // Subtracts one vector from another.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static Vector2F operator-(Vector2F a, Vector2F b) { return new Vector2F(a.x - b.x, a.y - b.y); }

        // Multiplies one vector by another.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static Vector2F operator*(Vector2F a, Vector2F b) { return new Vector2F(a.x * b.x, a.y * b.y); }

        // Divides one vector over another.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static Vector2F operator/(Vector2F a, Vector2F b) { return new Vector2F(a.x / b.x, a.y / b.y); }

        // Negates a vector.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static Vector2F operator-(Vector2F a) { return new Vector2F(-a.x, -a.y); }

        // Multiplies a vector by a number.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static Vector2F operator*(Vector2F a, float d) { return new Vector2F(a.x * d, a.y * d); }

        // Multiplies a vector by a number.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static Vector2F operator*(float d, Vector2F a) { return new Vector2F(a.x * d, a.y * d); }

        // Divides a vector by a number.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static Vector2F operator/(Vector2F a, float d) { return new Vector2F(a.x / d, a.y / d); }

        // Returns true if the vectors are equal.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static bool operator==(Vector2F lhs, Vector2F rhs)
        {
            // Returns false in the presence of NaN values.
            float diff_x = lhs.x - rhs.x;
            float diff_y = lhs.y - rhs.y;
            return (diff_x * diff_x + diff_y * diff_y) < kEpsilon * kEpsilon;
        }

        // Returns true if vectors are different.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static bool operator!=(Vector2F lhs, Vector2F rhs)
        {
            // Returns true in the presence of NaN values.
            return !(lhs == rhs);
        }

        // Converts a [[Vector3]] to a Vector2.
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static implicit operator Vector2F(Vector3F v)
        {
            return new Vector2F(v.x, v.y);
        }

        // Converts a Vector2 to a [[Vector3]].
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static implicit operator Vector3(Vector2F v)
        {
            return new Vector3(v.x, v.y, 0);
        }

        static readonly Vector2F zeroVector = new Vector2F(0F, 0F);
        static readonly Vector2F oneVector = new Vector2F(1F, 1F);
        static readonly Vector2F upVector = new Vector2F(0F, 1F);
        static readonly Vector2F downVector = new Vector2F(0F, -1F);
        static readonly Vector2F leftVector = new Vector2F(-1F, 0F);
        static readonly Vector2F rightVector = new Vector2F(1F, 0F);
        static readonly Vector2F positiveInfinityVector = new Vector2F(float.PositiveInfinity, float.PositiveInfinity);
        static readonly Vector2F negativeInfinityVector = new Vector2F(float.NegativeInfinity, float.NegativeInfinity);

        // Shorthand for writing @@Vector2(0, 0)@@
        public static Vector2F zero { get { return zeroVector; } }
        // Shorthand for writing @@Vector2(1, 1)@@
        public static Vector2F one { get { return oneVector; }   }
        // Shorthand for writing @@Vector2(0, 1)@@
        public static Vector2F up { get { return upVector; } }
        // Shorthand for writing @@Vector2(0, -1)@@
        public static Vector2F down { get { return downVector; } }
        // Shorthand for writing @@Vector2(-1, 0)@@
        public static Vector2F left { get { return leftVector; } }
        // Shorthand for writing @@Vector2(1, 0)@@
        public static Vector2F right { get { return rightVector; } }
        // Shorthand for writing @@Vector2(float.PositiveInfinity, float.PositiveInfinity)@@
        public static Vector2F positiveInfinity { get { return positiveInfinityVector; } }
        // Shorthand for writing @@Vector2(float.NegativeInfinity, float.NegativeInfinity)@@
        public static Vector2F negativeInfinity { get { return negativeInfinityVector; } }

        // *Undocumented*
        public const float kEpsilon = 0.00001F;
        // *Undocumented*
        public const float kEpsilonNormalSqrt = 1e-15f;
    }
}
