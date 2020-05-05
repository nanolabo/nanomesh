using System.Collections.Generic;

namespace Nanolabo
{
    public class Vector3FComparer : IEqualityComparer<Vector3F>
    {
        private float tolerance;

        public Vector3FComparer(float tolerance)
        {
            this.tolerance = tolerance;
        }

        public bool Equals(Vector3F x, Vector3F y)
        {
            return (int)(x.x / tolerance) == (int)(y.x / tolerance)
                && (int)(x.y / tolerance) == (int)(y.y / tolerance)
                && (int)(x.z / tolerance) == (int)(y.z / tolerance);
        }

        public int GetHashCode(Vector3F obj)
        {
            return (int)(obj.x / tolerance) ^ ((int)(obj.y / tolerance) << 2) ^ ((int)(obj.z / tolerance) >> 2);
        }
    }
}