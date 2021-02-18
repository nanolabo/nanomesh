using System;

namespace Nanomesh
{
    public readonly struct Vector3F : IEquatable<Vector3F>, IAttribute<Vector3F>
    {
        public readonly float x;
        public readonly float y;
        public readonly float z;

        public Vector3F(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3F(float x, float y)
        {
            this.x = x;
            this.y = y;
            z = 0F;
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3F index!");
                }
            }
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector3F))
                return false;

            return Equals((Vector3F)other);
        }

        public bool Equals(Vector3F other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public static Vector3F operator +(in Vector3F a, in Vector3F b) { return new Vector3F(a.x + b.x, a.y + b.y, a.z + b.z); }

        public static Vector3F operator -(in Vector3F a, in Vector3F b) { return new Vector3F(a.x - b.x, a.y - b.y, a.z - b.z); }

        public static Vector3F operator -(in Vector3F a) { return new Vector3F(-a.x, -a.y, -a.z); }

        public static Vector3F operator *(in Vector3F a, float d) { return new Vector3F(a.x * d, a.y * d, a.z * d); }

        public static Vector3F operator *(float d, in Vector3F a) { return new Vector3F(a.x * d, a.y * d, a.z * d); }

        public static Vector3 operator *(double d, in Vector3F a) { return new Vector3(a.x * d, a.y * d, a.z * d); }

        public static Vector3F operator /(in Vector3F a, float d) { return new Vector3F(MathUtils.DivideSafe(a.x, d), MathUtils.DivideSafe(a.y, d), MathUtils.DivideSafe(a.z, d)); }

        public static bool operator ==(in Vector3F lhs, in Vector3F rhs)
        {
            float diff_x = lhs.x - rhs.x;
            float diff_y = lhs.y - rhs.y;
            float diff_z = lhs.z - rhs.z;
            float sqrmag = diff_x * diff_x + diff_y * diff_y + diff_z * diff_z;
            return sqrmag < MathUtils.εf;
        }

        public static bool operator !=(in Vector3F lhs, in Vector3F rhs)
        {
            return !(lhs == rhs);
        }
        public static Vector3F Cross(in Vector3F lhs, in Vector3F rhs)
        {
            return new Vector3F(
                lhs.y * rhs.z - lhs.z * rhs.y,
                lhs.z * rhs.x - lhs.x * rhs.z,
                lhs.x * rhs.y - lhs.y * rhs.x);
        }

        public static float Dot(in Vector3F lhs, in Vector3F rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }

        public static Vector3F Normalize(in Vector3F value)
        {
            float mag = Magnitude(value);
            return value / mag;
        }

        public Vector3F Normalized => Vector3F.Normalize(this);

        public static float Distance(in Vector3F a, in Vector3F b)
        {
            float diff_x = a.x - b.x;
            float diff_y = a.y - b.y;
            float diff_z = a.z - b.z;
            return MathF.Sqrt(diff_x * diff_x + diff_y * diff_y + diff_z * diff_z);
        }

        public static float Magnitude(in Vector3F vector)
        {
            return MathF.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }

        public float SqrMagnitude => x * x + y * y + z * z;

        public static Vector3F Min(in Vector3F lhs, in Vector3F rhs)
        {
            return new Vector3F(MathF.Min(lhs.x, rhs.x), MathF.Min(lhs.y, rhs.y), MathF.Min(lhs.z, rhs.z));
        }

        public static Vector3F Max(in Vector3F lhs, in Vector3F rhs)
        {
            return new Vector3F(MathF.Max(lhs.x, rhs.x), MathF.Max(lhs.y, rhs.y), MathF.Max(lhs.z, rhs.z));
        }

        static readonly Vector3F zeroVector = new Vector3F(0f, 0f, 0f);
        static readonly Vector3F oneVector = new Vector3F(1f, 1f, 1f);
        static readonly Vector3F positiveInfinityVector = new Vector3F(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        static readonly Vector3F negativeInfinityVector = new Vector3F(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        public static Vector3F Zero => zeroVector;

        public static Vector3F One => oneVector;

        public static Vector3F PositiveInfinity => positiveInfinityVector;

        public static Vector3F NegativeInfinity => negativeInfinityVector;

        public static float AngleRadians(in Vector3F from, in Vector3F to)
        {
            float denominator = MathF.Sqrt(from.SqrMagnitude * to.SqrMagnitude);
            if (denominator < 1e-15F)
                return 0F;

            float dot = MathF.Clamp(Dot(from, to) / denominator, -1F, 1F);
            return MathF.Acos(dot);
        }

        public static float AngleDegrees(in Vector3F from, in Vector3F to)
        {
            return AngleRadians(from, to) / MathF.PI * 180f;
        }

        public override string ToString()
        {
            return $"{x}, {y}, {z}";
        }

        public Vector3F Interpolate(double ratio, in Vector3F otherAttribute)
        {
            return ratio * this + (1 - ratio) * otherAttribute;
        }
    }
}