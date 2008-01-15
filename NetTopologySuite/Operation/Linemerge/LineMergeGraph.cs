using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Planargraph;
using NPack.Interfaces;


namespace GisSharpBlog.NetTopologySuite.Operation.Linemerge
{
    /// <summary>
    /// A planar graph of edges that is analyzed to sew the edges together. The 
    /// <c>marked</c> flag on <c>com.vividsolutions.planargraph.Edge</c>s 
    /// and <c>com.vividsolutions.planargraph.Node</c>s indicates whether they have been
    /// logically deleted from the graph.
    /// </summary>
    public class LineMergeGraph<TCoordinate> : PlanarGraph<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// Adds an Edge, DirectedEdges, and Nodes for the given LineString representation
        /// of an edge. 
        /// </summary>
        public void AddEdge(ILineString<TCoordinate> lineString)
        {
            if (lineString.IsEmpty)
            {
                return;
            }

            IEnumerable<TCoordinate> coordinates = lineString.Coordinates.WithoutRepeatedPoints();
            Pair<TCoordinate> startPair = Slice.GetPair(coordinates).Value;
            Pair<TCoordinate> endPair = Slice.GetLastPair(coordinates).Value;

            Node<TCoordinate> startNode = getNode(startPair.First);
            Node<TCoordinate> endNode = getNode(endPair.Second);

            DirectedEdge<TCoordinate> directedEdge0 = new LineMergeDirectedEdge<TCoordinate>(startNode, endNode,
                                                                   startPair.Second, true);

            DirectedEdge<TCoordinate> directedEdge1 = new LineMergeDirectedEdge<TCoordinate>(endNode, startNode,
                                                                   endPair.First, false);
            Edge<TCoordinate> edge = new LineMergeEdge<TCoordinate>(lineString);
            edge.SetDirectedEdges(directedEdge0, directedEdge1);
            AddInternal(edge);
        }

        private Node<TCoordinate> getNode(TCoordinate coordinate)
        {
            Node<TCoordinate> node = FindNode(coordinate);

            if (node == null)
            {
                node = new Node<TCoordinate>(coordinate);
                Add(node);
            }

            return node;
        }
    }
}