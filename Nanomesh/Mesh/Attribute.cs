using System;

namespace Nanolabo
{
    public struct Attribute : IEquatable<Attribute>
    {
        public Vector3F normal;
        public Vector3F color;
        public Vector2F uv;

        public bool Equals(Attribute other)
        {
            return normal == other.normal
                && color == other.color
                && uv == other.uv;
        }

        public override int GetHashCode()
        {
            unsafe
            {
                return normal.GetHashCode() ^ (color.GetHashCode() << 2) ^ (uv.GetHashCode() >> 2);
            }
        }
    }
}
