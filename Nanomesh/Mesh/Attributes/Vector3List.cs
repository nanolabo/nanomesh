using System.Collections;

namespace Nanomesh
{
    public class Vector3List : IAttributeList
    {
        private Vector3[] _array;

        public Vector3List(Vector3[] array)
        {
            _array = array;
        }

        public Vector3List(int size)
        {
            _array = new Vector3[size];
        }

        public object this[int index] { get { return _array[index]; } set { _array[index] = (Vector3)value; } }

        public int Length => _array.Length;

        public double Weight { get; set; } = 1;

        public IAttributeList Clone()
        {
            return this;
        }

        public IAttributeList CreateNew(int size)
        {
            return new Vector3List(size);
        }

        public IEnumerator GetEnumerator() => _array.GetEnumerator();

        public void Interpolate(int indexA, int indexB, double ratio)
        {
            var result = ratio * _array[indexA] + (1 - ratio) * _array[indexB];
            _array[indexA] = result;
            _array[indexB] = result;
        }
    }
}