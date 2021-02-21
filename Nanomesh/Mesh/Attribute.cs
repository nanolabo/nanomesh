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
        bool IsAlmostEqual(T other);
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
        public abstract AttributeListBase CreateNew(int length);
        public abstract object this[int index] { get; set; }
        public void Copy(int indexFrom, AttributeListBase from, int indexTo, AttributeListBase to)
        {
            // TODO : Make this more generic to avoid casting object
            to[indexTo] = from[indexFrom];
        }
        public abstract double GetAttributePenalty(int indexA, int indexB);
    }

    public class AttributeList<A1, A2> : AttributeListBase
        where A1 : IAttribute<A1>
        where A2 : IAttribute<A2>
    {
        private Attribute<A1, A2>[] _array;

        public AttributeList(int length)
        {
            _array = new Attribute<A1, A2>[length];
        }

        public AttributeList(Attribute<A1, A2>[] array)
        {
            _array = array;
        }

        public override object this[int index] { get => _array[index]; set => _array[index] = (Attribute<A1, A2>)value; }

        public override int Length => _array.Length;

        public override MergeContextBase CreateMergeContext() => new MergeContext<Attribute<A1, A2>>((int index) => _array[index]);
        
        public override AttributeListBase CreateNew(int length) => new AttributeList<A1, A2>(length);

        public override double GetAttributePenalty(int indexA, int indexB)
        {
            double penalty = 0;
            if (_array[indexA].a1.IsAlmostEqual(_array[indexB].a1))
                penalty += 1;
            if (_array[indexA].a2.IsAlmostEqual(_array[indexB].a2))
                penalty += 1;
            return penalty;
        }

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
    }

    public class MergeContext<T> : MergeContextBase
    {
        Func<int, T> _getValue;
        Dictionary<T, int> _mergedAttributes; // TODO : Pool this
        Dictionary<int, int> _indexMapping; // TODO : Pool this

        public MergeContext(Func<int, T> getValue)
        {
            _getValue = getValue;
            _indexMapping = new Dictionary<int, int>();
            _mergedAttributes = new Dictionary<T, int>();
        }

        public override int Length => _mergedAttributes.Count;

        public override void Add(int index)
        {
            T item = _getValue(index);
            _mergedAttributes.TryAdd(item, index);
            _indexMapping.TryAdd(index, _mergedAttributes[item]);
        }

        public override int GetMapped(int oldIndex)
        {
            return _indexMapping[oldIndex];
        }
    }

    public abstract class MergeContextBase
    {
        public abstract void Add(int index);
        public abstract int GetMapped(int oldIndex);
        public abstract int Length { get; }
    }
}