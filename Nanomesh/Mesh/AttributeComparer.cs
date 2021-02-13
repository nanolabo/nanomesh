using System.Collections.Generic;

namespace Nanomesh
{
    public class AttributeComparer : IEqualityComparer<Attribute>
    {
        private Vector3FComparer _vec3FComp = new Vector3FComparer(0.001f);
        private Vector2FComparer _vec2FComp = new Vector2FComparer(0.001f);

        public AttributeComparer(float tolerance)
        {
            _vec3FComp = new Vector3FComparer(tolerance);
            _vec2FComp = new Vector2FComparer(tolerance);
        }

        public bool Equals(Attribute x, Attribute y)
        {
            return x.normal.Equals(y.normal) && x.uv.Equals(y.uv);
        }

        public int GetHashCode(Attribute obj)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + obj.normal.GetHashCode();
                hash = hash * 23 + obj.uv.GetHashCode();
                return hash;
            }
        }
    }
}