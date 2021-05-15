using System;

namespace Nanomesh
{
    public readonly struct Vector2F : IEquatable<Vector2F>, IInterpolable<Vector2F>
    {
        public readonly float x;
        public readonly float y;

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
        }

        // Constructs a new vector with given x, y components.
        public Vector2F(float x, float y) { this.x = x; this.y = y; }

        // Linearly interpolates between two vectors.
        public static Vector2F Lerp(Vector2F a, Vector2F b, float t)
        {
            t = MathF.Clamp(t, 0, 1);
            return new Vector2F(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t
            );
        }

        // Linearly interpolates between two vectors without clamping the interpolant
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
            {
                return target;
            }

            float dist = MathF.Sqrt(sqDist);

            return new Vector2F(current.x + toVector_x / dist * maxDistanceDelta,
                current.y + toVector_y / dist * maxDistanceDelta);
        }

        // Multiplies two vectors component-wise.
        public static Vector2F Scale(Vector2F a, Vector2F b) { return new Vector2F(a.x * b.x, a.y * b.y); }

        public static Vector2F Normalize(in Vector2F value)
        {
            float mag = Magnitude(in value);
            if (mag > K_EPSILON)
            {
                return value / mag;
            }
            else
            {
                return Zero;
            }
        }

        public Vector2F Normalize() => Normalize(in this);

        public static float SqrMagnitude(in Vector2F a) => a.x * a.x + a.y * a.y;

        /// <summary>
        /// Returns the squared length of this vector (RO).
        /// </summary>
        public float SqrMagnitude() => SqrMagnitude(in this);

        public static float Magnitude(in Vector2F vector) => (float)Math.Sqrt(SqrMagnitude(in vector));

        public float Magnitude() => Magnitude(this);

        // used to allow Vector2s to be used as keys in hash tables
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2);
        }

        // also required for being able to use Vector2s as keys in hash tables
        public override bool Equals(object other)
        {
            if (!(other is Vector2F))
            {
                return false;
            }

            return Equals((Vector2F)other);
        }


        public bool Equals(Vector2F other)
        {
            return Vector2FComparer.Default.Equals(this, other);
            //return x == other.x && y == other.y;
        }

        public static Vector2F Reflect(Vector2F inDirection, Vector2F inNormal)
        {
            float factor = -2F * Dot(inNormal, inDirection);
            return new Vector2F(factor * inNormal.x + inDirection.x, factor * inNormal.y + inDirection.y);
        }

        public static Vector2F Perpendicular(Vector2F inDirection)
        {
            return new Vector2F(-inDirection.y, inDirection.x);
        }

        /// <summary>
        /// Returns the dot Product of two vectors.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static float Dot(Vector2F lhs, Vector2F rhs) { return lhs.x * rhs.x + lhs.y * rhs.y; }

        /// <summary>
        /// Returns the angle in radians between /from/ and /to/.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float AngleRadians(Vector2F from, Vector2F to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            float denominator = MathF.Sqrt(from.SqrMagnitude() * to.SqrMagnitude());
            if (denominator < K_EPSILON_NORMAL_SQRT)
            {
                return 0F;
            }

            float dot = MathF.Clamp(Dot(from, to) / denominator, -1F, 1F);
            return MathF.Acos(dot);
        }

        public static float AngleDegrees(Vector2F from, Vector2F to)
        {
            return AngleRadians(from, to) / MathF.PI * 180f;
        }

        /// <summary>
        /// Returns the signed angle in degrees between /from/ and /to/. Always returns the smallest possible angle
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float SignedAngle(Vector2F from, Vector2F to)
        {
            float unsigned_angle = AngleDegrees(from, to);
            float sign = MathF.Sign(from.x * to.y - from.y * to.x);
            return unsigned_angle * sign;
        }

        /// <summary>
        /// Returns the distance between /a/ and /b/.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Distance(Vector2F a, Vector2F b)
        {
            float diff_x = a.x - b.x;
            float diff_y = a.y - b.y;
            return MathF.Sqrt(diff_x * diff_x + diff_y * diff_y);
        }

        /// <summary>
        /// Returns a copy of /vector/ with its magnitude clamped to /maxLength/.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static Vector2F ClampMagnitude(Vector2F vector, float maxLength)
        {
            float sqrMagnitude = vector.SqrMagnitude();
            if (sqrMagnitude > maxLength * maxLength)
            {
                float mag = MathF.Sqrt(sqrMagnitude);

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

        /// <summary>
        /// Returns a vector that is made from the smallest components of two vectors.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static Vector2F Min(Vector2F lhs, Vector2F rhs) { return new Vector2F(MathF.Min(lhs.x, rhs.x), MathF.Min(lhs.y, rhs.y)); }

        /// <summary>
        /// Returns a vector that is made from the largest components of two vectors.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static Vector2F Max(Vector2F lhs, Vector2F rhs) { return new Vector2F(MathF.Max(lhs.x, rhs.x), MathF.Max(lhs.y, rhs.y)); }

        public Vector2F Interpolate(Vector2F other, double ratio) => this * ratio + other * (1 - ratio);

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector2F operator +(Vector2F a, Vector2F b) { return new Vector2F(a.x + b.x, a.y + b.y); }

        /// <summary>
        /// Subtracts one vector from another.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector2F operator -(Vector2F a, Vector2F b) { return new Vector2F(a.x - b.x, a.y - b.y); }

        /// <summary>
        /// Multiplies one vector by another.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector2F operator *(Vector2F a, Vector2F b) { return new Vector2F(a.x * b.x, a.y * b.y); }

        /// <summary>
        /// Divides one vector over another.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector2F operator /(Vector2F a, Vector2F b) { return new Vector2F(a.x / b.x, a.y / b.y); }

        /// <summary>
        /// Negates a vector.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Vector2F operator -(Vector2F a) { return new Vector2F(-a.x, -a.y); }

        /// <summary>
        /// Multiplies a vector by a number.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Vector2F operator *(Vector2F a, float d) { return new Vector2F(a.x * d, a.y * d); }

        public static Vector2 operator *(Vector2F a, double d) { return new Vector2(a.x * d, a.y * d); }

        /// <summary>
        /// Multiplies a vector by a number.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Vector2F operator *(float d, Vector2F a) { return new Vector2F(a.x * d, a.y * d); }

        public static Vector2 operator *(double d, Vector2F a) { return new Vector2(a.x * d, a.y * d); }

        /// <summary>
        /// Divides a vector by a number.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Vector2F operator /(Vector2F a, float d) { return new Vector2F(a.x / d, a.y / d); }

        /// <summary>
        /// Returns true if the vectors are equal.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(Vector2F lhs, Vector2F rhs)
        {
            // Returns false in the presence of NaN values.
            float diff_x = lhs.x - rhs.x;
            float diff_y = lhs.y - rhs.y;
            return (diff_x * diff_x + diff_y * diff_y) < K_EPSILON * K_EPSILON;
        }

        /// <summary>
        /// Returns true if vectors are different.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(Vector2F lhs, Vector2F rhs)
        {
            // Returns true in the presence of NaN values.
            return !(lhs == rhs);
        }

        /// <summary>
        /// Converts a [[Vector3]] to a Vector2.
        /// </summary>
        /// <param name="v"></param>
        public static implicit operator Vector2F(Vector3F v)
        {
            return new Vector2F(v.x, v.y);
        }

        /// <summary>
        /// Converts a Vector2 to a [[Vector3]].
        /// </summary>
        /// <param name="v"></param>
        public static implicit operator Vector3(Vector2F v)
        {
            return new Vector3(v.x, v.y, 0);
        }

        public static readonly Vector2F zeroVector = new Vector2F(0F, 0F);
        public static readonly Vector2F oneVector = new Vector2F(1F, 1F);
        public static readonly Vector2F upVector = new Vector2F(0F, 1F);
        public static readonly Vector2F downVector = new Vector2F(0F, -1F);
        public static readonly Vector2F leftVector = new Vector2F(-1F, 0F);
        public static readonly Vector2F rightVector = new Vector2F(1F, 0F);
        public static readonly Vector2F positiveInfinityVector = new Vector2F(float.PositiveInfinity, float.PositiveInfinity);
        public static readonly Vector2F negativeInfinityVector = new Vector2F(float.NegativeInfinity, float.NegativeInfinity);

        public static Vector2F Zero => zeroVector;

        public static Vector2F One => oneVector;

        public static Vector2F Up => upVector;

        public static Vector2F Down => downVector;

        public static Vector2F Left => leftVector;

        public static Vector2F Right => rightVector;

        public static Vector2F PositiveInfinity => positiveInfinityVector;

        public static Vector2F NegativeInfinity => negativeInfinityVector;

        public const float K_EPSILON = 0.00001F;

        public const float K_EPSILON_NORMAL_SQRT = 1e-15f;
    }
}
