using System.Collections;

namespace Nanomesh
{
    public class Vector2FList : IAttributeList
    {
        private Vector2F[] _array;

        public Vector2FList(Vector2F[] array)
        {
            _array = array;
        }

        public Vector2FList(int size)
        {
            _array = new Vector2F[size];
        }

        public object this[int index] { get { return _array[index]; } set { _array[index] = (Vector2F)value; } }

        public int Length => _array.Length;

        public double Weight { get; set; } = 1;

        public IEnumerator GetEnumerator() => _array.GetEnumerator();

        public void Interpolate(int indexA, int indexB, double ratio)
        {
            var result = ratio * _array[indexA] + (1 - ratio) * _array[indexB];
            _array[indexA] = result;
            _array[indexB] = result;
        }

        public IAttributeList Clone()
        {
            return this;
        }

        public IAttributeList CreateNew(int size)
        {
            return new Vector2FList(size);
        }
    }
}