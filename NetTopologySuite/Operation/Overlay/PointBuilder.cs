using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// Constructs <see cref="IPoint{TCoordinate}"/>s from the 
    /// nodes of an overlay graph.
    /// </summary>
    public class PointBuilder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IGeometryFactory<TCoordinate> _geometryFactory;
        private readonly OverlayOp<TCoordinate> _op;
        //private PointLocator<TCoordinate> _ptLocator;

        // [codekaizen 2008-01-06] parameter 'ptLocator' isn't used in JTS source PointBuilder.java rev. 1.16
        public PointBuilder(OverlayOp<TCoordinate> op, IGeometryFactory<TCoordinate> geometryFactory)
        {
            _op = op;
            _geometryFactory = geometryFactory;
            //_ptLocator = ptLocator;
        }

        /// <returns>
        /// A list of the <see cref="IPoint{TCoordinate}"/>s in the result 
        /// of the specified overlay operation.
        /// </returns>
        public IEnumerable<IPoint<TCoordinate>> Build(SpatialFunctions opCode)
        {
            return ExtractNonCoveredResultNodes(opCode);
            //IEnumerable<Node<TCoordinate>> nodeList = collectNodes(opCode);
            //IEnumerable<IPoint<TCoordinate>> resultPointList = simplifyPoints(nodeList);
            //return resultPointList;
        }

        private IEnumerable<IPoint<TCoordinate>> ExtractNonCoveredResultNodes(SpatialFunctions opCode)
        {
            List<IPoint<TCoordinate>> ret = new List<IPoint<TCoordinate>>();
            foreach(Node<TCoordinate> node in _op.Graph.Nodes)
            {

                // filter out nodes which are known to be in the result
                if (node.IsInResult)
                    continue;
                // if an incident edge is in the result, then the node coordinate is included already
                if (node.IsIncidentEdgeInResult())
                    continue;
                if (node.Edges.Degree == 0 || opCode == SpatialFunctions.Intersection)
                {

                    /**
                     * For nodes on edges, only INTERSECTION can result in edge nodes being included even
                     * if none of their incident edges are included
                     */
                    Label label = node.Label.GetValueOrDefault();
                    if (OverlayOp<TCoordinate>.IsResultOfOp(label, opCode))
                    {
                        FilterCoveredNodeToPoint(node, ret);
                    }
                }
            }
            return ret;
        }

        ///<summary>
        /// Converts non-covered nodes to Point objects and adds them to the result.
        /// A node is covered if it is contained in another element Geometry
        /// with higher dimension (e.g. a node point might be contained in a polygon,
        /// in which case the point can be eliminated from the result).
        ///</summary>
        /// <param name="n">the node to test</param>
        /// <param name="lst">collection of uncovered points</param>
        private void FilterCoveredNodeToPoint(Node<TCoordinate> n, ICollection<IPoint<TCoordinate>> lst)
        {
            TCoordinate coord = n.Coordinate;
            if (!_op.IsCoveredByLineOrArea(coord))
                lst.Add(_geometryFactory.CreatePoint(coord));
        }

        /*
        private IEnumerable<Node<TCoordinate>> collectNodes(SpatialFunctions opCode)
        {
            // add nodes from edge intersections which have not already been included in the result
            foreach (Node<TCoordinate> node in _op.Graph.Nodes)
            {
                if (!node.IsInResult)
                {
                    Debug.Assert(node.Label.HasValue);
                    Label label = node.Label.Value;

                    if (OverlayOp<TCoordinate>.IsResultOfOp(label, opCode))
                    {
                        yield return node;
                    }
                }
            }
        }

        // This method simplifies the resultant Geometry by finding and eliminating
        // "covered" points.
        // A point is covered if it is contained in another element Geometry
        // with higher dimension (e.g. a point might be contained in a polygon,
        // in which case the point can be eliminated from the resultant).
        private IEnumerable<IPoint<TCoordinate>> simplifyPoints(IEnumerable<Node<TCoordinate>> resultNodeList)
        {
            List<IPoint<TCoordinate>> ret = new List<IPoint<TCoordinate>>();
            foreach (Node<TCoordinate> node in resultNodeList)
            {
                TCoordinate coord = node.Coordinate;

                if (!_op.IsCoveredByLineOrArea(coord))
                {
                    ret.Add(_geometryFactory.CreatePoint(coord));
                    //yield return pt;
                }
            }
            return ret;
        }
        */
    }
}