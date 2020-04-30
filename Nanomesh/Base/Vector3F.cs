﻿using System;

namespace Nanolabo
{
    public struct Vector3F
    {
        public const float ε = 0.00001F;

        public float x;
        public float y;
        public float z;

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

            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
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

        public static Vector3F operator +(Vector3F a, Vector3F b) { return new Vector3F(a.x + b.x, a.y + b.y, a.z + b.z); }

        public static Vector3F operator -(Vector3F a, Vector3F b) { return new Vector3F(a.x - b.x, a.y - b.y, a.z - b.z); }

        public static Vector3F operator -(Vector3F a) { return new Vector3F(-a.x, -a.y, -a.z); }

        public static Vector3F operator *(Vector3F a, float d) { return new Vector3F(a.x * d, a.y * d, a.z * d); }

        public static Vector3F operator *(float d, Vector3F a) { return new Vector3F(a.x * d, a.y * d, a.z * d); }

        public static Vector3F operator /(Vector3F a, float d) { return new Vector3F(a.x / d, a.y / d, a.z / d); }

        public static bool operator ==(Vector3F lhs, Vector3F rhs)
        {
            float diff_x = lhs.x - rhs.x;
            float diff_y = lhs.y - rhs.y;
            float diff_z = lhs.z - rhs.z;
            float sqrmag = diff_x * diff_x + diff_y * diff_y + diff_z * diff_z;
            return sqrmag < ε * ε;
        }

        public static bool operator !=(Vector3F lhs, Vector3F rhs)
        {
            return !(lhs == rhs);
        }
        public static Vector3F Cross(Vector3F lhs, Vector3F rhs)
        {
            return new Vector3F(
                lhs.y * rhs.z - lhs.z * rhs.y,
                lhs.z * rhs.x - lhs.x * rhs.z,
                lhs.x * rhs.y - lhs.y * rhs.x);
        }

        public static float Dot(Vector3F lhs, Vector3F rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }

        public static Vector3F Normalize(Vector3F value)
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

        public Vector3F Normalized => Vector3F.Normalize(this);

        public static float Distance(Vector3F a, Vector3F b)
        {
            float diff_x = a.x - b.x;
            float diff_y = a.y - b.y;
            float diff_z = a.z - b.z;
            return MathF.Sqrt(diff_x * diff_x + diff_y * diff_y + diff_z * diff_z);
        }

        public static float Magnitude(Vector3F vector)
        {
            return MathF.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }

        public float SqrMagnitude => x * x + y * y + z * z;

        public static Vector3F Min(Vector3F lhs, Vector3F rhs)
        {
            return new Vector3F(MathF.Min(lhs.x, rhs.x), MathF.Min(lhs.y, rhs.y), MathF.Min(lhs.z, rhs.z));
        }

        public static Vector3F Max(Vector3F lhs, Vector3F rhs)
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

        public static float Angle(Vector3F from, Vector3F to)
        {
            float denominator = MathF.Sqrt(from.SqrMagnitude * to.SqrMagnitude);
            if (denominator < 1e-15F)
                return 0F;

            float dot = Math.Clamp(Dot(from, to) / denominator, -1F, 1F);
            return MathF.Acos(dot);
        }

        public override string ToString()
        {
            return $"{x}, {y}, {z}";
        }
    }
}