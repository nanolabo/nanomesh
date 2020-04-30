using System.Diagnostics;

namespace Nanolabo
{
    public class NormalsModifier
    {
		public void Run(ConnectedMesh mesh, float smoothingAngle)
        {
			int[] positionToNode = mesh.GetPositionToNode();

			for (int p = 0; p < positionToNode.Length; p++)
			{
				int nodeIndex = positionToNode[p];
				if (nodeIndex < 0)
					continue;

				Debug.Assert(!mesh.nodes[nodeIndex].IsRemoved);

				Vector3F meanNormal = Vector3F.Zero;

				int sibling = nodeIndex;
				do
				{
					// Compute face normal
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

					if (angle  * 2f > smoothingAngle)
					{
						mesh.attributes[mesh.nodes[sibling].attribute].normal = faceNormal;
					}
					else
					{
						mesh.attributes[mesh.nodes[sibling].attribute].normal = meanNormal;
					}

				} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndex);
			}
		}
	}
}
