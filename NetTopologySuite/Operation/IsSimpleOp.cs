using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
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
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private struct EndpointInfo
        {
            private readonly TCoordinate _point;
            private Boolean _isClosed;
            private Int32 _degree;

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

            public EndpointInfo(TCoordinate pt)
            {
                _point = pt;
                _isClosed = false;
                _degree = 0;
            }

            public void AddEndpoint(Boolean isClosed)
            {
                Degree++;
                IsClosed |= isClosed;
            }
        }

        public Boolean IsSimple(ILineString<TCoordinate> geom)
        {
            return IsSimpleLinearGeometry(geom);
        }

        public Boolean IsSimple(IMultiLineString<TCoordinate> geom)
        {
            return IsSimpleLinearGeometry(geom);
        }

        /// <summary>
        /// A MultiPoint is simple if it has no repeated points.
        /// </summary>
        public Boolean IsSimple(IMultiPoint<TCoordinate> mp)
        {
            if (mp.IsEmpty)
            {
                return true;
            }

            ISet<TCoordinate> points = new ListSet<TCoordinate>();

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

        private Boolean IsSimpleLinearGeometry(IGeometry<TCoordinate> geom)
        {
            if (geom.IsEmpty)
            {
                return true;
            }

            GeometryGraph<TCoordinate> graph = new GeometryGraph<TCoordinate>(0, geom);
            LineIntersector<TCoordinate> li = new RobustLineIntersector<TCoordinate>();
            SegmentIntersector<TCoordinate> si = graph.ComputeSelfNodes(li, true);
            
            // if no self-intersection, must be simple
            if (!si.HasIntersection)
            {
                return true;
            }

            if (si.HasProperIntersection)
            {
                return false;
            }

            if (hasNonEndpointIntersection(graph))
            {
                return false;
            }
            if (hasClosedEndpointIntersection(graph))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// For all edges, check if there are any intersections which are NOT at an endpoint.
        /// The Geometry is not simple if there are intersections not at endpoints.
        /// </summary>
        private static Boolean hasNonEndpointIntersection(PlanarGraph<TCoordinate> graph)
        {
            foreach (Edge<TCoordinate> e in graph.Edges)
            {
                Int32 maxSegmentIndex = e.MaximumSegmentIndex;

                foreach (EdgeIntersection<TCoordinate> intersection in e.EdgeIntersectionList)
                {
                    if (!intersection.IsEndPoint(maxSegmentIndex))
                    {
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
        private Boolean hasClosedEndpointIntersection(PlanarGraph<TCoordinate> graph)
        {
            SortedList<TCoordinate, EndpointInfo> endPoints = new SortedList<TCoordinate, EndpointInfo>();

            foreach (Edge<TCoordinate> e in graph.Edges)
            {
                Boolean isClosed = e.IsClosed;
                TCoordinate p0 = e.GetCoordinate(0);
                addEndpoint(endPoints, p0, isClosed);
                TCoordinate p1 = e.GetCoordinate(e.PointCount - 1);
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
        private static void addEndpoint(IDictionary<TCoordinate, EndpointInfo> endPoints, TCoordinate p, Boolean isClosed)
        {
            EndpointInfo eiInfo;

            if(!endPoints.TryGetValue(p, out eiInfo))
            {
                eiInfo = new EndpointInfo(p);
                endPoints.Add(p, eiInfo);
            }

            eiInfo.AddEndpoint(isClosed);
        }
    }
}