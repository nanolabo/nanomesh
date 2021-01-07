using Nanomesh.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nanomesh
{
    public partial class DecimateModifier
    {
		public bool preciseMode = false;

		private ConnectedMesh _mesh;

		private SymmetricMatrix[] _matrices;
		private FastHashSet<EdgeCollapse> _pairs;
		private LinkedHashSet<EdgeCollapse> _mins = new LinkedHashSet<EdgeCollapse>();

		private int _lastProgress = int.MinValue;
		private int _initialTriangleCount;

		const double _ƐDET = 0.001f;
		const double _ƐPRIO = 0.00001f;

		const double _OFFSET_HARD = 1e6;
		const double _OFFSET_NOCOLLAPSE = 1e300;

		public static double Benchmark()
		{
			SharedMesh sharedMesh = PrimitiveUtils.CreateIcoSphere(1, 7);
			ConnectedMesh mesh = ConnectedMesh.Build(sharedMesh);

			NormalsModifier normals = new NormalsModifier();
			normals.Run(mesh, 30);

			double ms = Profiling.Time(() => {
				DecimateModifier decimateModifier = new DecimateModifier();
				decimateModifier.DecimateToRatio(mesh, 0.50f);
			}).TotalMilliseconds;

			//ExporterOBJ.Save(mesh.ToSharedMesh(), Environment.ExpandEnvironmentVariables("%UserProfile%/Desktop/Output.obj"));

			return ms;
		}

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

				int progress = (int)MathF.Round(100f * (_initialTriangleCount - mesh.FaceCount) / (_initialTriangleCount - targetTriangleCount));
				if (progress >= _lastProgress + 10)
				{
					Console.WriteLine("Progress : " + progress + "%");
					_lastProgress = progress;
				}
			}

			// TODO : mesh.Compact();
		}

		private void Initialize(ConnectedMesh mesh)
		{
			this._mesh = mesh;

			_initialTriangleCount = mesh.FaceCount;

			_matrices = new SymmetricMatrix[mesh.positions.Length];
			_pairs = new FastHashSet<EdgeCollapse>();
			_lastProgress = -1;

			InitializePairs();
			CalculateQuadrics();
			CalculateErrors();
		}

		private void Iterate()
		{
			EdgeCollapse pair = GetPairWithMinimumError();

			Debug.Assert(CheckMins());
			Debug.Assert(CheckPair(pair));

			_pairs.Remove(pair);
			_mins.Remove(pair);

#if DEBUG
			if (pair.error >= offset_nocollapse)
				Console.WriteLine("Going too far ! Destroying borders");
#endif
			//Console.WriteLine(pair);

			CollapseEdge(pair);
		}

		private bool CheckPairs()
		{
			foreach (var pair in _pairs)
			{
				CheckPair(pair);
			}

			return true;
		}

		private bool CheckMins()
		{
			foreach (var min in _mins)
			{
				Debug.Assert(_pairs.Contains(min));
			}

			return true;
		}

		private bool CheckPair(EdgeCollapse pair)
		{
			Debug.Assert(pair.posA != pair.posB, "Positions must be different");
			Debug.Assert(!_mesh.nodes[_mesh.PositionToNode[pair.posA]].IsRemoved, $"Position 1 is unreferenced {_mesh.PositionToNode[pair.posA]}");
			Debug.Assert(!_mesh.nodes[_mesh.PositionToNode[pair.posB]].IsRemoved, $"Position 2 is unreferenced {_mesh.PositionToNode[pair.posB]}");

			return true;
		}

		private EdgeCollapse GetPairWithMinimumError()
		{
			if (_mins.Count == 0)
				ComputeMins();

			var edge = _mins.First;

			return edge.Value;
		}

		private int MinsCount => (int)MathF.Clamp(0.01f * _mesh.faceCount + 50000, 0, _pairs.Count);

		private void ComputeMins()
		{
            MinHeap<EdgeCollapse> queue = new MinHeap<EdgeCollapse>(MinsCount);
            foreach (var pair in _pairs)
            {
                queue.Add(pair);
            }

			_mins = new LinkedHashSet<EdgeCollapse>(queue.Elements);
		}

		private void InitializePairs()
		{
			_pairs.Clear();
			_mins.Clear();

			for (int p = 0; p < _mesh.PositionToNode.Length; p++)
			{
				int nodeIndex = _mesh.PositionToNode[p];
				if (nodeIndex < 0)
					continue;

				int sibling = nodeIndex;
				do
				{
					int firstRelative = _mesh.nodes[sibling].relative;
					int secondRelative = _mesh.nodes[firstRelative].relative;

					var pair = new EdgeCollapse(_mesh.nodes[firstRelative].position, _mesh.nodes[secondRelative].position);

					_pairs.Add(pair);

					Debug.Assert(CheckPair(pair));

				} while ((sibling = _mesh.nodes[sibling].sibling) != nodeIndex);
			}
		}

		private void CalculateQuadrics()
		{
			for (int p = 0; p < _mesh.PositionToNode.Length; p++)
			{
				CalculateQuadric(p);
			}
		}

		private void CalculateQuadric(in int position)
		{
			int nodeIndex = _mesh.PositionToNode[position];
			if (nodeIndex < 0) // TODO : Remove this check
				return;

			Debug.Assert(!_mesh.nodes[nodeIndex].IsRemoved);

			SymmetricMatrix symmetricMatrix = new SymmetricMatrix();

			int sibling = nodeIndex;
			do
			{
				Debug.Assert(_mesh.CheckRelatives(sibling));

				Vector3 normal;

				// Todo : Look for unsassigned attribute instead, to handle cases where we have normals but not everywhere
				if (/*mesh.attributes.Length > 0*/ false)
				{
					normal = (Vector3)_mesh.attributes[_mesh.nodes[sibling].attribute].normal;

					int posA = _mesh.nodes[sibling].position;
					int posB = _mesh.nodes[_mesh.nodes[sibling].relative].position;
					int posC = _mesh.nodes[_mesh.nodes[_mesh.nodes[sibling].relative].relative].position;

					double area = 0.5 * Vector3.Cross(
						_mesh.positions[posB] - _mesh.positions[posA],
						_mesh.positions[posC] - _mesh.positions[posA]).Length;

					normal = normal.Normalized;

					normal = area * normal;
				}
				else
				{
					// Use triangle normal if there are no normals
					int posA = _mesh.nodes[sibling].position;
					int posB = _mesh.nodes[_mesh.nodes[sibling].relative].position;
					int posC = _mesh.nodes[_mesh.nodes[_mesh.nodes[sibling].relative].relative].position;

					normal = Vector3.Cross(
						_mesh.positions[posB] - _mesh.positions[posA],
						_mesh.positions[posC] - _mesh.positions[posA]);

					//double angle = Vector3.AngleRadians(mesh.positions[posB] - mesh.positions[posA], mesh.positions[posC] - mesh.positions[posA]);
					//double length = ((mesh.positions[posB] - mesh.positions[posA]).Length + (mesh.positions[posC] - mesh.positions[posA]).Length) / 2;

					//normal = normal.Normalized;
					normal = 0.5 * normal;
				}

				double dot = Vector3.Dot(-normal, _mesh.positions[_mesh.nodes[sibling].position]);

				symmetricMatrix += new SymmetricMatrix(normal.x, normal.y, normal.z, dot);

			} while ((sibling = _mesh.nodes[sibling].sibling) != nodeIndex);

			_matrices[position] = symmetricMatrix;
		}

		private void CalculateErrors()
		{
			var enumerator = _pairs.GetEnumerator();

			while (enumerator.MoveNext())
			{
				EdgeCollapse pair = enumerator.Current;
				CalculateError(pair);
			}
		}

		private void CalculateError(EdgeCollapse pair)
		{
			Debug.Assert(CheckPair(pair));

			int node1 = _mesh.PositionToNode[pair.posA];
			int node2 = _mesh.PositionToNode[pair.posB];

			_mesh.GetEdgeType(node1, node2, out IEdgeType edgeType);
			pair.type = edgeType;

			Vector3 posA = _mesh.positions[pair.posA];
			Vector3 posB = _mesh.positions[pair.posB];

			switch (edgeType)
			{
				// Use quadric error to determine optimal vertex position only makes sense for manifold edges
				case SURFACIC_HARD_EDGE edg_surfAB: // + offset
				case SURFACIC edg_surf:
					{
						SymmetricMatrix quadric = _matrices[pair.posA] + _matrices[pair.posB];
						double det = quadric.DeterminantXYZ();

						if (det > _ƐDET || det < -_ƐDET)
						{
							pair.result = new Vector3(
								-1d / det * quadric.DeterminantX(),
								+1d / det * quadric.DeterminantY(),
								-1d / det * quadric.DeterminantZ());
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
						SymmetricMatrix q = _matrices[pair.posA] + _matrices[pair.posB];
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
						SymmetricMatrix q = _matrices[pair.posA] + _matrices[pair.posB];
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
						SymmetricMatrix quadric = _matrices[pair.posA] + _matrices[pair.posB];
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
						pair.error = _OFFSET_NOCOLLAPSE - 1; // Never collapse, but still do it before A-Shapes
					}
					break;
				case SURFACIC_BORDER_AB edg_bordAB:
					{
						// Todo : Put a warning when trying to collapse A-Shapes
						pair.result = (posA + posB) / 2;
						pair.error = _OFFSET_NOCOLLAPSE; // Never collapse A-Shapes
					}
					break;
				default:
					{
						// Todo : Fix such cases. It should not happen
						pair.result = (posA + posB) / 2;
						pair.error = _OFFSET_NOCOLLAPSE; // Never collapse unknown shapes
					}
					break;
			}

			// Ponderate error with edge length to collapse first shortest edges
			// Todo : Make it less sensitive to model scale
			pair.error = Math.Abs(pair.error);// + εprio * Vector3.Magnitude(p2 - p1); 

			if (pair.error >= _OFFSET_NOCOLLAPSE && CollapseWillInvert(pair))
				pair.error = _OFFSET_NOCOLLAPSE;
		}

		public bool CollapseWillInvert(EdgeCollapse edge)
		{
			int nodeIndexA = _mesh.PositionToNode[edge.posA];
			int nodeIndexB = _mesh.PositionToNode[edge.posB];
			Vector3 positionA = _mesh.positions[edge.posA];
			Vector3 positionB = _mesh.positions[edge.posB];

			int sibling = nodeIndexA;
			do
			{
				int posC = _mesh.nodes[_mesh.nodes[sibling].relative].position;
				int posD = _mesh.nodes[_mesh.nodes[_mesh.nodes[sibling].relative].relative].position;

				if (posC == edge.posB || posD == edge.posB)
					continue;

				float dot = Vector3F.Dot(
					Vector3F.Cross(_mesh.positions[posC] - positionA, _mesh.positions[posD] - positionA).Normalized,
					Vector3F.Cross(_mesh.positions[posC] - edge.result, _mesh.positions[posD] - edge.result).Normalized);

				if (dot < -_ƐDET)
					return false;

			} while ((sibling = _mesh.nodes[sibling].sibling) != nodeIndexA);

			sibling = nodeIndexB;
			do
			{
				int posC = _mesh.nodes[_mesh.nodes[sibling].relative].position;
				int posD = _mesh.nodes[_mesh.nodes[_mesh.nodes[sibling].relative].relative].position;

				if (posC == edge.posA || posD == edge.posA)
					continue;

				float dot = Vector3F.Dot(
					Vector3F.Cross(_mesh.positions[posC] - positionB, _mesh.positions[posD] - positionB).Normalized,
					Vector3F.Cross(_mesh.positions[posC] - edge.result, _mesh.positions[posD] - edge.result).Normalized);

				if (dot < -_ƐDET)
					return false;

			} while ((sibling = _mesh.nodes[sibling].sibling) != nodeIndexB);

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
		private double ComputeLineicError(in Vector3 A, in Vector3 B, in Vector3 X) 
		{
			return Vector3.DistancePointLine(X, A, B);
		}

		private double ComputeVertexError(in SymmetricMatrix q, double x, double y, double z)
		{
			return q[0] * x * x + 2 * q[1] * x * y + 2 * q[2] * x * z + 2 * q[3] * x
				 + q[4] * y * y + 2 * q[5] * y * z + 2 * q[6] * y
				 + q[7] * z * z + 2 * q[8] * z
				 + q[9];
		}

		private void InterpolateAttributes(EdgeCollapse pair)
		{
			int nodeIndexA = _mesh.PositionToNode[pair.posA];
			int nodeIndexB = _mesh.PositionToNode[pair.posB];
			int posB = pair.posB;

			Vector3 positionA = _mesh.positions[pair.posA];
			Vector3 positionB = _mesh.positions[pair.posB];

			int siblingOfA = nodeIndexA;
			do // Iterator over faces around A
			{
				int relativeOfA = siblingOfA;
				do // Circulate around face
				{
					if (_mesh.nodes[relativeOfA].position == posB)
					{
						Vector3 positionN = pair.result;
						double AN = Vector3.Magnitude(positionA - positionN);
						double BN = Vector3.Magnitude(positionB - positionN);
						double ratio = (float)MathUtils.DivideSafe(AN, AN + BN);

						// Normals
						Vector3F normalAtA = _mesh.attributes[_mesh.nodes[siblingOfA].attribute].normal;
						Vector3F normalAtB = _mesh.attributes[_mesh.nodes[relativeOfA].attribute].normal;

						// Todo : Interpolate differently depending on pair type
						//normalAtA = ratio * normalAtA + (1 - ratio) * normalAtB;
						normalAtA = ratio * normalAtB + (1 - ratio) * normalAtA;
						//normalAtA = (normalAtA + normalAtA) / 2;
						normalAtA.Normalize();

						_mesh.attributes[_mesh.nodes[siblingOfA].attribute].normal = normalAtA;
						_mesh.attributes[_mesh.nodes[relativeOfA].attribute].normal = normalAtA;

						// UVs
						Vector2F uvAtA = _mesh.attributes[_mesh.nodes[siblingOfA].attribute].uv;
						Vector2F uvAtB = _mesh.attributes[_mesh.nodes[relativeOfA].attribute].uv;

						//uvAtA = ratio * uvAtA + (1 - ratio) * uvAtB;
						uvAtA = ratio * uvAtB + (1 - ratio) * uvAtA;
						//uvAtA = (uvAtA + uvAtB) / 2;

						_mesh.attributes[_mesh.nodes[siblingOfA].attribute].uv = uvAtA;
						_mesh.attributes[_mesh.nodes[relativeOfA].attribute].uv = uvAtA;

						break;
					}
				} while ((relativeOfA = _mesh.nodes[relativeOfA].relative) != siblingOfA);

			} while ((siblingOfA = _mesh.nodes[siblingOfA].sibling) != nodeIndexA);
		}

		private Dictionary<Vector3F, int> _normalToAttr = new Dictionary<Vector3F, int>(new Vector3FComparer(0.001f));

		private void MergeAttributes(in int nodeIndex)
		{
			_normalToAttr.Clear();

			int sibling = nodeIndex;
			do
			{
				_normalToAttr.TryAdd(_mesh.attributes[_mesh.nodes[sibling].attribute].normal, _mesh.nodes[sibling].attribute);
			} while ((sibling = _mesh.nodes[sibling].sibling) != nodeIndex);

			sibling = nodeIndex;
			do
			{
				_mesh.nodes[sibling].attribute = _normalToAttr[_mesh.attributes[_mesh.nodes[sibling].attribute].normal];
			} while ((sibling = _mesh.nodes[sibling].sibling) != nodeIndex);
		}

		private void CollapseEdge(EdgeCollapse pair)
		{
			int nodeIndexA = _mesh.PositionToNode[pair.posA];
			int nodeIndexB = _mesh.PositionToNode[pair.posB];

			int posA = pair.posA;
			int posB = pair.posB;

			// Remove all edges around A
			int sibling = nodeIndexA;
			int relative;
			do
			{
				relative = sibling;
				while ((relative = _mesh.nodes[relative].relative) != sibling)
				{
					int posC = _mesh.nodes[relative].position;
					var pairAC = new EdgeCollapse(posA, posC);
					_pairs.Remove(pairAC); // Todo : Optimization by only removing first pair (first edge)
					_mins.Remove(pairAC);
				} 

			} while ((sibling = _mesh.nodes[sibling].sibling) != nodeIndexA);

			// Remove all edges around B
			sibling = nodeIndexB;
			do
			{
				relative = sibling;
				while ((relative = _mesh.nodes[relative].relative) != sibling)
				{
					int posC = _mesh.nodes[relative].position;
					var pairBC = new EdgeCollapse(posB, posC);
					_pairs.Remove(pairBC);
					_mins.Remove(pairBC);
				}

			} while ((sibling = _mesh.nodes[sibling].sibling) != nodeIndexB);

			// Interpolates attributes
			InterpolateAttributes(pair);

			// Collapse edge
			int validNode = _mesh.CollapseEdge(nodeIndexA, nodeIndexB);

			// A disconnected triangle has been collapsed, there are no edges to register
			if (validNode < 0)
				return;

			_mesh.positions[posA] = pair.result;
			
			CalculateQuadric(posA); // Required ?

			MergeAttributes(validNode);

			// Recreate edges around new point and recompute collapse quadric errors
			sibling = validNode;
			do
			{
				relative = sibling;
				while ((relative = _mesh.nodes[relative].relative) != sibling)
				{
					int posC = _mesh.nodes[relative].position;

                    // Update quadrics and errors one level deeper
                    // Mathematically more correct, at the cost of performance
                    if (preciseMode)
					{
                        int sibling2 = relative;
                        while ((sibling2 = _mesh.nodes[sibling2].sibling) != relative)
                        {
                            int relative2 = sibling2;
                            while ((relative2 = _mesh.nodes[relative2].relative) != sibling2)
                            {
                                int posD = _mesh.nodes[relative2].position;
                                if (posD == posC)
                                    continue;

                                if (_pairs.TryGetValue(new EdgeCollapse(posC, posD), out EdgeCollapse actualPair))
                                {
                                    _mins.Remove(actualPair);
                                    CalculateQuadric(posD);
                                    CalculateError(actualPair);
                                    //AddMin(actualPair);
                                }
                            }
                        }
                    }

                    if (validNode < 0)
						continue;

					var pairAC = new EdgeCollapse(posA, posC);

					// Optimization by not adding a pair that has already been added
					if (_pairs.Contains(pairAC))
						continue;

					Debug.Assert(CheckPair(pairAC));

					CalculateQuadric(posC); // Required ? Shouldn't we keep original quadrics ?

					CalculateError(pairAC);

					_pairs.Add(pairAC);
					//AddMin(pairAC);
				}
			} while ((sibling = _mesh.nodes[sibling].sibling) != validNode);
		}
	}
}