using Nanomesh.Collections;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;

namespace Nanolabo
{
    public partial class DecimateModifier
    {
		private ConnectedMesh mesh;

		private SymmetricMatrix[] matrices;
		private HashSet<EdgeCollapse> pairs;
		private LinkedHashSet<EdgeCollapse> mins = new LinkedHashSet<EdgeCollapse>();

		private int lastProgress;
		private int initialTriangleCount;

		const double εdet = 0.001f;
		const double εprio = 0.00001f;

		const double offset_hard = 1e6;
		const double offset_nocollapse = 1e300;

		public void DecimateToRatio(ConnectedMesh mesh, float targetTriangleRatio)
		{
			targetTriangleRatio = MathF.Clamp(targetTriangleRatio, 0f, 1f);
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
			pairs = new HashSet<EdgeCollapse>();
			lastProgress = -1;

			InitializePairs();
			CalculateQuadrics();
			CalculateErrors();
		}

		private void Iterate()
		{
			EdgeCollapse pair = GetPairWithMinimumError();

			Debug.Assert(CheckMins());
			Debug.Assert(CheckPair(pair));

			pairs.Remove(pair);
			mins.Remove(pair);

#if DEBUG
			if (pair.error >= offset_nocollapse)
				Console.WriteLine("Going too far ! Destroying borders");
#endif	
			//Console.WriteLine(pair);

			CollapseEdge(pair);
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

		private bool CheckPair(EdgeCollapse pair)
		{
			Debug.Assert(pair.posA != pair.posB, "Positions must be different");
			Debug.Assert(!mesh.nodes[mesh.PositionToNode[pair.posA]].IsRemoved, $"Position 1 is unreferenced {mesh.PositionToNode[pair.posA]}");
			Debug.Assert(!mesh.nodes[mesh.PositionToNode[pair.posB]].IsRemoved, $"Position 2 is unreferenced {mesh.PositionToNode[pair.posB]}");

			return true;
		}

		private EdgeCollapse GetPairWithMinimumError()
		{
			if (mins.Count == 0)
				ComputeMins();

			var edge = mins.First;

			return edge.Value;
		}

		private int MinsCount => (int)MathF.Clamp(0.001f * mesh.faceCount + 500, 0, pairs.Count);

		private void ComputeMins()
		{
			//Console.WriteLine("Compute Mins");

			int minsCount = MinsCount;

			/// Using Linq
			//mins = new LinkedHashSet<EdgeCollapse>(pairs.OrderBy(x => x).Take(MinsCount)); // Todo : find faster sorting

			/// Using priority queue
			//FastPriorityQueue<FPQEdge> queue = new FastPriorityQueue<FPQEdge>(MinsCount);
			//foreach (var pair in pairs)
			//{
			//	queue.Enqueue(new FPQEdge(pair), (float)pair.error);
			//}
			//mins = new LinkedHashSet<EdgeCollapse>(queue.Select(x => x.edge));

			/// Using MinHeap
			//MinHeap<EdgeCollapse> queue = new MinHeap<EdgeCollapse>(MinsCount);
			//foreach (var pair in pairs)
			//{
			//	queue.Add(pair);
			//}
			//mins = new LinkedHashSet<EdgeCollapse>(queue.Elements);

			/// Using Manual Sorting
			//EdgeCollapse[] edges = new EdgeCollapse[mc];
			//foreach (var pair in pairs)
			//{
			//	int k = minsCount - 1;
			//	while (k >= 0 && (edges[k] == null || edges[k].CompareTo(pair) > 0))
			//	{
			//		var swap = edges[k];
			//		edges[k] = pair;
			//		if (k < minsCount - 1)
			//			edges[k + 1] = swap;
			//		k--;
			//	}
			//}
			//mins = new LinkedHashSet<EdgeCollapse>(edges);

			/// Using LinkedHashSet
			mins.Add(pairs.First());
			foreach (var pair in pairs)
			{
				if (mins.Count >= minsCount)
				{
					PushMin(pair);
				}
				else
				{
					AddMin(pair);
				}
			}
		}

		public class FPQEdge : FastPriorityQueueNode
		{
			public FPQEdge(EdgeCollapse edge)
			{
				this.edge = edge;
			}

			public EdgeCollapse edge;
		}

		private void AddMin(EdgeCollapse item)
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

		private void PushMin(EdgeCollapse item)
		{
			var current = mins.Last;
			while (current != null && item.CompareTo(current.Value) < 0)
			{
				current = current.Previous;
			}

			if (current == mins.Last)
				return;

			if (current == null)
				mins.PushBefore(item, mins.First);
			else
				mins.PushAfter(item, current);
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
					int firstRelative = mesh.nodes[sibling].relative;
					int secondRelative = mesh.nodes[firstRelative].relative;

					var pair = new EdgeCollapse(mesh.nodes[firstRelative].position, mesh.nodes[secondRelative].position);

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

				Vector3 normal;

				// Todo : Look for unsassigned attribute instead, to handle cases where we have normals but not everywhere
				if (/*mesh.attributes.Length > 0*/ false)
				{
					normal = (Vector3)mesh.attributes[mesh.nodes[sibling].attribute].normal;

					int posA = mesh.nodes[sibling].position;
					int posB = mesh.nodes[mesh.nodes[sibling].relative].position;
					int posC = mesh.nodes[mesh.nodes[mesh.nodes[sibling].relative].relative].position;

					double area = 0.5 * Vector3.Cross(
						mesh.positions[posB] - mesh.positions[posA],
						mesh.positions[posC] - mesh.positions[posA]).Length;

					normal.Normalize();

					normal = area * normal;
				}
				else
				{
					// Use triangle normal if there are no normals
					int posA = mesh.nodes[sibling].position;
					int posB = mesh.nodes[mesh.nodes[sibling].relative].position;
					int posC = mesh.nodes[mesh.nodes[mesh.nodes[sibling].relative].relative].position;

					normal = Vector3.Cross(
						mesh.positions[posB] - mesh.positions[posA],
						mesh.positions[posC] - mesh.positions[posA]);

					//double angle = Vector3.AngleRadians(mesh.positions[posB] - mesh.positions[posA], mesh.positions[posC] - mesh.positions[posA]);
					//double length = ((mesh.positions[posB] - mesh.positions[posA]).Length + (mesh.positions[posC] - mesh.positions[posA]).Length) / 2;
					double area = 0.5 * normal.Length;

					normal.Normalize();

					normal = area * normal;
				}

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
				EdgeCollapse pair = enumerator.Current;
				CalculateError(pair);
			}
		}

		private void CalculateError(EdgeCollapse pair)
		{
			Debug.Assert(CheckPair(pair));

			int node1 = mesh.PositionToNode[pair.posA];
			int node2 = mesh.PositionToNode[pair.posB];

			var edgeType = mesh.GetEdgeType(node1, node2);
			pair.type = edgeType;

			Vector3 posA = mesh.positions[pair.posA];
			Vector3 posB = mesh.positions[pair.posB];

			switch (edgeType)
			{
				// Use quadric error to determine optimal vertex position only makes sense for manifold edges
				case SURFACIC_HARD_EDGE edg_surfAB: // + offset
				case SURFACIC edg_surf:
					{
						SymmetricMatrix quadric = matrices[pair.posA] + matrices[pair.posB];
						double det = quadric.DeterminantXYZ();

						if (det > εdet || det < -εdet)
						{
							pair.result.x = -1d / det * quadric.DeterminantX();
							pair.result.y = +1d / det * quadric.DeterminantY();
							pair.result.z = -1d / det * quadric.DeterminantZ();
							pair.error = ComputeVertexError(quadric, pair.result.x, pair.result.y, pair.result.z);
						}
						else
						{
							// Not cool when it goes there...
							Vector3 posC = (posA + posB) / 2d;
							double error1 = ComputeVertexError(quadric, posA.x, posA.y, posA.z);
							double error2 = ComputeVertexError(quadric, posB.x, posB.y, posB.z);
							double error3 = ComputeVertexError(quadric, posC.x, posC.y, posC.z);
							pair.error = Math.Min(error1, Math.Min(error2, error3));
							if (error1 == pair.error) pair.result = posA;
							else if (error2 == pair.error) pair.result = posB;
							else pair.result = posC;
						}
						//if (edgeType is SURFACIC_HARD_EDGE)
						//	pair.error += offset_hard;
					}
					break;
				case SURFACIC_BORDER_A_HARD_B edg_surfbordAhardB: // + offset
				case SURFACIC_HARD_A edg_surfhardA:
				case SURFACIC_BORDER_A edg_surfbordA:
					{
						SymmetricMatrix q = matrices[pair.posA] + matrices[pair.posB];
						pair.error = ComputeVertexError(q, posA.x, posA.y, posA.z);
						pair.result = posA;
						//if (edgeType is SURFACIC_BORDER_A_HARD_B)
						//	pair.error += offset_hard;
					}
					break;
				case SURFACIC_BORDER_B_HARD_A edg_surfbordBhardA: // + offset
				case SURFACIC_HARD_B edg_surfhardB:
				case SURFACIC_BORDER_B edg_surfbordB:
					{
						SymmetricMatrix q = matrices[pair.posA] + matrices[pair.posB];
						pair.error = ComputeVertexError(q, posB.x, posB.y, posB.z);
						pair.result = posB;
						//if (edgeType is SURFACIC_BORDER_B_HARD_A)
						//	pair.error += offset_hard;
					}
					break;
				case BORDER_AB edg_bord:
					{
						/*
						// Todo : Improve quality by finding analytical solution that minimizes the error
						Vector3 borderA = mesh.positions[mesh.nodes[edg_bord.borderNodeA].position];
						Vector3 borderB = mesh.positions[mesh.nodes[edg_bord.borderNodeB].position];
						var error1 = ComputeLineicError(borderA, borderB, posA);
						var error2 = ComputeLineicError(borderA, borderB, posB);
						//var error1 = 0.1 * ComputeLineicError(p1o, p1, p2o);
						//var error2 = 0.1 * ComputeLineicError(p1o, p2, p2o);
						if (error1 < error2)
						{
							pair.error = error1;
							pair.result = posA;
						}
						else
						{
							pair.error = error2;
							pair.result = posB;
						}
						/*/
						SymmetricMatrix quadric = matrices[pair.posA] + matrices[pair.posB];
						Vector3 posC = (posA + posB) / 2d;
						double error1 = ComputeVertexError(quadric, posA.x, posA.y, posA.z);
						double error2 = ComputeVertexError(quadric, posB.x, posB.y, posB.z);
						double error3 = ComputeVertexError(quadric, posC.x, posC.y, posC.z) + 1000;
						pair.error = Math.Min(error1, Math.Min(error2, error3));
						if (error1 == pair.error) pair.result = posA;
						else if (error2 == pair.error) pair.result = posB;
						else pair.result = posC;
						//*/
					}
					break;
				case SURFACIC_HARD_AB edg_surfAB:
					{
						pair.result = (posA + posB) / 2;
						pair.error = offset_nocollapse - 1; // Never collapse, but still do it before A-Shapes
					}
					break;
				case SURFACIC_BORDER_AB edg_bordAB:
					{
						// Todo : Put a warning when trying to collapse A-Shapes
						pair.result = (posA + posB) / 2;
						pair.error = offset_nocollapse; // Never collapse A-Shapes
					}
					break;
				default:
					{
						// Todo : Fix such cases. It should not happen
						pair.result = (posA + posB) / 2;
						pair.error = offset_nocollapse; // Never collapse unknown shapes
					}
					break;
			}

			// Ponderate error with edge length to collapse first shortest edges
			// Todo : Make it less sensitive to model scale
			pair.error = Math.Abs(pair.error);// + εprio * Vector3.Magnitude(p2 - p1); 

			if (pair.error >= offset_nocollapse && CollapseWillInvert(pair))
				pair.error = offset_nocollapse;
		}

		public bool CollapseWillInvert(EdgeCollapse edge)
		{
			int nodeIndexA = mesh.PositionToNode[edge.posA];
			int nodeIndexB = mesh.PositionToNode[edge.posB];
			Vector3 positionA = mesh.positions[edge.posA];
			Vector3 positionB = mesh.positions[edge.posB];

			int sibling = nodeIndexA;
			do
			{
				int posC = mesh.nodes[mesh.nodes[sibling].relative].position;
				int posD = mesh.nodes[mesh.nodes[mesh.nodes[sibling].relative].relative].position;

				if (posC == edge.posB || posD == edge.posB)
					continue;

				float dot = Vector3F.Dot(
					Vector3F.Cross(mesh.positions[posC] - positionA, mesh.positions[posD] - positionA).Normalized,
					Vector3F.Cross(mesh.positions[posC] - edge.result, mesh.positions[posD] - edge.result).Normalized);

				if (dot < -εdet)
					return false;

			} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndexA);

			sibling = nodeIndexB;
			do
			{
				int posC = mesh.nodes[mesh.nodes[sibling].relative].position;
				int posD = mesh.nodes[mesh.nodes[mesh.nodes[sibling].relative].relative].position;

				if (posC == edge.posA || posD == edge.posA)
					continue;

				float dot = Vector3F.Dot(
					Vector3F.Cross(mesh.positions[posC] - positionB, mesh.positions[posD] - positionB).Normalized,
					Vector3F.Cross(mesh.positions[posC] - edge.result, mesh.positions[posD] - edge.result).Normalized);

				if (dot < -εdet)
					return false;

			} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndexB);

			return true;
		}

		/// <summary>
		/// A |\
		///   | \
		///   |__\ X
		///   |  /
		///   | /
		/// B |/
		/// </summary>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <param name="X"></param>
		/// <returns></returns>
		private double ComputeLineicError(Vector3 A, Vector3 B, Vector3 X) 
		{
			return Vector3.DistancePointLine(X, A, B);
		}

		private double ComputeVertexError(SymmetricMatrix q, double x, double y, double z)
		{
			return q[0] * x * x + 2 * q[1] * x * y + 2 * q[2] * x * z + 2 * q[3] * x
				 + q[4] * y * y + 2 * q[5] * y * z + 2 * q[6] * y
				 + q[7] * z * z + 2 * q[8] * z
				 + q[9];
		}

		private void InterpolateAttributes(EdgeCollapse pair)
		{
			int nodeIndexA = mesh.PositionToNode[pair.posA];
			int nodeIndexB = mesh.PositionToNode[pair.posB];
			int posB = pair.posB;

			Vector3 positionA = mesh.positions[pair.posA];
			Vector3 positionB = mesh.positions[pair.posB];

			int siblingOfA = nodeIndexA;
			do // Iterator over faces around A
			{
				int relativeOfA = siblingOfA;
				do // Circulate around face
				{
					if (mesh.nodes[relativeOfA].position == posB)
					{
						Vector3 positionN = pair.result;
						double AN = Vector3.Magnitude(positionA - positionN);
						double BN = Vector3.Magnitude(positionB - positionN);
						double ratio = (float)MathUtils.DivideSafe(AN, AN + BN);

						// Normals
						Vector3F normalAtA = mesh.attributes[mesh.nodes[siblingOfA].attribute].normal;
						Vector3F normalAtB = mesh.attributes[mesh.nodes[relativeOfA].attribute].normal;

						// Todo : Interpolate differently depending on pair type
						//normalAtA = ratio * normalAtA + (1 - ratio) * normalAtB;
						normalAtA = ratio * normalAtB + (1 - ratio) * normalAtA;
						//normalAtA = (normalAtA + normalAtA) / 2;
						normalAtA.Normalize();

						mesh.attributes[mesh.nodes[siblingOfA].attribute].normal = normalAtA;
						mesh.attributes[mesh.nodes[relativeOfA].attribute].normal = normalAtA;

						// UVs
						Vector2F uvAtA = mesh.attributes[mesh.nodes[siblingOfA].attribute].uv;
						Vector2F uvAtB = mesh.attributes[mesh.nodes[relativeOfA].attribute].uv;

						//uvAtA = ratio * uvAtA + (1 - ratio) * uvAtB;
						uvAtA = ratio * uvAtB + (1 - ratio) * uvAtA;
						//uvAtA = (uvAtA + uvAtB) / 2;

						mesh.attributes[mesh.nodes[siblingOfA].attribute].uv = uvAtA;
						mesh.attributes[mesh.nodes[relativeOfA].attribute].uv = uvAtA;

						break;
					}
				} while ((relativeOfA = mesh.nodes[relativeOfA].relative) != siblingOfA);

			} while ((siblingOfA = mesh.nodes[siblingOfA].sibling) != nodeIndexA);
		}

		private void MergeAttributes(int nodeIndex)
		{
			Dictionary<Vector3F, int> normalToAttr = new Dictionary<Vector3F, int>(new Vector3FComparer(0.001f));
			
			int sibling = nodeIndex;
			do
			{
				normalToAttr.TryAdd(mesh.attributes[mesh.nodes[sibling].attribute].normal, mesh.nodes[sibling].attribute);
			} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndex);

			sibling = nodeIndex;
			do
			{
				mesh.nodes[sibling].attribute = normalToAttr[mesh.attributes[mesh.nodes[sibling].attribute].normal];
			} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndex);
		}

		private void CollapseEdge(EdgeCollapse pair)
		{
			int nodeIndexA = mesh.PositionToNode[pair.posA];
			int nodeIndexB = mesh.PositionToNode[pair.posB];

			int posA = pair.posA;
			int posB = pair.posB;

			// Remove all edges around A
			int sibling = nodeIndexA;
			int relative;
			do
			{
				relative = sibling;
				while ((relative = mesh.nodes[relative].relative) != sibling)
				{
					int posC = mesh.nodes[relative].position;
					var pairAC = new EdgeCollapse(posA, posC);
					pairs.Remove(pairAC); // Todo : Optimization by only removing first pair (first edge)
					mins.Remove(pairAC);
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
					var pairBC = new EdgeCollapse(posB, posC);
					pairs.Remove(pairBC);
					mins.Remove(pairBC);
				}

			} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndexB);

			// Interpolates attributes
			InterpolateAttributes(pair);

			// Collapse edge
			int validNode = mesh.CollapseEdge(nodeIndexA, nodeIndexB);

			// A disconnected triangle has been collapsed, there are no edges to register
			if (validNode < 0)
				return;

			mesh.positions[posA] = pair.result;
			
			CalculateQuadric(posA); // Required ?

			MergeAttributes(validNode);

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
					//{
					//	int sibling2 = relative;
					//	while ((sibling2 = mesh.nodes[sibling2].sibling) != relative)
					//	{
					//		int relative2 = sibling2;
					//		while ((relative2 = mesh.nodes[relative2].relative) != sibling2)
					//		{
					//			int posD = mesh.nodes[relative2].position;
					//			if (posD == posC)
					//				continue;
					//			if (pairs.TryGetValue(new EdgeCollapse(posC, posD), out EdgeCollapse actualPair))
					//			{
					//				mins.Remove(actualPair);
					//				CalculateQuadric(posD);
					//				CalculateError(actualPair);
					//				AddMin(actualPair);
					//			}
					//		}
					//	}
					//}

					if (validNode < 0)
						continue;

					var pairAC = new EdgeCollapse(posA, posC);

					// Optimization by not adding a pair that has already been added
					if (pairs.Contains(pairAC))
						continue;

					Debug.Assert(CheckPair(pairAC));

					CalculateQuadric(posC); // Required ? Shouldn't we keep original quadrics ?

					CalculateError(pairAC);

					pairs.Add(pairAC);
					AddMin(pairAC);
				}
			} while ((sibling = mesh.nodes[sibling].sibling) != validNode);
		}
	}
}