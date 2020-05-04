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

		const double offset_hard = 1e6;
		const double offset_nocollapse = 1e12;

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

			Console.WriteLine(pair.type);

#if DEBUG
			if (pair.error >= offset_nocollapse)
				Console.WriteLine("Going too far ! Destroying borders");
#endif

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

		private bool CheckPair(PairCollapse pair)
		{
			Debug.Assert(pair.posA != pair.posB, "Positions must be different");
			Debug.Assert(!mesh.nodes[mesh.PositionToNode[pair.posA]].IsRemoved, $"Position 1 is unreferenced {mesh.PositionToNode[pair.posA]}");
			Debug.Assert(!mesh.nodes[mesh.PositionToNode[pair.posB]].IsRemoved, $"Position 2 is unreferenced {mesh.PositionToNode[pair.posB]}");

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

				Vector3 normal;

				// Todo : Look for unsassign attribute instead, to handle cases where we have normals but not everywhere
				if (mesh.attributes.Length > 0)
				{
					normal = (Vector3)mesh.attributes[mesh.nodes[sibling].attribute].normal;
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
				}

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

			int node1 = mesh.PositionToNode[pair.posA];
			int node2 = mesh.PositionToNode[pair.posB];

			var edgeType = mesh.GetEdgeType(node1, node2);
#if DEBUG
			pair.type = edgeType;
#endif

			double error = 0;
			Vector3 result;

			Vector3 p1 = mesh.positions[pair.posA];
			Vector3 p2 = mesh.positions[pair.posB];

			switch (edgeType)
			{
				// Use quadric error to determine optimal vertex position only makes sense for manifold edges
				case IEdgeType.SURFACIC_HARD_AB edg_surfAB: // + offset
				case IEdgeType.SURFACIC edg_surf:
					{
						SymmetricMatrix q = matrices[pair.posA] + matrices[pair.posB];
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
						if (edgeType is IEdgeType.SURFACIC_HARD_AB)
							error += offset_hard;
					}
					break;
				case IEdgeType.SURFACIC_BORDER_A_HARD_B edg_surfbordAhardB: // + offset
				case IEdgeType.SURFACIC_HARD_A edg_surfhardA: // Todo : Check if hardness fades along edge or not
				case IEdgeType.SURFACIC_BORDER_A edg_surfbordA:
					{
						SymmetricMatrix q = matrices[pair.posA] + matrices[pair.posB];
						error = ComputeVertexError(q, p1.x, p1.y, p1.z);
						result = p1;
						//if (edgeType is IEdgeType.SURFACIC_BORDER_A_HARD_B)
						//	error += offset_hard;
					}
					break;
				case IEdgeType.SURFACIC_BORDER_B_HARD_A edg_surfbordBhardA: // + offset
				case IEdgeType.SURFACIC_HARD_B edg_surfhardB: // Todo : Check if hardness fades along edge or not
				case IEdgeType.SURFACIC_BORDER_B edg_surfbordB:
					{
						SymmetricMatrix q = matrices[pair.posA] + matrices[pair.posB];
						error = ComputeVertexError(q, p2.x, p2.y, p2.z);
						result = p2;
						//if (edgeType is IEdgeType.SURFACIC_BORDER_B_HARD_A)
						//	error += offset_hard;
					}
					break;
				case IEdgeType.BORDER_AB edg_bord:
					{
						// Todo : Improve quality by finding analytical solution that minimizes the error
						Vector3 p1o = mesh.positions[mesh.nodes[edg_bord.borderNodeA].position];
						Vector3 p2o = mesh.positions[mesh.nodes[edg_bord.borderNodeB].position];
						var error1 = ComputeLineicError(p1, p2, p2o);
						var error2 = ComputeLineicError(p2, p1, p1o);
						error = Math.Min(error1, error2);
						if (error1 == error) result = p1;
						else result = p2;
					}
					break;
				case IEdgeType.SURFACIC_BORDER_AB edg_bordAB:
					{
						// Todo : Put a warning when trying to collapse A-Shapes
						result = Vector3.Zero;
						error = offset_nocollapse; // Never collapse A-Shapes
					}
					break;
				default:
					{
						// Todo : Fix such cases. It should not happen
						result = Vector3.Zero;
						error = offset_nocollapse; // Never collapse unknown shapes
					}
					break;
			}

			// Ponderate error with edge length to collapse first shortest edges
			// Todo : Make it less sensitive to model scale
			error = Math.Abs(error) + εprio * Vector3.Magnitude(p2 - p1); 

			pair.result = result;
			pair.error = error;
		}

		private double ComputeLineicError(Vector3 A, Vector3 B, Vector3 C) 
		{
			var θ = Vector3.AngleRadians(B - A, C - A);
			return Vector3.Magnitude(B - A) * Math.Sin(θ);
		}

		private double ComputeVertexError(SymmetricMatrix q, double x, double y, double z)
		{
			return q[0] * x * x + 2 * q[1] * x * y + 2 * q[2] * x * z + 2 * q[3] * x
				 + q[4] * y * y + 2 * q[5] * y * z + 2 * q[6] * y
				 + q[7] * z * z + 2 * q[8] * z
				 + q[9];
		}

		private void InterpolateAttributes(PairCollapse pair)
		{
			int nodeIndexA = mesh.PositionToNode[pair.posA];
			int nodeIndexB = mesh.PositionToNode[pair.posB];

			int posA = pair.posA;
			int posB = pair.posB;

			Vector3 posAv = mesh.positions[posA];
			Vector3 posBv = mesh.positions[posB];
			Vector3 posCv = pair.result;

			int sibling = nodeIndexA;
			do
			{
				int B = mesh.nodes[mesh.nodes[sibling].relative].position;
				int C = mesh.nodes[mesh.nodes[mesh.nodes[sibling].relative].relative].position;

				Vector3F faceNormal1 = Vector3.Cross(
					mesh.positions[B] - posAv,
					mesh.positions[C] - posAv).Normalized;

				Vector3F faceNormal2 = Vector3.Cross(
					mesh.positions[B] - posCv,
					mesh.positions[C] - posCv).Normalized;

				Vector3F normal = mesh.attributes[mesh.nodes[sibling].attribute].normal;

				//if (Vector3F.Dot(faceNormal1, normal) < 0) faceNormal1 *= -1;
				//if (Vector3F.Dot(faceNormal2, normal) < 0) faceNormal2 *= -1;

				normal.x = MathUtils.DivideSafe(faceNormal2.x, faceNormal1.x) * normal.x;
				normal.y = MathUtils.DivideSafe(faceNormal2.y, faceNormal1.y) * normal.y;
				normal.z = MathUtils.DivideSafe(faceNormal2.z, faceNormal1.z) * normal.z;

				normal.Normalize();

				//mesh.attributes[mesh.nodes[sibling].attribute].normal = normal;

			} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndexA);

			sibling = nodeIndexB;
			do
			{
				int B = mesh.nodes[mesh.nodes[sibling].relative].position;
				int C = mesh.nodes[mesh.nodes[mesh.nodes[sibling].relative].relative].position;

				Vector3F faceNormal1 = Vector3.Cross(
					mesh.positions[B] - posBv,
					mesh.positions[C] - posBv).Normalized;

				Vector3F faceNormal2 = Vector3.Cross(
					mesh.positions[B] - posCv,
					mesh.positions[C] - posCv).Normalized;

				Vector3F normal = mesh.attributes[mesh.nodes[sibling].attribute].normal;

				//if (Vector3F.Dot(faceNormal1, normal) < 0) faceNormal1 *= -1;
				//if (Vector3F.Dot(faceNormal2, normal) < 0) faceNormal2 *= -1;

				normal.x = MathUtils.DivideSafe(faceNormal2.x, faceNormal1.x) * normal.x;
				normal.y = MathUtils.DivideSafe(faceNormal2.y, faceNormal1.y) * normal.y;
				normal.z = MathUtils.DivideSafe(faceNormal2.z, faceNormal1.z) * normal.z;

				normal.Normalize();

				//mesh.attributes[mesh.nodes[sibling].attribute].normal = normal;

			} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndexB);
		}

		private void CollapseEdge(PairCollapse pair)
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
					var pairAC = new PairCollapse(posA, posC);
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
					var pairBC = new PairCollapse(posB, posC);
					pairs.Remove(pairBC);
					mins.Remove(pairBC);
				}

			} while ((sibling = mesh.nodes[sibling].sibling) != nodeIndexB);

			// Interpolates attributes
			//InterpolateAttributes(pair);

			// Collapse edge
			int validNode = mesh.CollapseEdge(nodeIndexA, nodeIndexB);

			// A disconnected triangle has been collapsed, there are no edges to register
			if (validNode < 0)
				return;

			mesh.positions[posA] = pair.result;
			
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
					int sibling2 = relative;
					while ((sibling2 = mesh.nodes[sibling2].sibling) != relative)
					{
						int relative2 = sibling2;
						while ((relative2 = mesh.nodes[relative2].relative) != sibling2)
						{
							int posD = mesh.nodes[relative2].position;
							if (posD == posC)
								continue;
							if (pairs.TryGetValue(new PairCollapse(posC, posD), out PairCollapse actualPair))
							{
								mins.Remove(actualPair);
								CalculateQuadric(posD);
								CalculateError(actualPair);
								AddMin(actualPair);
							}
						}
					}

					if (validNode < 0)
						continue;

					var pairAC = new PairCollapse(posA, posC);

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

			// Normals (not the best way)
			sibling = validNode;
			do
			{
				mesh.attributes[mesh.nodes[sibling].attribute].normal = Vector3F.Zero;

			} while ((sibling = mesh.nodes[sibling].sibling) != validNode);

			sibling = validNode;
			do
			{
				int B = mesh.nodes[mesh.nodes[sibling].relative].position;
				int C = mesh.nodes[mesh.nodes[mesh.nodes[sibling].relative].relative].position;

				Vector3F faceNormal = Vector3.Cross(
					mesh.positions[B] - mesh.positions[posA],
					mesh.positions[C] - mesh.positions[posA]).Normalized;

				mesh.attributes[mesh.nodes[sibling].attribute].normal += faceNormal;

			} while ((sibling = mesh.nodes[sibling].sibling) != validNode);

			sibling = validNode;
			do
			{
				Vector3F normal = mesh.attributes[mesh.nodes[sibling].attribute].normal;
				normal.Normalize();
				mesh.attributes[mesh.nodes[sibling].attribute].normal = normal;

			} while ((sibling = mesh.nodes[sibling].sibling) != validNode);
		}
	}
}