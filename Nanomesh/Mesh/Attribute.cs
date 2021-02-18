using System;
using System.Collections;
using System.Collections.Generic;

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

    public interface IAttribute<T>
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
}