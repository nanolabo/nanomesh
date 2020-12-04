using System;

namespace Nanomesh
{
    public struct VertexData : IEquatable<VertexData>
    {
        public int position;
        public int normal;
        public int uv;

        public override int GetHashCode()
        {
            unchecked
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