using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.GeometriesGraph;

namespace NetTopologySuite.Operation
{
    /// <summary>
    /// Tests whether a <see cref="Geometry"/> is simple.
    /// In general, the SFS specification of simplicity
    /// follows the rule:
    /// <list type="bullet">
    /// <item><description>
    /// A Geometry is simple if and only if the only self-intersections are at boundary points.
    /// </description></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Simplicity is defined for each <see cref="Geometry"/>} subclass as follows:
    /// <list type="bullet">
    /// <item><description>Valid <see cref="IPolygonal"/> geometries are simple by definition, so
    /// <c>IsSimple</c> trivially returns true.<br/>
    /// (Note: this means that <tt>IsSimple</tt> cannot be used to test
    /// for (invalid) self-intersections in <tt>Polygon</tt>s.
    /// In order to check if a <tt>Polygonal</tt> geometry has self-intersections,
    /// use <see cref="Geometry.IsValid()" />).</description></item>
    /// <item><description><b><see cref="ILineal"/></b> geometries are simple if and only if they do <i>not</i> self-intersect at interior points
    /// (i.e. points other than boundary points).
    /// This is equivalent to saying that no two linear components satisfy the SFS <see cref="Geometry.Touches(Geometry)"/>
    /// predicate.</description></item>
    /// <item><description><b>Zero-dimensional (<see cref="IPuntal"/>)</b> geometries are simple if and only if they have no
    /// repeated points.</description></item>
    /// <item><description><b>Empty</b> <see cref="Geometry"/>s are <i>always</i> simple by definition.</description></item>
    /// </list>
    /// For <see cref="ILineal"/> geometries the evaluation of simplicity
    /// can be customized by supplying a <see cref="IBoundaryNodeRule"/>
    /// to define how boundary points are determined.
    /// The default is the SFS-standard <see cref="BoundaryNodeRules.Mod2BoundaryNodeRule"/>.
    /// Note that under the <tt>Mod-2</tt> rule, closed <tt>LineString</tt>s (rings)
    /// will never satisfy the <tt>touches</tt> predicate at their endpoints, since these are
    /// interior points, not boundary points.
    /// If it is required to test whether a set of <c>LineString</c>s touch
    /// only at their endpoints, use <c>IsSimpleOp</c> with <see cref="BoundaryNodeRules.EndpointBoundaryRule"/>.
    /// For example, this can be used to validate that a set of lines form a topologically valid
    /// linear network.
    /// </remarks>
    [Obsolete("Replaced by NetTopologySuite.Operation.Valid.IsSimpleOp")]
    public class IsSimpleOp
    {
        private readonly Geometry _inputGeom;
        private readonly bool _isClosedEndpointsInInterior = true;
        private Coordinate _nonSimpleLocation;

        /// <summary>
        /// Creates a simplicity checker using the default SFS Mod-2 Boundary Node Rule
        /// </summary>
        /// <param name="geom">The geometry to test</param>
        public IsSimpleOp(Geometry geom)
        {
            _inputGeom = geom;
        }

        /// <summary>
        /// Creates a simplicity checker using a given <see cref="IBoundaryNodeRule"/>
        /// </summary>
        /// <param name="geom">The geometry to test</param>
        /// <param name="boundaryNodeRule">The rule to use</param>
        public IsSimpleOp(Geometry geom, IBoundaryNodeRule boundaryNodeRule)
        {
            _inputGeom = geom;
            _isClosedEndpointsInInterior = !boundaryNodeRule.IsInBoundary(2);
        }

        /// <summary>
        /// Tests whether the geometry is simple.
        /// </summary>
        /// <returns>true if the geometry is simple</returns>
        public bool IsSimple()
        {
            _nonSimpleLocation = null;
            return ComputeSimple(_inputGeom);
        }

        private bool ComputeSimple(Geometry geom)
        {
            _nonSimpleLocation = null;
            if (geom.IsEmpty) return true;
            if (geom is LineString) return IsSimpleLinearGeometry(geom);
            if (geom is MultiLineString) return IsSimpleLinearGeometry(geom);
            if (geom is MultiPoint) return IsSimpleMultiPoint((MultiPoint) geom);
            if (geom is IPolygonal) return IsSimplePolygonal(geom);
            if (geom is GeometryCollection) return IsSimpleGeometryCollection(geom);
            // all other geometry types are simple by definition
            return true;
        }

        /// <summary>
        /// Gets a coordinate for the location where the geometry fails to be simple.
        /// (i.e. where it has a non-boundary self-intersection).
        /// <see cref="IsSimple()"/> must be called before this location is accessed
        /// </summary>
        /// <returns> a coordinate for the location of the non-boundary self-intersection
        /// or <c>null</c> if the geometry is simple</returns>
        public Coordinate NonSimpleLocation => _nonSimpleLocation;

        private bool IsSimpleMultiPoint(MultiPoint mp)
        {
            if (mp.IsEmpty)
                return true;

            var points = new HashSet<Coordinate>();
            for (int i = 0; i < mp.NumGeometries; i++)
            {
                var pt = (Point)mp.GetGeometryN(i);
                var p = pt.Coordinate;
                if (points.Contains(p))
                {
                    _nonSimpleLocation = p;
                    return false;
                }
                points.Add(p);
            }
            return true;
        }

        /// <summary>
        /// Computes simplicity for polygonal geometries.
        /// Polygonal geometries are simple if and only if
        /// all of their component rings are simple.
        /// </summary>
        /// <param name="geom">A Polygonal geometry</param>
        /// <returns><c>true</c> if the geometry is simple</returns>
        private bool IsSimplePolygonal(Geometry geom)
        {
            var rings = LinearComponentExtracter.GetLines(geom);
            foreach (LinearRing ring in rings)
            {
                if (!IsSimpleLinearGeometry(ring))
                    return false;
            }
            return true;
        }

        /// <summary>Semantics for GeometryCollection is
        /// simple if all components are simple.</summary>
        /// <param name="geom">A GeometryCollection</param>
        /// <returns><c>true</c> if the geometry is simple</returns>
        private bool IsSimpleGeometryCollection(Geometry geom)
        {
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var comp = geom.GetGeometryN(i);
                if (!ComputeSimple(comp))
                    return false;
            }
            return true;
        }

        private bool IsSimpleLinearGeometry(Geometry geom)
        {
            if (geom.IsEmpty)
                return true;

            var graph = new GeometryGraph(0, geom);
            var li = new RobustLineIntersector();
            var si = graph.ComputeSelfNodes(li, true);
            // if no self-intersection, must be simple
            if (!si.HasIntersection) return true;
            if (si.HasProperIntersection)
            {
                _nonSimpleLocation = si.ProperIntersectionPoint;
                return false;
            }
            if (HasNonEndpointIntersection(graph)) return false;
            if (_isClosedEndpointsInInterior)
            {
                if (HasClosedEndpointIntersection(graph)) return false;
            }
            return true;
        }

        /// <summary>
        /// For all edges, check if there are any intersections which are NOT at an endpoint.
        /// The Geometry is not simple if there are intersections not at endpoints.
        /// </summary>
        /// <param name="graph"></param>
        private bool HasNonEndpointIntersection(GeometryGraph graph)
        {
            foreach (var e in graph.Edges)
            {
                int maxSegmentIndex = e.MaximumSegmentIndex;
                foreach (var ei in e.EdgeIntersectionList)
                {
                    if (!ei.IsEndPoint(maxSegmentIndex))
                    {
                        _nonSimpleLocation = ei.Coordinate;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        private class EndpointInfo
        {
            public readonly Coordinate Point;
            public bool IsClosed; /*{ get; private set; }*/
            public int Degree; /* { get; private set; } */

            /// <summary>
            /// Creates an instance of this class
            /// </summary>
            /// <param name="pt">The endpoint</param>
            public EndpointInfo(Coordinate pt)
            {
                Point = pt;
                //IsClosed = false;
                //Degree = 0;
            }

            public void AddEndpoint(bool isClosed)
            {
                Degree++;
                IsClosed |= isClosed;
            }
        }

        /// <summary>
       /// Tests that no edge intersection is the endpoint of a closed line.
       /// This ensures that closed lines are not touched at their endpoint,
       /// which is an interior point according to the Mod-2 rule
       /// To check this we compute the degree of each endpoint.
       /// The degree of endpoints of closed lines
       /// must be exactly 2.
        /// </summary>
        private bool HasClosedEndpointIntersection(GeometryGraph graph)
        {
            var endPoints = new SortedDictionary<Coordinate, EndpointInfo>();
            foreach (var e in graph.Edges)
            {
                bool isClosed = e.IsClosed;
                var p0 = e.GetCoordinate(0);
                AddEndpoint(endPoints, p0, isClosed);
                var p1 = e.GetCoordinate(e.NumPoints - 1);
                AddEndpoint(endPoints, p1, isClosed);
            }

            foreach (var eiInfo in endPoints.Values)
            {
                if (eiInfo.IsClosed && eiInfo.Degree != 2)
                {
                    _nonSimpleLocation = eiInfo.Point;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Add an endpoint to the map, creating an entry for it if none exists.
        /// </summary>
        /// <param name="endPoints"></param>
        /// <param name="p"></param>
        /// <param name="isClosed"></param>
        private static void AddEndpoint(IDictionary<Coordinate, EndpointInfo> endPoints, Coordinate p, bool isClosed)
        {
            EndpointInfo eiInfo;
            if (!endPoints.TryGetValue(p, out eiInfo))
            {
                eiInfo = new EndpointInfo(p);
                endPoints.Add(p, eiInfo);
            }
            eiInfo.AddEndpoint(isClosed);
        }
    }
}
