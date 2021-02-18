using System;

namespace Nanomesh
{
    public readonly struct BoneWeight : IEquatable<BoneWeight>, IAttribute<BoneWeight>
    {
        public readonly int index0;
        public readonly int index1;
        public readonly int index2;
        public readonly int index3;
        public readonly float weight0;
        public readonly float weight1;
        public readonly float weight2;
        public readonly float weight3;

        public BoneWeight(int index0, int index1, int index2, int index3, float weight0, float weight1, float weight2, float weight3)
        {
            this.index0 = index0;
            this.index1 = index1;
            this.index2 = index2;
            this.index3 = index3;
            this.weight0 = weight0;
            this.weight1 = weight1;
            this.weight2 = weight2;
            this.weight3 = weight3;
        }

        public bool Equals(BoneWeight other)
        {
            return index0 == other.index0
                && index1 == other.index1
                && index2 == other.index2
                && index3 == other.index3
                && weight0 == other.weight0
                && weight1 == other.weight1
                && weight2 == other.weight2
                && weight3 == other.weight3;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + index0;
                hash = hash * 31 + index1;
                hash = hash * 31 + index2;
                hash = hash * 31 + index3;
                hash = hash * 31 + weight0.GetHashCode();
                hash = hash * 31 + weight1.GetHashCode();
                hash = hash * 31 + weight2.GetHashCode();
                hash = hash * 31 + weight3.GetHashCode();
                return hash;
            }
        }

        public BoneWeight Interpolate(double ratio, in BoneWeight boneWeight)
        {
            return new BoneWeight(
                ratio < 0.5f ? index0 : boneWeight.index0,
                ratio < 0.5f ? index1 : boneWeight.index1,
                ratio < 0.5f ? index2 : boneWeight.index2,
                ratio < 0.5f ? index3 : boneWeight.index3,
                (float)(ratio * weight0 + (1 - ratio) * boneWeight.weight0),
                (float)(ratio * weight1 + (1 - ratio) * boneWeight.weight1),
                (float)(ratio * weight2 + (1 - ratio) * boneWeight.weight2),
                (float)(ratio * weight3 + (1 - ratio) * boneWeight.weight3));
        }
    }
}