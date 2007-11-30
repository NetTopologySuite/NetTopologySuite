using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// A map of nodes, indexed by the coordinate of the node.
    /// </summary>
    public class NodeMap<TCoordinate> : IEnumerable<Node<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        private readonly SortedList<TCoordinate, Node<TCoordinate>> _nodeMap 
            = new SortedList<TCoordinate, Node<TCoordinate>>();
        private readonly NodeFactory<TCoordinate> _nodeFactory;

        public NodeMap(NodeFactory<TCoordinate> nodeFact)
        {
            _nodeFactory = nodeFact;
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

        public Node<TCoordinate> AddNode(Node<TCoordinate> n)
        {
            Node<TCoordinate> node;

            if(!_nodeMap.TryGetValue(n.Coordinate, out node))
            {
                _nodeMap.Add(n.Coordinate, n);
                return n;
            }

            node.MergeLabel(n);
            return node;
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

        public IEnumerator<Node<TCoordinate>> GetEnumerator()
        {
            return _nodeMap.Values.GetEnumerator();
        }

        public IEnumerable<Node<TCoordinate>> GetBoundaryNodes(Int32 geomIndex)
        {
            foreach (Node<TCoordinate> node in this)
            {
                if(node.Label == null)
                {
                    continue;
                }

                if (node.Label.Value[geomIndex] == Locations.Boundary)
                {
                    yield return node;
                }
            }
        }

        public void Write(StreamWriter outstream)
        {
            foreach (Node<TCoordinate> node in this)
            {
                node.Write(outstream);
            }
        }

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}