using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.GeometriesGraph;

namespace NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// Constructs <c>Point</c>s from the nodes of an overlay graph.
    /// </summary>
    public class PointBuilder
    {
        private readonly OverlayOp _op;
        private readonly IGeometryFactory _geometryFactory;
        //private PointLocator _ptLocator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        /// <param name="geometryFactory"></param>
        /// <param name="ptLocator"></param>
        public PointBuilder(OverlayOp op, IGeometryFactory geometryFactory, PointLocator ptLocator)
        {
            _op = op;
            _geometryFactory = geometryFactory;
            //_ptLocator = ptLocator;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opCode"></param>
        /// <returns>
        /// A list of the Points in the result of the specified overlay operation.
        /// </returns>
        public IList<IGeometry> Build(SpatialFunction opCode)
        {
            var nodeList = CollectNodes(opCode);
            var resultPointList = SimplifyPoints(nodeList);
            return resultPointList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opCode"></param>
        /// <returns></returns>
        private IList<Node> CollectNodes(SpatialFunction opCode)
        {
            IList<Node> resultNodeList = new List<Node>();
            // add nodes from edge intersections which have not already been included in the result
            foreach (Node n in _op.Graph.Nodes)
            {
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
        private IList<IGeometry> SimplifyPoints(IEnumerable<Node> resultNodeList)
        {
            IList<IGeometry> nonCoveredPointList = new List<IGeometry>();
            foreach (Node n in resultNodeList)
            {
                Coordinate coord = n.Coordinate;
                if (!_op.IsCoveredByLA(coord))
                {
                    IPoint pt = _geometryFactory.CreatePoint(coord);
                    nonCoveredPointList.Add(pt);
                }
            }
            return nonCoveredPointList;
        }
    }
}
