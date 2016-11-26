using System;
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
        private readonly List<IGeometry> _resultPointList = new List<IGeometry>();
        //private PointLocator _ptLocator;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="op">The operation</param>
        /// <param name="geometryFactory">The geometry factory</param>
        public PointBuilder(OverlayOp op, IGeometryFactory geometryFactory)
        {
            _op = op;
            _geometryFactory = geometryFactory;
            //_ptLocator = ptLocator;
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="op">The operation</param>
        /// <param name="geometryFactory">The geometry factory</param>
        /// <param name="ptLocator">The point locator</param>
        [Obsolete("point locateor no longer used!")]
        public PointBuilder(OverlayOp op, IGeometryFactory geometryFactory, PointLocator ptLocator = null)
            :this(op,geometryFactory)
        { }

        /// <summary>
        /// Computes the Point geometries which will appear in the result,
        /// given the specified overlay operation.
        /// </summary>
        /// <param name="opCode">The spatial function</param>
        /// <returns>
        /// A list of the Points in the result.
        /// </returns>
        public IList<IGeometry> Build(SpatialFunction opCode)
        {
            ExtractNonCoveredResultNodes(opCode);
            /**
             * It can happen that connected result nodes are still covered by
             * result geometries, so must perform this filter.
             * (For instance, this can happen during topology collapse).
             */
            return _resultPointList;
        }

        /// <summary>
        /// Determines nodes which are in the result, and creates <see cref="IPoint"/>s for them.
        /// </summary>
        /// <remarks>
        /// This method determines nodes which are candidates for the result via their
        /// labelling and their graph topology.
        /// </remarks>
        /// <param name="opCode">The overlay operation</param>
        private void ExtractNonCoveredResultNodes(SpatialFunction opCode)
        {
            // testing only
            //if (true) return resultNodeList;

            foreach (var n in _op.Graph.Nodes)
            {
                // filter out nodes which are known to be in the result
                if (n.IsInResult)
                    continue;
                // if an incident edge is in the result, then the node coordinate is included already
                if (n.IsIncidentEdgeInResult())
                    continue;
                if (n.Edges.Degree == 0 || opCode == SpatialFunction.Intersection)
                {

                    /**
                     * For nodes on edges, only INTERSECTION can result in edge nodes being included even
                     * if none of their incident edges are included
                     */
                    Label label = n.Label;
                    if (OverlayOp.IsResultOfOp(label, opCode))
                    {
                        FilterCoveredNodeToPoint(n);
                    }
                }
            }
            //Console.Writeline("connectedResultNodes collected = " + connectedResultNodes.size());
        }

        /// <summary>
        /// Converts non-covered nodes to Point objects and adds them to the result.
        /// </summary>
        /// <remarks>
        /// A node is covered if it is contained in another element Geometry
        /// with higher dimension (e.g. a node point might be contained in a polygon,
        /// in which case the point can be eliminated from the result).
        /// </remarks>
        /// <param name="n">The node to test</param>
        private void FilterCoveredNodeToPoint(Node n)
        {
            Coordinate coord = n.Coordinate;
            if (!_op.IsCoveredByLA(coord))
            {
                var pt = _geometryFactory.CreatePoint(coord);
                _resultPointList.Add(pt);
            }
        }
    }
}
