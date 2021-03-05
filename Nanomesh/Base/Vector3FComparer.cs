using System.Collections.Generic;

namespace Nanomesh
{
    public class Vector3FComparer : IEqualityComparer<Vector3F>
    {
        private static Vector3FComparer _instance;
        public static Vector3FComparer Default => _instance ?? (_instance = new Vector3FComparer(0.001f));


        private readonly float _tolerance;

        public Vector3FComparer(float tolerance)
        {
            this._tolerance = tolerance;
        }

        public bool Equals(Vector3F x, Vector3F y)
        {
            return (int)(x.x / _tolerance) == (int)(y.x / _tolerance)
                && (int)(x.y / _tolerance) == (int)(y.y / _tolerance)
                && (int)(x.z / _tolerance) == (int)(y.z / _tolerance);
        }

        public int GetHashCode(Vector3F obj)
        {
            return (int)(obj.x / _tolerance) ^ ((int)(obj.y / _tolerance) << 2) ^ ((int)(obj.z / _tolerance) >> 2);
        }
    }
}