using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nanolabo
{
    public partial class DecimateModifier
    {
		private ConnectedMesh mesh;

		private SymmetricMatrix[] matrices;
		private HashSet<PairCollapse> pairs;
		private LinkedHashSet<PairCollapse> mins = new LinkedHashSet<PairCollapse>();

		private int lastProgress;
		private int initialTriangleCount;

		const double εdet = 0.001f;
		const double εprio = 0.00001f;

		public void DecimateToRatio(ConnectedMesh mesh, float targetTriangleRatio)
		{
			targetTriangleRatio = Math.Clamp(targetTriangleRatio, 0f, 1f);
			DecimateToPolycount(mesh, (int)MathF.Round(targetTriangleRatio * mesh.FaceCount));
		}

		public void DecimateToError(ConnectedMesh mesh, float maximumError)
		{
			Initialize(mesh);

			while (GetPairWithMinimumError().error <= maximumError)
			{
				Iterate();

				// TODO : Add Progress
			}

			// TODO : mesh.Compact();
		}

		public void DecimatePolycount(ConnectedMesh mesh, int polycount)
		{
			DecimateToPolycount(mesh, (int)MathF.Round(mesh.FaceCount - polycount));
		}

		public void DecimateToPolycount(ConnectedMesh mesh, int targetTriangleCount)
		{
			Initialize(mesh);

			while (mesh.FaceCount > targetTriangleCount)
			{
				Iterate();

				int progress = (int)MathF.Round(100f * (initialTriangleCount - mesh.FaceCount) / (initialTriangleCount - targetTriangleCount));
				if (progress > lastProgress)
				{
					if (progress % 10 == 0)
						Console.WriteLine("Progress : " + progress + "%");
					lastProgress = progress;
				}
			}

			// TODO : mesh.Compact();
		}

		private void Initialize(ConnectedMesh mesh)
		{
			this.mesh = mesh;

			initialTriangleCount = mesh.FaceCount;

			matrices = new SymmetricMatrix[mesh.positions.Length];
			pairs = new HashSet<PairCollapse>();
			lastProgress = -1;

			InitializePairs();
			CalculateQuadrics();
			CalculateErrors();
		}

		private void Iterate()
		{
			PairCollapse pair = GetPairWithMinimumError();

			Debug.Assert(CheckMins());
			Debug.Assert(CheckPair(pair));

			pairs.Remove(pair);
			mins.Remove(pair);

			CollapseEdge(mesh.PositionToNode[pair.pos1], mesh.PositionToNode[pair.pos2], pair.result);
		}

		private bool CheckPairs()
		{
			foreach (var pair in pairs)
			{
				CheckPair(pair);
			}

			return true;
		}

		private bool CheckMins()
		{
			foreach (var min in mins)
			{
				Debug.Assert(pairs.Contains(min));
			}

			return true;
		}

		private bool CheckPair(PairCollapse pair)
		{
			Debug.Assert(pair.pos1 != pair.pos2, "Positions must be different");
			Debug.Assert(!mesh.nodes[mesh.PositionToNode[pair.pos1]].IsRemoved, $"Position 1 is unreferenced {mesh.PositionToNode[pair.pos1]}");
			Debug.Assert(!mesh.nodes[mesh.PositionToNode[pair.pos2]].IsRemoved, $"Position 2 is unreferenced {mesh.PositionToNode[pair.pos2]}");

			return true;
		}

		private PairCollapse GetPairWithMinimumError()
		{
			if (mins.Count == 0)
				ComputeMins();

			return mins.First.Value;
		}

		private int MinsCount => Math.Clamp((int)(0.01f * mesh.faceCount) + 100, 0, pairs.Count);

		private void ComputeMins()
		{
			// Find the k smallest elements (ordered)
			// https://www.desmos.com/calculator/eoxaztxqaf
			mins = new LinkedHashSet<PairCollapse>(pairs.OrderBy(x => x).Take(MinsCount)); // Todo : find faster sorting
		}

		private void AddMin(PairCollapse item)
		{
			var current = mins.Last;
			while (current != null && item.CompareTo(current.Value) < 0)
			{
				current = current.Previous;
			}

			if (current == mins.Last)
				return;

			if (current == null)
				mins.AddBefore(item, mins.First);
			else
				mins.AddAfter(item, current);
		}

		private void InitializePairs()
		{
			pairs.Clear();
			mins.Clear();

			for (int p = 0; p < mesh.PositionToNode.Length; p++)
			{
				int nodeIndex = mesh.PositionToNode[p];
				if (nodeIndex < 0)
					continue;

				int sibling = nodeIndex;
				do
				{
					Node firstRelative = mesh.nodes[mesh.nodes[sibling].relative];

					var pair = new PairCollapse(firstRelative.position, mesh.nodes[firstRelative.relative].position);

					pairs.Add(pair);

					Debug.Assert(CheckPair(pair));

				} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndex);
			}
		}

		private void CalculateQuadrics()
		{
			for (int p = 0; p < mesh.PositionToNode.Length; p++)
			{
				CalculateQuadric(p);
			}
		}

		private void CalculateQuadric(int position)
		{
			int nodeIndex = mesh.PositionToNode[position];
			if (nodeIndex < 0) // TODO : Remove this check
				return;

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

				double dot = Vector3.Dot(-normal, mesh.positions[mesh.nodes[sibling].position]);

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
			Debug.Assert(CheckPair(pair));

			int node1 = mesh.PositionToNode[pair.pos1];
			int node2 = mesh.PositionToNode[pair.pos2];

			var edgeType = mesh.GetEdgeType(node1, node2, out int otherNodeIndex1, out int otherNodeIndex2);
#if DEBUG
			pair.type = edgeType;
#endif

			double error;
			Vector3 result;

			Vector3 p1 = mesh.positions[pair.pos1];
			Vector3 p2 = mesh.positions[pair.pos2];

			// Use quadric error to determine optimal vertex position only makes sense for manifold edges
			if (edgeType == ConnectedMesh.EdgeType.Manifold)
			{
				SymmetricMatrix q = matrices[pair.pos1] + matrices[pair.pos2];
				double det = q.Determinant(0, 1, 2, 1, 4, 5, 2, 5, 7);

				if (det > εdet || det < -εdet)
				{
					result.x = (float)(-1 / det * q.Determinant(1, 2, 3, 4, 5, 6, 5, 7, 8));
					result.y = (float)(1 / det * q.Determinant(0, 2, 3, 1, 5, 6, 2, 7, 8));
					result.z = (float)(-1 / det * q.Determinant(0, 1, 3, 1, 4, 6, 2, 5, 8));

					error = ComputeVertexError(q, result.x, result.y, result.z);
				}
				else
				{
					Vector3 p3 = (p1 + p2) / 2;
					double error1 = ComputeVertexError(q, p1.x, p1.y, p1.z);
					double error2 = ComputeVertexError(q, p2.x, p2.y, p2.z);
					double error3 = ComputeVertexError(q, p3.x, p3.y, p3.z);

					error = Math.Min(error1, Math.Min(error2, error3));
					if (error1 == error) result = p1;
					else if (error2 == error) result = p2;
					else result = p3;
				}
			}
			else if (edgeType == ConnectedMesh.EdgeType.TShapeA)
			{
				SymmetricMatrix q = matrices[pair.pos1] + matrices[pair.pos2];
				error = ComputeVertexError(q, p1.x, p1.y, p1.z);
				result = p1;
			}
			else if (edgeType == ConnectedMesh.EdgeType.TShapeB)
			{
				SymmetricMatrix q = matrices[pair.pos1] + matrices[pair.pos2];
				error = ComputeVertexError(q, p2.x, p2.y, p2.z);
				result = p2;
			}
			else if (edgeType == ConnectedMesh.EdgeType.AShape)
			{
				result = Vector3.Zero;
				error = double.PositiveInfinity; // Never collapse A-Shapes
			}
			else if (edgeType == ConnectedMesh.EdgeType.Border)
			{
				Vector3 p1o = mesh.positions[mesh.nodes[otherNodeIndex1].position];
				Vector3 p2o = mesh.positions[mesh.nodes[otherNodeIndex2].position];

				var error1 = ComputeLineicError(p1, p2, p2o);
				var error2 = ComputeLineicError(p2, p1, p1o);
				error = Math.Min(error1, error2);
				if (error1 == error) result = p1;
				else result = p2;
			}
			else
			{
				result = Vector3.Zero;
				error = double.PositiveInfinity; // Never collapse unknown shapes
			}

			// Ponderate error with edge length to collapse first shortest edges
			error = Math.Abs(error) + εprio * Vector3.Magnitude(p2 - p1); 

			pair.result = result;
			pair.error = error;
		}

		private double ComputeLineicError(Vector3 A, Vector3 B, Vector3 C) 
		{
			var θ = Vector3.Angle(B - A, C - A);
			return Vector3.Magnitude(B - A) * Math.Sin(θ);
		}

		private double ComputeVertexError(SymmetricMatrix q, double x, double y, double z)
		{
			return q[0] * x * x + 2 * q[1] * x * y + 2 * q[2] * x * z + 2 * q[3] * x
				+ q[4] * y * y + 2 * q[5] * y * z + 2 * q[6] * y
				+ q[7] * z * z + 2 * q[8] * z
				+ q[9];
		}

		private void CollapseEdge(int nodeIndexA, int nodeIndexB, Vector3 position)
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
					var pair = new PairCollapse(posA, posC);
					pairs.Remove(pair); // Todo : Optimization by only removing first pair (first edge)
					mins.Remove(pair);
				} 

			} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndexA);

			// Remove all edges around B
			sibling = nodeIndexB;
			do
			{
				relative = sibling;
				while ((relative = mesh.nodes[relative].relative) != sibling)
				{
					int posC = mesh.nodes[relative].position;
					var pair = new PairCollapse(posB, posC);
					pairs.Remove(pair);
					mins.Remove(pair);
				}

			} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndexB);

			// Collapse edge
			int validNode = mesh.CollapseEdge(nodeIndexA, nodeIndexB, position);

			// A disconnected triangle has been collapsed, there are no edges to register
			if (validNode < 0)
				return;
			
			CalculateQuadric(posA); // Required ?

			// Recreate edges around new point and recompute collapse quadric errors
			sibling = validNode;
			do
			{
				relative = sibling;
				while ((relative = mesh.nodes[relative].relative) != sibling)
				{
					int posC = mesh.nodes[relative].position;

					// Update quadrics and errors one level deeper
					// Mathematically more correct, at the cost of performance
					//int sibling2 = relative;
					//while ((sibling2 = mesh.nodes[sibling2].sibling) != relative)
					//{
					//	int relative2 = sibling2;
					//	while ((relative2 = mesh.nodes[relative2].relative) != sibling2)
					//	{
					//		int posD = mesh.nodes[relative2].position;
					//		if (posD == posC)
					//			continue;
					//		if (pairs.TryGetValue(new PairCollapse(posC, posD), out PairCollapse actualPair))
					//		{
					//			mins.Remove(actualPair);
					//			CalculateQuadric(posD);
					//			CalculateError(actualPair);
					//			AddMin(actualPair);
					//		}
					//	}
					//}

					if (validNode < 0)
						continue;

					var pair = new PairCollapse(posA, posC);

					// Optimization by not adding a pair that has already been added
					if (pairs.Contains(pair))
						continue;

					Debug.Assert(CheckPair(pair));

					CalculateQuadric(posC); // Required ? Shouldn't we keep original quadrics ?

					CalculateError(pair);

					pairs.Add(pair);
					AddMin(pair);
				}
			} while ((sibling = mesh.nodes[sibling].sibling) != validNode);
		}
	}
}