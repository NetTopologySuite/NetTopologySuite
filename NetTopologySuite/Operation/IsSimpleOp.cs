using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.GeometriesGraph.Index;
using Iesi_NTS.Collections;

namespace GisSharpBlog.NetTopologySuite.Operation
{
    /// <summary>
    /// Tests whether a <c>Geometry</c> is simple.
    /// Only <c>Geometry</c>s whose definition allows them
    /// to be simple or non-simple are tested.  (E.g. Polygons must be simple
    /// by definition, so no test is provided.  To test whether a given Polygon is valid,
    /// use <c>Geometry.IsValid</c>)
    /// </summary>
    public class IsSimpleOp
    {
        /// <summary>
        /// 
        /// </summary>
        public IsSimpleOp() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <returns></returns>
        public bool IsSimple(ILineString geom)
        {
            return IsSimpleLinearGeometry(geom);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <returns></returns>
        public bool IsSimple(IMultiLineString geom)
        {
            return IsSimpleLinearGeometry(geom);
        }

        /// <summary>
        /// A MultiPoint is simple if it has no repeated points.
        /// </summary>
        public bool IsSimple(IMultiPoint mp)
        {
            if (mp.IsEmpty) 
                return true;
            ISet points = new ListSet();
            for (int i = 0; i < mp.NumGeometries; i++)
            {
                IPoint pt = (IPoint) mp.GetGeometryN(i);
                ICoordinate p = pt.Coordinate;
                if (points.Contains(p))
                    return false;
                points.Add(p);
            }
            return true;
        }   

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <returns></returns>
        private bool IsSimpleLinearGeometry(IGeometry geom)
        {
            if (geom.IsEmpty) 
                return true;

            GeometryGraph graph = new GeometryGraph(0, geom);
            LineIntersector li = new RobustLineIntersector();
            SegmentIntersector si = graph.ComputeSelfNodes(li, true);
            // if no self-intersection, must be simple
            if (!si.HasIntersection) return true;
            if (si.HasProperIntersection) return false;
            if (HasNonEndpointIntersection(graph)) return false;
            if (HasClosedEndpointIntersection(graph)) return false;
            return true;
        }

        /// <summary>
        /// For all edges, check if there are any intersections which are NOT at an endpoint.
        /// The Geometry is not simple if there are intersections not at endpoints.
        /// </summary>
        /// <param name="graph"></param>
        private bool HasNonEndpointIntersection(GeometryGraph graph)
        {
            for (IEnumerator i = graph.GetEdgeEnumerator(); i.MoveNext(); )
            {
                Edge e = (Edge) i.Current;
                int maxSegmentIndex = e.MaximumSegmentIndex;
                for (IEnumerator eiIt = e.EdgeIntersectionList.GetEnumerator(); eiIt.MoveNext(); )
                {
                    EdgeIntersection ei = (EdgeIntersection) eiIt.Current;
                    if (!ei.IsEndPoint(maxSegmentIndex))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public class EndpointInfo
        {
            private ICoordinate pt;

            /// <summary>
            /// 
            /// </summary>
            public ICoordinate Point
            {
                get { return pt; }
                set { pt = value; }
            }

            private bool isClosed = false;

            /// <summary>
            /// 
            /// </summary>
            public bool IsClosed
            {
                get { return isClosed; }
                set { isClosed = value; }
            }

            private int degree;

            /// <summary>
            /// 
            /// </summary>
            public int Degree
            {
                get { return degree; }
                set { degree = value; }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pt"></param>
            public EndpointInfo(ICoordinate pt)
            {
                this.pt = pt;
                isClosed = false;
                degree = 0;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="isClosed"></param>
            public void AddEndpoint(bool isClosed)
            {
                Degree++;
                this.IsClosed |= isClosed;
            }
        }

        /// <summary> 
        /// Test that no edge intersection is the
        /// endpoint of a closed line.  To check this we compute the
        /// degree of each endpoint. The degree of endpoints of closed lines
        /// must be exactly 2.
        /// </summary>
        /// <param name="graph"></param>
        private bool HasClosedEndpointIntersection(GeometryGraph graph)
        {
            IDictionary endPoints = new SortedList();
            for (IEnumerator i = graph.GetEdgeEnumerator(); i.MoveNext(); )
            {
                Edge e = (Edge) i.Current;
                bool isClosed = e.IsClosed;                
                ICoordinate p0 = e.GetCoordinate(0);
                AddEndpoint(endPoints, p0, isClosed);
                ICoordinate p1 = e.GetCoordinate(e.NumPoints - 1);
                AddEndpoint(endPoints, p1, isClosed);
            }
            for (IEnumerator i = endPoints.Values.GetEnumerator(); i.MoveNext(); )
            {
                EndpointInfo eiInfo = (EndpointInfo) i.Current;
                if (eiInfo.IsClosed && eiInfo.Degree != 2)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Add an endpoint to the map, creating an entry for it if none exists.
        /// </summary>
        /// <param name="endPoints"></param>
        /// <param name="p"></param>
        /// <param name="isClosed"></param>
        private void AddEndpoint(IDictionary endPoints, ICoordinate p, bool isClosed)
        {
            EndpointInfo eiInfo = (EndpointInfo) endPoints[p];
            if (eiInfo == null)
            {
                eiInfo = new EndpointInfo(p);
                endPoints.Add(p, eiInfo);
            }
            eiInfo.AddEndpoint(isClosed);
        }
    }
}
