using System.Collections;

namespace Nanomesh
{
    public class BoneWeightList : IAttributeList
    {
        private BoneWeight[] _array;

        public BoneWeightList(BoneWeight[] array)
        {
            _array = array;
        }

        public BoneWeightList(int size)
        {
            _array = new BoneWeight[size];
        }

        public object this[int index] { get { return _array[index]; } set { _array[index] = (BoneWeight)value; } }

        public int Length => _array.Length;

        public double Weight { get; set; } = 1;

        public IEnumerator GetEnumerator() => _array.GetEnumerator();

        public void Interpolate(int indexA, int indexB, double ratio)
        {
            var itemA = _array[indexA];
            var itemB = _array[indexB];

            var result = new BoneWeight(
                ratio < 0.5f ? itemA.index0 : itemB.index0,
                ratio < 0.5f ? itemA.index1 : itemB.index1,
                ratio < 0.5f ? itemA.index2 : itemB.index2,
                ratio < 0.5f ? itemA.index3 : itemB.index3,
                (float)(ratio * itemA.weight0 + (1 - ratio) * itemB.weight0),
                (float)(ratio * itemA.weight1 + (1 - ratio) * itemB.weight1),
                (float)(ratio * itemA.weight2 + (1 - ratio) * itemB.weight2),
                (float)(ratio * itemA.weight3 + (1 - ratio) * itemB.weight3));

            _array[indexA] = result;
            _array[indexB] = result;
        }

        public IAttributeList Clone()
        {
            return this;
        }

        public IAttributeList CreateNew(int size)
        {
            return new BoneWeightList(size);
        }
    }
}