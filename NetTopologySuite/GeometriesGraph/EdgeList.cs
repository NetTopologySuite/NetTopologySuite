using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A EdgeList is a list of Edges.  It supports locating edges
    /// that are pointwise equals to a target edge.
    /// </summary>
    public class EdgeList<TCoordinate> : IList<Edge<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly List<Edge<TCoordinate>> _edges = new List<Edge<TCoordinate>>();

        /// <summary>
        /// An index of the edges, for fast lookup.
        /// a Quadtree is used, because this index needs to be dynamic
        /// (e.g. allow insertions after queries).
        /// An alternative would be to use an ordered set based on the values
        /// of the edge coordinates.
        /// </summary>
        private readonly ISpatialIndex<IExtents<TCoordinate>, Edge<TCoordinate>> _index 
            = new Quadtree<TCoordinate, Edge<TCoordinate>>();


        #region IList<Edge<TCoordinate>> Members

        /// <summary> 
        /// Insert an edge unless it is already in the list.
        /// </summary>
        public void Add(Edge<TCoordinate> e)
        {
            _edges.Add(e);
            _index.Insert(e.Extents, e);
        }

        public void AddRange(IEnumerable<Edge<TCoordinate>> edges)
        {
            _edges.AddRange(edges);
        }

        public int IndexOf(Edge<TCoordinate> item)
        {
            return _edges.IndexOf(item);
        }

        public void Insert(int index, Edge<TCoordinate> item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void RemoveRange(IEnumerable<Edge<TCoordinate>> items)
        {
            foreach (Edge<TCoordinate> item in items)
            {
                Remove(item);
            }
        }

        // <FIX> fast lookup for edges
        /// <summary>
        /// If there is an edge equal to e already in the list, return it.
        /// Otherwise return null.
        /// </summary>
        /// <returns>  
        /// equal edge, if there is one already in the list,
        /// null otherwise.
        /// </returns>
        public Edge<TCoordinate> FindEqualEdge(Edge<TCoordinate> e)
        {
            IEnumerable<Edge<TCoordinate>> result = _index.Query(e.Extents);

            foreach (Edge<TCoordinate> edge in result)
            {
                if (edge.Equals(e))
                {
                    return edge;
                }
            }

            return null;
        }

        public Edge<TCoordinate> this[Int32 index]
        {
            get { return _edges[index]; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// If the edge e is already in the list, return its index.
        /// </summary>
        /// <returns>  
        /// Index, if e is already in the list,
        /// -1 otherwise.
        /// </returns>
        public Int32 FindEdgeIndex(Edge<TCoordinate> e)
        {
            return _edges.FindIndex(delegate(Edge<TCoordinate> match)
                                    {
                                        return e == match;
                                    });
        }

        public void Write(StreamWriter outstream)
        {
            outstream.Write("MULTILINESTRING ( ");

            Boolean pastFirstEdge = false;

            foreach (Edge<TCoordinate> e in _edges)
            {
                if (pastFirstEdge)
                {
                    outstream.Write(",");
                }
                else
                {
                    pastFirstEdge = true;
                }

                outstream.Write("(");

                Boolean pastFirstCoordinate = false;

                foreach (TCoordinate coordinate in e.Coordinates)
                {
                    if (pastFirstCoordinate)
                    {
                        outstream.Write(",");
                    }
                    else
                    {
                        pastFirstCoordinate = true;
                    }

                    outstream.Write(coordinate[Ordinates.X] + " " + coordinate[Ordinates.Y]);
                }

                outstream.WriteLine(")");
            }

            outstream.Write(")  ");
        }

        #region ICollection<Edge<TCoordinate>> Members

        public void Clear()
        {
            _edges.Clear();
        }

        public Boolean Contains(Edge<TCoordinate> item)
        {
            return _edges.Contains(item);
        }

        public void CopyTo(Edge<TCoordinate>[] array, Int32 arrayIndex)
        {
            _edges.CopyTo(array, arrayIndex);
        }

        public Int32 Count
        {
            get { return _edges.Count; }
        }

        public Boolean IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Remove the selected Edge element from the list if present.
        /// </summary>
        /// <param name="e">Edge element to remove from list.</param>
        public Boolean Remove(Edge<TCoordinate> e)
        {
            return _edges.Remove(e);
        }

        #endregion

        #region IEnumerable<Edge<TCoordinate>> Members

        public IEnumerator<Edge<TCoordinate>> GetEnumerator()
        {
            return _edges.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _edges.GetEnumerator();
        }

        #endregion
    }
}