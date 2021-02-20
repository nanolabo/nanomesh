using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nanomesh
{
    public struct Attribute2 : IEquatable<Attribute2>
    {
        // TODO : Separate attributes ? To be spec'ed
        public Vector3F normal;
        public Vector3F color;
        public Vector2F uv;
        public BoneWeight boneWeight;

        public bool Equals(Attribute2 other)
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

    public interface IAttribute
    {
        T Interpolate<T>(double ratio, in T otherAttribute);
    }

    public interface IAttribute<T> : IEquatable<T>
    {
        T Interpolate(double ratio, in T otherAttribute);
    }

    public interface IAttributeList
    {
        bool AreSame(int indexA, int indexB);
        void Interpolate(int indexA, int indexB, double ratio);
        int Length { get; }
        double Weight { get; set; }
        IAttributeList Clone();
        IAttributeList CreateNew(int size);
        IList Array { get; }
    }

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

        public int Length => _array.Length;

        public double Weight { get; set; } = 1;

        public IList Array => _array;

        public bool AreSame(int indexA, int indexB)
        {
            return Vector3FComparer.Default.Equals(_array[indexA], _array[indexB]);
        }

        public IAttributeList Clone()
        {
            return this;
        }

        public IAttributeList CreateNew(int size)
        {
            return new Vector3List(size);
        }

        public void Interpolate(int indexA, int indexB, double ratio)
        {
            var result = ratio * _array[indexA] + (1 - ratio) * _array[indexB];
            _array[indexA] = result;
            _array[indexB] = result;
        }
    }

    public class Vector3FList : IAttributeList
    {
        private Vector3F[] _array;

        public Vector3FList(Vector3F[] array)
        {
            _array = array;
        }

        public Vector3FList(int size)
        {
            _array = new Vector3F[size];
        }

        public int Length => _array.Length;

        public IList Array => _array;

        public double Weight { get; set; } = 1;

        public bool AreSame(int indexA, int indexB)
        {
            return Vector3FComparer.Default.Equals(_array[indexA], _array[indexB]);
        }

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
            return new Vector3FList(size);
        }
    }

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

        public int Length => _array.Length;

        public IList Array => _array;

        public double Weight { get; set; } = 1;

        public bool AreSame(int indexA, int indexB)
        {
            return Vector2FComparer.Default.Equals(_array[indexA], _array[indexB]);
        }

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

        public int Length => _array.Length;

        public IList Array => _array;

        public double Weight { get; set; } = 1;

        public bool AreSame(int indexA, int indexB)
        {
            return _array[indexA].Equals(_array[indexB]);
        }

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

    public readonly struct Attribute<A1, A2> : IEquatable<Attribute<A1, A2>>
        where A1 : IAttribute<A1>
        where A2 : IAttribute<A2>
    {
        public readonly A1 a1;
        public readonly A2 a2;

        public Attribute(A1 a1, A2 a2)
        {
            this.a1 = a1;
            this.a2 = a2;
        }

        public bool Equals(Attribute<A1, A2> other)
        {
            return a1.Equals(other.a1)
                && a2.Equals(other.a2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Attribute<A1, A2> attribute)
                return Equals(attribute);
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + a1.GetHashCode();
                hash = hash * 31 + a2.GetHashCode();
                return hash;
            }
        }
    }

    public abstract class AttributeListBase
    {
        public abstract void Interpolate(int indexA, int indexB, double ratio);
        public abstract int Length { get; }
        public abstract int[] Merge();
        public abstract MergeContextBase CreateMergeContext();
    }

    public class AttributeList<A1, A2> : AttributeListBase
        where A1 : IAttribute<A1>
        where A2 : IAttribute<A2>
    {
        private Attribute<A1, A2>[] _array;

        public override int Length => _array.Length;

        public override MergeContextBase CreateMergeContext() => new MergeContext(this);

        public override void Interpolate(int indexA, int indexB, double ratio)
        {
            _array[indexA] = new Attribute<A1, A2>(
                _array[indexA].a1.Interpolate(ratio, in _array[indexB].a1),
                _array[indexA].a2.Interpolate(ratio, in _array[indexB].a2));
        }

        public override int[] Merge()
        {
            Dictionary<Attribute<A1, A2>, int> mergedAttributes = new Dictionary<Attribute<A1, A2>, int>();

            int[] indexMapping = new int[_array.Length];

            for (int i = 0; i < _array.Length; i++)
            {
                mergedAttributes.TryAdd(_array[i], mergedAttributes.Count);
                indexMapping[i] = mergedAttributes[_array[i]];
            }

            return indexMapping;
        }

        public class MergeContext : MergeContextBase
        {
            AttributeList<A1, A2> _attributeList;
            Dictionary<Attribute<A1, A2>, int> _mergedAttributes;
            int[] _indexMapping;

            public MergeContext(AttributeList<A1, A2> attributeList)
            {
                _attributeList = attributeList;
                _indexMapping = new int[attributeList._array.Length];
                _mergedAttributes = new Dictionary<Attribute<A1, A2>, int>();
            }

            public override int Length => _mergedAttributes.Count;

            public override void Merge(int index)
            {
                _mergedAttributes.TryAdd(_attributeList._array[index], _mergedAttributes.Count);
                _indexMapping[index] = _mergedAttributes[_attributeList._array[index]];
            }

            public override int[] AssignBack()
            {
                _attributeList._array = _mergedAttributes.Keys.ToArray();
                return _indexMapping;
            }
        }
    }

    public abstract class MergeContextBase
    {
        public abstract void Merge(int index);
        public abstract int[] AssignBack();
        public abstract int Length { get; }
    }
}