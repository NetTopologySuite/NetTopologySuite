using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayNG;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Triangulate.QuadEdge;
using NetTopologySuite.Triangulate.Tri;
using NetTopologySuite.Utilities;
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Algorithm.Hull
{
    /// <summary>
    /// Constructs a concave hull of a set of points.
    /// The hull is constructed by removing the longest outer edges
    /// of the Delaunay Triangulation of the points
    /// until a target criterium is reached.
    /// <para/>
    /// The target criteria are:
    /// <list type="table">
    /// <item><term>Maximum Edge Length Ratio</term><description>the length of the longest edge of the hull is no larger
    /// than this value.</description></item>
    /// <item><term>Maximum Edge Length Factor</term><description>determine the Maximum Edge Length
    /// as a fraction of the difference between the longest and shortest edge lengths
    /// in the Delaunay Triangulation.
    /// This normalizes the <b>Maximum Edge Length</b> to be scale-free.
    /// A value of 1 produces the convex hull; a value of 0 produces maximum concaveness.</description></item>
    /// <item><term>Maximum Area Ratio</term><description>the ratio of the concave hull area to the convex hull area
    /// will be no larger than this value.
    /// A value of 1 produces the convex hull; a value of 0 produces maximum concaveness.</description></item>
    /// </list>
    /// The preferred criterium is the <b>Maximum Edge Length Ratio</b>, since it is
    /// scale-free and local(so that no assumption needs to be made about the
    /// total amount of concavity present).
    /// Other length criteria can be used by setting the Maximum Edge Length.
    /// For example, use a length relative to the longest edge length
    /// in the Minimum Spanning Tree of the point set.
    /// Or, use a length derived from the <see cref="UniformGridEdgeLength(Geometry)"/> value.
    /// <para/>
    /// The computed hull is always a single connected <see cref="Polygon"/>
    /// (unless it is degenerate, in which case it will be a <see cref="Point"/> or a <see cref="LineString"/>).
    /// This constraint may cause the concave hull to fail to meet the target criteria.
    /// <para/>
    /// Optionally the concave hull can be allowed to contain holes.
    /// Note that this may result in substantially slower computation,
    /// and it can produce results of lower quality.
    /// </summary>
    /// <author>Martin Davis</author>
    public class ConcaveHull
    {
        /// <summary>
        /// Computes the approximate edge length of
        /// a uniform square grid having the same number of
        /// points as a geometry and the same area as its convex hull.
        /// This value can be used to determine a suitable length threshold value
        /// for computing a concave hull.
        /// A value from 2 to 4 times the uniform grid length
        /// seems to produce reasonable results.
        /// </summary>
        /// <param name="geom">A geometry</param>
        /// <returns>The approximate uniform grid length</returns>
        public static double UniformGridEdgeLength(Geometry geom)
        {
            double areaCH = geom.ConvexHull().Area;
            int numPts = geom.NumPoints;
            return Math.Sqrt(areaCH / numPts);
        }

        /// <summary>
        /// Computes the concave hull of the vertices in a geometry
        /// using the target criteria of maximum edge length.
        /// </summary>
        /// <param name="geom">The input geometry</param>
        /// <param name="maxLength">The target maximum edge length</param>
        /// <returns>The concave hull</returns>
        public static Geometry ConcaveHullByLength(Geometry geom, double maxLength)
        {
            return ConcaveHullByLength(geom, maxLength, false);
        }

        /// <summary>
        /// Computes the concave hull of the vertices in a geometry
        /// using the target criteria of maximum edge length,
        /// and optionally allowing holes.
        /// </summary>
        /// <param name="geom">The input geometry</param>
        /// <param name="maxLength">The target maximum edge length</param>
        /// <param name="isHolesAllowed">A flag whether holes are allowed in the result</param>
        /// <returns>The concave hull</returns>
        public static Geometry ConcaveHullByLength(Geometry geom, double maxLength, bool isHolesAllowed)
        {
            var hull = new ConcaveHull(geom)
            {
                MaximumEdgeLength = maxLength,
                HolesAllowed = isHolesAllowed
            };
            return hull.GetHull();
        }

        /// <summary>
        /// Computes the concave hull of the vertices in a geometry
        /// using the target criteria of maximum edge length ratio.
        /// The edge length ratio is a fraction of the length difference
        /// between the longest and shortest edges
        /// in the Delaunay Triangulation of the input points. 
        /// </summary>
        /// <param name="geom">The input geometry</param>
        /// <param name="lengthRatio">The target edge length ratio</param>
        /// <returns>The concave hull</returns>
        public static Geometry ConcaveHullByLengthRatio(Geometry geom, double lengthRatio)
        {
            return ConcaveHullByLengthRatio(geom, lengthRatio, false);
        }

        /// <summary>
        /// Computes the concave hull of the vertices in a geometry
        /// using the target criteria of maximum edge length ratio,
        /// and optionally allowing holes.
        /// The edge length factor is a fraction of the length difference
        /// between the longest and shortest edges
        /// in the Delaunay Triangulation of the input points. 
        /// </summary>
        /// <param name="geom">The input geometry</param>
        /// <param name="lengthRatio">The target edge length ratio</param>
        /// <param name="isHolesAllowed">A flag whether holes are allowed in the result</param>
        /// <returns>The concave hull</returns>
        public static Geometry ConcaveHullByLengthRatio(Geometry geom, double lengthRatio, bool isHolesAllowed)
        {
            var hull = new ConcaveHull(geom)
            {
                MaximumEdgeLengthRatio = lengthRatio,
                HolesAllowed = isHolesAllowed
            };
            return hull.GetHull();
        }

        /// <summary>
        /// Computes the concave hull of the vertices in a geometry
        /// using the target criteria of maximum area ratio.
        /// </summary>
        /// <param name="geom">The input geometry</param>
        /// <param name="areaRatio">The target maximum area ratio</param>
        /// <returns>The concave hull</returns>
        public static Geometry ConcaveHullByArea(Geometry geom, double areaRatio)
        {
            var hull = new ConcaveHull(geom)
            {
                MaximumAreaRatio = areaRatio
            };
            return hull.GetHull();
        }

        private readonly Geometry _inputGeometry;
        private double _maxEdgeLength = 0.0;
        private double _maxEdgeLengthRatio = -1;
        private double _maxAreaRatio = 0.0;
        private bool _isHolesAllowed = false;
        private readonly GeometryFactory _geomFactory;


        /// <summary>
        /// Creates a new instance for a given geometry.
        /// </summary>
        /// <param name="geom">The input geometry</param>
        public ConcaveHull(Geometry geom)
        {
            _inputGeometry = geom;
            _geomFactory = geom.Factory;
        }

        /// <summary>Gets or sets the target maximum edge length for the concave hull.
        /// The length value must be zero or greater.
        /// <list type="bullet">
        /// <item><description>The value 0.0 produces the concave hull of smallest area
        /// that is still connected.</description></item>
        /// <item><description>Larger values produce less concave results.
        /// A value equal or greater than the longest Delaunay Triangulation edge length
        /// produces the convex hull.</description></item>
        /// </list>
        /// The <see cref="UniformGridEdgeLength(Geometry)"/> value may be used as
        /// the basis for estimating an appropriate target maximum edge length.
        /// </summary>
        /// <seealso cref="UniformGridEdgeLength(Geometry)"/>
        /// <returns>The target maximum edge length for the concave hull</returns>
        public double MaximumEdgeLength
        {
            get => _maxEdgeLength;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Edge length must be non-negative");
                _maxEdgeLength = value;
                _maxEdgeLengthRatio = -1;
            }
        }

        /// <summary>
        /// Gets or sets the target maximum edge length ratio for the concave hull.
        /// The edge length ratio is a fraction of the difference
        /// between the longest and shortest edge lengths
        /// in the Delaunay Triangulation of the input points.
        /// It is a value in the range 0 to 1.
        /// <list type="bullet">
        /// <item><description>The value 0.0 produces a concave hull of minimum area
        /// that is still connected.</description></item>
        /// <item><description>The value 1.0 produces the convex hull.</description></item>
        /// </list> 
        /// </summary>
        /// <returns>The target maximum edge length factor for the concave hull</returns>
        public double MaximumEdgeLengthRatio
        {
            get => _maxEdgeLengthRatio;

            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException(nameof(value), "Edge length ratio must be in range [0,1]e");
                _maxEdgeLengthRatio = value;
            }
        }

        /// <summary>
        /// Gets or sets the target maximum concave hull area as a ratio of the convex hull area.
        /// It is a value in the range 0 to 1.
        /// <list>
        /// <item><description>The value 0.0 produces a concave hull with the smallest area
        /// that is still connected.</description></item>
        /// <item><description>The value 1.0 produces the convex hull
        /// (unless a maximum edge length is also specified).</description></item>
        /// </list>
        /// </summary>
        public double MaximumAreaRatio
        {
            get => _maxAreaRatio;
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException(nameof(value), "Area ratio must be in range [0,1]");
                _maxAreaRatio = value;
            }
        }

        /// <summary>
        /// Gets or sets whether holes are allowed in the concave hull polygon.
        /// </summary>
        public bool HolesAllowed
        {
            get => _isHolesAllowed;
            set => _isHolesAllowed = value;
        }

        /// <summary>
        /// Gets the computed concave hull.
        /// </summary>
        /// <returns>The concave hull</returns>
        public Geometry GetHull()
        {
            if (_inputGeometry.IsEmpty)
            {
                return _geomFactory.CreatePolygon();
            }
            var triList = CreateDelaunayTriangulation(_inputGeometry);
            if (_maxEdgeLengthRatio >= 0)
            {
                _maxEdgeLength = ComputeTargetEdgeLength(triList, _maxEdgeLengthRatio);
            }
            if (triList.Count == 0)
                return _inputGeometry.ConvexHull();
            ComputeHull(triList);
            var hull = ToPolygon(triList, _geomFactory);
            return hull;
        }

        private static double ComputeTargetEdgeLength(IList<HullTri> triList,
            double edgeLengthRatio)
        {
            if (edgeLengthRatio == 0) return 0;
            double maxEdgeLen = -1;
            double minEdgeLen = -1;
            foreach (var tri in triList)
            {
                for (int i = 0; i < 3; i++)
                {
                    double len = tri.GetCoordinate(i).Distance(tri.GetCoordinate(Tri.Next(i)));
                    if (len > maxEdgeLen)
                        maxEdgeLen = len;
                    if (minEdgeLen < 0 || len < minEdgeLen)
                        minEdgeLen = len;
                }
            }
            //-- if ratio = 1 ensure all edges are included
            if (edgeLengthRatio == 1)
                return 2 * maxEdgeLen;

            return edgeLengthRatio * (maxEdgeLen - minEdgeLen) + minEdgeLen;
        }

        private void ComputeHull(IList<HullTri> triList)
        {
            //-- used if area is the threshold criteria
            double areaConvex = Tri.AreaOf(triList);
            double areaConcave = areaConvex;

            var queue = InitQueue(triList);
            // remove tris in order of decreasing size (edge length)
            while (!queue.IsEmpty())
            {
                if (IsBelowAreaThreshold(areaConcave, areaConvex))
                    break;

                var tri = queue.Poll();

                if (IsBelowLengthThreshold(tri))
                    break;

                if (IsRemovable(tri, triList))
                {
                    //-- the non-null adjacents are now on the border
                    var adj0 = (HullTri)tri.GetAdjacent(0);
                    var adj1 = (HullTri)tri.GetAdjacent(1);
                    var adj2 = (HullTri)tri.GetAdjacent(2);

                    //-- remove tri
                    tri.Remove();
                    triList.Remove(tri);
                    areaConcave -= tri.Area;

                    //-- if holes not allowed, add new border adjacents to queue
                    if (!_isHolesAllowed)
                    {
                        AddBorderTri(adj0, queue);
                        AddBorderTri(adj1, queue);
                        AddBorderTri(adj2, queue);
                    }
                }
            }
        }

        private PriorityQueue<HullTri> InitQueue(IList<HullTri> triList)
        {
            var queue = new PriorityQueue<HullTri>();
            foreach (var tri in triList)
            {
                if (!_isHolesAllowed)
                {
                    //-- add only border triangles which could be eroded
                    // (if tri has only 1 adjacent it can't be removed because that would isolate a vertex)
                    if (tri.NumAdjacent != 2)
                        continue;
                    tri.SetSizeToBorder();
                }
                queue.Add(tri);
            }
            return queue;
        }

        /// <summary>
        /// Adds a Tri to the queue.
        /// Only add tris with a single border edge.
        /// The ordering size is the length of the border edge.
        /// </summary>
        /// <param name="tri">The Tri to add</param>
        /// <param name="queue">The priority queue</param>
        private void AddBorderTri(HullTri tri, PriorityQueue<HullTri> queue)
        {
            if (tri == null) return;
            if (tri.NumAdjacent != 2) return;
            tri.SetSizeToBorder();
            queue.Add(tri);
        }

        private bool IsBelowAreaThreshold(double areaConcave, double areaConvex)
        {
            return areaConcave / areaConvex <= _maxAreaRatio;
        }

        private bool IsBelowLengthThreshold(HullTri tri)
        {
            double len;
            if (_isHolesAllowed)
            {
                len = tri.LengthOfLongestEdge();
            }
            else
            {
                len = tri.LengthOfBorder();
            }
            return len < _maxEdgeLength;
        }

        /// <summary>
        /// Tests whether a Tri can be removed while preserving
        /// the connectivity of the hull.
        /// </summary>
        /// <param name="tri">The Tri to test</param>
        /// <param name="triList">A triangulation</param>
        /// <returns><c>true</c> if the Tri can be removed</returns>
        private bool IsRemovable(HullTri tri, IList<HullTri> triList)
        {
            if (_isHolesAllowed)
            {
                /*
                 * Don't remove if that would separate a single vertex
                 */
                if (HasVertexSingleAdjacent(tri, triList))
                    return false;
                return HullTri.IsConnected(triList, tri);
            }

            //-- compute removable for no holes allowed
            int numAdj = tri.NumAdjacent;
            /*
             * Tri must have exactly 2 adjacent tris.
             * If it it has only 0 or 1 adjacent then removal would remove a vertex.
             * If it has 3 adjacent then it is not on border.
             */
            if (numAdj != 2) return false;
            /*
             * The tri cannot be removed if it is connecting, because
             * this would create more than one result polygon.
             */
            return !IsConnecting(tri);
        }

        private static bool HasVertexSingleAdjacent(HullTri tri, IList<HullTri> triList)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Degree(tri.GetCoordinate(i), triList) <= 1)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// The degree of a Tri vertex is the number of tris containing it.
        /// This must be done by searching the entire triangulation,
        /// since the containing tris may not be adjacent or edge-connected. 
        /// </summary>
        /// <param name="v">A vertex coordinate</param>
        /// <param name="triList">A triangulation</param>
        /// <returns>The degree of the vertex</returns>
        private static int Degree(Coordinate v, IList<HullTri> triList)
        {
            int degree = 0;
            foreach (var tri in triList)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (v.Equals2D(tri.GetCoordinate(i)))
                        degree++;
                }
            }
            return degree;
        }

        /// <summary>
        /// Tests if a tri is the only one connecting its 2 adjacents.
        /// Assumes that the tri is on the border of the triangulation
        /// and that the triangulation does not contain holes
        /// </summary>
        /// <param name="tri">The tri to test</param>
        /// <returns><c>true</c> if the tri is the only connection</returns>
        private static bool IsConnecting(Tri tri)
        {
            int adj2Index = Adjacent2VertexIndex(tri);
            bool isInterior = IsInteriorVertex(tri, adj2Index);
            return !isInterior;
        }

        /// <summary>
        /// A vertex of a triangle is interior if it
        /// is fully surrounded by triangles.
        /// </summary>
        /// <param name="triStart">A tri containing the vertex</param>
        /// <param name="index">The vertex index</param>
        /// <returns><c>true</c> if the vertex is interior</returns>
        private static bool IsInteriorVertex(Tri triStart, int index)
        {
            var curr = triStart;
            int currIndex = index;
            do
            {
                var adj = curr.GetAdjacent(currIndex);
                if (adj == null) return false;
                int adjIndex = adj.GetIndex(curr);
                curr = adj;
                currIndex = Tri.Next(adjIndex);
            }
            while (curr != triStart);
            return true;
        }

        private static int Adjacent2VertexIndex(Tri tri)
        {
            if (tri.HasAdjacent(0) && tri.HasAdjacent(1)) return 1;
            if (tri.HasAdjacent(1) && tri.HasAdjacent(2)) return 2;
            if (tri.HasAdjacent(2) && tri.HasAdjacent(0)) return 0;
            return -1;
        }

        private class HullTri : Tri, IComparable<HullTri>
        {
            public HullTri(Coordinate p0, Coordinate p1, Coordinate p2)
                    : base(p0, p1, p2)
            {
                Size = LengthOfLongestEdge();
            }

            public double Size { get; set; }

            /// <summary>
            /// Sets the size to be the length of the border edges.
            /// This is used when constructing hull without holes,
            /// by erosion from the triangulation border.
            /// </summary>
            public void SetSizeToBorder()
            {
                Size = LengthOfBorder();
            }

            public bool IsMarked { get; set; }

            public bool IsBorder() => IsBorder(0) || IsBorder(1) || IsBorder(2);


            public bool IsBorder(int index)
            {
                return !HasAdjacent(index);
            }

            public int BorderIndex
            {
                get
                {
                    if (IsBorder(0)) return 0;
                    if (IsBorder(1)) return 1;
                    if (IsBorder(2)) return 2;
                    return -1;
                }
            }

            /// <summary>
            /// Gets the most CCW border edge index.
            /// This assumes there is at least one non-border edge.
            /// </summary>
            /// <returns>The CCW border edge index</returns>
            public int BorderIndexCCW
            {
                get
                {
                    int index = BorderIndex;
                    int prevIndex = Prev(index);
                    if (IsBorder(prevIndex))
                    {
                        return prevIndex;
                    }
                    return index;
                }
            }

            /// <summary>
            /// Gets the most CW border edge index.
            /// This assumes there is at least one non-border edge.
            /// </summary>
            /// <returns>The CW border edge index</returns>
            public int BorderIndexCW
            {
                get
                {
                    int index = BorderIndex;
                    int nextIndex = Next(index);
                    if (IsBorder(nextIndex))
                    {
                        return nextIndex;
                    }
                    return index;
                }
            }

            public double LengthOfLongestEdge()
            {
                return Triangle.LongestSideLength(P0, P1, P2);
            }

            public double LengthOfBorder()
            {
                double len = 0.0;
                for (int i = 0; i < 3; i++)
                {
                    if (!HasAdjacent(i))
                    {
                        len += GetCoordinate(i).Distance(GetCoordinate(Tri.Next(i)));
                    }
                }
                return len;
            }

            public HullTri NextBorderTri()
            {
                var tri = this;
                //-- start at first non-border edge CW
                int index = Next(BorderIndexCW);
                //-- scan CCW around vertex for next border tri
                do
                {
                    var adjTri = (HullTri)tri.GetAdjacent(index);
                    if (adjTri == this)
                        throw new InvalidOperationException("No outgoing border edge found");
                    index = Next(adjTri.GetIndex(tri));
                    tri = adjTri;
                }
                while (!tri.IsBorder(index));
                return (tri);
            }

            /// <summary>
            /// PriorityQueues sort in ascending order.
            /// To sort with the largest at the head,
            /// smaller sizes must compare as greater than larger sizes.
            /// (i.e. the normal numeric comparison is reversed).
            /// If the sizes are identical (which should be an infrequent case),
            /// the areas are compared, with larger areas sorting before smaller.
            /// (The rationale is that larger areas indicate an area of lower point density,
            /// which is more likely to be in the exterior of the computed shape.)
            /// This improves the determinism of the queue ordering. 
            /// </summary>
            public int CompareTo(HullTri o)
            {
                /*
                 * If size is identical compare areas to ensure a (more) deterministic ordering.
                 * Larger areas sort before smaller ones.
                 */
                if (Size == o.Size)
                {
                    return -Area.CompareTo(o.Area);
                }
                return -Size.CompareTo(o.Size);
            }

            public static bool IsConnected(IList<HullTri> triList, HullTri exceptTri)
            {
                if (triList.Count == 0) return false;
                ClearMarks(triList);
                var triStart = FindTri(triList, exceptTri);
                if (triStart == null) return false;
                MarkConnected(triStart, exceptTri);
                exceptTri.IsMarked = true;
                return AreAllMarked(triList);
            }

            public static void ClearMarks(IList<HullTri> triList)
            {
                foreach (var tri in triList)
                    tri.IsMarked = false;
            }

            public static HullTri FindTri(IList<HullTri> triList, Tri exceptTri)
            {
                foreach (var tri in triList)
                    if (tri != exceptTri) return tri;

                return null;
            }

            public static bool AreAllMarked(IList<HullTri> triList)
            {
                foreach (var tri in triList)
                    if (!tri.IsMarked)
                        return false;
                return true;
            }

            public static void MarkConnected(HullTri triStart, Tri exceptTri)
            {
                var queue = new Stack<HullTri>();
                queue.Push(triStart);
                while (queue.Count > 0)
                {
                    var tri = queue.Pop();
                    tri.IsMarked = true;
                    for (int i = 0; i < 3; i++)
                    {
                        var adj = (HullTri)tri.GetAdjacent(i);
                        //-- don't connect thru this tri
                        if (adj == exceptTri)
                            continue;
                        if (adj != null && !adj.IsMarked)
                        {
                            queue.Push(adj);
                        }
                    }
                }
            }
        }

        private static IList<HullTri> CreateDelaunayTriangulation(Geometry geom)
        {
            //TODO: implement a DT on Tris directly?
            var dt = new DelaunayTriangulationBuilder();
            dt.SetSites(geom);
            var subdiv = dt.GetSubdivision();
            var triList = ToTris(subdiv);
            return triList;
        }

        private static IList<HullTri> ToTris(QuadEdgeSubdivision subdiv)
        {
            var visitor = new HullTriVisitor();
            subdiv.VisitTriangles(visitor, false);
            var triList = visitor.GetTriangles();
            TriangulationBuilder.Build(triList);
            return triList;
        }

        private class HullTriVisitor : ITriangleVisitor
        {
            private readonly List<HullTri> _triList = new List<HullTri>();

            public void Visit(QuadEdge[] triEdges)
            {
                var p0 = triEdges[0].Orig.Coordinate;
                var p1 = triEdges[1].Orig.Coordinate;
                var p2 = triEdges[2].Orig.Coordinate;
                HullTri tri;
                if (Triangle.IsCCW(p0, p1, p2))
                {
                    tri = new HullTri(p0, p2, p1);
                }
                else
                {
                    tri = new HullTri(p0, p1, p2);
                }
                _triList.Add(tri);
            }

            public IList<HullTri> GetTriangles()
            {
                return _triList;
            }
        }

        private Geometry ToPolygon(IList<HullTri> triList, GeometryFactory geomFactory)
        {
            if (!_isHolesAllowed)
            {
                return ExtractPolygon(triList, geomFactory);
            }
            //-- in case holes are present use union (slower but handles holes)
            return Union(triList, geomFactory);
        }

        private Geometry ExtractPolygon(IList<HullTri> triList, GeometryFactory geomFactory)
        {
            if (triList.Count == 1)
            {
                Tri tri = triList[0];
                return tri.ToPolygon(geomFactory);
            }
            var pts = TraceBorder(triList);
            return geomFactory.CreatePolygon(pts);
        }

        private static Geometry Union(IList<HullTri> triList, GeometryFactory geomFactory)
        {
            var polys = new List<Polygon>();
            foreach (var tri in triList) {
                var poly = tri.ToPolygon(geomFactory);
                polys.Add(poly);
            }
            return CoverageUnion.Union(geomFactory.BuildGeometry(polys));
        }

        /// <summary>
        /// Extracts the coordinates along the border of a triangulation,
        /// by tracing CW around the border triangles.
        /// Assumption: there are at least 2 tris, they are connected,
        /// and there are no holes.
        /// So each tri has at least one non-border edge, and there is only one border.
        /// </summary>
        /// <param name="triList">The triangulation</param>
        /// <returns>The border of the triangulation</returns>
        private static Coordinate[] TraceBorder(IList<HullTri> triList)
        {
            var triStart = FindBorderTri(triList);
            var coordList = new CoordinateList();
            var tri = triStart;
            do
            {
                int borderIndex = tri.BorderIndexCCW;
                //-- add border vertex
                coordList.Add(tri.GetCoordinate(borderIndex).Copy(), false);
                int nextIndex = Tri.Next(borderIndex);
                //-- if next edge is also border, add it and move to next
                if (tri.IsBorder(nextIndex))
                {
                    coordList.Add(tri.GetCoordinate(nextIndex).Copy(), false);
                    //borderIndex = nextIndex;
                }
                //-- find next border tri CCW around non-border edge
                tri = tri.NextBorderTri();
            } while (tri != triStart);
            coordList.CloseRing();
            return coordList.ToCoordinateArray();
        }

        private static HullTri FindBorderTri(IList<HullTri> triList)
        {
            foreach (var tri in triList) {
                if (tri.IsBorder()) return tri;
            }
            Assert.ShouldNeverReachHere("No border triangles found");
            return null;
        }
    }
}
