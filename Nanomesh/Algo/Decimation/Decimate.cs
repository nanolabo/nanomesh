﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nanomesh
{
    public partial class DecimateModifier
    {
        public bool UpdateFarNeighbors = false;
        public bool UpdateMinsOnCollapse = true;
        public float MergeNormalsThreshold = MathF.Cos(30 * MathF.PI / 180f);

        private ConnectedMesh _mesh;

        private SymmetricMatrix[] _matrices;
        private FastHashSet<EdgeCollapse> _pairs;
        private LinkedHashSet<EdgeCollapse> _mins;

        private int _lastProgress = int.MinValue;
        private int _initialTriangleCount;
        private const double _ƐDET = 0.001f;
        private const double _ƐPRIO = 0.00001f;
        private const double _OFFSET_HARD = 1e6;
        private const double _OFFSET_NOCOLLAPSE = 1e300;

        public ConnectedMesh Mesh => _mesh;

        public void Initialize(ConnectedMesh mesh)
        {
            _mesh = mesh;

            _initialTriangleCount = mesh.FaceCount;

            _matrices = new SymmetricMatrix[mesh.positions.Length];
            _pairs = new FastHashSet<EdgeCollapse>();
            _mins = new LinkedHashSet<EdgeCollapse>();

            InitializePairs();

            for (int p = 0; p < _mesh.PositionToNode.Length; p++)
            {
                CalculateQuadric(p);
            }

            foreach (EdgeCollapse pair in _pairs)
            {
                CalculateError(pair);
            }
        }

        public void DecimateToError(float maximumError)
        {
            while (GetPairWithMinimumError().error <= maximumError && _pairs.Count > 0)
            {
                Iterate();
            }
        }

        public void DecimateToRatio(float targetTriangleRatio)
        {
            targetTriangleRatio = MathF.Clamp(targetTriangleRatio, 0f, 1f);
            DecimateToPolycount((int)MathF.Round(targetTriangleRatio * _mesh.FaceCount));
        }

        public void DecimatePolycount(int polycount)
        {
            DecimateToPolycount((int)MathF.Round(_mesh.FaceCount - polycount));
        }

        public void DecimateToPolycount(int targetTriangleCount)
        {
            while (_mesh.FaceCount > targetTriangleCount && _pairs.Count > 0)
            {
                Iterate();

                int progress = (int)MathF.Round(100f * (_initialTriangleCount - _mesh.FaceCount) / (_initialTriangleCount - targetTriangleCount));
                if (progress >= _lastProgress + 10)
                {
                    Console.WriteLine("Progress : " + progress + "%");
                    _lastProgress = progress;
                }
            }
        }

        public void Iterate()
        {
            EdgeCollapse pair = GetPairWithMinimumError();

            Debug.Assert(_mesh.CheckEdge(_mesh.PositionToNode[pair.posA], _mesh.PositionToNode[pair.posB]));
            Debug.Assert(pair.error < _OFFSET_NOCOLLAPSE, "Decimation is too aggressive");

            _pairs.Remove(pair);
            _mins.Remove(pair);

            CollapseEdge(pair);
        }

        public double GetMinimumError()
        {
            return GetPairWithMinimumError()?.error ?? double.PositiveInfinity;
        }

        private EdgeCollapse GetPairWithMinimumError()
        {
            if (_mins.Count == 0)
            {
                ComputeMins();
            }

            LinkedHashSet<EdgeCollapse>.LinkedHashNode<EdgeCollapse> edge = _mins.First;

            return edge?.Value;
        }

        private int MinsCount => MathF.Clamp(500, 0, _pairs.Count);

        private void ComputeMins()
        {
            _mins = new LinkedHashSet<EdgeCollapse>(_pairs.OrderBy(x => x).Take(MinsCount));
        }

        private void InitializePairs()
        {
            _pairs.Clear();
            _mins.Clear();

            for (int p = 0; p < _mesh.PositionToNode.Length; p++)
            {
                int nodeIndex = _mesh.PositionToNode[p];
                if (nodeIndex < 0)
                {
                    continue;
                }

                int sibling = nodeIndex;
                do
                {
                    int firstRelative = _mesh.nodes[sibling].relative;
                    int secondRelative = _mesh.nodes[firstRelative].relative;

                    EdgeCollapse pair = new EdgeCollapse(_mesh.nodes[firstRelative].position, _mesh.nodes[secondRelative].position);

                    _pairs.Add(pair);

                    Debug.Assert(_mesh.CheckEdge(_mesh.PositionToNode[pair.posA], _mesh.PositionToNode[pair.posB]));

                } while ((sibling = _mesh.nodes[sibling].sibling) != nodeIndex);
            }
        }

        private void CalculateQuadric(int position)
        {
            int nodeIndex = _mesh.PositionToNode[position];
            if (nodeIndex < 0) // TODO : Remove this check
            {
                return;
            }

            Debug.Assert(!_mesh.nodes[nodeIndex].IsRemoved);

            SymmetricMatrix symmetricMatrix = new SymmetricMatrix();

            int sibling = nodeIndex;
            do
            {
                Debug.Assert(_mesh.CheckRelatives(sibling));

                Vector3 faceNormal = _mesh.GetFaceNormal(sibling);
                double dot = Vector3.Dot(-faceNormal, _mesh.positions[_mesh.nodes[sibling].position]);
                symmetricMatrix += new SymmetricMatrix(faceNormal.x, faceNormal.y, faceNormal.z, dot);

            } while ((sibling = _mesh.nodes[sibling].sibling) != nodeIndex);

            _matrices[position] = symmetricMatrix;
        }

        private readonly HashSet<int> _adjacentEdges = new HashSet<int>(3);

        private IEnumerable<int> GetAdjacentPositions(int nodeIndex, int nodeAvoid)
        {
            _adjacentEdges.Clear();

            int posToAvoid = _mesh.nodes[nodeAvoid].position;

            int sibling = nodeIndex;
            do
            {
                for (int relative = sibling; (relative = _mesh.nodes[relative].relative) != sibling;)
                {
                    if (_mesh.nodes[relative].position != posToAvoid)
                    {
                        _adjacentEdges.Add(_mesh.nodes[relative].position);
                    }
                }
            } while ((sibling = _mesh.nodes[sibling].sibling) != nodeIndex);

            return _adjacentEdges;
        }

        private EdgeTopology GetEdgeTopo(EdgeCollapse edge)
        {
            if (edge.Topology == EdgeTopology.Undefined)
            {
                edge.SetTopology(_mesh.GetEdgeTopo(_mesh.PositionToNode[edge.posA], _mesh.PositionToNode[edge.posB]));
            }
            return edge.Topology;
        }

        private void CalculateError(EdgeCollapse pair)
        {
            Debug.Assert(_mesh.CheckEdge(_mesh.PositionToNode[pair.posA], _mesh.PositionToNode[pair.posB]));

            Vector3 posA = _mesh.positions[pair.posA];
            Vector3 posB = _mesh.positions[pair.posB];
            Vector3 posC = (posB + posA) / 2;

            int nodeA = _mesh.PositionToNode[pair.posA];
            int nodeB = _mesh.PositionToNode[pair.posB];

            EdgeTopology edgeTopo = GetEdgeTopo(pair);

            double errorCollapseToA = 0;
            double errorCollapseToB = 0;
            double errorCollapseToC = 0;
            double errorCollapseToO = 0;
            Vector3 posO = Vector3.PositiveInfinity;

            switch (edgeTopo)
            {
                case EdgeTopology.Border:
                case EdgeTopology.UvBreak:
                case EdgeTopology.HardEdge:
                case EdgeTopology.Surface:
                    {
                        // If a node is smooth (no hard edge connected, no uv break or no border), we can compute a quadric error
                        // Otherwise, we add up linear errors for every non smooth source.
                        // If both nodes of the edge are smooth, we can find the optimal position to collapse to by inverting the
                        // quadric matrix, otherwise, we pick the best between A, B, and the position in the middle, C.

                        SymmetricMatrix q = _matrices[pair.posA] + _matrices[pair.posB];
                        double det = q.DeterminantXYZ();

                        if (det > _ƐDET || det < -_ƐDET)
                        {
                            posO = new Vector3(
                                -1d / det * q.DeterminantX(),
                                +1d / det * q.DeterminantY(),
                                -1d / det * q.DeterminantZ());
                            errorCollapseToO = ComputeVertexError(q, pair.result.x, pair.result.y, pair.result.z);
                        }
                        else
                        {
                            errorCollapseToO = _OFFSET_NOCOLLAPSE;
                        }

                        errorCollapseToA = ComputeVertexError(q, posA.x, posA.y, posA.z);
                        errorCollapseToB = ComputeVertexError(q, posB.x, posB.y, posB.z);
                        errorCollapseToC = ComputeVertexError(q, posC.x, posC.y, posC.z);

                        int pA = _mesh.nodes[nodeA].position;
                        int pB = _mesh.nodes[nodeB].position;

                        // We multiply by edge length to be agnotics with quadrics error.
                        // Otherwise it becomes too scale dependent
                        double length = (posB - posA).Length;
                        double coeff_hard = 1.0 * length;
                        double coeff_uvs = 2.0 * length;
                        double coeff_border = 3.0 * length;

                        foreach (int pD in GetAdjacentPositions(nodeA, nodeB))
                        {
                            Vector3 posD = _mesh.positions[pD];
                            EdgeCollapse edge = new EdgeCollapse(pA, pD);
                            if (_pairs.TryGetValue(edge, out EdgeCollapse realEdge))
                            {
                                switch (GetEdgeTopo(realEdge))
                                {
                                    case EdgeTopology.Surface:
                                        break;
                                    case EdgeTopology.HardEdge:
                                        errorCollapseToO += coeff_hard * ComputeLineicError(posO, posD, posA);
                                        errorCollapseToB += coeff_hard * ComputeLineicError(posB, posD, posA);
                                        errorCollapseToC += coeff_hard * ComputeLineicError(posC, posD, posA);
                                        break;
                                    case EdgeTopology.UvBreak:
                                        errorCollapseToO += coeff_uvs * ComputeLineicError(posO, posD, posA);
                                        errorCollapseToB += coeff_uvs * ComputeLineicError(posB, posD, posA);
                                        errorCollapseToC += coeff_uvs * ComputeLineicError(posC, posD, posA);
                                        break;
                                    case EdgeTopology.Border:
                                        errorCollapseToO = _OFFSET_NOCOLLAPSE;
                                        errorCollapseToB += coeff_border * ComputeLineicError(posB, posD, posA);
                                        errorCollapseToC += coeff_border * ComputeLineicError(posC, posD, posA);
                                        break;
                                }
                            }
                        }

                        foreach (int pD in GetAdjacentPositions(nodeB, nodeA))
                        {
                            Vector3 posD = _mesh.positions[pD];
                            EdgeCollapse edge = new EdgeCollapse(pB, pD);
                            if (_pairs.TryGetValue(edge, out EdgeCollapse realEdge))
                            {
                                switch (GetEdgeTopo(realEdge))
                                {
                                    case EdgeTopology.Surface:
                                        break;
                                    case EdgeTopology.HardEdge:
                                        errorCollapseToO += coeff_hard * ComputeLineicError(posO, posD, posB);
                                        errorCollapseToA += coeff_hard * ComputeLineicError(posA, posD, posB);
                                        errorCollapseToC += coeff_hard * ComputeLineicError(posC, posD, posB);
                                        break;
                                    case EdgeTopology.UvBreak:
                                        errorCollapseToO += coeff_uvs * ComputeLineicError(posO, posD, posB);
                                        errorCollapseToA += coeff_uvs * ComputeLineicError(posA, posD, posB);
                                        errorCollapseToC += coeff_uvs * ComputeLineicError(posC, posD, posB);
                                        break;
                                    case EdgeTopology.Border:
                                        errorCollapseToO = _OFFSET_NOCOLLAPSE;
                                        errorCollapseToA += coeff_border * ComputeLineicError(posA, posD, posB);
                                        errorCollapseToC += coeff_border * ComputeLineicError(posC, posD, posB);
                                        break;
                                }
                            }
                        }

                        errorCollapseToC *= 0.5;

                        MathUtils.SelectMin(
                            errorCollapseToO, errorCollapseToA, errorCollapseToB, errorCollapseToC,
                            posO, posA, posB, posC,
                            out pair.error, out pair.result);

                        // Contrary to smooth edge, we know right away here that there will both nodes are not smooth
                        // We will still rely on quadrics if the only hard edge is the edge to collapse, but we add
                        // a penalty
                        //if (edgeTopo == EdgeTopology.HardEdge)
                        //{
                        //	pair.error *= 1;
                        //}
                    }
                    break;
            }

            pair.error = Math.Max(0d, pair.error);

            // TODO : Make it insensitive to model scale
            // TODO : Prevent flipping triangles
        }

        // TODO : Fix this (doesn't seems to work properly
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
                {
                    continue;
                }

                float dot = Vector3F.Dot(
                    Vector3F.Cross(_mesh.positions[posC] - positionA, _mesh.positions[posD] - positionA).Normalized,
                    Vector3F.Cross(_mesh.positions[posC] - edge.result, _mesh.positions[posD] - edge.result).Normalized);

                if (dot < -_ƐDET)
                {
                    return false;
                }
            } while ((sibling = _mesh.nodes[sibling].sibling) != nodeIndexA);

            sibling = nodeIndexB;
            do
            {
                int posC = _mesh.nodes[_mesh.nodes[sibling].relative].position;
                int posD = _mesh.nodes[_mesh.nodes[_mesh.nodes[sibling].relative].relative].position;

                if (posC == edge.posA || posD == edge.posA)
                {
                    continue;
                }

                float dot = Vector3F.Dot(
                    Vector3F.Cross(_mesh.positions[posC] - positionB, _mesh.positions[posD] - positionB).Normalized,
                    Vector3F.Cross(_mesh.positions[posC] - edge.result, _mesh.positions[posD] - edge.result).Normalized);

                if (dot < -_ƐDET)
                {
                    return false;
                }
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
            return q.m0 * x * x + 2 * q.m1 * x * y + 2 * q.m2 * x * z + 2 * q.m3 * x
                 + q.m4 * y * y + 2 * q.m5 * y * z + 2 * q.m6 * y
                 + q.m7 * z * z + 2 * q.m8 * z
                 + q.m9;
        }

        private void InterpolateAttributes(EdgeCollapse pair)
        {
            int posA = pair.posA;
            int posB = pair.posB;

            int nodeIndexA = _mesh.PositionToNode[posA];
            int nodeIndexB = _mesh.PositionToNode[posB];

            Vector3 positionA = _mesh.positions[posA];
            Vector3 positionB = _mesh.positions[posB];

            HashSet<int> procAttributes = new HashSet<int>();

            Vector3 positionN = pair.result;
            double AN = Vector3.Magnitude(positionA - positionN);
            double BN = Vector3.Magnitude(positionB - positionN);
            double ratio = MathUtils.DivideSafe(AN, AN + BN);

            /* // Other way (same results I think)
            double ratio = 0;
            double dot = Vector3.Dot(pair.result - positionA, positionB - positionA);
            if (dot > 0)
                ratio = Math.Sqrt(dot);
            ratio /= (positionB - positionA).Length;
			*/

            int siblingOfA = nodeIndexA;
            do // Iterator over faces around A
            {
                int relativeOfA = siblingOfA;
                do // Circulate around face
                {
                    if (_mesh.nodes[relativeOfA].position == posB)
                    {
                        if (procAttributes.Contains(_mesh.nodes[relativeOfA].attribute))
                        {
                            continue;
                        }

                        if (procAttributes.Contains(_mesh.nodes[siblingOfA].attribute))
                        {
                            continue;
                        }

                        // Normals
                        {
                            Vector3F normalAtA = _mesh.attributes[_mesh.nodes[siblingOfA].attribute].normal;
                            Vector3F normalAtB = _mesh.attributes[_mesh.nodes[relativeOfA].attribute].normal;

                            float dot = Vector3F.Dot(normalAtA, normalAtB);

                            if (dot >= MergeNormalsThreshold)
                            {
                                // TODO : Interpolate differently depending on pair type
                                normalAtA = ratio * normalAtB + (1 - ratio) * normalAtA;
                                normalAtA = normalAtA.Normalized;

                                _mesh.attributes[_mesh.nodes[siblingOfA].attribute].normal = normalAtA;
                                _mesh.attributes[_mesh.nodes[relativeOfA].attribute].normal = normalAtA;
                            }
                            else
                            {
                                _mesh.attributes[_mesh.nodes[siblingOfA].attribute].normal = normalAtA;
                                _mesh.attributes[_mesh.nodes[relativeOfA].attribute].normal = normalAtB;
                            }
                        }

                        // UVs
                        {
                            Vector2F uvAtA = _mesh.attributes[_mesh.nodes[siblingOfA].attribute].uv;
                            Vector2F uvAtB = _mesh.attributes[_mesh.nodes[relativeOfA].attribute].uv;

                            uvAtA = ratio * uvAtB + (1 - ratio) * uvAtA;

                            // TODO : Don't interpolate UVs of different islands
                            _mesh.attributes[_mesh.nodes[siblingOfA].attribute].uv = uvAtA;
                            _mesh.attributes[_mesh.nodes[relativeOfA].attribute].uv = uvAtA;
                        }

                        // Boneweights
                        unsafe
                        {
                            BoneWeight boneWeightA = _mesh.attributes[_mesh.nodes[siblingOfA].attribute].boneWeight;
                            BoneWeight boneWeightB = _mesh.attributes[_mesh.nodes[relativeOfA].attribute].boneWeight;

                            Dictionary<int, float> newBoneWeight = new Dictionary<int, float>();

                            // Map weights and indices
                            for (int i = 0; i < 4; i++)
                            {
                                newBoneWeight.TryAdd(boneWeightA.GetIndex(i), 0);
                                newBoneWeight.TryAdd(boneWeightB.GetIndex(i), 0);
                                newBoneWeight[boneWeightA.GetIndex(i)] += (float)((1 - ratio) * boneWeightA.GetWeight(i));
                                newBoneWeight[boneWeightB.GetIndex(i)] += (float)(ratio * boneWeightB.GetWeight(i));
                            }

                            int* newIndices = stackalloc int[4];
                            float* newWeights = stackalloc float[4];

                            // Order from biggest to smallest weight, and drop bones above 4th
                            float totalWeight = 0;
                            int k = 0;
                            foreach (KeyValuePair<int, float> boneWeightN in newBoneWeight.OrderByDescending(x => x.Value))
                            {
                                newIndices[k] = boneWeightN.Key;
                                newWeights[k] = boneWeightN.Value;
                                totalWeight += boneWeightN.Value;
                                if (k == 3)
                                {
                                    break;
                                }

                                k++;
                            }

                            // Normalize
                            if (totalWeight > 0)
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    newWeights[k] /= totalWeight;
                                }
                            }

                            boneWeightA = new BoneWeight(
                                newIndices[0], newIndices[1], newIndices[2], newIndices[3],
                                newWeights[0], newWeights[1], newWeights[2], newWeights[3]);

                            _mesh.attributes[_mesh.nodes[siblingOfA].attribute].boneWeight = boneWeightA;
                            _mesh.attributes[_mesh.nodes[relativeOfA].attribute].boneWeight = boneWeightA;
                        }

                        procAttributes.Add(_mesh.nodes[siblingOfA].attribute);
                        procAttributes.Add(_mesh.nodes[relativeOfA].attribute);

                        break;
                    }
                } while ((relativeOfA = _mesh.nodes[relativeOfA].relative) != siblingOfA);

            } while ((siblingOfA = _mesh.nodes[siblingOfA].sibling) != nodeIndexA);
        }

        private readonly Dictionary<Attribute, int> _uniqueAttributes = new Dictionary<Attribute, int>(new AttributeComparer(0.0001f));

        private void MergeAttributes(int nodeIndex)
        {
            _uniqueAttributes.Clear();

            int sibling = nodeIndex;
            do
            {
                _uniqueAttributes.TryAdd(_mesh.attributes[_mesh.nodes[sibling].attribute], _mesh.nodes[sibling].attribute);
            } while ((sibling = _mesh.nodes[sibling].sibling) != nodeIndex);

            sibling = nodeIndex;
            do
            {
                _mesh.nodes[sibling].attribute = _uniqueAttributes[_mesh.attributes[_mesh.nodes[sibling].attribute]];
            } while ((sibling = _mesh.nodes[sibling].sibling) != nodeIndex);
        }

        private readonly HashSet<EdgeCollapse> _edgeToRefresh = new HashSet<EdgeCollapse>();

        private void CollapseEdge(EdgeCollapse pair)
        {
            int nodeIndexA = _mesh.PositionToNode[pair.posA];
            int nodeIndexB = _mesh.PositionToNode[pair.posB];

            int posA = pair.posA;
            int posB = pair.posB;

            // Remove all edges around A
            int sibling = nodeIndexA;
            //for (relative = sibling; relative != sibling; relative = _mesh.nodes[relative].relative)
            //for (sibling = nodeIndexA; sibling != nodeIndexA; sibling = _mesh.nodes[sibling].sibling)
            do
            {
                for (int relative = sibling; (relative = _mesh.nodes[relative].relative) != sibling;)
                {
                    int posC = _mesh.nodes[relative].position;
                    EdgeCollapse pairAC = new EdgeCollapse(posA, posC);
                    // Todo : Optimization by only removing first pair (first edge)
                    if (_pairs.Remove(pairAC))
                    {
                        _mins.Remove(pairAC);
                    }
                }
            } while ((sibling = _mesh.nodes[sibling].sibling) != nodeIndexA);

            // Remove all edges around B
            sibling = nodeIndexB;
            do
            {
                for (int relative = sibling; (relative = _mesh.nodes[relative].relative) != sibling;)
                {
                    int posC = _mesh.nodes[relative].position;
                    EdgeCollapse pairBC = new EdgeCollapse(posB, posC);
                    if (_pairs.Remove(pairBC))
                    {
                        _mins.Remove(pairBC);
                    }
                }
            } while ((sibling = _mesh.nodes[sibling].sibling) != nodeIndexB);

            // Interpolates attributes
            InterpolateAttributes(pair);

            // Collapse edge ✨
            int validNode = _mesh.CollapseEdge(nodeIndexA, nodeIndexB);

            // A disconnected triangle has been collapsed, there are no edges to register
            if (validNode < 0)
            {
                return;
            }

            posA = _mesh.nodes[validNode].position;

            _mesh.positions[posA] = pair.result;

            MergeAttributes(validNode);

            CalculateQuadric(posA);

            _edgeToRefresh.Clear();

            sibling = validNode;
            do
            {
                for (int relative = sibling; (relative = _mesh.nodes[relative].relative) != sibling;)
                {
                    int posC = _mesh.nodes[relative].position;
                    _edgeToRefresh.Add(new EdgeCollapse(posA, posC));

                    if (UpdateFarNeighbors)
                    {
                        int sibling2 = relative;
                        while ((sibling2 = _mesh.nodes[sibling2].sibling) != relative)
                        {
                            int relative2 = sibling2;
                            while ((relative2 = _mesh.nodes[relative2].relative) != sibling2)
                            {
                                int posD = _mesh.nodes[relative2].position;
                                if (posD != posC)
                                {
                                    _edgeToRefresh.Add(new EdgeCollapse(posC, posD));
                                }
                            }
                        }
                    }
                }
            } while ((sibling = _mesh.nodes[sibling].sibling) != validNode);

            foreach (EdgeCollapse edge in _edgeToRefresh)
            {
                CalculateQuadric(edge.posB);
                edge.SetTopology(EdgeTopology.Undefined);
                _pairs.Remove(edge);
                _pairs.Add(edge);
            }

            foreach (EdgeCollapse edge in _edgeToRefresh)
            {
                CalculateError(edge);
                _mins.Remove(edge);
                if (UpdateMinsOnCollapse)
                {
                    _mins.AddMin(edge);
                }
            }
        }
    }
}