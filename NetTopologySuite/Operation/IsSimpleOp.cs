#define C5
using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
#if C5
using C5;
#endif
using GeoAPI.DataStructures.Collections.Generic;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.GeometriesGraph.Index;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation
{
    /// <summary>
    /// Tests whether a <see cref="IGeometry{TCoordinate}"/> is simple.
    /// </summary>
    /// <remarks>
    /// Only <see cref="IGeometry{TCoordinate}"/>s whose definition allows them
    /// to be simple or non-simple are tested. (e.g. <see cref="IPolygon{TCoordinate}"/>s
    /// must be simple by definition, so no test is provided.  
    /// To test whether a given Polygon is valid, use <see cref="IGeometry.IsValid"/>)
    /// </remarks>
    public class IsSimpleOp<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {



        private readonly IGeometry<TCoordinate> _geom;
        private Boolean _isClosedEndpointsInInterior = true;
        private TCoordinate _nonSimpleLocation;

        ///<summary>
        /// Creates a simplicity checker using the default SFS Mod-2 Boundary Node Rule
        ///</summary>
        [Obsolete("IsSimpleOp(IGeometry<TCoordinate>)")]
        public IsSimpleOp()
        {}

        ///<summary>
        /// Creates a simplicity checker using the default SFS Mod-2 Boundary Node Rule
        ///</summary>
        ///<param name="geometry">the geometry to test</param>
        public IsSimpleOp(IGeometry<TCoordinate> geometry)
        {
            _geom = geometry;
        }

        ///<summary>
        /// Creates a simplicity checker using a given <see cref="Algorithm.IBoundaryNodeRule"/>
        ///</summary>
        ///<param name="geometry">the geometry to test</param>
        /// <param name="boundaryNodeRule">the rule to use.</param>
        public IsSimpleOp(IGeometry<TCoordinate> geometry, IBoundaryNodeRule boundaryNodeRule)
        {
            _geom = geometry;
            _isClosedEndpointsInInterior = !boundaryNodeRule.IsInBoundary(2);
        }

        public Boolean IsSimple()
        {
            if (_geom is ILineString<TCoordinate>) 
                return isSimpleLinearGeometry(_geom);

            if (_geom is IMultiLineString<TCoordinate>)
                return isSimpleLinearGeometry(_geom);

            if (_geom is IMultiPoint<TCoordinate>) 
                return isSimpleMultiPoint((IMultiPoint<TCoordinate>)_geom);

            return true;
        }

        public TCoordinate NonSimpleLocation
        {
            get { return _nonSimpleLocation; }
        }

        [Obsolete]
        public Boolean IsSimple(ILineString<TCoordinate> geom)
        {
            return isSimpleLinearGeometry(geom);
        }

        [Obsolete]
        public Boolean IsSimple(IMultiLineString<TCoordinate> geom)
        {
            return isSimpleLinearGeometry(geom);
        }

        /// <summary>
        /// A MultiPoint is simple if it has no repeated points.
        /// </summary>
        [Obsolete]
        public Boolean IsSimple(IMultiPoint<TCoordinate> mp)
        {
            return isSimpleMultiPoint(mp);
        }

        private static Boolean isSimpleMultiPoint(IMultiPoint<TCoordinate> mp)
        {
            if (mp.IsEmpty)
            {
                return true;
            }
#if C5
            TreeSet<TCoordinate> points = new TreeSet<TCoordinate>();
#else
            ISet<TCoordinate> points = new ListSet<TCoordinate>();
#endif
            for (Int32 i = 0; i < mp.Count; i++)
            {
                IPoint<TCoordinate> pt = mp[i];
                TCoordinate p = pt.Coordinate;

                if (points.Contains(p))
                {
                    return false;
                }

                points.Add(p);
            }

            return true;
        }

        private Boolean isSimpleLinearGeometry(IGeometry<TCoordinate> geom)
        {
            if (geom.IsEmpty)
            {
                return true;
            }

            GeometryGraph<TCoordinate> graph = new GeometryGraph<TCoordinate>(0, geom);
            LineIntersector<TCoordinate> li = CGAlgorithms<TCoordinate>.CreateRobustLineIntersector(geom.Factory);
            SegmentIntersector<TCoordinate> si = graph.ComputeSelfNodes(li, true);

            // if no self-intersection, must be simple
            if (!si.HasIntersection)
            {
                return true;
            }

            if (si.HasProperIntersection)
            {
                _nonSimpleLocation = si.ProperIntersectionPoint;
                return false;
            }

            if (hasNonEndpointIntersection(graph))
            {
                return false;
            }
            if (_isClosedEndpointsInInterior && hasClosedEndpointIntersection(graph))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// For all edges, check if there are any intersections which are NOT at an endpoint.
        /// The Geometry is not simple if there are intersections not at endpoints.
        /// </summary>
        private Boolean hasNonEndpointIntersection(PlanarGraph<TCoordinate> graph)
        {
            foreach (Edge<TCoordinate> e in graph.Edges)
            {
                Int32 maxSegmentIndex = e.MaximumSegmentIndex;

                foreach (EdgeIntersection<TCoordinate> intersection in e.EdgeIntersections)
                {
                    if (!intersection.IsEndPoint(maxSegmentIndex))
                    {
                        _nonSimpleLocation = intersection.Coordinate;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary> 
        /// Test that no edge intersection is the
        /// endpoint of a closed line.  To check this we compute the
        /// degree of each endpoint. The degree of endpoints of closed lines
        /// must be exactly 2.
        /// </summary>
        private static Boolean hasClosedEndpointIntersection(PlanarGraph<TCoordinate> graph)
        {
            SortedList<TCoordinate, EndpointInfo> endPoints = new SortedList<TCoordinate, EndpointInfo>();

            foreach (Edge<TCoordinate> e in graph.Edges)
            {
                Boolean isClosed = e.IsClosed;
                TCoordinate p0 = e.Coordinates[0];
                addEndpoint(endPoints, p0, isClosed);
                TCoordinate p1 = e.Coordinates[e.PointCount - 1];
                addEndpoint(endPoints, p1, isClosed);
            }

            foreach (EndpointInfo info in endPoints.Values)
            {
                if (info.IsClosed && info.Degree != 2)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Add an endpoint to the map, creating an entry for it if none exists.
        /// </summary>
#if C5        /// 
        private static void addEndpoint(System.Collections.Generic.IDictionary<TCoordinate, EndpointInfo> endPoints, TCoordinate p,
                                        Boolean isClosed)
#else
        private static void addEndpoint(IDictionary<TCoordinate, EndpointInfo> endPoints, TCoordinate p,
                                        Boolean isClosed)
#endif
        {
            EndpointInfo eiInfo;

            if (!endPoints.TryGetValue(p, out eiInfo))
            {
                eiInfo = new EndpointInfo(p);
                endPoints.Add(p, eiInfo);
            }

            eiInfo.AddEndpoint(isClosed);
        }

        #region Nested type: EndpointInfo

        //[FObermaier: If this is a struct, eiInfo in the Dictionary is not beeing updated!
        //If this is supposed to be a struct it needs to be removed and reinserted in the
        //Dictionary]
        private class EndpointInfo
        {
            private readonly TCoordinate _point;
            private Int32 _degree;
            private Boolean _isClosed;

            public EndpointInfo(TCoordinate pt)
            {
                _point = pt;
                _isClosed = false;
                _degree = 0;
            }

            public TCoordinate Point
            {
                get { return _point; }
            }

            public Boolean IsClosed
            {
                get { return _isClosed; }
                private set { _isClosed = value; }
            }

            public Int32 Degree
            {
                get { return _degree; }
                private set { _degree = value; }
            }

            public void AddEndpoint(Boolean isClosed)
            {
                Degree++;
                IsClosed |= isClosed;
            }
        }

        #endregion
    }
}