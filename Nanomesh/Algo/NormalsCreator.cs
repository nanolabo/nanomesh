using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nanomesh
{
    public class NormalsModifier
    {
        public struct PosAndAttribute : IEquatable<PosAndAttribute>
        {
            public int position;
            public Attribute attribute;

            public override int GetHashCode()
            {
                return position.GetHashCode() ^ (attribute.GetHashCode() << 2);
            }

            public bool Equals(PosAndAttribute other)
            {
                return position == other.position && attribute.Equals(other.attribute);
            }
        }

        public void Run(ConnectedMesh mesh, float smoothingAngle)
        {
            float cosineThreshold = MathF.Cos(smoothingAngle * MathF.PI / 180f);

            int[] positionToNode = mesh.GetPositionToNode();

            Dictionary<PosAndAttribute, int> attributeToIndex = new Dictionary<PosAndAttribute, int>();

            for (int p = 0; p < positionToNode.Length; p++)
            {
                int nodeIndex = positionToNode[p];
                if (nodeIndex < 0)
                {
                    continue;
                }

                Debug.Assert(!mesh.nodes[nodeIndex].IsRemoved);

                int sibling1 = nodeIndex;
                do
                {
                    Vector3F sum = Vector3F.Zero;

                    Vector3F normal1 = mesh.GetFaceNormal(sibling1);

                    int sibling2 = nodeIndex;
                    do
                    {
                        Vector3F normal2 = mesh.GetFaceNormal(sibling2);

                        float dot = Vector3F.Dot(normal1, normal2);

                        if (dot >= cosineThreshold)
                        {
                            // Area and angle weighting (it gives better results)
                            sum += mesh.GetFaceArea(sibling2) * mesh.GetAngleRadians(sibling2) * normal2;
                        }

                    } while ((sibling2 = mesh.nodes[sibling2].sibling) != nodeIndex);

                    sum = sum.Normalized;


                } while ((sibling1 = mesh.nodes[sibling1].sibling) != nodeIndex);
            }

            // Assign new attributes

            // TODO : Fix
        }
    }
}