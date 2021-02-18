using System;

namespace Nanomesh
{
    public struct VertexData : IEquatable<VertexData>
    {
        public int position;
        public Attribute attribute;

        public override int GetHashCode()
        {
            unchecked {
                int hash = 17;
                hash = hash * 31 + position;
                //hash = hash * 31 + attribute.GetHashCode();
                return hash;
            }
        }

        public bool Equals(VertexData other)
        {
            return position.Equals(other.position);
                //&& attribute.Equals(other.attribute);
        }
    }
}