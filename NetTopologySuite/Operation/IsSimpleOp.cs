using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.GeometriesGraph.Index;
using Wintellect.PowerCollections;

namespace NetTopologySuite.Operation
{
    ///<summary>
    /// Tests whether a <see cref="IGeometry"/> is simple.
    /// In general, the SFS specification of simplicity
    /// follows the rule:
    /// <list type="Bullet">
    /// <item> 
    /// A Geometry is simple if and only if the only self-intersections are at boundary points.
    /// </item>  
    /// </list>
    /// This definition relies on the definition of boundary points.
    /// The SFS uses the Mod-2 rule to determine which points are on the boundary of
    /// lineal geometries, but this class supports
    /// using other <see cref="IBoundaryNodeRule"/>s as well.
    /// </summary>
    /// <remarks>
    /// Simplicity is defined for each <see cref="IGeometry"/>} subclass as follows:
    /// <list type="Bullet">
    /// <item>Valid polygonal geometries are simple by definition, so
    ///<c>IsSimple</c> trivially returns true.
    ///(Hint: in order to check if a polygonal geometry has self-intersections,
    ///use <see cref="NetTopologySuite.Geometries.Geometry.IsValid()" />).</item>
    ///<item>Linear geometries are simple iff they do not self-intersect at points
    ///other than boundary points. 
    ///(Using the Mod-2 rule, this means that closed linestrings
    ///cannot be touched at their endpoints, since these are
    ///interior points, not boundary points).</item>
    ///<item>Zero-dimensional geometries (points) are simple iff they have no
    ///repeated points.</item>
    ///<item>Empty <see cref="IGeometry"/> are always simple</item>
    ///</list>
    /// </remarks>
    public class IsSimpleOp
    {
        private IGeometry _geom;
        private bool _isClosedEndpointsInInterior = true;
        private Coordinate _nonSimpleLocation;

        /// <summary>
        /// Creates a simplicity checker using the default SFS Mod-2 Boundary Node Rule
        /// </summary>
        [Obsolete("Use IsSimpleOp(IGeometry geom)")]
        public IsSimpleOp()
        {
        }

        ///<summary>
        /// Creates a simplicity checker using the default SFS Mod-2 Boundary Node Rule
        ///</summary>
        /// <param name="geom">The geometry to test</param>
        public IsSimpleOp(IGeometry geom)
        {
            _geom = geom;
        }

        ///<summary>
        /// Creates a simplicity checker using a given <see cref="IBoundaryNodeRule"/>
        ///</summary>
        /// <param name="geom">The geometry to test</param>
        /// <param name="boundaryNodeRule">The rule to use</param>
        public IsSimpleOp(IGeometry geom, IBoundaryNodeRule boundaryNodeRule)
        {
            _geom = geom;
            _isClosedEndpointsInInterior = !boundaryNodeRule.IsInBoundary(2);
        }

        ///<summary>
        /// Tests whether the geometry is simple.
        ///</summary>
        /// <returns>true if the geometry is simple</returns>
        public bool IsSimple()
        {
            _nonSimpleLocation = null;
            if (_geom is ILineString) return IsSimpleLinearGeometry(_geom);
            if (_geom is IMultiLineString) return IsSimpleLinearGeometry(_geom);
            if (_geom is IMultiPoint) return IsSimpleMultiPoint((IMultiPoint) _geom);
            // all other geometry types are simple by definition
            return true;
        }

        ///<summary>
        /// Gets a coordinate for the location where the geometry fails to be simple.
        /// (i.e. where it has a non-boundary self-intersection).
        /// <see cref="IsSimple()"/> must be called before this location is accessed
        ///</summary>
        public Coordinate NonSimpleLocation
        {
            get { return _nonSimpleLocation; }
        }

        /// <summary>
        /// Reports whether a <see cref="ILineString"/> is simple.
        /// </summary>
        /// <param name="geom">The lineal geometry to test</param>
        /// <returns>True if the geometry is simple</returns>
        [Obsolete("Use IsSimple()")]
        public bool IsSimple(ILineString geom)
        {
            return IsSimpleLinearGeometry(geom);
        }

        /// <summary>
        /// Reports whether a <see cref="IMultiLineString"/> is simple.
        /// </summary>
        /// <param name="geom">The lineal geometry to test</param>
        /// <returns>True if the geometry is simple</returns>
        [Obsolete("Use IsSimple()")]
        public bool IsSimple(IMultiLineString geom)
        {
            return IsSimpleLinearGeometry(geom);
        }

        /// <summary>
        /// A MultiPoint is simple if it has no repeated points.
        /// </summary>
        [Obsolete("Use IsSimple()")]
        public bool IsSimple(IMultiPoint mp)
        {
            return IsSimpleMultiPoint(mp);
        }

        private bool IsSimpleMultiPoint(IMultiPoint mp)
        {
            if (mp.IsEmpty) 
                return true;
            
            Set<Coordinate> points = new Set<Coordinate>();
            for (int i = 0; i < mp.NumGeometries; i++)
            {
                IPoint pt = (IPoint)mp.GetGeometryN(i);
                Coordinate p = pt.Coordinate;
                if (points.Contains(p))
                {
                    _nonSimpleLocation = p;
                    return false;
                }
                points.Add(p);
            }
            return true;
        }

        private bool IsSimpleLinearGeometry(IGeometry geom)
        {
            if (geom.IsEmpty) 
                return true;

            GeometryGraph graph = new GeometryGraph(0, geom);
            LineIntersector li = new RobustLineIntersector();
            SegmentIntersector si = graph.ComputeSelfNodes(li, true);
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
            foreach (Edge e in graph.Edges)
            {
                int maxSegmentIndex = e.MaximumSegmentIndex;
                foreach (EdgeIntersection ei in e.EdgeIntersectionList)
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
            public Coordinate Point;
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
            IDictionary<Coordinate, EndpointInfo> endPoints = new OrderedDictionary<Coordinate, EndpointInfo>();
            foreach (Edge e in graph.Edges)
            {
                //int maxSegmentIndex = e.MaximumSegmentIndex;
                bool isClosed = e.IsClosed;
                Coordinate p0 = e.GetCoordinate(0);
                AddEndpoint(endPoints, p0, isClosed);
                Coordinate p1 = e.GetCoordinate(e.NumPoints - 1);
                AddEndpoint(endPoints, p1, isClosed);
            }

            foreach (EndpointInfo eiInfo in endPoints.Values)
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
