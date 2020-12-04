using System.Collections.Generic;

namespace Nanomesh
{
    public class Vector3Comparer : IEqualityComparer<Vector3>
    {
        private double tolerance;

        public Vector3Comparer(double tolerance)
        {
            this.tolerance = tolerance;
        }

        public bool Equals(Vector3 x, Vector3 y)
        {
            return (int)(x.x / tolerance) == (int)(y.x / tolerance)
                && (int)(x.y / tolerance) == (int)(y.y / tolerance)
                && (int)(x.z / tolerance) == (int)(y.z / tolerance);
        }

        public int GetHashCode(Vector3 obj)
        {
            return (int)(obj.x / tolerance) ^ ((int)(obj.y / tolerance) << 2) ^ ((int)(obj.z / tolerance) >> 2);
        }
    }
}