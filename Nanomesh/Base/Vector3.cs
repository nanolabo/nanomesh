using System;

namespace Nanomesh
{
    public readonly struct Vector3
    {
        public readonly double x;
        public readonly double y;
        public readonly double z;

        public Vector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3(double x, double y)
        {
            this.x = x;
            this.y = y;
            z = 0.0;
        }

        public double this[int index]
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
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector3))
            {
                return false;
            }

            return Equals((Vector3)other);
        }

        public bool Equals(Vector3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public static Vector3 operator +(in Vector3 a, in Vector3 b) { return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); }

        public static Vector3 operator -(in Vector3 a, in Vector3 b) { return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); }

        public static Vector3 operator -(in Vector3 a) { return new Vector3(-a.x, -a.y, -a.z); }

        public static Vector3 operator *(in Vector3 a, double d) { return new Vector3(a.x * d, a.y * d, a.z * d); }

        public static Vector3 operator *(double d, in Vector3 a) { return new Vector3(a.x * d, a.y * d, a.z * d); }

        public static Vector3 operator /(in Vector3 a, double d) { return new Vector3(MathUtils.DivideSafe(a.x, d), MathUtils.DivideSafe(a.y, d), MathUtils.DivideSafe(a.z, d)); }

        public static bool operator ==(in Vector3 lhs, in Vector3 rhs)
        {
            double diff_x = lhs.x - rhs.x;
            double diff_y = lhs.y - rhs.y;
            double diff_z = lhs.z - rhs.z;
            double sqrmag = diff_x * diff_x + diff_y * diff_y + diff_z * diff_z;
            return sqrmag < MathUtils.εd;
        }

        public static bool operator !=(in Vector3 lhs, in Vector3 rhs)
        {
            return !(lhs == rhs);
        }
        public static Vector3 Cross(in Vector3 lhs, in Vector3 rhs)
        {
            return new Vector3(
                lhs.y * rhs.z - lhs.z * rhs.y,
                lhs.z * rhs.x - lhs.x * rhs.z,
                lhs.x * rhs.y - lhs.y * rhs.x);
        }

        public static implicit operator Vector3F(Vector3 vec)
        {
            return new Vector3F((float)vec.x, (float)vec.y, (float)vec.z);
        }

        public static explicit operator Vector3(Vector3F vec)
        {
            return new Vector3(vec.x, vec.y, vec.z);
        }

        public static double Dot(in Vector3 lhs, in Vector3 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }

        public static Vector3 Normalize(in Vector3 value)
        {
            double mag = Magnitude(value);
            return value / mag;
        }

        public Vector3 Normalized => Vector3.Normalize(this);

        public static double Distance(in Vector3 a, in Vector3 b)
        {
            double diff_x = a.x - b.x;
            double diff_y = a.y - b.y;
            double diff_z = a.z - b.z;
            return Math.Sqrt(diff_x * diff_x + diff_y * diff_y + diff_z * diff_z);
        }

        public static double Magnitude(in Vector3 vector)
        {
            return Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }

        public static Vector3 ProjectPointOnLine(in Vector3 linePoint, in Vector3 lineVec, in Vector3 point)
        {
            Vector3 linePointToPoint = point - linePoint;
            return linePoint + lineVec * Dot(linePointToPoint, lineVec);
        }

        public static double DistancePointLine(in Vector3 point, in Vector3 lineStart, in Vector3 lineEnd)
        {
            return Magnitude(ProjectPointOnLine(lineStart, (lineEnd - lineStart).Normalized, point) - point);
        }

        public double LengthSquared => x * x + y * y + z * z;

        public double Length => Math.Sqrt(x * x + y * y + z * z);

        public static Vector3 Min(in Vector3 lhs, in Vector3 rhs)
        {
            return new Vector3(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y), Math.Min(lhs.z, rhs.z));
        }

        public static Vector3 Max(in Vector3 lhs, in Vector3 rhs)
        {
            return new Vector3(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z));
        }

        private static readonly Vector3 zeroVector = new Vector3(0f, 0f, 0f);
        private static readonly Vector3 oneVector = new Vector3(1f, 1f, 1f);
        private static readonly Vector3 positiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        private static readonly Vector3 negativeInfinityVector = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        public static Vector3 Zero => zeroVector;

        public static Vector3 One => oneVector;

        public static Vector3 PositiveInfinity => positiveInfinityVector;

        public static Vector3 NegativeInfinity => negativeInfinityVector;

        public static double AngleRadians(in Vector3 from, in Vector3 to)
        {
            double denominator = Math.Sqrt(from.LengthSquared * to.LengthSquared);
            if (denominator < 1e-15F)
            {
                return 0F;
            }

            double dot = MathF.Clamp(Dot(from, to) / denominator, -1.0, 1.0);
            return Math.Acos(dot);
        }

        public static double AngleDegrees(in Vector3 from, in Vector3 to)
        {
            return AngleRadians(from, to) / Math.PI * 180d;
        }

        public override string ToString()
        {
            return $"{x}, {y}, {z}";
        }
    }
}