using System;

namespace Nanomesh
{
    public readonly struct Vector2 : IEquatable<Vector2>, IInterpolable<Vector2>
    {
        public readonly double x;
        public readonly double y;

        // Access the /x/ or /y/ component using [0] or [1] respectively.
        public double this[int index]
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
        public Vector2(double x, double y) { this.x = x; this.y = y; }

        // Linearly interpolates between two vectors.
        public static Vector2 Lerp(Vector2 a, Vector2 b, double t)
        {
            t = MathF.Clamp(t, 0, 1);
            return new Vector2(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t
            );
        }

        // Linearly interpolates between two vectors without clamping the interpolant
        public static Vector2 LerpUnclamped(Vector2 a, Vector2 b, double t)
        {
            return new Vector2(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t
            );
        }

        // Moves a point /current/ towards /target/.
        public static Vector2 MoveTowards(Vector2 current, Vector2 target, double maxDistanceDelta)
        {
            // avoid vector ops because current scripting backends are terrible at inlining
            double toVector_x = target.x - current.x;
            double toVector_y = target.y - current.y;

            double sqDist = toVector_x * toVector_x + toVector_y * toVector_y;

            if (sqDist == 0 || (maxDistanceDelta >= 0 && sqDist <= maxDistanceDelta * maxDistanceDelta))
            {
                return target;
            }

            double dist = Math.Sqrt(sqDist);

            return new Vector2(current.x + toVector_x / dist * maxDistanceDelta,
                current.y + toVector_y / dist * maxDistanceDelta);
        }

        // Multiplies two vectors component-wise.
        public static Vector2 Scale(Vector2 a, Vector2 b) => new Vector2(a.x * b.x, a.y * b.y);

        public static Vector2 Normalize(in Vector2 value)
        {
            double mag = Magnitude(in value);
            if (mag > K_EPSILON)
            {
                return value / mag;
            }
            else
            {
                return Zero;
            }
        }

        public Vector2 Normalize() => Normalize(in this);

        public static double SqrMagnitude(in Vector2 a) => a.x * a.x + a.y * a.y;

        /// <summary>
        /// Returns the squared length of this vector (RO).
        /// </summary>
        public double SqrMagnitude() => SqrMagnitude(in this);

        public static double Magnitude(in Vector2 vector) => Math.Sqrt(SqrMagnitude(in vector));

        public double Magnitude() => Magnitude(this);

        // used to allow Vector2s to be used as keys in hash tables
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2);
        }

        // also required for being able to use Vector2s as keys in hash tables
        public override bool Equals(object other)
        {
            if (!(other is Vector2))
            {
                return false;
            }

            return Equals((Vector2)other);
        }


        public bool Equals(Vector2 other)
        {
            return x == other.x && y == other.y;
        }

        public static Vector2 Reflect(Vector2 inDirection, Vector2 inNormal)
        {
            double factor = -2F * Dot(inNormal, inDirection);
            return new Vector2(factor * inNormal.x + inDirection.x, factor * inNormal.y + inDirection.y);
        }


        public static Vector2 Perpendicular(Vector2 inDirection)
        {
            return new Vector2(-inDirection.y, inDirection.x);
        }

        /// <summary>
        /// Returns the dot Product of two vectors.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static double Dot(Vector2 lhs, Vector2 rhs) { return lhs.x * rhs.x + lhs.y * rhs.y; }

        /// <summary>
        /// Returns the angle in radians between /from/ and /to/.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static double AngleRadians(Vector2 from, Vector2 to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            double denominator = Math.Sqrt(from.SqrMagnitude() * to.SqrMagnitude());
            if (denominator < K_EPSILON_NORMAL_SQRT)
            {
                return 0F;
            }

            double dot = MathF.Clamp(Dot(from, to) / denominator, -1F, 1F);
            return Math.Acos(dot);
        }

        public static double AngleDegrees(Vector2 from, Vector2 to)
        {
            return AngleRadians(from, to) / MathF.PI * 180f;
        }

        /// <summary>
        /// Returns the signed angle in degrees between /from/ and /to/. Always returns the smallest possible angle
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static double SignedAngle(Vector2 from, Vector2 to)
        {
            double unsigned_angle = AngleDegrees(from, to);
            double sign = Math.Sign(from.x * to.y - from.y * to.x);
            return unsigned_angle * sign;
        }

        /// <summary>
        /// Returns the distance between /a/ and /b/.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Distance(Vector2 a, Vector2 b)
        {
            double diff_x = a.x - b.x;
            double diff_y = a.y - b.y;
            return Math.Sqrt(diff_x * diff_x + diff_y * diff_y);
        }

        /// <summary>
        /// Returns a copy of /vector/ with its magnitude clamped to /maxLength/.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static Vector2 ClampMagnitude(Vector2 vector, double maxLength)
        {
            double sqrMagnitude = vector.SqrMagnitude();
            if (sqrMagnitude > maxLength * maxLength)
            {
                double mag = Math.Sqrt(sqrMagnitude);

                //these intermediate variables force the intermediate result to be
                //of double precision. without this, the intermediate result can be of higher
                //precision, which changes behavior.
                double normalized_x = vector.x / mag;
                double normalized_y = vector.y / mag;
                return new Vector2(normalized_x * maxLength,
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
        public static Vector2 Min(Vector2 lhs, Vector2 rhs) { return new Vector2(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y)); }

        /// <summary>
        /// Returns a vector that is made from the largest components of two vectors.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static Vector2 Max(Vector2 lhs, Vector2 rhs) { return new Vector2(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y)); }

        public Vector2 Interpolate(Vector2 other, double ratio) => this * ratio + other * (1 - ratio);

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector2 operator +(Vector2 a, Vector2 b) { return new Vector2(a.x + b.x, a.y + b.y); }

        /// <summary>
        /// Subtracts one vector from another.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector2 operator -(Vector2 a, Vector2 b) { return new Vector2(a.x - b.x, a.y - b.y); }

        /// <summary>
        /// Multiplies one vector by another.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector2 operator *(Vector2 a, Vector2 b) { return new Vector2(a.x * b.x, a.y * b.y); }

        /// <summary>
        /// Divides one vector over another.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector2 operator /(Vector2 a, Vector2 b) { return new Vector2(a.x / b.x, a.y / b.y); }

        /// <summary>
        /// Negates a vector.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Vector2 operator -(Vector2 a) { return new Vector2(-a.x, -a.y); }

        /// <summary>
        /// Multiplies a vector by a number.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Vector2 operator *(Vector2 a, double d) { return new Vector2(a.x * d, a.y * d); }

        /// <summary>
        /// Multiplies a vector by a number.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Vector2 operator *(double d, Vector2 a) { return new Vector2(a.x * d, a.y * d); }

        /// <summary>
        /// Divides a vector by a number.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Vector2 operator /(Vector2 a, double d) { return new Vector2(a.x / d, a.y / d); }

        /// <summary>
        /// Returns true if the vectors are equal.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(Vector2 lhs, Vector2 rhs)
        {
            // Returns false in the presence of NaN values.
            double diff_x = lhs.x - rhs.x;
            double diff_y = lhs.y - rhs.y;
            return (diff_x * diff_x + diff_y * diff_y) < K_EPSILON * K_EPSILON;
        }

        /// <summary>
        /// Returns true if vectors are different.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(Vector2 lhs, Vector2 rhs)
        {
            // Returns true in the presence of NaN values.
            return !(lhs == rhs);
        }

        /// <summary>
        /// Converts a [[Vector3]] to a Vector2.
        /// </summary>
        /// <param name="v"></param>
        public static implicit operator Vector2(Vector3F v)
        {
            return new Vector2(v.x, v.y);
        }

        /// <summary>
        /// Converts a Vector2 to a [[Vector3]].
        /// </summary>
        /// <param name="v"></param>
        public static implicit operator Vector3(Vector2 v)
        {
            return new Vector3(v.x, v.y, 0);
        }

        public static implicit operator Vector2F(Vector2 vec)
        {
            return new Vector2F((float)vec.x, (float)vec.y);
        }

        public static explicit operator Vector2(Vector2F vec)
        {
            return new Vector2(vec.x, vec.y);
        }

        public static readonly Vector2 zeroVector = new Vector2(0F, 0F);
        public static readonly Vector2 oneVector = new Vector2(1F, 1F);
        public static readonly Vector2 upVector = new Vector2(0F, 1F);
        public static readonly Vector2 downVector = new Vector2(0F, -1F);
        public static readonly Vector2 leftVector = new Vector2(-1F, 0F);
        public static readonly Vector2 rightVector = new Vector2(1F, 0F);
        public static readonly Vector2 positiveInfinityVector = new Vector2(double.PositiveInfinity, double.PositiveInfinity);
        public static readonly Vector2 negativeInfinityVector = new Vector2(double.NegativeInfinity, double.NegativeInfinity);

        public static Vector2 Zero => zeroVector;

        public static Vector2 One => oneVector;

        public static Vector2 Up => upVector;

        public static Vector2 Down => downVector;

        public static Vector2 Left => leftVector;

        public static Vector2 Right => rightVector;

        public static Vector2 PositiveInfinity => positiveInfinityVector;

        public static Vector2 NegativeInfinity => negativeInfinityVector;

        public const double K_EPSILON = 0.00001F;

        public const double K_EPSILON_NORMAL_SQRT = 1e-15f;
    }
}
