using System;
using System.Collections.Generic;
using System.Linq;

namespace Nanomesh
{
    public readonly struct BoneWeight : IEquatable<BoneWeight>, IInterpolable<BoneWeight>
    {
        public readonly int index0;
        public readonly int index1;
        public readonly int index2;
        public readonly int index3;
        public readonly float weight0;
        public readonly float weight1;
        public readonly float weight2;
        public readonly float weight3;

        public int GetIndex(int i)
        {
            switch (i)
            {
                case 0: return index0;
                case 1: return index1;
                case 2: return index2;
                case 3: return index3;
                default: return -1;
            }
        }

        public float GetWeight(int i)
        {
            switch (i)
            {
                case 0: return weight0;
                case 1: return weight1;
                case 2: return weight2;
                case 3: return weight3;
                default: return -1;
            }
        }

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

        public unsafe BoneWeight Interpolate(BoneWeight other, double ratio)
        {
            BoneWeight boneWeightA = this;
            BoneWeight boneWeightB = other;

            Dictionary<int, float> newBoneWeight = new Dictionary<int, float>();

            // Map weights and indices
            for (int i = 0; i < 4; i++)
            {
                newBoneWeight.TryAdd(boneWeightA.GetIndex(i), 0);
                newBoneWeight.TryAdd(boneWeightB.GetIndex(i), 0);
                newBoneWeight[boneWeightA.GetIndex(i)] += (float)((1 - ratio) * boneWeightA.GetWeight(i));
                newBoneWeight[boneWeightB.GetIndex(i)] += (float)(ratio * boneWeightB.GetWeight(i));
            }

            int* newIndices = stackalloc int[4];
            float* newWeights = stackalloc float[4];

            // Order from biggest to smallest weight, and drop bones above 4th
            float totalWeight = 0;
            int k = 0;
            foreach (var boneWeightN in newBoneWeight.OrderByDescending(x => x.Value))
            {
                newIndices[k] = boneWeightN.Key;
                newWeights[k] = boneWeightN.Value;
                totalWeight += boneWeightN.Value;
                if (k == 3)
                    break;
                k++;
            }

            // Normalize
            if (totalWeight > 0)
            {
                for (int j = 0; j < 4; j++)
                {
                    newWeights[k] /= totalWeight;
                }
            }

            return new BoneWeight(
                newIndices[0], newIndices[1], newIndices[2], newIndices[3],
				newWeights[0], newWeights[1], newWeights[2], newWeights[3]);

            //return new BoneWeight(
            //    ratio < 0.5f ? index0 : other.index0,
            //    ratio < 0.5f ? index1 : other.index1,
            //    ratio < 0.5f ? index2 : other.index2,
            //    ratio < 0.5f ? index3 : other.index3,
            //    (float)(ratio * weight0 + (1 - ratio) * other.weight0),
            //    (float)(ratio * weight1 + (1 - ratio) * other.weight1),
            //    (float)(ratio * weight2 + (1 - ratio) * other.weight2),
            //    (float)(ratio * weight3 + (1 - ratio) * other.weight3));
        }
    }
}