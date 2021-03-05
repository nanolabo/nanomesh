using System.Collections.Generic;

namespace Nanomesh
{
    public class Vector3Comparer : IEqualityComparer<Vector3>
    {
        private readonly double _tolerance;

        public Vector3Comparer(double tolerance)
        {
            _tolerance = tolerance;
        }

        public bool Equals(Vector3 x, Vector3 y)
        {
            return (int)(x.x / _tolerance) == (int)(y.x / _tolerance)
                && (int)(x.y / _tolerance) == (int)(y.y / _tolerance)
                && (int)(x.z / _tolerance) == (int)(y.z / _tolerance);
        }

        public int GetHashCode(Vector3 obj)
        {
            return (int)(obj.x / _tolerance) ^ ((int)(obj.y / _tolerance) << 2) ^ ((int)(obj.z / _tolerance) >> 2);
        }
    }
}