using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Index;
using NetTopologySuite.Simplify;
using NetTopologySuite.Utilities;
using System.Collections.Generic;

namespace NetTopologySuite.Coverage
{
    /// <summary>
    /// Computes a Topology-Preserving Visvalingnam-Whyatt simplification
    /// of a set of input lines.
    /// The simplified lines will contain no more intersections than are present
    /// in the original input, and line endpoints are preserved.
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
        /// <summary>
        /// Simplifies a set of lines, preserving the topology of the lines.
        /// </summary>
        /// <param name="lines">The lines to simplify</param>
        /// <param name="distanceTolerance">The simplification tolerance</param>
        /// <returns>The simplified lines</returns>
        public static MultiLineString Simplify(MultiLineString lines, double distanceTolerance)
        {
            var simp = new TPVWSimplifier(lines, distanceTolerance);
            var result = (MultiLineString)simp.Simplify();
            return result;
        }

        /// <summary>
        /// Simplifies a set of lines, preserving the topology of the lines between
        /// themselves and a set of linear constraints.
        /// </summary>
        /// <param name="lines">The lines to simplify</param>
        /// <param name="constraints">The linear constraints</param>
        /// <param name="distanceTolerance">The simplification tolerance</param>
        /// <returns>The simplified lines</returns>
        public static MultiLineString Simplify(MultiLineString lines,
            MultiLineString constraints, double distanceTolerance)
        {
            var simp = new TPVWSimplifier(lines, distanceTolerance) { 
                Constraints = constraints
            };
            var result = (MultiLineString)simp.Simplify();
            return result;
        }

        private readonly MultiLineString _input;
        private readonly double _areaTolerance;
        private readonly GeometryFactory _geomFactory;
        private MultiLineString _constraints = null;

        private TPVWSimplifier(MultiLineString lines, double distanceTolerance)
        {
            _input = lines;
            _areaTolerance = distanceTolerance * distanceTolerance;
            _geomFactory = _input.Factory;
        }

        private MultiLineString Constraints
        {
            get => _constraints;
            set => _constraints = value;
        }

        private Geometry Simplify()
        {
            var edges = CreateEdges(_input);
            var constraintEdges = CreateEdges(_constraints);

            var edgeIndex = new EdgeIndex();
            edgeIndex.Add(edges);
            edgeIndex.Add(constraintEdges);

            var result = new LineString[edges.Count];
            for (int i = 0; i < edges.Count; i++)
            {
                var edge = edges[i];
                var ptsSimp = edge.Simplify(edgeIndex);
                result[i] = _geomFactory.CreateLineString(ptsSimp);
            }
            return _geomFactory.CreateMultiLineString(result);
        }

        private List<Edge> CreateEdges(MultiLineString lines)
        {
            var edges = new List<Edge>();
            if (lines == null)
                return edges;
            for (int i = 0; i < lines.NumGeometries; i++)
            {
                var line = (LineString)lines.GetGeometryN(i);
                edges.Add(new Edge(line, _areaTolerance));
            }
            return edges;
        }

        private sealed class Edge
        {
            private readonly double _areaTolerance;
            private readonly LinkedLine _linkedLine;
            private readonly int _minEdgeSize;

            private readonly VertexSequencePackedRtree _vertexIndex;
            private readonly Envelope _envelope;

            public Edge(LineString inputLine, double areaTolerance)
            {
                _areaTolerance = areaTolerance;
                _envelope = inputLine.EnvelopeInternal;
                var pts = inputLine.Coordinates;
                _linkedLine = new LinkedLine(pts);
                _minEdgeSize = _linkedLine.IsRing ? 3 : 2;

                _vertexIndex = new VertexSequencePackedRtree(pts);
                //-- remove ring duplicate final vertex
                if (_linkedLine.IsRing)
                {
                    _vertexIndex.RemoveAt(pts.Length - 1);
                }
            }

            private Coordinate GetCoordinate(int index)
            {
                return _linkedLine.GetCoordinate(index);
            }

            public Envelope Envelope
            {
                get => _envelope;
            }

            public int Count
            {
                get => _linkedLine.Count;
            }

            public Coordinate[] Simplify(EdgeIndex edgeIndex)
            {
                var cornerQueue = CreateQueue();

                while (!cornerQueue.IsEmpty()
                    && Count > _minEdgeSize)
                {
                    var corner = cornerQueue.Poll();
                    //-- a corner may no longer be valid due to removal of adjacent corners
                    if (corner.IsRemoved)
                        continue;
                    //System.out.println(corner.toLineString(edge));
                    //-- done when all small corners are removed
                    if (corner.Area > _areaTolerance)
                        break;
                    if (IsRemovable(corner, edgeIndex))
                    {
                        RemoveCorner(corner, cornerQueue);
                    }
                }
                return _linkedLine.Coordinates;
            }

            private PriorityQueue<Corner> CreateQueue()
            {
                var cornerQueue = new PriorityQueue<Corner>();
                for (int i = 1; i < _linkedLine.Count - 1; i++)
                {
                    AddCorner(i, cornerQueue);
                }
                return cornerQueue;
            }

            private void AddCorner(int i, PriorityQueue<Corner> cornerQueue)
            {
                if (!_linkedLine.IsCorner(i))
                    return;
                var corner = new Corner(_linkedLine, i);
                if (corner.Area <= _areaTolerance)
                {
                    cornerQueue.Add(corner);
                }
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
                for (int i = 0; i < result.Length; i++)
                {
                    int index = result[i];

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

            private int[] Query(Envelope cornerEnv)
            {
                return _vertexIndex.Query(cornerEnv);
            }

            /// <summary>
            /// Removes a corner by removing the apex vertex from the ring.
            /// Two new corners are created with apexes
            /// at the other vertices of the corner
            /// (if they are non-convex and thus removable).
            /// </summary>
            /// <param name="corner">The corner to remove</param>
            /// <param name="cornerQueue">The corner queue</param>
            private void RemoveCorner(Corner corner, PriorityQueue<Corner> cornerQueue)
            {
                int index = corner.Index;
                int prev = _linkedLine.Prev(index);
                int next = _linkedLine.Next(index);
                _linkedLine.Remove(index);
                _vertexIndex.RemoveAt(index);

                //-- potentially add the new corners created
                AddCorner(prev, cornerQueue);
                AddCorner(next, cornerQueue);
            }

            public override string ToString()
            {
                return _linkedLine.ToString();
            }
        }

        private sealed class EdgeIndex
        {

            private readonly STRtree<Edge> _index = new STRtree<Edge>();

            public void Add(IList<Edge> edges)
            {
                foreach (var edge in edges)
                {
                    Add(edge);
                }
            }

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
