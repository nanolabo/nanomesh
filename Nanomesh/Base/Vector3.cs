using System;

namespace Nanolabo
{
    public struct Vector3
    {
        public const float ε = 0.00001F;

        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3(float x, float y)
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
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector3))
                return false;

            return Equals((Vector3)other);
        }

        public bool Equals(Vector3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); }

        public static Vector3 operator -(Vector3 a, Vector3 b) { return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); }

        public static Vector3 operator -(Vector3 a) { return new Vector3(-a.x, -a.y, -a.z); }

        public static Vector3 operator *(Vector3 a, float d) { return new Vector3(a.x * d, a.y * d, a.z * d); }

        public static Vector3 operator *(float d, Vector3 a) { return new Vector3(a.x * d, a.y * d, a.z * d); }

        public static Vector3 operator /(Vector3 a, float d) { return new Vector3(a.x / d, a.y / d, a.z / d); }

        public static bool operator ==(Vector3 lhs, Vector3 rhs)
        {
            float diff_x = lhs.x - rhs.x;
            float diff_y = lhs.y - rhs.y;
            float diff_z = lhs.z - rhs.z;
            float sqrmag = diff_x * diff_x + diff_y * diff_y + diff_z * diff_z;
            return sqrmag < ε * ε;
        }

        public static bool operator !=(Vector3 lhs, Vector3 rhs)
        {
            return !(lhs == rhs);
        }
        public static Vector3 Cross(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(
                lhs.y * rhs.z - lhs.z * rhs.y,
                lhs.z * rhs.x - lhs.x * rhs.z,
                lhs.x * rhs.y - lhs.y * rhs.x);
        }

        public static float Dot(Vector3 lhs, Vector3 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }

        public static Vector3 Normalize(Vector3 value)
        {
            float mag = Magnitude(value);
            if (mag > ε)
                return value / mag;
            else
                return Zero;
        }

        public void Normalize()
        {
            float mag = Magnitude(this);
            if (mag > ε)
                this = this / mag;
            else
                this = Zero;
        }

        public Vector3 Normalized => Vector3.Normalize(this);

        public static float Distance(Vector3 a, Vector3 b)
        {
            float diff_x = a.x - b.x;
            float diff_y = a.y - b.y;
            float diff_z = a.z - b.z;
            return MathF.Sqrt(diff_x * diff_x + diff_y * diff_y + diff_z * diff_z);
        }

        public static float Magnitude(Vector3 vector)
        {
            return MathF.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }

        public float SqrMagnitude => x * x + y * y + z * z;

        public static Vector3 Min(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(MathF.Min(lhs.x, rhs.x), MathF.Min(lhs.y, rhs.y), MathF.Min(lhs.z, rhs.z));
        }

        public static Vector3 Max(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(MathF.Max(lhs.x, rhs.x), MathF.Max(lhs.y, rhs.y), MathF.Max(lhs.z, rhs.z));
        }

        static readonly Vector3 zeroVector = new Vector3(0f, 0f, 0f);
        static readonly Vector3 oneVector = new Vector3(1f, 1f, 1f);
        static readonly Vector3 positiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        static readonly Vector3 negativeInfinityVector = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        public static Vector3 Zero => zeroVector;

        public static Vector3 One => oneVector;

        public static Vector3 PositiveInfinity => positiveInfinityVector;

        public static Vector3 NegativeInfinity => negativeInfinityVector;
    }
}