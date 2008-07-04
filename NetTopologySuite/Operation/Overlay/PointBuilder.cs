using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;

namespace GisSharpBlog.NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// Constructs <c>Point</c>s from the nodes of an overlay graph.
    /// </summary>
    public class PointBuilder
    {
        private OverlayOp op;
        private IGeometryFactory geometryFactory;
        private PointLocator ptLocator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        /// <param name="geometryFactory"></param>
        /// <param name="ptLocator"></param>
        public PointBuilder(OverlayOp op, IGeometryFactory geometryFactory, PointLocator ptLocator)
        {
            this.op = op;
            this.geometryFactory = geometryFactory;
            this.ptLocator = ptLocator;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opCode"></param>
        /// <returns>
        /// A list of the Points in the result of the specified overlay operation.
        /// </returns>
        public IList Build(SpatialFunction opCode)
        {
            IList nodeList = CollectNodes(opCode);
            IList resultPointList = SimplifyPoints(nodeList);
            return resultPointList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opCode"></param>
        /// <returns></returns>
        private IList CollectNodes(SpatialFunction opCode)
        {
            IList resultNodeList = new ArrayList();
            // add nodes from edge intersections which have not already been included in the result
            IEnumerator nodeit = op.Graph.Nodes.GetEnumerator();
            while (nodeit.MoveNext()) 
            {
                Node n = (Node) nodeit.Current;
                if (!n.IsInResult)
                {
                    Label label = n.Label;
                    if (OverlayOp.IsResultOfOp(label, opCode))                    
                        resultNodeList.Add(n);                    
                }
            }
            return resultNodeList;
        }

        /// <summary>
        /// This method simplifies the resultant Geometry by finding and eliminating
        /// "covered" points.
        /// A point is covered if it is contained in another element Geometry
        /// with higher dimension (e.g. a point might be contained in a polygon,
        /// in which case the point can be eliminated from the resultant).
        /// </summary>
        /// <param name="resultNodeList"></param>
        /// <returns></returns>
        private IList SimplifyPoints(IList resultNodeList)
        {
            IList nonCoveredPointList = new ArrayList();
            IEnumerator it = resultNodeList.GetEnumerator();
            while (it.MoveNext()) 
            {
                Node n = (Node)it.Current;
                ICoordinate coord = n.Coordinate;
                if (!op.IsCoveredByLA(coord))
                {
                    IPoint pt = geometryFactory.CreatePoint(coord);
                    nonCoveredPointList.Add(pt);
                }
            }
            return nonCoveredPointList;
        }
    }
}
