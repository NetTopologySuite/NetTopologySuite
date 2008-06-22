using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// An <see cref="EdgeList{TCoordinate}"/> is a list of 
    /// <see cref="Edge{TCoordinate}"/>s. It supports locating edges 
    /// that are pointwise equal to a target edge.
    /// </summary>
    public class EdgeList<TCoordinate> : IList<Edge<TCoordinate>>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, 
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly IGeometryFactory<TCoordinate> _geoFactory;
        private readonly List<Edge<TCoordinate>> _edges = new List<Edge<TCoordinate>>();

        /// <summary>
        /// An index of the edges, for fast lookup.
        /// a Quadtree is used, because this index needs to be dynamic
        /// (e.g. allow insertions after queries).
        /// An alternative would be to use an ordered set based on the values
        /// of the edge coordinates.
        /// </summary>
        private ISpatialIndex<IExtents<TCoordinate>, Edge<TCoordinate>> _index;

        public EdgeList(IGeometryFactory<TCoordinate> geoFactory)
        {
            if (geoFactory == null) throw new ArgumentNullException("geoFactory");

            _geoFactory = geoFactory;
            initIndex();
        }

        public override String ToString()
        {
            StringBuilder buffer = new StringBuilder();
            StringWriter writer = new StringWriter(buffer);

            writer.Write("MULTILINESTRING ( ");

            Boolean pastFirstEdge = false;

            foreach (Edge<TCoordinate> e in _edges)
            {
                if (pastFirstEdge)
                {
                    writer.Write(",");
                }
                else
                {
                    pastFirstEdge = true;
                }

                writer.Write("(");

                Boolean pastFirstCoordinate = false;

                foreach (TCoordinate coordinate in e.Coordinates)
                {
                    if (pastFirstCoordinate)
                    {
                        writer.Write(",");
                    }
                    else
                    {
                        pastFirstCoordinate = true;
                    }

                    writer.Write(coordinate);
                }

                writer.WriteLine(")");
            }

            writer.Write(")  ");

            return buffer.ToString();
        }

        #region IList<Edge<TCoordinate>> Members

        /// <summary> 
        /// Insert an edge unless it is already in the list.
        /// </summary>
        public void Add(Edge<TCoordinate> e)
        {
            _edges.Add(e);
            _index.Insert(e);
        }

        public void AddRange(IEnumerable<Edge<TCoordinate>> edges)
        {
            _edges.AddRange(edges);
        }

        public int IndexOf(Edge<TCoordinate> item)
        {
            return _edges.IndexOf(item);
        }

        public void Insert(Int32 index, Edge<TCoordinate> item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(Int32 index)
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

        // <FIX> MD fast lookup for edges
        /// <summary>
        /// If there is an edge equal to e already in the list, return it.
        /// Otherwise return null.
        /// </summary>
        /// <returns>  
        /// An equal edge, if there is one already in the list,
        /// <see langword="null"/> otherwise.
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

        #region ICollection<Edge<TCoordinate>> Members

        public void Clear()
        {
            _edges.Clear();
            initIndex();
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
            throw new NotSupportedException();
            //return _index.Remove(e) && 
            //       _edges.Remove(e);
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

        private void initIndex()
        {
            // TODO: reevaluate whether a dynamic RTree would work better here.
            _index = new Quadtree<TCoordinate, Edge<TCoordinate>>(_geoFactory);
        }
    }
}