using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// A map of nodes, indexed by the coordinate of the node.
    /// </summary>
    public class NodeMap<TCoordinate> : IEnumerable<Node<TCoordinate>>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private readonly NodeFactory<TCoordinate> _nodeFactory;

        private readonly SortedList<TCoordinate, Node<TCoordinate>> _nodeMap
            = new SortedList<TCoordinate, Node<TCoordinate>>();

        public NodeMap(NodeFactory<TCoordinate> nodeFact)
        {
            _nodeFactory = nodeFact;
        }

        #region IEnumerable<Node<TCoordinate>> Members

        public IEnumerator<Node<TCoordinate>> GetEnumerator()
        {
            return _nodeMap.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public override String ToString()
        {
            return _nodeMap.Count + " Nodes mapped";
        }

        /// <summary> 
        /// This method expects that a node has a coordinate value.
        /// </summary>
        public Node<TCoordinate> AddNode(TCoordinate coord)
        {
            Node<TCoordinate> node;

            if (!_nodeMap.TryGetValue(coord, out node))
            {
                node = _nodeFactory.CreateNode(coord);
                _nodeMap.Add(coord, node);
            }

            return node;
        }

        public Node<TCoordinate> AddNode(Node<TCoordinate> node)
        {
            if (node == null) throw new ArgumentNullException("node");

            Node<TCoordinate> mappedNode;
            TCoordinate coordinate = node.Coordinate;

            if (!_nodeMap.TryGetValue(coordinate, out mappedNode))
            {
                _nodeMap.Add(coordinate, node);
                return node;
            }

            // update
            mappedNode.MergeLabel(node);
            return mappedNode;
        }

        /// <summary> 
        /// Adds a node for the start point of this EdgeEnd
        /// (if one does not already exist in this map).
        /// Adds the EdgeEnd to the (possibly new) node.
        /// </summary>
        public void Add(EdgeEnd<TCoordinate> e)
        {
            TCoordinate p = e.Coordinate;
            Node<TCoordinate> n = AddNode(p);
            n.Add(e);
        }

        /// <returns> 
        /// The node if found; null otherwise.
        /// </returns>
        public Node<TCoordinate> Find(TCoordinate coord)
        {
            Node<TCoordinate> node;
            _nodeMap.TryGetValue(coord, out node);
            return node;
        }

        public IEnumerable<Node<TCoordinate>> GetBoundaryNodes(Int32 geomIndex)
        {
            foreach (Node<TCoordinate> node in this)
            {
                if (node.Label == null)
                {
                    continue;
                }

                if (node.Label.Value[geomIndex].On == Locations.Boundary)
                {
                    yield return node;
                }
            }
        }

        //public void Write(StreamWriter outstream)
        //{
        //    foreach (Node<TCoordinate> node in this)
        //    {
        //        node.Write(outstream);
        //    }
        //}
    }
}