using System.Collections.Generic;

namespace Nanomesh
{
    public class Vector2FComparer : IEqualityComparer<Vector2F>
    {
        private static Vector2FComparer _instance;
        public static Vector2FComparer Default => _instance ?? (_instance = new Vector2FComparer(0.0001f));

        private float _tolerance;

        public Vector2FComparer(float tolerance)
        {
            this._tolerance = tolerance;
        }

        public bool Equals(Vector2F x, Vector2F y)
        {
            return (int)(x.x / _tolerance) == (int)(y.x / _tolerance)
                && (int)(x.y / _tolerance) == (int)(y.y / _tolerance);
        }

        public int GetHashCode(Vector2F obj)
        {
            return (int)(obj.x / _tolerance) ^ ((int)(obj.y / _tolerance) << 2);
        }
    }
}