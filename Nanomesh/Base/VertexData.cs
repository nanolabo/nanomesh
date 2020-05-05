using System;

namespace Nanolabo
{
    public struct VertexData : IEquatable<VertexData>
    {
        public int position;
        public int normal;
        public int uv;

        public override int GetHashCode()
        {
            unsafe
            {
                return position ^ (normal << 2) ^ (uv >> 2);
            }
        }

        public bool Equals(VertexData other)
        {
            return position == other.position
                && normal == other.normal
                && uv == other.uv;
        }
    }
}