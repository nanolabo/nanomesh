using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nanolabo
{
    public class DecimateModifier
    {
		private ConnectedMesh mesh;

		private int[] positionToNode;
		private SymmetricMatrix[] matrices;
		private HashSet<PairCollapse> pairs;

		public void Run(ConnectedMesh mesh, float targetTriangleRatio)
		{
			targetTriangleRatio = Math.Clamp(targetTriangleRatio, 0.001f, 1f);
			Run(mesh, (int)MathF.Round(targetTriangleRatio * mesh.FaceCount));
		}

		public void Run(ConnectedMesh mesh, int targetTriangleCount)
		{
			this.mesh = mesh;
			
			matrices = new SymmetricMatrix[mesh.positions.Length];
			pairs = new HashSet<PairCollapse>();

			int initialTriangleCount = mesh.FaceCount;
			int lastProgress = -1;

			positionToNode = mesh.GetPositionToNode();
			InitializePairs();
			CalculateQuadrics();
			CalculateErrors();

			while (mesh.FaceCount > targetTriangleCount)
			{
				var minPair = pairs.Min();
				pairs.Remove(minPair);

				CollapseEdge(positionToNode[minPair.pos1], positionToNode[minPair.pos2], minPair.result);
				//Console.WriteLine("Collapse : " + minPair.error);

				int progress = (int)MathF.Round(100f * (initialTriangleCount - mesh.FaceCount) / (initialTriangleCount - targetTriangleCount));
				if (progress > lastProgress)
				{
					if (progress % 10 == 0)
						Console.WriteLine("Progress : " + progress + "%");
					lastProgress = progress;
				}
			}

			//mesh.Compact();
		}

		private void InitializePairs()
		{
			pairs.Clear();

			for (int p = 0; p < positionToNode.Length; p++)
			{
				int nodeIndex = positionToNode[p];

				int sibling = nodeIndex;
				do
				{
					Node firstRelative = mesh.nodes[mesh.nodes[sibling].relative];

					var pair = new PairCollapse();
					pair.pos1 = firstRelative.position;
					pair.pos2 = mesh.nodes[firstRelative.relative].position;
					pairs.Add(pair);

				} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndex);
			}
		}

		private void CalculateQuadrics()
		{
			for (int p = 0; p < positionToNode.Length; p++)
			{
				CalculateQuadric(p);
			}
		}

		private void CalculateQuadric(int position)
		{
			int nodeIndex = positionToNode[position];

			Debug.Assert(!mesh.nodes[nodeIndex].IsRemoved);

			SymmetricMatrix symmetricMatrix = new SymmetricMatrix();

			int sibling = nodeIndex;
			do
			{
				Debug.Assert(mesh.CheckRelatives(sibling));

				// Compute triangle normal
				// TODO : Use vertex normal instead to use smoothing

				int posA = mesh.nodes[sibling].position;
				int posB = mesh.nodes[mesh.nodes[sibling].relative].position;
				int posC = mesh.nodes[mesh.nodes[mesh.nodes[sibling].relative].relative].position;

				Vector3 normal = Vector3.Cross(
					mesh.positions[posB] - mesh.positions[posA],
					mesh.positions[posC] - mesh.positions[posA]);

				normal.Normalize();

				float dot = Vector3.Dot(-normal, mesh.positions[mesh.nodes[sibling].position]);

				symmetricMatrix += new SymmetricMatrix(normal.x, normal.y, normal.z, dot);

			} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndex);

			matrices[position] = symmetricMatrix;
		}

		private void CalculateErrors()
		{
			var enumerator = pairs.GetEnumerator();

			while (enumerator.MoveNext())
			{
				PairCollapse pair = enumerator.Current;
				CalculateError(pair);
			}
		}

		private void CalculateError(PairCollapse pair)
		{
			SymmetricMatrix q = matrices[pair.pos1] + matrices[pair.pos2];
			bool border = false;
			float error = 0;
			float det = q.Determinant(0, 1, 2, 1, 4, 5, 2, 5, 7);

			Vector3 result = new Vector3();

			if (det != 0 && !border)
			{
				result.x = -1 / det * q.Determinant(1, 2, 3, 4, 5, 6, 5, 7, 8);
				result.y = 1 / det * q.Determinant(0, 2, 3, 1, 5, 6, 2, 7, 8);
				result.z = -1 / det * q.Determinant(0, 1, 3, 1, 4, 6, 2, 5, 8);

				error = VertexError(q, result.x, result.y, result.z);
			}
			else
			{
				Vector3 p1 = mesh.positions[pair.pos1];
				Vector3 p2 = mesh.positions[pair.pos2];
				Vector3 p3 = (p1 + p2) / 2;
				float error1 = VertexError(q, p1.x, p1.y, p1.z);
				float error2 = VertexError(q, p2.x, p2.y, p2.z);
				float error3 = VertexError(q, p3.x, p3.y, p3.z);
				error = MathF.Min(error1, MathF.Min(error2, error3));
				if (error1 == error) result = p1;
				if (error2 == error) result = p2;
				if (error3 == error) result = p3;
			}

			pair.result = result;
			pair.error = error;
		}

		public void CollapseEdge(int nodeIndexA, int nodeIndexB, Vector3 position)
		{
			int posA = mesh.nodes[nodeIndexA].position;
			int posB = mesh.nodes[nodeIndexB].position;

			// Remove all edges around A
			int sibling = nodeIndexA;
			int relative;
			do
			{
				relative = sibling;
				while ((relative = mesh.nodes[relative].relative) != sibling)
				{
					int posC = mesh.nodes[relative].position;
					pairs.Remove(new PairCollapse { pos1 = posA, pos2 = posC });
				} 

			} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndexA);

			// Remove all edges around A
			sibling = nodeIndexB;
			do
			{
				relative = sibling;
				while ((relative = mesh.nodes[relative].relative) != sibling)
				{
					int posC = mesh.nodes[relative].position;
					pairs.Remove(new PairCollapse { pos1 = posB, pos2 = posC });
				}

			} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndexB);

			// Collapse edge
			int validNode = mesh.CollapseEdge(nodeIndexA, nodeIndexB, position);

			// Actualize position to nodes
			positionToNode[posA] = validNode;

			// Recompute quadric at this position
			CalculateQuadric(posA);

			// Recreate edges around new point and recompute collapse quadric errors
			sibling = validNode;
			do
			{
				relative = sibling;
				while ((relative = mesh.nodes[relative].relative) != sibling)
				{
					int posC = mesh.nodes[relative].position;

					var pair = new PairCollapse { pos1 = posA, pos2 = posC };
					CalculateError(pair);

					pairs.Add(pair);

					// Actualize position to nodes
					positionToNode[posC] = relative;
				}

			} while ((sibling = mesh.nodes[sibling].sibling) != validNode);
		}

		private float VertexError(SymmetricMatrix q, float x, float y, float z)
		{
			return q[0] * x * x + 2 * q[1] * x * y + 2 * q[2] * x * z
				+ 2 * q[3] * x + q[4] * y * y
				+ 2 * q[5] * y * z + 2 * q[6] * y
				+ q[7] * z * z
				+ 2 * q[8] * z
				+ q[9];
		}

		public class PairCollapse : IComparable
		{
			public int pos1;
			public int pos2;
			public Vector3 result;
			public float error;

			public override int GetHashCode()
			{
				unsafe
				{
					return pos1 + pos2;
				}
			}

			public override bool Equals(object obj)
			{
				PairCollapse pc = (PairCollapse)obj;
				return (pc.pos1 == pos1 && pc.pos2 == pos2) || (pc.pos1 == pos2 && pc.pos2 == pos1);
			}

			public int CompareTo(object obj)
			{
				PairCollapse y = (PairCollapse)obj;
				return this == y ? 0 : this.error > y.error ? 1 : -1;
			}

			public static bool operator ==(PairCollapse x, PairCollapse y)
			{
				return x.GetHashCode() == y.GetHashCode() && x.Equals(y);
			}

			public static bool operator !=(PairCollapse x, PairCollapse y)
			{
				return x.GetHashCode() != y.GetHashCode() || !x.Equals(y);
			}
		}

		internal class PairComparer : IComparer<PairCollapse>
		{
			public int Compare(PairCollapse x, PairCollapse y)
			{
				return x == y ? 0 : x.error > y.error ? 1 : -1;
			}
		}
    }
}