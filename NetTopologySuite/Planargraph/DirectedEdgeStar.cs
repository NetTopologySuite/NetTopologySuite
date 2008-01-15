using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Planargraph
{
    /// <summary>
    /// A sorted collection of <see cref="DirectedEdge{TCoordinate}"/>s 
    /// which leave a <see cref="Node{TCoordinate}"/>
    /// in a <see cref="PlanarGraph{TCoordinate}"/>.
    /// </summary>
    public class DirectedEdgeStar<TCoordinate> : IEnumerable<DirectedEdge<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// The underlying list of outgoing DirectedEdges.
        /// </summary>
        private readonly List<DirectedEdge<TCoordinate>> _outEdges = new List<DirectedEdge<TCoordinate>>();
        private Boolean _isSorted = false;

        /// <summary>
        /// Adds a new member to this DirectedEdgeStar.
        /// </summary>
        public void Add(DirectedEdge<TCoordinate> de)
        {
            _outEdges.Add(de);
            _isSorted = false;
        }

        /// <summary>
        /// Drops a member of this DirectedEdgeStar.
        /// </summary>
        public void Remove(DirectedEdge<TCoordinate> de)
        {
            _outEdges.Remove(de);
        }

        /// <summary>
        /// Returns an Iterator over the DirectedEdges, in ascending order by 
        /// angle with the positive x-axis.
        /// </summary>
        public IEnumerator<DirectedEdge<TCoordinate>> GetEnumerator()
        {
            sortEdges();
            return _outEdges.GetEnumerator();
        }

        /// <summary>
        /// Returns the number of edges around the Node associated with this 
        /// DirectedEdgeStar.
        /// </summary>
        public Int32 Degree
        {
            get { return _outEdges.Count; }
        }

        /// <summary>
        /// Returns the coordinate for the node at wich this star is based.
        /// </summary>
        public TCoordinate Coordinate
        {
            get
            {
                if(_outEdges.Count == 0)
                {
                    return default(TCoordinate);
                }
                else
                {
                    sortEdges();
                    return _outEdges[0].Coordinate;
                }
            }
        }

        /// <summary>
        /// Returns the DirectedEdges, in ascending order by angle with the positive 
        /// x-axis.
        /// </summary>
        public IList<DirectedEdge<TCoordinate>> Edges
        {
            get
            {
                sortEdges();
                return _outEdges.AsReadOnly();
            }
        }

        /// <summary>
        /// Returns the zero-based index of the given Edge, after sorting in 
        /// ascending order by angle with the positive x-axis.
        /// </summary>
        public Int32 GetIndex(Edge<TCoordinate> edge)
        {
            sortEdges();

            return _outEdges.FindIndex(delegate(DirectedEdge<TCoordinate> query) { return query.Edge == edge; });
        }

        /// <summary>
        /// Returns the zero-based index of the given DirectedEdge, after sorting in ascending order
        /// by angle with the positive x-axis.
        /// </summary>
        public Int32 GetIndex(DirectedEdge<TCoordinate> directedEdge)
        {
            if (directedEdge == null)
            {
                throw new ArgumentNullException("directedEdge");
            }

            sortEdges();

            return _outEdges.FindIndex(delegate(DirectedEdge<TCoordinate> query) { return directedEdge == query; });
        }

        /// <summary> 
        /// Returns the remainder when i is divided by the number of edges in this
        /// DirectedEdgeStar. 
        /// </summary>
        public Int32 GetIndex(Int32 i)
        {
            Int32 modi = i % _outEdges.Count;

            //I don't think modi can be 0 (assuming i is positive) [Jon Aquino 10/28/2003] 
            if (modi < 0)
            {
                modi += _outEdges.Count;
            }

            return modi;
        }

        /// <summary>
        /// Returns the DirectedEdge on the left-hand side of the given DirectedEdge (which
        /// must be a member of this DirectedEdgeStar). 
        /// </summary>
        public DirectedEdge<TCoordinate> GetNextEdge(DirectedEdge<TCoordinate> dirEdge)
        {
            Int32 i = GetIndex(dirEdge);
            return _outEdges[GetIndex(i + 1)];
        }

        private void sortEdges()
        {
            if (!_isSorted)
            {
                _outEdges.Sort();
                _isSorted = true;
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