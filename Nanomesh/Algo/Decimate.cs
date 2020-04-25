using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nanolabo
{
    public class DecimateModifier
    {
		private ConnectedMesh mesh;

		private SymmetricMatrix[] matrices;
		private HashSet<PairCollapse> pairs;
		private LinkedHashSet<PairCollapse> mins = new LinkedHashSet<PairCollapse>();

		private int lastProgress;
		private int initialTriangleCount;

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

				// Todo : Progress
			}

			Console.WriteLine("over");
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

			//mesh.Compact();
		}

		private void Initialize(ConnectedMesh mesh)
		{
			SortedSet<PairCollapse> test = new SortedSet<PairCollapse>();
			Debug.Assert(test.Add(new PairCollapse { pos1 = 1, pos2 = 2 }));
			Debug.Assert(!test.Add(new PairCollapse { pos1 = 1, pos2 = 2 }));
			Debug.Assert(test.Remove(new PairCollapse { pos1 = 1, pos2 = 2 }));
			Debug.Assert(!test.Remove(new PairCollapse { pos1 = 1, pos2 = 2 }));

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

			RemoveMin(pair);
			pairs.Remove(pair);

			CollapseEdge(mesh.PositionToNode[pair.pos1], mesh.PositionToNode[pair.pos2], pair.result);
			//Console.WriteLine("Collapse : " + minPair.error);
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

		private int MinsCount => 100;//Math.Clamp((int)(0.01f * mesh.faceCount) + 100, 0, pairs.Count);

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

		private void RemoveMin(PairCollapse item)
		{
			mins.Remove(item);
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

					var pair = new PairCollapse();
					pair.pos1 = firstRelative.position;
					pair.pos2 = mesh.nodes[firstRelative.relative].position;

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
			if (nodeIndex < 0)
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
			Debug.Assert(CheckPair(pair));

			int node1 = mesh.PositionToNode[pair.pos1];
			int node2 = mesh.PositionToNode[pair.pos2];

			SymmetricMatrix q = matrices[pair.pos1] + matrices[pair.pos2];

			var edgeInfo = mesh.GetEdgeInfo(node1, node2, out int otherNodeIndex1, out int otherNodeIndex2);

			float error = 0;
			float det = q.Determinant(0, 1, 2, 1, 4, 5, 2, 5, 7);

			Vector3 result = new Vector3();

			Vector3 p1 = mesh.positions[pair.pos1];
			Vector3 p2 = mesh.positions[pair.pos2];

			// Use quadric error to determine optimal vertex position only makes sense for manifold edges
			if (edgeInfo == ConnectedMesh.EdgeInfo.Manifold)
			{
				if (det != 0)
				{
					result.x = -1 / det * q.Determinant(1, 2, 3, 4, 5, 6, 5, 7, 8);
					result.y = 1 / det * q.Determinant(0, 2, 3, 1, 5, 6, 2, 7, 8);
					result.z = -1 / det * q.Determinant(0, 1, 3, 1, 4, 6, 2, 5, 8);

					error = VertexError(q, result.x, result.y, result.z);
				}
				else
				{
					Vector3 p3 = (p1 + p2) / 2;
					float error1 = VertexError(q, p1.x, p1.y, p1.z);
					float error2 = VertexError(q, p2.x, p2.y, p2.z);
					float error3 = VertexError(q, p3.x, p3.y, p3.z);

					error = MathF.Min(error1, MathF.Min(error2, error3));
					if (error1 == error) result = p1;
					if (error2 == error) result = p2;
					if (error3 == error) result = p3;
				}
			}
			else if (edgeInfo == ConnectedMesh.EdgeInfo.TShapeA)
			{
				result = p1;
				error = VertexError(q, p1.x, p1.y, p1.z);
			}
			else if (edgeInfo == ConnectedMesh.EdgeInfo.TShapeB)
			{
				result = p2;
				error = VertexError(q, p2.x, p2.y, p2.z);
			}
			else if (edgeInfo == ConnectedMesh.EdgeInfo.AShape)
			{
				error = 1000000;
			}
			else
			{
				Vector3 p1o = mesh.positions[mesh.nodes[otherNodeIndex1].position];
				Vector3 p2o = mesh.positions[mesh.nodes[otherNodeIndex2].position];

				var error1 = ComputeLineicError(p1, p2, p2o);
				var error2 = ComputeLineicError(p2, p1, p1o);
				error = MathF.Min(error1, error2);
				if (error1 == error) result = p1;
				if (error2 == error) result = p2;
			}

			// TODO : Ponderate error with edge length to collapse first shortest edges ?
			//error += Vector3.Magnitude(p2 - p1);

			pair.result = result;
			pair.error = error;
		}

		private float ComputeLineicError(Vector3 A, Vector3 B, Vector3 C) 
		{
			var θ = Vector3.Angle(B - A, C - A);
			var h = Vector3.Magnitude(B - A) * MathF.Sin(θ);

			return h;
		}

		private float ComputeLineicError(PairCollapse borderEdge)
		{
			var nodeIndex = mesh.PositionToNode[borderEdge.pos2];

			int relative = nodeIndex;
			while ((relative = mesh.nodes[relative].relative) != nodeIndex)
			{
				if (!mesh.IsEdgeManifold(relative, nodeIndex))
					goto next;
			}

			throw new Exception($"Pair '{borderEdge}' is not a border since pos2 is manifold");

		next:;

			Vector3 A = mesh.positions[borderEdge.pos1];
			Vector3 B = mesh.positions[borderEdge.pos2];
			Vector3 C = mesh.positions[mesh.nodes[relative].position];

			var θ = Vector3.Angle(B - A, C - A);
			var h = Vector3.Magnitude(B - A) * MathF.Sin(θ);

			return h;
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
					var pair = new PairCollapse { pos1 = posA, pos2 = posC };
					Debug.Assert(CheckMins());
					bool a = mins.Contains(pair);
					pairs.Remove(pair); // Todo : Optimization by only removing first pair (first edge)
					Debug.Assert(!(posA == 1487 && posC == 382));
					mins.Remove(pair);
					bool b = mins.Contains(pair);
					if (a == true)
						Debug.Assert(b == false);
					Debug.Assert(CheckMins());
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
					var pair = new PairCollapse { pos1 = posB, pos2 = posC };
					pairs.Remove(pair);
					RemoveMin(pair);
					Debug.Assert(CheckMins());
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

					if (validNode < 0)
						continue; 

					var pair = new PairCollapse { pos1 = posA, pos2 = posC };

					// Optimization by not adding a pair that has already been added
					if (pairs.Contains(pair))
						continue;

					Debug.Assert(CheckPair(pair));

					CalculateQuadric(posC);

					CalculateError(pair);

					pairs.Add(pair);
					AddMin(pair);
					Debug.Assert(CheckMins());
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

		public class PairCollapse : IComparable<PairCollapse>, IEquatable<PairCollapse>
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
				return Compare(this, pc) == 0;
			}

			public bool Equals(PairCollapse pc)
			{
				return Compare(this, pc) == 0;
			}

			public int CompareTo(PairCollapse other)
			{
				return Compare(this, other);
			}

			private static int Compare(PairCollapse x, PairCollapse y)
			{
				int lret = 0;
				if (Object.ReferenceEquals(x, y))
				{
					lret = 0;
				}
				else if (Object.ReferenceEquals(null, x))
				{
					lret = 1;
				}
				else if (Object.ReferenceEquals(null, y))
				{
					lret = -1;
				}
				else
				{
					lret = ((x.pos1 == y.pos1 && x.pos2 == y.pos2) || (x.pos1 == y.pos2 && x.pos2 == y.pos1)) ? 0 : x.error > y.error ? 1 : -1;
				}

				return lret;
			}

			public static bool operator ==(PairCollapse x, PairCollapse y)
			{
				return Compare(x, y) == 0;
			}

			public static bool operator !=(PairCollapse x, PairCollapse y)
			{
				return Compare(x, y) != 0; ;
			}

			public override string ToString()
			{
				return $"{pos1}-{pos2} error:{error}";
			}
		}

		private class PairComparer : IComparer<PairCollapse>
		{
			public int Compare(PairCollapse x, PairCollapse y)
			{
				return x.CompareTo(y);
			}
		}
	}
}