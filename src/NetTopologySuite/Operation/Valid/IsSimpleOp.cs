using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Tests whether a <c>Geometry</c> is simple as defined by the OGC SFS specification.
    /// <para/>
    /// Simplicity is defined for each <see cref="Geometry"/>
    /// type as follows:
    /// <list type="bullet">
    /// <item><term>Point</term><description>geometries are simple.</description></item>
    /// <item><term>MultiPoint</term><description>geometries are simple if every point is unique</description></item>
    /// <item><term>LineString</term><description>geometries are simple if they do not self-intersect at interior points
    /// (i.e.points other than the endpoints).</description></item>
    /// <item><term>MultiLineString</term><description>geometries are simple if 
    /// their elements are simple and they intersect only at points 
    /// which are boundary points of both elements. 
    /// (The notion of boundary points can be user-specified - see below).</description></item>
    /// <item><term>Polygonal</term><description>geometries have no definition of simplicity.
    /// The <c>IsSimple</c> code checks if all polygon rings are simple.
    /// (Note: this means that<tt>IsSimple</tt> cannot be used to test 
    /// for <i>all</i> self-intersections in <tt>Polygon</tt> s.  
    /// In order to check if a <tt>IPolygonal</tt> geometry has self-intersections,
    /// use <see cref="Geometry.IsValid"/>.</description></item>
    /// <item><term>GeometryCollection</term><description>geometries are simple if all their elements are simple.</description></item>
    /// <item><description>Empty geometries are simple</description></item>
    /// </list>
    /// For <see cref="ILineal"/> geometries the evaluation of simplicity
    /// can be customized by supplying a <see cref="IBoundaryNodeRule"/>
    /// to define how boundary points are determined.
    /// The default is the SFS-standard <see cref="BoundaryNodeRules.Mod2BoundaryRule"/>.
    /// <para/>
    /// Note that under the <tt>Mod-2</tt> rule, closed <tt>LineString</tt>s (rings)
    /// have no boundary.
    /// This means that an intersection at their endpoints makes the geometry non-simple.
    /// If it is required to test whether a set of <c>LineString</c>s touch
    /// only at their endpoints, use <see cref="BoundaryNodeRules.EndpointBoundaryRule"/>.
    /// For example, this can be used to validate that a collection of lines
    /// form a topologically valid linear network.
    /// <para/>
    /// By default this class finds a single non-simple location.
    /// To find all non-simple locations, set <see cref="FindAllLocations"/>
    /// before calling <see cref="IsSimple()"/>, and retrieve the locations
    /// via <see cref="NonSimpleLocations"/>.
    /// This can be used to find all intersection points in a linear network.
    /// </summary>
    /// <seealso cref="IBoundaryNodeRule"/>
    /// <seealso cref="Geometry.IsEmpty"/>
    public class IsSimpleOp
    {
        /// <summary>
        /// Tests whether a geometry is simple.
        /// </summary>
        /// <param name="geom">The input geometry</param>
        /// <returns><c>true</c> if the geometry is simple</returns>
        public static bool IsSimple(Geometry geom)
        {
            var op = new IsSimpleOp(geom);
            return op.IsSimple();
        }

        /// <summary>
        /// Gets a non-simple location in a geometry, if any.
        /// </summary>
        /// <param name="geom">The input geometry</param>
        /// <returns>A non-simple location, or <c>null</c> if the geometry is simple</returns>
        public static Coordinate GetNonSimpleLocation(Geometry geom)
        {
            var op = new IsSimpleOp(geom);
            return op.NonSimpleLocation;
        }

        private readonly Geometry _inputGeom;
        private readonly bool _isClosedEndpointsInInterior;

        private bool _isSimple;
        private List<Coordinate> _nonSimplePts;

        /// <summary>
        /// Creates a simplicity checker using the default SFS Mod-2 Boundary Node Rule
        /// </summary>
        /// <param name="geom">The geometry to test</param>
        public IsSimpleOp(Geometry geom)
            : this(geom, BoundaryNodeRules.Mod2BoundaryRule)
        {

        }

        /// <summary>
        /// Creates a simplicity checker using a given <see cref="IBoundaryNodeRule"/>
        /// </summary>
        /// <param name="geom">The geometry to test</param>
        /// <param name="boundaryNodeRule">The boundary node rule to use</param>
        public IsSimpleOp(Geometry geom, IBoundaryNodeRule boundaryNodeRule)
        {
            _inputGeom = geom;
            _isClosedEndpointsInInterior = !boundaryNodeRule.IsInBoundary(2);
        }

        /// <summary>Gets or sets a value indicating if all non-simple points should be reported.</summary>
        public bool FindAllLocations { get; set; }

        /// <summary>
        /// Tests whether the geometry is simple.
        /// </summary>
        /// <returns><c>true</c> if the geometry is simple.</returns>
        public bool IsSimple()
        {
            Compute();
            return _isSimple;
        }

        /// <summary>
        /// Gets the coordinate for an location where the geometry
        /// fails to be simple (i.e. where it has a non-boundary
        /// self-intersection).
        /// </summary>
        /// <returns>A <c>Coordinate</c> for the location of the non-boundary self-intersection
        /// or <c>null</c> if the geometry is simple</returns>
        public Coordinate NonSimpleLocation
        {
            get
            {
                Compute();
                if (_nonSimplePts.Count == 0) return null;
                return _nonSimplePts[0];
            }
        }

        /// <summary>
        /// Gets all non-simple intersection locations.
        /// </summary>
        /// <returns>A list of the <c>Coordinate</c>s of non-simple locations.</returns>
        public IList<Coordinate> NonSimpleLocations
        {
            get
            {
                Compute();
                return _nonSimplePts;
            }
        }

        private void Compute()
        {
            if (_nonSimplePts != null) return;
            _nonSimplePts = new List<Coordinate>();
            _isSimple = ComputeSimple(_inputGeom);
        }

        private bool ComputeSimple(Geometry geom)
        {
            if (geom.IsEmpty) return true;
            switch (geom)
            {
                case Point _:
                    return true;
                //case LineString _:
                //    return IsSimpleLinearGeometry(geom);
                //case MultiLineString _:
                //    return IsSimpleLinearGeometry(geom);
                case ILineal _:
                    return IsSimpleLinearGeometry(geom);
                case MultiPoint mp:
                    return IsSimpleMultiPoint(mp);
                case IPolygonal _:
                    return IsSimplePolygonal(geom);
                case GeometryCollection _:
                    return IsSimpleGeometryCollection(geom);
                default:
                    // all other geometry types are simple by definition
                    return true;
            }
        }

        private bool IsSimpleMultiPoint(MultiPoint mp)
        {
            if (mp.IsEmpty) return true;

            bool res = true;
            var points = new HashSet<Coordinate>();
            for (int i = 0; i < mp.NumGeometries; i++)
            {
                var pt = (Point) mp.GetGeometryN(i);
                var p = pt.Coordinate;
                if (points.Contains(p))
                {
                    _nonSimplePts.Add(p);
                    res = false;
                    if (!FindAllLocations)
                        break;
                }
                else
                    points.Add(p);
            }

            return res;
        }

        /// <summary>
        /// Computes simplicity for polygonal geometries.
        /// Polygonal geometries are simple if and only if
        /// all of their component rings are simple.
        /// </summary>
        /// <param name="geom">A <see cref="IPolygonal"/> geometry</param>
        /// <returns><c>true</c> if the geometry is simple</returns>
        private bool IsSimplePolygonal(Geometry geom)
        {
            bool res = true;
            var rings = LinearComponentExtracter.GetLines(geom);
            foreach (var ring in rings)
            {
                if (!IsSimpleLinearGeometry(ring))
                {
                    res = false;
                    if (!FindAllLocations)
                        break;
                }
            }

            return res;
        }

        /// <summary>
        /// Semantics for GeometryCollection is
        /// simple if all components are simple.
        /// </summary>
        /// <param name="geom">A geometry collection</param>
        /// <returns><c>true</c> if the geometry is simple</returns>
        private bool IsSimpleGeometryCollection(Geometry geom)
        {
            bool res = true;
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var comp = geom.GetGeometryN(i);
                if (!ComputeSimple(comp))
                {
                    res = false;
                    if (!FindAllLocations)
                        break;
                }
            }

            return res;
        }

        private bool IsSimpleLinearGeometry(Geometry geom)
        {
            if (geom.IsEmpty) return true;
            var segStrings = ExtractSegmentStrings(geom);
            var segInt = new NonSimpleIntersectionFinder(_isClosedEndpointsInInterior, FindAllLocations, _nonSimplePts);
            var noder = new MCIndexNoder();
            noder.SegmentIntersector = segInt;
            noder.ComputeNodes(segStrings);
            if (segInt.HasIntersection)
            {
                return false;
            }

            return true;
        }

        private static List<ISegmentString> ExtractSegmentStrings(Geometry geom)
        {
            var segStrings = new List<ISegmentString>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var line = (LineString) geom.GetGeometryN(i);
                var ss = new BasicSegmentString(line.Coordinates, null);
                segStrings.Add(ss);
            }

            return segStrings;
        }

        private class NonSimpleIntersectionFinder : ISegmentIntersector
        {
            private readonly bool _isClosedEndpointsInInterior;
            private readonly bool _isFindAll;

            readonly LineIntersector _li = new RobustLineIntersector();
            private readonly List<Coordinate> _intersectionPts;

            //private bool _hasInteriorInt;
            //private bool _hasInteriorVertexInt;
            //private bool _hasEqualSegments;
            //private bool _hasInteriorEndpointInt;

            /// <summary>
            /// Creates an instance of this class
            /// </summary>
            /// <param name="isClosedEndpointsInInterior">A flag indicating if closed endpoints belong to the interior</param>
            /// <param name="isFindAll">A flag indicating that all non-simple intersection points should be found</param>
            /// <param name="intersectionPts">A list to add the non-simple intersection points to.</param>
            public NonSimpleIntersectionFinder(bool isClosedEndpointsInInterior, bool isFindAll,
                List<Coordinate> intersectionPts)
            {
                _isClosedEndpointsInInterior = isClosedEndpointsInInterior;
                _isFindAll = isFindAll;
                _intersectionPts = intersectionPts;
            }

            /// <summary>
            /// Tests whether an intersection was found.
            /// </summary>
            /// <returns><c>true</c> if an intersection was found.</returns>
            public bool HasIntersection
            {
                get => _intersectionPts.Count > 0;
            }

            /// <inheritdoc cref="ISegmentIntersector.ProcessIntersections"/>
            public void ProcessIntersections(ISegmentString ss0, int segIndex0, ISegmentString ss1, int segIndex1)
            {

                // don't test a segment with itself
                bool isSameSegString = ss0 == ss1;
                bool isSameSegment = isSameSegString && segIndex0 == segIndex1;
                if (isSameSegment) return;

                bool hasInt = FindIntersection(ss0, segIndex0, ss1, segIndex1);

                if (hasInt)
                {
                    // found an intersection!
                    _intersectionPts.Add(_li.GetIntersection(0));
                }
            }

            private bool FindIntersection(ISegmentString ss0, int segIndex0,
                ISegmentString ss1, int segIndex1)
            {
                var p00 = ss0.Coordinates[segIndex0];
                var p01 = ss0.Coordinates[segIndex0 + 1];
                var p10 = ss1.Coordinates[segIndex1];
                var p11 = ss1.Coordinates[segIndex1 + 1];


                _li.ComputeIntersection(p00, p01, p10, p11);
                if (!_li.HasIntersection) return false;

                /*
                 * Check for an intersection in the interior of a segment.
                 */
                bool hasInteriorInt = _li.IsInteriorIntersection();
                if (hasInteriorInt) return true;

                /*
                 * Check for equal segments (which will produce two intersection points).
                 * These also intersect in interior points, so are non-simple.
                 * (This is not triggered by zero-length segments, since they
                 * are filtered out by the MC index).
                 */
                bool hasEqualSegments = _li.IntersectionNum >= 2;
                if (hasEqualSegments) return true;

                /*
                 * Following tests assume non-adjacent segments.
                 */
                bool isSameSegString = ss0 == ss1;
                bool isAdjacentSegment = isSameSegString && Math.Abs(segIndex1 - segIndex0) <= 1;
                if (isAdjacentSegment) return false;

                /*
                 * At this point there is a single intersection point 
                 * which is a vertex in each segString.
                 * Classify them as endpoints or interior
                 */
                bool isIntersectionEndPt0 = IsIntersectionEndpoint(ss0, segIndex0, _li, 0);
                bool isIntersectionEndPt1 = IsIntersectionEndpoint(ss1, segIndex1, _li, 1);

                bool hasInteriorVertexInt = !(isIntersectionEndPt0 && isIntersectionEndPt1);
                if (hasInteriorVertexInt) return true;

                /* 
                 * Both intersection vertices must be endpoints.
                 * Final check is if one or both of them is interior due
                 * to being endpoint of a closed ring.
                 * This only applies to different lines
                 * (which avoids reporting ring endpoints).
                 */
                if (_isClosedEndpointsInInterior && !isSameSegString)
                {
                    bool hasInteriorEndpointInt = ss0.IsClosed || ss1.IsClosed;
                    if (hasInteriorEndpointInt) return true;
                }

                return false;
            }

            /// <summary>
            /// Tests whether an intersection vertex is an endpoint of a segment string.
            /// </summary>
            /// <param name="ss">The segment string</param>
            /// <param name="ssIndex">The index of the segment in <paramref name="ss"/></param>
            /// <param name="li">The line intersector</param>
            /// <param name="liSegmentIndex">The index of the segment in intersector</param>
            /// <returns><c>true</c> if the intersection vertex is an endpoint</returns>
            private static bool IsIntersectionEndpoint(ISegmentString ss, int ssIndex,
                LineIntersector li, int liSegmentIndex)
            {
                int vertexIndex = IntersectionVertexIndex(li, liSegmentIndex);
                /*
                 * If the vertex is the first one of the segment, check if it is the start endpoint.
                 * Otherwise check if it is the end endpoint.
                 */
                if (vertexIndex == 0)
                {
                    return ssIndex == 0;
                }
                else
                {
                    return ssIndex + 2 == ss.Count;
                }
            }

            /// <summary>
            /// Finds the vertex index in a segment of an intersection
            /// which is known to be a vertex.
            /// </summary>
            /// <param name="li">The line intersector</param>
            /// <param name="segmentIndex">The intersection segment index</param>
            /// <returns>
            /// The vertex index (0 or 1) in the segment vertex of the intersection point
            /// </returns>
            private static int IntersectionVertexIndex(LineIntersector li, int segmentIndex)
            {
                var intPt = li.GetIntersection(0);
                var endPt0 = li.GetEndpoint(segmentIndex, 0);
                return intPt.Equals2D(endPt0) ? 0 : 1;
            }

            /// <inheritdoc cref="ISegmentIntersector.IsDone"/>
            public bool IsDone
            {
                get
                {
                    if (_isFindAll) return false;
                    return _intersectionPts.Count > 0;
                }
            }
        }

    }
}
