using System;
using System.Collections.Generic;

namespace Nanomesh
{
    public struct VertexData : IEquatable<VertexData>
    {
        public int position;
        public List<object> attributes; // TODO : This is not optimal regarding memory

        public VertexData(int pos)
        {
            position = pos;
            attributes = new List<object>();
        }

        public override int GetHashCode()
        {
            unchecked {
                int hash = 17;
                hash = hash * 31 + position;
                foreach (var attr in attributes)
                {
                    hash = hash * 31 + attr.GetHashCode();
                }
                return hash;
            }
        }

        public bool Equals(VertexData other)
        {
            if (!position.Equals(other.position))
                return false;

            if (attributes.Count != other.attributes.Count)
                return false;

            for (int i = 0; i < attributes.Count; i++)
            {
                if (!attributes[i].Equals(other.attributes[i]))
                    return false;
            }

            return true;
        }
    }
}