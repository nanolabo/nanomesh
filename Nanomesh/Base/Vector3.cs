﻿using System;

namespace Nanomesh
{
    public struct Vector3
    {
        public double x;
        public double y;
        public double z;

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

        public static Vector3 operator *(Vector3 a, double d) { return new Vector3(a.x * d, a.y * d, a.z * d); }

        public static Vector3 operator *(double d, Vector3 a) { return new Vector3(a.x * d, a.y * d, a.z * d); }

        public static Vector3 operator /(Vector3 a, double d) { return new Vector3(MathUtils.DivideSafe(a.x, d), MathUtils.DivideSafe(a.y, d), MathUtils.DivideSafe(a.z, d)); }

        public static bool operator ==(Vector3 lhs, Vector3 rhs)
        {
            double diff_x = lhs.x - rhs.x;
            double diff_y = lhs.y - rhs.y;
            double diff_z = lhs.z - rhs.z;
            double sqrmag = diff_x * diff_x + diff_y * diff_y + diff_z * diff_z;
            return sqrmag < MathUtils.εd;
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

        public static implicit operator Vector3F(Vector3 vec) => new Vector3F((float)vec.x, (float)vec.y, (float)vec.z);
        public static explicit operator Vector3(Vector3F vec) => new Vector3(vec.x, vec.y, vec.z);

        public static double Dot(Vector3 lhs, Vector3 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }

        public static Vector3 Normalize(Vector3 value)
        {
            double mag = Magnitude(value);
            return value / mag;
        }

        public void Normalize()
        {
            double mag = Magnitude(this);
            this /= mag;
        }

        public Vector3 Normalized => Vector3.Normalize(this);

        public static double Distance(Vector3 a, Vector3 b)
        {
            double diff_x = a.x - b.x;
            double diff_y = a.y - b.y;
            double diff_z = a.z - b.z;
            return Math.Sqrt(diff_x * diff_x + diff_y * diff_y + diff_z * diff_z);
        }

        public static double Magnitude(Vector3 vector)
        {
            return Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }

        public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point)
        {
            Vector3 linePointToPoint = point - linePoint;
            return linePoint + lineVec * Dot(linePointToPoint, lineVec);
        }

        public static double DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            return Magnitude(ProjectPointOnLine(lineStart, lineEnd - lineStart, point) - point);
        }

        public double LengthSquared => x * x + y * y + z * z;

        public double Length => Math.Sqrt(x * x + y * y + z * z);

        public static Vector3 Min(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y), Math.Min(lhs.z, rhs.z));
        }

        public static Vector3 Max(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z));
        }

        static readonly Vector3 zeroVector = new Vector3(0f, 0f, 0f);
        static readonly Vector3 oneVector = new Vector3(1f, 1f, 1f);
        static readonly Vector3 positiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        static readonly Vector3 negativeInfinityVector = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        public static Vector3 Zero => zeroVector;

        public static Vector3 One => oneVector;

        public static Vector3 PositiveInfinity => positiveInfinityVector;

        public static Vector3 NegativeInfinity => negativeInfinityVector;

        public static double AngleRadians(Vector3 from, Vector3 to)
        {
            double denominator = Math.Sqrt(from.LengthSquared * to.LengthSquared);
            if (denominator < 1e-15F)
                return 0F;

            double dot = MathF.Clamp(Dot(from, to) / denominator, -1.0, 1.0);
            return Math.Acos(dot);
        }

        public static double AngleDegrees(Vector3 from, Vector3 to)
        {
            return AngleRadians(from, to) / Math.PI * 180d;
        }

        public override string ToString()
        {
            return $"{x}, {y}, {z}";
        }
    }
}