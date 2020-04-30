using System.Diagnostics;

namespace Nanolabo
{
    public class NormalsModifier
    {
		public void Run(ConnectedMesh mesh)
        {
			int[] positionToNode = mesh.GetPositionToNode();

			for (int p = 0; p < positionToNode.Length; p++)
			{
				int nodeIndex = positionToNode[p];
				if (nodeIndex < 0)
					continue;

				Debug.Assert(!mesh.nodes[nodeIndex].IsRemoved);

				Vector3F normal = Vector3F.Zero;

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

					normal += faceNormal;

				} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndex);

				normal.Normalize();

				sibling = nodeIndex;
				do
				{
					mesh.attributes[mesh.nodes[sibling].attribute].normal = normal;
				} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndex);
			}
		}
	}
}
