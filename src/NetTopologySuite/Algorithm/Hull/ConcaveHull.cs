using NetTopologySuite.Geometries;
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
    /// until a target criterion is reached.
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
    /// </list>
    /// The preferred criterion is the <b>Maximum Edge Length Ratio</b>, since it is
    /// scale-free and local(so that no assumption needs to be made about the
    /// total amount of concaveness present).
    /// Other length criteria can be used by setting the Maximum Edge Length directly.
    /// For example, use a length relative to the longest edge length
    /// in the Minimum Spanning Tree of the point set.
    /// Or, use a length derived from the <see cref="UniformGridEdgeLength(Geometry)"/> value.
    /// <para/>
    /// The computed hull is always a single connected <see cref="Polygon"/>
    /// (unless it is degenerate, in which case it will be a <see cref="Point"/> or a <see cref="LineString"/>).
    /// This constraint may cause the concave hull to fail to meet the target criteria.
    /// <para/>
    /// Optionally the concave hull can be allowed to contain holes.
    /// Note that when using the area-based criterion
    /// this may result in substantially slower computation.
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
        /// using the target criterion of maximum edge length.
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
        /// using the target criterion of maximum edge length,
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
        /// using the target criterion of maximum edge length ratio.
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
        /// using the target criterion of maximum edge length ratio,
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

        private readonly Geometry _inputGeometry;
        private double _maxEdgeLength = 0.0;
        private double _maxEdgeLengthRatio = -1;
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
                    throw new ArgumentOutOfRangeException(nameof(value), "Edge length ratio must be in range [0,1]");
                _maxEdgeLengthRatio = value;
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
            var triList = HullTriangulation.CreateDelaunayTriangulation(_inputGeometry);
            if (_maxEdgeLengthRatio >= 0)
            {
                _maxEdgeLength = ComputeTargetEdgeLength(triList, _maxEdgeLengthRatio);
            }
            if (triList.Count == 0)
                return _inputGeometry.ConvexHull();

            ComputeHull(triList);

            var hull = ToGeometry(triList, _geomFactory);
            return hull;
        }

        private static double ComputeTargetEdgeLength(IList<Tri> triList, double edgeLengthRatio)
        {
            if (edgeLengthRatio == 0) return 0;
            double maxEdgeLen = -1;
            double minEdgeLen = -1;
            foreach (var tri in triList)
            {
                for (int i = 0; i < 3; i++)
                {
                    double len = tri.GetCoordinate(i).Distance(tri.GetCoordinate(HullTri.Next(i)));
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

        /// <summary>Computes the concave hull using edge length as the target criteria.
        /// </summary>
        /// <remarks>
        /// The erosion is done in two phases: first the border, then any
        /// internal holes (if required).
        /// This allows an fast connection check to be used
        /// when eroding holes,
        /// which makes this much more efficient than the area-based algorithm.
        /// </remarks>
        /// <param name="triList">The triangulation</param>
        private void ComputeHull(IList<Tri> triList)
        {
            ComputeHullBorder(triList);
            if (HolesAllowed)
            {
                ComputeHullHoles(triList);
            }
        }

        private void ComputeHullBorder(IList<Tri> triList)
        {
            var queue = CreateBorderQueue(triList);
            // remove tris in order of decreasing size (edge length)
            while (!queue.IsEmpty())
            {
                var tri = queue.Poll();

                if (IsBelowLengthThreshold(tri))
                    break;

                if (IsRemovableBorder(tri))
                {
                    //-- the non-null adjacents are now on the border
                    var adj0 = (HullTri)tri.GetAdjacent(0);
                    var adj1 = (HullTri)tri.GetAdjacent(1);
                    var adj2 = (HullTri)tri.GetAdjacent(2);

                    tri.Remove(triList);

                    //-- add border adjacents to queue
                    AddBorderTri(adj0, queue);
                    AddBorderTri(adj1, queue);
                    AddBorderTri(adj2, queue);
                }
            }
        }

        private PriorityQueue<HullTri> CreateBorderQueue(IList<Tri> triList)
        {
            var queue = new PriorityQueue<HullTri>();
            foreach (HullTri tri in triList)
            {
                //-- add only border triangles which could be eroded
                // (if tri has only 1 adjacent it can't be removed because that would isolate a vertex)
                if (tri.NumAdjacent != 2)
                    continue;
                tri.SetSizeToBoundary();
                queue.Add(tri);
            }
            return queue;
        }

        /// <summary>
        /// Adds a Tri to the queue.
        /// Only add tris with a single border edge,
        /// sice otherwise that would risk isolating a vertex.
        /// Sets the ordering size to the length of the border edge.
        /// </summary>
        /// <param name="tri">The Tri to add</param>
        /// <param name="queue">The priority queue to add to</param>
        private void AddBorderTri(HullTri tri, PriorityQueue<HullTri> queue)
        {
            if (tri == null) return;
            if (tri.NumAdjacent != 2) return;
            tri.SetSizeToBoundary();
            queue.Add(tri);
        }

        private bool IsBelowLengthThreshold(HullTri tri)
        {
            return tri.LengthOfBoundary < _maxEdgeLength;
        }

        private void ComputeHullHoles(IList<Tri> triList)
        {
            var candidateHoles = FindCandidateHoles(triList, _maxEdgeLength);
            // remove tris in order of decreasing size (edge length)
            foreach (HullTri tri in candidateHoles)
            {
                if (tri.IsRemoved
                    || tri.IsBorder()
                    || tri.HasBoundaryTouch)
                    continue;
                RemoveHole(triList, tri);
            }
        }

        /// <summary>
        /// Finds tris which may be the start of holes.
        /// </summary>
        /// <remarks>
        /// Only tris which have a long enough edge and which do not touch the current hull
        /// boundary are included.<br/>
        /// This avoids the risk of disconnecting the result polygon.
        /// The list is sorted in decreasing order of edge length.
        /// </remarks>
        /// <param name="triList">The triangulation</param>
        /// <param name="minEdgeLen">The minimum length of edges to consider</param>
        /// <returns>A list of candidate tris that may start a hole</returns>
        private static IList<Tri> FindCandidateHoles(IList<Tri> triList, double minEdgeLen)
        {
            var candidates = new List<Tri>();
            foreach (HullTri tri in triList)
            {
                if (tri.Size < minEdgeLen) continue;
                bool isTouchingBoundary = tri.IsBorder() || tri.HasBoundaryTouch;
                if (!isTouchingBoundary)
                {
                    candidates.Add(tri);
                }
            }
            // sort by HullTri comparator - longest edge length first
            candidates.Sort();
            return candidates;
        }

        /// <summary>
        /// Erodes a hole starting at a given triangle,
        /// and eroding all adjacent triangles with boundary edge length above target.
        /// </summary>
        /// <param name="triList">The triangulation</param>
        /// <param name="triHole">A tri which is a hole</param>
        private void RemoveHole(IList<Tri> triList, HullTri triHole)
        {
            var queue = new PriorityQueue<HullTri>();
            queue.Add(triHole);

            while (!queue.IsEmpty())
            {
                var tri = queue.Poll();

                if (tri != triHole && IsBelowLengthThreshold(tri))
                    break;

                if (tri == triHole || IsRemovableHole(tri))
                {
                    //-- the non-null adjacents are now on the border
                    var adj0 = (HullTri)tri.GetAdjacent(0);
                    var adj1 = (HullTri)tri.GetAdjacent(1);
                    var adj2 = (HullTri)tri.GetAdjacent(2);

                    tri.Remove(triList);

                    //-- add border adjacents to queue
                    AddBorderTri(adj0, queue);
                    AddBorderTri(adj1, queue);
                    AddBorderTri(adj2, queue);
                }
            }
        }


        private bool IsRemovableBorder(HullTri tri)
        {
            /*
             * Tri must have exactly 2 adjacent tris (i.e. a single boundary edge).
             * If it it has only 0 or 1 adjacent then removal would remove a vertex.
             * If it has 3 adjacent then it is not on border.
             */
            if (tri.NumAdjacent != 2) return false;
            /*
             * The tri cannot be removed if it is connecting, because
             * this would create more than one result polygon.
             */
            return !tri.IsConnecting;
        }

        private bool IsRemovableHole(HullTri tri)
        {
            /*
             * Tri must have exactly 2 adjacent tris (i.e. a single boundary edge).
             * If it it has only 0 or 1 adjacent then removal would remove a vertex.
             * If it has 3 adjacent then it is not connected to hole.
             */
            if (tri.NumAdjacent != 2) return false;
            /*
             * Ensure removal does not disconnect hull area.
             * This is a fast check which ensure holes and boundary
             * do not touch at single points.
             * (But it is slightly over-strict, since it prevents
             * any touching holes.)
             */
            return !tri.HasBoundaryTouch;
        }

        private Geometry ToGeometry(IList<Tri> triList, GeometryFactory geomFactory)
        {
            if (!_isHolesAllowed)
            {
                return HullTriangulation.TraceBoundaryPolygon(triList, geomFactory);
            }
            //-- in case holes are present use union (slower but handles holes)
            return HullTriangulation.Union(triList, geomFactory);
        }

    }
}
