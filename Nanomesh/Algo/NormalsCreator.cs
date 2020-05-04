using System.Collections.Generic;
using System.Diagnostics;

namespace Nanolabo
{
    public class NormalsModifier
    {
		public void Run(ConnectedMesh mesh, float smoothingAngle)
        {
			int[] positionToNode = mesh.GetPositionToNode();
			Dictionary<Attribute, int> attributeToIndex = new Dictionary<Attribute, int>();

			for (int p = 0; p < positionToNode.Length; p++)
			{
				int nodeIndex = positionToNode[p];
				if (nodeIndex < 0)
					continue;

				Debug.Assert(!mesh.nodes[nodeIndex].IsRemoved);

				Vector3F meanNormal = Vector3F.Zero;

				// Computes mean normal at position
				int sibling = nodeIndex;
				do
				{
					int posA = mesh.nodes[sibling].position;
					int posB = mesh.nodes[mesh.nodes[sibling].relative].position;
					int posC = mesh.nodes[mesh.nodes[mesh.nodes[sibling].relative].relative].position;

					Vector3F faceNormal = Vector3.Cross(
						mesh.positions[posB] - mesh.positions[posA],
						mesh.positions[posC] - mesh.positions[posA]);

					faceNormal.Normalize();

					meanNormal += faceNormal;

				} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndex);

				meanNormal.Normalize();

				sibling = nodeIndex;
				do
				{
					int posA = mesh.nodes[sibling].position;
					int posB = mesh.nodes[mesh.nodes[sibling].relative].position;
					int posC = mesh.nodes[mesh.nodes[mesh.nodes[sibling].relative].relative].position;

					Vector3F faceNormal = Vector3.Cross(
						mesh.positions[posB] - mesh.positions[posA],
						mesh.positions[posC] - mesh.positions[posA]);

					faceNormal.Normalize();

					float angle = Vector3F.AngleDegrees(meanNormal, faceNormal);

					Attribute attribute = mesh.attributes[mesh.nodes[sibling].attribute];

					if (angle  * 2f > smoothingAngle)
					{
						attribute.normal = faceNormal;
					}
					else
					{
						attribute.normal = meanNormal;
					}

					// We only create a new attribute if it is entirely different
					attributeToIndex.TryAdd(attribute, attributeToIndex.Count);

					mesh.nodes[sibling].attribute = attributeToIndex[attribute];

				} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndex);
			}

			// Assign new attributes
			mesh.attributes = new Attribute[attributeToIndex.Count];
			foreach (var pair in attributeToIndex)
			{
				mesh.attributes[pair.Value] = pair.Key;
			}
		}
	}
}
