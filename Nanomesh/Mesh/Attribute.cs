using System;

namespace Nanomesh
{
    public struct Attribute : IEquatable<Attribute>
    {
        // TODO : Separate attributes ? To be spec'ed
        public Vector3F normal;
        public Vector3F color;
        public Vector2F uv;
        public BoneWeight boneWeight;

        public bool Equals(Attribute other)
        {
            return normal == other.normal
                && color == other.color
                && uv == other.uv;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + normal.GetHashCode();
                hash = hash * 31 + color.GetHashCode();
                hash = hash * 31 + uv.GetHashCode();
                hash = hash * 31 + boneWeight.GetHashCode();
                return hash;
            }
        }
    }
}