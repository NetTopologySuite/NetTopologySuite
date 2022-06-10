using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayNG;
using NetTopologySuite.Triangulate.Polygon;
using NetTopologySuite.Triangulate.Tri;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Algorithm.Hull
{
    /// <summary>
    /// Constructs a concave hull of a set of polygons, respecting
    /// the polygons as constraints.
    /// A concave hull is a possibly non-convex polygon containing all the input polygons.
    /// A given set of polygons has a sequence of hulls of increasing concaveness,
    /// determined by a numeric target parameter.
    /// The computed hull "fills the gap" between the polygons,
    /// and does not intersect their interior.
    /// <para/>
    /// The concave hull is constructed by removing the longest outer edges
    /// of the Delaunay Triangulation of the space between the polygons,
    /// until the target criterion parameter is reached.
    /// <para/>
    /// The target criteria are:
    /// <list type="bullet">
    /// <item><term>Maximum Edge Length</term><description>the length of the longest edge between the polygons is no larger
    /// than this value.</description></item>
    /// <item><term>Maximum Edge Length Ratio</term><description>determine the Maximum Edge Length
    /// as a fraction of the difference between the longest and shortest edge lengths
    /// between the polygons.
    /// This normalizes the <b>Maximum Edge Length</b> to be scale-free.
    /// A value of 1 produces the convex hull; a value of 0 produces the original polygons.</description></item>
    /// </list>
    /// The preferred criterion is the <b>Maximum Edge Length Ratio</b>, since it is
    /// scale-free and local (so that no assumption needs to be made about the
    /// total amount of concaveness present).
    /// <para/>
    /// Optionally the concave hull can be allowed to contain holes, via
    /// <see cref="HolesAllowed"/>.
    /// <para/>
    /// The hull can be specified as being "tight", which means it follows the outer boundaries
    /// of the input polygons.
    /// <para/>
    /// The input polygons must form a valid MultiPolygon
    /// (i.e.they must be non - overlapping).
    /// </summary>
    /// <author>Martin Davis</author>
    public class ConcaveHullOfPolygons
    {
        /// <summary>
        /// Computes a concave hull of set of polygons
        /// using the target criterion of maximum edge length.
        /// </summary>
        /// <param name="polygons">The input polygons</param>
        /// <param name="maxLength">The target maximum edge length</param>
        /// <returns>The concave hull</returns>
        public static Geometry ConcaveHullByLength(Geometry polygons, double maxLength)
        {
            return ConcaveHullByLength(polygons, maxLength, false, false);
        }

        /// <summary>
        /// Computes a concave hull of set of polygons
        /// using the target criterion of maximum edge length,
        /// and allowing control over whether the hull boundary is tight
        /// and can contain holes.
        /// </summary>
        /// <param name="polygons">The input polygons</param>
        /// <param name="maxLength">The target maximum edge length</param>
        /// <param name="isTight">A flag indicating if the hull should be tight to the outside of the polygons</param>
        /// <param name="isHolesAllowed">A flag indicating if holes are allowed in the hull polygon</param>
        /// <returns>The concave hull</returns>
        public static Geometry ConcaveHullByLength(Geometry polygons, double maxLength,
            bool isTight, bool isHolesAllowed)
        {
            var hull = new ConcaveHullOfPolygons(polygons) {
                MaximumEdgeLength = maxLength,
                HolesAllowed = isHolesAllowed,
                Tight = isTight
            };
            return hull.GetHull();
        }

        /// <summary>
        /// Computes a concave hull of set of polygons
        /// using the target criterion of maximum edge length ratio.
        /// </summary>
        /// <param name="polygons">The input polygons</param>
        /// <param name="lengthRatio">The target maximum edge length ratio</param>
        public static Geometry ConcaveHullByLengthRatio(Geometry polygons, double lengthRatio)
        {
            return ConcaveHullByLengthRatio(polygons, lengthRatio, false, false);
        }

        /// <summary>
        /// Computes a concave hull of set of polygons
        /// using the target criterion of maximum edge length ratio,
        /// and allowing control over whether the hull boundary is tight
        /// and can contain holes.
        /// </summary>
        /// <param name="polygons">The input polygons</param>
        /// <param name="lengthRatio">The target maximum edge length ratio</param>
        /// <param name="isTight">A flag indicating if the hull should be tight to the outside of the polygons</param>
        /// <param name="isHolesAllowed">A flag indicating if holes are allowed in the hull polygon</param>
        /// <returns>The concave hull</returns>
        public static Geometry ConcaveHullByLengthRatio(Geometry polygons, double lengthRatio,
            bool isTight, bool isHolesAllowed)
        {
            var hull = new ConcaveHullOfPolygons(polygons) {
                MaximumEdgeLengthRatio = lengthRatio,
                HolesAllowed = isHolesAllowed,
                Tight = isTight
            };
            return hull.GetHull();
        }

        /// <summary>
        /// Computes a concave fill area between a set of polygons,
        /// using the target criterion of maximum edge length.
        /// </summary>
        /// <param name="polygons">The input polygons</param>
        /// <param name="maxLength">The target maximum edge length</param>
        /// <returns>The concave fill</returns>
        public static Geometry ConcaveFillByLength(Geometry polygons, double maxLength)
        {
            var hull = new ConcaveHullOfPolygons(polygons) {
                MaximumEdgeLength = maxLength
            };
            return hull.GetFill();
        }

        /// <summary>
        /// Computes a concave fill area between a set of polygons,
        /// using the target criterion of maximum edge length ratio.
        /// </summary>
        /// <param name="polygons">The input polygons</param>
        /// <param name="lengthRatio">The target maximum edge length ratio</param>
        /// <returns>The concave fill</returns>
        public static Geometry ConcaveFillByLengthRatio(Geometry polygons, double lengthRatio)
        {
            var hull = new ConcaveHullOfPolygons(polygons) {
                MaximumEdgeLengthRatio = lengthRatio
            };
            return hull.GetFill();
        }

        private const int FRAME_EXPAND_FACTOR = 4;

        private readonly Geometry _inputPolygons;
        private double _maxEdgeLength = -1;
        private double _maxEdgeLengthRatio = -1;
        private bool _isHolesAllowed = false;
        private bool _isTight = false;

        private readonly GeometryFactory _geomFactory;
        private LinearRing[] _polygonRings;

        private ISet<Tri> _hullTris;
        private Queue<Tri> _borderTriQue;

        /// <summary>
        /// Records the edge index of the longest border edge for border tris,
        /// so it can be tested for length and possible removal.
        /// </summary>
        private readonly IDictionary<Tri, int> _borderEdgeMap = new Dictionary<Tri, int>();

        /// <summary>
        /// Creates a new instance for a given geometry.
        /// </summary>
        /// <param name="polygons">The input geometry</param>
        public ConcaveHullOfPolygons(Geometry polygons)
        {
            if (!(polygons is Polygon || polygons is MultiPolygon)) {
                throw new ArgumentException("Input must be polygonal");
            }
            _inputPolygons = polygons;
            _geomFactory = _inputPolygons.Factory;
        }

        /// <summary>
        /// Gets or sets the target maximum edge length for the concave hull.
        /// The length value must be zero or greater.
        /// <list type="bullet">
        /// <item><description>The value 0.0 produces the input polygons.</description></item>
        /// <item><description>Larger values produce less concave results.</description></item>
        /// <item><description>Above a certain large value the result is the convex hull of the input.</description></item>
        /// </list>
        /// <para/>
        /// The edge length ratio provides a scale-free parameter which
        /// is intended to produce similar concave results for a variety of inputs.
        /// </summary>
        public double MaximumEdgeLength
        {
            get => _maxEdgeLength;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Edge length must be non-negative");
                _maxEdgeLength = value;
                _maxEdgeLengthRatio = -1;
            }
        }

        /// <summary>Gets or sets the target maximum edge length ratio for the concave hull.
        /// The edge length ratio is a fraction of the difference
        /// between the longest and shortest edge lengths
        /// in the Delaunay Triangulation of the area between the input polygons.
        /// (Roughly speaking, it is a fraction of the difference between
        /// the shortest and longest distances between the input polygons.)
        /// It is a value in the range 0 to 1.
        /// <list type="bullet">
        /// <item><description>The value 0.0 produces the original input polygons.</description></item>
        /// <item><description>The value 1.0 produces the convex hull.</description></item>
        /// </list> 
        /// </summary>
        public double MaximumEdgeLengthRatio
        {
            get => _maxEdgeLengthRatio;
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException("Edge length ratio must be in range [0,1]");
                this._maxEdgeLengthRatio = value;
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether holes are allowed in the concave hull polygon.
        /// </summary>
        public bool HolesAllowed
        {
            get => _isHolesAllowed;
            set => _isHolesAllowed = value;
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the boundary of the hull
        /// polygon is kept tight to the outer edges of the input polygons.
        /// </summary>
        public bool Tight
        {
            get => _isTight;
            set => _isTight = value;
        }

        /// <summary>
        /// Gets the computed concave hull.
        /// </summary>
        /// <returns>The concave hull</returns>
        public Geometry GetHull()
        {
            if (_inputPolygons.IsEmpty)
            {
                return CreateEmptyHull();
            }
            BuildHullTris();
            var hull = CreateHullGeometry(_hullTris, true);
            return hull;
        }

        /// <summary>
        /// Gets the concave fill, which is the area between the input polygons,
        /// subject to the concaveness control parameter.
        /// </summary>
        /// <returns>The concave fill</returns>
        public Geometry GetFill()
        {
            _isTight = true;
            if (_inputPolygons.IsEmpty)
            {
                return CreateEmptyHull();
            }
            BuildHullTris();
            var fill = CreateHullGeometry(_hullTris, false);
            return fill;
        }

        private Geometry CreateEmptyHull()
        {
            return _geomFactory.CreatePolygon();
        }

        private void BuildHullTris()
        {
            _polygonRings = ExtractShellRings(_inputPolygons);
            var frame = CreateFrame(_inputPolygons.EnvelopeInternal, _polygonRings, _geomFactory);
            var cdt = new ConstrainedDelaunayTriangulator(frame);
            var tris = cdt.GetTriangles();
            //System.out.println(tris);

            var framePts = frame.ExteriorRing.Coordinates;
            if (_maxEdgeLengthRatio >= 0)
            {
                _maxEdgeLength = ComputeTargetEdgeLength(tris, framePts, _maxEdgeLengthRatio);
            }

            _hullTris = RemoveFrameCornerTris(tris, framePts);

            RemoveBorderTris();
            if (_isHolesAllowed) RemoveHoleTris();
        }

        private static double ComputeTargetEdgeLength(IList<Tri> triList,
            Coordinate[] frameCorners,
            double edgeLengthRatio)
        {
            if (edgeLengthRatio == 0) return 0;
            double maxEdgeLen = -1;
            double minEdgeLen = -1;
            foreach (var tri in triList)
            {
                //-- don't include frame triangles
                if (IsFrameTri(tri, frameCorners))
                    continue;

                for (int i = 0; i < 3; i++)
                {
                    //-- constraint edges are not used to determine ratio
                    if (!tri.HasAdjacent(i))
                        continue;

                    double len = tri.GetLength(i);
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

        private static bool IsFrameTri(Tri tri, Coordinate[] frameCorners)
        {
            int index = VertexIndex(tri, frameCorners);
            bool isFrameTri = index >= 0;
            return isFrameTri;
        }

        private ISet<Tri> RemoveFrameCornerTris(IList<Tri> tris, Coordinate[] frameCorners)
        {
            var hullTris = new HashSet<Tri>();
            _borderTriQue = new Queue<Tri>();
            foreach (var tri in tris)
            {
                int index = VertexIndex(tri, frameCorners);
                bool isFrameTri = index >= 0;
                if (isFrameTri)
                {
                    /*
                     * Frame tris are adjacent to at most one border tri,
                     * which is opposite the frame corner vertex.
                     * Or, the opposite tri may be another frame tri,
                     * which is not added as a border tri.
                     */
                    int oppIndex = Tri.OppEdge(index);
                    var oppTri = tri.GetAdjacent(oppIndex);
                    bool isBorderTri = oppTri != null && !IsFrameTri(oppTri, frameCorners);
                    if (isBorderTri)
                    {
                        AddBorderTri(tri, oppIndex);
                    }
                    //-- remove the frame tri
                    tri.Remove();
                }
                else
                {
                    hullTris.Add(tri);
                }
            }
            return hullTris;
        }

        /// <summary>
        /// Get the tri vertex index of some point in a list,
        /// or -1 if none are vertices.
        /// </summary>
        /// <param name="tri">The tri to test for containing a point</param>
        /// <param name="pts">The points to test</param>
        /// <returns>The vertex index of a point, or -1</returns>
        private static int VertexIndex(Tri tri, Coordinate[] pts)
        {
            foreach (var p in pts)
            {
                int index = tri.GetIndex(p);
                if (index >= 0)
                    return index;
            }
            return -1;
        }

        private void RemoveBorderTris()
        {
            while (_borderTriQue.Count > 0)
            {
                var tri = _borderTriQue.Dequeue();
                //-- tri might have been removed already
                if (!_hullTris.Contains(tri))
                {
                    continue;
                }
                if (IsRemovable(tri))
                {
                    AddBorderTris(tri);
                    RemoveBorderTri(tri);
                }
            }
        }

        private void RemoveHoleTris()
        {
            while (true)
            {
                var holeTri = FindHoleSeedTri(_hullTris);
                if (holeTri == null)
                    return;
                AddBorderTris(holeTri);
                RemoveBorderTri(holeTri);
                RemoveBorderTris();
            }
        }

        private Tri FindHoleSeedTri(ISet<Tri> tris)
        {
            foreach (var tri in tris)
            {
                if (IsHoleSeedTri(tri))
                    return tri;
            }
            return null;
        }

        private bool IsHoleSeedTri(Tri tri)
        {
            if (IsBorderTri(tri))
                return false;

            for (int i = 0; i < 3; i++)
            {
                if (tri.HasAdjacent(i)
                    && tri.GetLength(i) > _maxEdgeLength)
                    return true;
            }
            return false;
        }

        private bool IsBorderTri(Tri tri)
        {
            for (int i = 0; i < 3; i++)
            {
                if (!tri.HasAdjacent(i))
                    return true;
            }
            return false;
        }

        private bool IsRemovable(Tri tri)
        {
            //-- remove non-bridging tris if keeping hull boundary tight
            if (_isTight && IsTouchingSinglePolygon(tri))
                return true;

            //-- check if outside edge is longer than threshold
            if (_borderEdgeMap.TryGetValue(tri, out int borderEdgeIndex))
            {
                double edgeLen = tri.GetLength(borderEdgeIndex);
                if (edgeLen > _maxEdgeLength)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Tests whether a triangle touches a single polygon at all vertices.
        /// If so, it is a candidate for removal if the hull polygon
        /// is being kept tight to the outer boundary of the input polygons.
        /// Tris which touch more than one polygon are called "bridging".
        /// </summary>
        /// <param name="tri"></param>
        /// <returns><c>true</c> if the tri touches a single polygon</returns>
        private bool IsTouchingSinglePolygon(Tri tri)
        {
            var envTri = Envelope(tri);
            foreach (var ring in _polygonRings)
            {
                //-- optimization heuristic: a touching tri must be in ring envelope
                if (ring.EnvelopeInternal.Intersects(envTri))
                {
                    if (HasAllVertices(ring, tri))
                        return true;
                }
            }
            return false;
        }

        private void AddBorderTris(Tri tri)
        {
            AddBorderTri(tri, 0);
            AddBorderTri(tri, 1);
            AddBorderTri(tri, 2);
        }

        /// <summary>
        /// Adds an adjacent tri to the current border.
        /// The adjacent edge is recorded as the border edge for the tri.
        /// Note that only edges adjacent to another tri can become border edges.
        /// Since constraint-adjacent edges do not have an adjacent tri,
        /// they can never be on the border and thus will not be removed
        /// due to being shorter than the length threshold.
        /// The tri containing them may still be removed via another edge, however. 
        /// </summary>
        /// <param name="tri">The tri adjacent to the tri to be added to the border</param>
        /// <param name="index">The index of the adjacent tri</param>
        private void AddBorderTri(Tri tri, int index)
        {
            var adj = tri.GetAdjacent(index);
            if (adj == null)
                return;
            _borderTriQue.Enqueue(adj);
            int borderEdgeIndex = adj.GetIndex(tri);
            _borderEdgeMap[adj] = borderEdgeIndex;
        }

        private void RemoveBorderTri(Tri tri)
        {
            tri.Remove();
            _hullTris.Remove(tri);
            _borderEdgeMap.Remove(tri);
        }

        private static bool HasAllVertices(LinearRing ring, Tri tri)
        {
            for (int i = 0; i < 3; i++)
            {
                var v = tri.GetCoordinate(i);
                if (!HasVertex(ring, v))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool HasVertex(LinearRing ring, Coordinate v)
        {
            for (int i = 1; i < ring.NumPoints; i++)
            {
                if (v.Equals2D(ring.GetCoordinateN(i)))
                {
                    return true;
                }
            }
            return false;
        }

        private static Envelope Envelope(Tri tri)
        {
            var env = new Envelope(tri.GetCoordinate(0), tri.GetCoordinate(1));
            env.ExpandToInclude(tri.GetCoordinate(2));
            return env;
        }

        private Geometry CreateHullGeometry(ISet<Tri> hullTris, bool isIncludeInput)
        {
            if (!isIncludeInput && hullTris.Count > 0)
                return CreateEmptyHull();

            //-- union triangulation
            var triCoverage = Tri.ToGeometry(hullTris, _geomFactory);
            //System.out.println(triCoverage);
            var fillGeometry = CoverageUnion.Union(triCoverage);

            if (!isIncludeInput)
            {
                return fillGeometry;
            }
            if (fillGeometry.IsEmpty)
            {
                return _inputPolygons.Copy();
            }
            //-- union with input polygons
            var geoms = new Geometry[] { fillGeometry, _inputPolygons };
            var geomColl = _geomFactory.CreateGeometryCollection(geoms);
            var hull = CoverageUnion.Union(geomColl);
            return hull;
        }

        /// <summary>
        /// Creates a rectangular "frame" around the input polygons,
        /// with the input polygons as holes in it.
        /// The frame is large enough that the constrained Delaunay triangulation
        /// of it should contain the convex hull of the input as edges.
        /// The frame corner triangles can be removed to produce a
        /// triangulation of the space around and between the input polygons.
        /// </summary>
        /// <param name="polygonsEnv"></param>
        /// <param name="polygonRings"></param>
        /// <param name="geomFactory"></param>
        /// <returns>The frame polygon</returns>
        private static Polygon CreateFrame(Envelope polygonsEnv, LinearRing[] polygonRings, GeometryFactory geomFactory)
        {
            double diam = polygonsEnv.Diameter;
            var envFrame = polygonsEnv.Copy();
            envFrame.ExpandBy(FRAME_EXPAND_FACTOR * diam);
            var frameOuter = (Polygon)geomFactory.ToGeometry(envFrame);
            var shell = (LinearRing)frameOuter.ExteriorRing.Copy();
            var frame = geomFactory.CreatePolygon(shell, polygonRings);
            return frame;
        }

        private static LinearRing[] ExtractShellRings(Geometry polygons)
        {
            var rings = new LinearRing[polygons.NumGeometries];
            for (int i = 0; i < polygons.NumGeometries; i++)
            {
                var consPoly = (Polygon)polygons.GetGeometryN(i);
                rings[i] = (LinearRing)consPoly.ExteriorRing.Copy();
            }
            return rings;
        }
    }
}
