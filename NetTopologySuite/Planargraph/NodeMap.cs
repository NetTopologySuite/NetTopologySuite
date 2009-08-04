using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Planargraph
{
    /// <summary>
    /// A map of <see cref="Node{TCoordinate}"/>s, indexed by the coordinate of the node.
    /// </summary>   
    public class NodeMap<TCoordinate> : IDictionary<TCoordinate, Node<TCoordinate>>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly SortedList<TCoordinate, Node<TCoordinate>> _nodeMap
            = new SortedList<TCoordinate, Node<TCoordinate>>();

        #region IDictionary<TCoordinate,Node<TCoordinate>> Members

        public Boolean Remove(TCoordinate key)
        {
            return _nodeMap.Remove(key);
        }

        /// <summary>
        /// Returns the Nodes in this NodeMap, sorted in ascending order
        /// by angle with the positive x-axis.
        /// </summary>
        public ICollection<Node<TCoordinate>> Values
        {
            get { return _nodeMap.Values; }
        }

        public void Add(TCoordinate key, Node<TCoordinate> value)
        {
            Add(value.Coordinate, value);
        }

        public Boolean ContainsKey(TCoordinate key)
        {
            return _nodeMap.ContainsKey(key);
        }

        public ICollection<TCoordinate> Keys
        {
            get { return _nodeMap.Keys; }
        }

        public Boolean TryGetValue(TCoordinate key, out Node<TCoordinate> value)
        {
            return _nodeMap.TryGetValue(key, out value);
        }

        public Node<TCoordinate> this[TCoordinate key]
        {
            get { return _nodeMap[key]; }
            set { _nodeMap[key] = value; }
        }

        void ICollection<KeyValuePair<TCoordinate, Node<TCoordinate>>>.Add(
            KeyValuePair<TCoordinate, Node<TCoordinate>> item)
        {
            (_nodeMap as ICollection<KeyValuePair<TCoordinate, Node<TCoordinate>>>).Add(item);
        }

        public void Clear()
        {
            _nodeMap.Clear();
        }

        Boolean ICollection<KeyValuePair<TCoordinate, Node<TCoordinate>>>.Contains(
            KeyValuePair<TCoordinate, Node<TCoordinate>> item)
        {
            return (_nodeMap as ICollection<KeyValuePair<TCoordinate, Node<TCoordinate>>>).Contains(item);
        }

        public void CopyTo(KeyValuePair<TCoordinate, Node<TCoordinate>>[] array, int arrayIndex)
        {
            (_nodeMap as ICollection<KeyValuePair<TCoordinate, Node<TCoordinate>>>).CopyTo(array, arrayIndex);
        }

        public Int32 Count
        {
            get { return _nodeMap.Count; }
        }

        Boolean ICollection<KeyValuePair<TCoordinate, Node<TCoordinate>>>.IsReadOnly
        {
            get { return false; }
        }

        Boolean ICollection<KeyValuePair<TCoordinate, Node<TCoordinate>>>.Remove(
            KeyValuePair<TCoordinate, Node<TCoordinate>> item)
        {
            return (_nodeMap as ICollection<KeyValuePair<TCoordinate, Node<TCoordinate>>>).Remove(item);
        }

        public IEnumerator<KeyValuePair<TCoordinate, Node<TCoordinate>>> GetEnumerator()
        {
            return _nodeMap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _nodeMap.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Adds a node to the map, replacing any that is already at that location.
        /// </summary>
        /// <param name="n">The <see cref="Node{TCoordinate}"/> to add.</param>
        /// <returns>The added <see cref="Node{TCoordinate}"/>.</returns>
        public Node<TCoordinate> Add(Node<TCoordinate> n)
        {
            TCoordinate key = n.Coordinate;

            if (_nodeMap.ContainsKey(key))
            {
                _nodeMap.Add(key, n);
            }

            return n;
        }

        /// <summary>
        /// Removes the <see cref="Node{TCoordinate}"/> at the given location, and returns it 
        /// in <paramref name="removedNode"/> (or sets <paramref name="removedNode"/> to 
        /// <see langword="null"/> if no <see cref="Node{TCoordinate}"/> existed with the given 
        /// <paramref name="key"/>).
        /// </summary>
        /// <param name="key">The key to remove the node at.</param>
        /// <param name="removedNode">The node which was removed.</param>
        /// <returns>
        /// <see langword="true"/> if the node at <paramref name="key"/> was found and 
        /// removed; <see langword="false"/> otherwise.
        /// </returns>
        public Boolean Remove(TCoordinate key, out Node<TCoordinate> removedNode)
        {
            Node<TCoordinate> node;
            Boolean removed = false;

            if (_nodeMap.TryGetValue(key, out node))
            {
                removed = _nodeMap.Remove(key);
            }

            removedNode = node;
            return removed;
        }

        /// <summary>
        /// Returns the Node at the given location, or <see langword="null"/> 
        /// if no <see cref="Node{TCoordinate}"/> was there.
        /// </summary>
        public Node<TCoordinate> Find(TCoordinate coord)
        {
            Node<TCoordinate> node;

            _nodeMap.TryGetValue(coord, out node);

            return node;
        }
    }
}