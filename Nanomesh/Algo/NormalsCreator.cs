using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nanolabo
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

			Vector3F getFaceNormal(int nodeIndex)
			{
				int posA = mesh.nodes[nodeIndex].position;
				int posB = mesh.nodes[mesh.nodes[nodeIndex].relative].position;
				int posC = mesh.nodes[mesh.nodes[mesh.nodes[nodeIndex].relative].relative].position;

				Vector3F faceNormal = Vector3.Cross(
					mesh.positions[posB] - mesh.positions[posA],
					mesh.positions[posC] - mesh.positions[posA]);

				faceNormal.Normalize();

				return faceNormal;
			}

			for (int p = 0; p < positionToNode.Length; p++)
			{
				int nodeIndex = positionToNode[p];
				if (nodeIndex < 0)
					continue;

				Debug.Assert(!mesh.nodes[nodeIndex].IsRemoved);

				int sibling1 = nodeIndex;
				do
				{
					Vector3F sum = Vector3F.Zero;

					int sibling2 = nodeIndex;
					do
					{
						if (sibling1 == sibling2)
						{
							sum += getFaceNormal(sibling2);
						}
						else
						{
							float dot = Vector3F.Dot(getFaceNormal(sibling1), getFaceNormal(sibling2));

							if (dot >= cosineThreshold)
							{
								sum += getFaceNormal(sibling2);
							}
						}

					} while ((sibling2 = mesh.nodes[sibling2].sibling) != nodeIndex);

					sum.Normalize();

					Attribute attribute = mesh.attributes[mesh.nodes[sibling1].attribute];
					attribute.normal = sum;

					PosAndAttribute posAndAttribute = new PosAndAttribute { position = p, attribute = attribute };

					attributeToIndex.TryAdd(posAndAttribute, attributeToIndex.Count);

					mesh.nodes[sibling1].attribute = attributeToIndex[posAndAttribute];

				} while ((sibling1 = mesh.nodes[sibling1].sibling) != nodeIndex);
			}

			// Assign new attributes
			mesh.attributes = new Attribute[attributeToIndex.Count];
			foreach (var pair in attributeToIndex)
			{
				mesh.attributes[pair.Value] = pair.Key.attribute;
			}
		}
	}
}
