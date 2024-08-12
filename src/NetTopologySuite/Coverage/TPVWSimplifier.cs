using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Index;
using NetTopologySuite.Simplify;
using NetTopologySuite.Utilities;
using System.Collections.Generic;
using System.Collections;
using System;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Coverage
{
    /// <summary>
    /// Computes a Topology-Preserving Visvalingnam-Whyatt simplification
    /// of a set of input lines.
    /// The simplified lines will contain no more intersections than are present
    /// in the original input.
    /// Line and ring endpoints are preserved, except for rings
    /// which are flagged as "free".
    /// Rings which are smaller than the tolerance area
    /// may be removed entirely, as long as they are flagged as removable.
    /// <para/>
    /// The amount of simplification is determined by a tolerance value,
    /// which is a non-zero quantity.
    /// It is the square root of the area tolerance used
    /// in the Visvalingam-Whyatt algorithm.
    /// This equates roughly to the maximum
    /// distance by which a simplfied line can change from the original.
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class TPVWSimplifier
    {
        public static void Simplify(Edge[] edges,
            CornerArea cornerArea,
            double removableSizeFactor)
        {
            var simp = new TPVWSimplifier(edges);
            simp.CornerArea = cornerArea;
            simp.RemovableRingSizeFactor = removableSizeFactor;
            simp.Simplify();
        }

        private CornerArea _cornerArea;
        private double _removableSizeFactor = 1.0;
        private readonly Edge[] _edges;

        public TPVWSimplifier(Edge[] edges)
        {
            _edges = edges;
        }

        public double RemovableRingSizeFactor
        {
            get => _removableSizeFactor;
            set => _removableSizeFactor = value;
        }

        public CornerArea CornerArea
        {
            get => _cornerArea;
            set => _cornerArea = value;
        }

        private void Simplify()
        {
            var edgeIndex = new EdgeIndex();
            Add(_edges, edgeIndex);

            for (int i = 0 ; i < _edges.Length; i++) {
              var edge = _edges[i];
              edge.Simplify(_cornerArea, edgeIndex);
            }
        }

        private void Add(Edge[] edges, EdgeIndex edgeIndex)
        {
            foreach (var edge in edges) {
                //-- don't include removed edges in index
                edge.UpdateRemoved(_removableSizeFactor);
                if (!edge.IsRemoved)
                {
                    //-- avoid fluffing up removed edges
                    edge.Init();
                    edgeIndex.Add(edge);
                }
            }
        }

        internal sealed class Edge
        {
            private const int MIN_EDGE_SIZE = 2;
            private const int MIN_RING_SIZE = 4;

            private LinkedLine _linkedLine;
            private readonly bool _isFreeRing;
            private readonly int _nPts;
            private Coordinate[] _pts;
            private VertexSequencePackedRtree _vertexIndex;
            private readonly Envelope _envelope;
            private bool _isRemoved;
            private readonly bool _isRemovable;
            private readonly double _distanceTolerance;

            /// <summary>
            /// Creates a new edge.
            /// The endpoints of the edge are preserved during simplification,
            /// unless it is a ring and the <paramref name="isFreeRing"/> flag is set.
            /// </summary>
            /// <param name="pts"></param>
            /// <param name="distanceTolerance">The simplification tolerance</param>
            /// <param name="isFreeRing">A flag indiciating if a ring endpoint can be removed</param>
            /// <param name="isRemovable"></param>
            public Edge(Coordinate[] pts, double distanceTolerance, bool isFreeRing, bool isRemovable)
            {
                _envelope = CoordinateArrays.Envelope(pts);
                _pts = pts;
                _nPts = pts.Length;
                _isFreeRing = isFreeRing;
                _isRemovable = isRemovable;
                _distanceTolerance  = distanceTolerance;
            }

            public void UpdateRemoved(double removableSizeFactor)
            {
                if (!_isRemovable)
                    return;
                double areaTolerance = _distanceTolerance * _distanceTolerance;
                _isRemoved = CoordinateArrays.IsRing(_pts)
                    && Algorithm.Area.OfRing(_pts) < removableSizeFactor * areaTolerance;
            }

            public void Init()
            {
                _linkedLine = new LinkedLine(_pts);
            }

            public double Tolerance => _distanceTolerance;

            public bool IsRemoved =>  _isRemoved;

            private Coordinate GetCoordinate(int index)
            {
                return _pts[index];
            }

            public Coordinate[] Coordinates
            {
                get
                {
                    if (_isRemoved)
                        return Array.Empty<Coordinate>();
                    return _linkedLine.Coordinates;
                }
            }

            public Envelope Envelope
            {
                get => _envelope;
            }

            public int Count
            {
                get => _linkedLine.Count;
            }

            public void Simplify(CornerArea cornerArea, EdgeIndex edgeIndex)
            {
                if (_isRemoved)
                    return;

                //-- don't simplify
                if (_distanceTolerance <= 0.0)
                    return;

                double areaTolerance = _distanceTolerance * _distanceTolerance;
                int minEdgeSize = _linkedLine.IsRing ? MIN_RING_SIZE : MIN_EDGE_SIZE;

                var cornerQueue = CreateQueue(areaTolerance, cornerArea);

                while (!cornerQueue.IsEmpty()
                    && Count > minEdgeSize)
                {
                    var corner = cornerQueue.Poll();
                    //-- a corner may no longer be valid due to removal of adjacent corners
                    if (corner.IsRemoved)
                        continue;
                    //System.out.println(corner.toLineString(edge));
                    //-- done when all small corners are removed
                    if (corner.Area > areaTolerance)
                        break;
                    if (IsRemovable(corner, edgeIndex))
                    {
                        RemoveCorner(corner, areaTolerance, cornerArea, cornerQueue);
                    }
                }
            }

            private PriorityQueue<Corner> CreateQueue(double areaTolerance, CornerArea cornerArea)
            {
                var cornerQueue = new PriorityQueue<Corner>();
                int minIndex = (_linkedLine.IsRing && _isFreeRing) ? 0 : 1;
                int maxIndex = _nPts - 1;
                for (int i = minIndex; i < maxIndex; i++)
                {
                    AddCorner(i, areaTolerance, cornerArea, cornerQueue);
                }
                return cornerQueue;
            }

            private void AddCorner(int i, double areaTolerance, CornerArea cornerArea, PriorityQueue<Corner> cornerQueue)
            {
                //-- add if this vertex can be a corner
                if (_isFreeRing || (i != 0 && i != _nPts - 1))
                {
                    double area = Area(i, cornerArea);
                    if (area <= areaTolerance)
                    {
                        var corner = new Corner(_linkedLine, i, area);
                        cornerQueue.Add(corner);
                    }
                }
            }

            private double Area(int index, CornerArea cornerArea)
            {
                var pp = _linkedLine.PrevCoordinate(index);
                var p = _linkedLine.GetCoordinate(index);
                var pn = _linkedLine.NextCoordinate(index);
                return cornerArea.Area(pp, p, pn);
            }

            private bool IsRemovable(Corner corner, EdgeIndex edgeIndex)
            {
                var cornerEnv = corner.Envelope;
                //-- check nearby lines for violating intersections
                //-- the query also returns this line for checking
                foreach (var edge in edgeIndex.Query(cornerEnv))
                {
                    if (HasIntersectingVertex(corner, cornerEnv, edge))
                        return false;
                    //-- check if corner base equals line (2-pts)
                    //-- if so, don't remove corner, since that would collapse to the line
                    if (edge != this && edge.Count == 2)
                    {
                        var linePts = edge._linkedLine.Coordinates;
                        if (corner.IsBaseline(linePts[0], linePts[1]))
                            return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// Tests if any vertices in a line intersect the corner triangle.
            /// Uses the vertex spatial index for efficiency.
            /// </summary>
            /// <param name="corner">The corner vertices</param>
            /// <param name="cornerEnv">The envelope of the corner</param>
            /// <param name="edge">The edge to test</param>
            /// <returns><c>true</c> if there is an intersecting vertex</returns>
            private bool HasIntersectingVertex(Corner corner, Envelope cornerEnv,
                Edge edge)
            {
                int[] result = edge.Query(cornerEnv);
                foreach (int index in result)
                {
                    var v = edge.GetCoordinate(index);
                    // ok if corner touches another line - should only happen at endpoints
                    if (corner.IsVertex(v))
                        continue;

                    //--- does corner triangle contain vertex?
                    if (corner.Intersects(v))
                        return true;
                }
                return false;
            }

            private void InitIndex()
            {
                _vertexIndex = new VertexSequencePackedRtree(_pts);
                //-- remove ring duplicate final vertex
                if (CoordinateArrays.IsRing(_pts))
                {
                    _vertexIndex.RemoveAt(_pts.Length - 1);
                }
            }


            private int[] Query(Envelope cornerEnv)
            {
                if (_vertexIndex == null)
                    InitIndex();
                return _vertexIndex.Query(cornerEnv);
            }

            /// <summary>
            /// Removes a corner by removing the apex vertex from the ring.
            /// Two new corners are created with apexes
            /// at the other vertices of the corner
            /// (if they are non-convex and thus removable).
            /// </summary>
            /// <param name="corner">The corner to remove</param>
            /// <param name="areaTolerance"></param>
            /// <param name="cornerArea"></param>
            /// <param name="cornerQueue">The corner queue</param>
            private void RemoveCorner(Corner corner, double areaTolerance, CornerArea cornerArea, PriorityQueue<Corner> cornerQueue)
            {
                int index = corner.Index;
                int prev = _linkedLine.Prev(index);
                int next = _linkedLine.Next(index);
                _linkedLine.Remove(index);
                _vertexIndex.RemoveAt(index);

                //-- potentially add the new corners created
                AddCorner(prev, areaTolerance, cornerArea, cornerQueue);
                AddCorner(next, areaTolerance, cornerArea, cornerQueue);
            }

            public override string ToString()
            {
                return _linkedLine.ToString();
            }
        }

        internal sealed class EdgeIndex
        {
            private readonly STRtree<Edge> _index = new STRtree<Edge>();

            public void Add(Edge edge)
            {
                _index.Insert(edge.Envelope, edge);
            }

            public IList<Edge> Query(Envelope queryEnv)
            {
                return _index.Query(queryEnv);
            }
        }

    }
}
