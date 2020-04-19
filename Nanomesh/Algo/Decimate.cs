using System;
using System.Collections.Generic;
using System.Linq;

namespace Nanolabo
{
    public class DecimateModifier
    {
		private ConnectedMesh mesh;

		private int[] positionToNode;
		private SymmetricMatrix[] matrices;
		private SortedSet<PairCollapse> pairs;

		public void Run(ConnectedMesh mesh, float targetTriangleRatio)
		{
			targetTriangleRatio = Math.Clamp(targetTriangleRatio, 0.001f, 1f);
			Run(mesh, (int)MathF.Round(targetTriangleRatio * mesh.FaceCount));
		}

		public void Run(ConnectedMesh mesh, int targetTriangleCount)
		{
			this.mesh = mesh;
			
			matrices = new SymmetricMatrix[mesh.positions.Length];
			pairs = new SortedSet<PairCollapse>(new PairComparer());

			int initialTriangleCount = mesh.FaceCount;
			int lastProgress = -1;

			while (mesh.FaceCount > targetTriangleCount)
			{
				positionToNode = mesh.GetPositionToNode();
				UpdateCosts();

				var minPair = pairs.Min();
				pairs.Remove(minPair);

				mesh.CollapseEdge(positionToNode[minPair.pos1], positionToNode[minPair.pos2], minPair.result);
				//Console.WriteLine("Collapse : " + minPair.error);

				int progress = (int)MathF.Round(100f * (initialTriangleCount - mesh.FaceCount) / (initialTriangleCount - targetTriangleCount));
				if (progress > lastProgress)
				{
					Console.WriteLine("Progress : " + progress + "%");
					lastProgress = progress;
				}
			}

			//mesh.Compact();
		}

		private void UpdateCosts()
		{
			pairs.Clear();

			for (int p = 0; p < positionToNode.Length; p++)
			{
				SymmetricMatrix symmetricMatrix = new SymmetricMatrix();

				int nodeIndex = positionToNode[p];
				if (mesh.nodes[nodeIndex].IsRemoved)
					continue;

				int sibling = nodeIndex;
				do
				{
					// Compute triangle normal
					// Todo : Use vertex normal instead to use smoothing
					int[] relatives = mesh.GetRelatives(sibling);

					if (mesh.nodes[relatives[0]].position < 0) continue;
					if (mesh.nodes[relatives[1]].position < 0) continue;
					if (mesh.nodes[relatives[2]].position < 0) continue;

					Vector3 normal = Vector3.Cross(
						mesh.positions[mesh.nodes[relatives[1]].position] - mesh.positions[mesh.nodes[relatives[0]].position],
						mesh.positions[mesh.nodes[relatives[2]].position] - mesh.positions[mesh.nodes[relatives[0]].position]);

					normal.Normalize();

					float dot = Vector3.Dot(-normal, mesh.positions[mesh.nodes[sibling].position]);

					symmetricMatrix += new SymmetricMatrix(normal.x, normal.y, normal.z, dot);

					for (int i = 1; i < relatives.Length; i++)
					{
						var pair = new PairCollapse
						{
							pos1 = mesh.nodes[relatives[i - 1]].position,
							pos2 = mesh.nodes[relatives[i]].position
						};

						pairs.Add(pair);
					}
				} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndex);

				matrices[p] = symmetricMatrix;
			}

			foreach (var pair in pairs)
			{
				pair.error = CalculateError(pair.pos1, pair.pos2, out Vector3 result);
				pair.result = result;

				//Console.WriteLine(pair.error);
			}
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

		private float CalculateError(int pos1, int pos2, out Vector3 result)
		{
			SymmetricMatrix q = matrices[pos1] + matrices[pos2];
			bool border = false;
			float error = 0;
			float det = q.Determinant(0, 1, 2, 1, 4, 5, 2, 5, 7);

			result = new Vector3();

			if (det != 0 && !border)
			{
				result.x = -1 / det * (q.Determinant(1, 2, 3, 4, 5, 6, 5, 7, 8));
				result.y = 1 / det * (q.Determinant(0, 2, 3, 1, 5, 6, 2, 7, 8));
				result.z = -1 / det * (q.Determinant(0, 1, 3, 1, 4, 6, 2, 5, 8));

				error = VertexError(q, result.x, result.y, result.z);
			}
			else
			{
				Vector3 p1 = mesh.positions[pos1];
				Vector3 p2 = mesh.positions[pos2];
				Vector3 p3 = (p1 + p2) / 2;
				float error1 = VertexError(q, p1.x, p1.y, p1.z);
				float error2 = VertexError(q, p2.x, p2.y, p2.z);
				float error3 = VertexError(q, p3.x, p3.y, p3.z);
				error = MathF.Min(error1, MathF.Min(error2, error3));
				if (error1 == error) result = p1;
				if (error2 == error) result = p2;
				if (error3 == error) result = p3;
			}

			return error;
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