#define C5
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;
using GisSharpBlog.NetTopologySuite.Noding;
using NPack.Interfaces;

#if goletas
using Goletas.Collections;
#else
#if C5
using C5;
#endif
#endif
namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// An <see cref="EdgeList{TCoordinate}"/> is a list of 
    /// <see cref="Edge{TCoordinate}"/>s. It supports locating edges 
    /// that are pointwise equal to a target edge.
    /// </summary>
    public class EdgeList<TCoordinate> : System.Collections.Generic.IList<Edge<TCoordinate>>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly List<Edge<TCoordinate>> _edges = new List<Edge<TCoordinate>>();
        private readonly IGeometryFactory<TCoordinate> _geoFactory;

#if goletas
        Goletas.Collections.SortedDictionary<OrientedCoordinateSequence<TCoordinate>, Edge<TCoordinate>> _ocaMap = 
            new Goletas.Collections.SortedDictionary<OrientedCoordinateSequence<TCoordinate>, Edge<TCoordinate>>();
#else
#if C5
        C5.TreeDictionary<OrientedCoordinateSequence<TCoordinate>, Edge<TCoordinate>> _ocaMap = 
            new TreeDictionary<OrientedCoordinateSequence<TCoordinate>, Edge<TCoordinate>>();
#else
        /// <summary>
        /// An index of the edges, for fast lookup.
        /// a Quadtree is used, because this index needs to be dynamic
        /// (e.g. allow insertions after queries).
        /// An alternative would be to use an ordered set based on the values
        /// of the edge coordinates.
        /// </summary>
        private ISpatialIndex<IExtents<TCoordinate>, Edge<TCoordinate>> _index;
#endif
#endif
#if goletas
#else
#if C5
#endif
#endif
        public EdgeList(IGeometryFactory<TCoordinate> geoFactory)
        {
            if (geoFactory == null) throw new ArgumentNullException("geoFactory");

            _geoFactory = geoFactory;
#if goletas
#else
#if C5
#else
            initIndex();
#endif
#endif
        }

        #region IList<Edge<TCoordinate>> Members

        /// <summary> 
        /// Insert an edge unless it is already in the list.
        /// </summary>
        public void Add(Edge<TCoordinate> e)
        {
            _edges.Add(e);
#if goletas
            OrientedCoordinateSequence<TCoordinate> oca = new OrientedCoordinateSequence<TCoordinate>(e.Coordinates);
            _ocaMap.Add(oca, e);
#else
#if C5
            OrientedCoordinateSequence<TCoordinate> oca = new OrientedCoordinateSequence<TCoordinate>(e.Coordinates);
            if (!_ocaMap.Contains(oca))
                _ocaMap.Add(oca, e);
#else
            _index.Insert(e);

#endif
#endif
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

        public Edge<TCoordinate> this[Int32 index]
        {
            get { return _edges[index]; }
            set { throw new NotSupportedException(); }
        }

        public void Clear()
        {
            _edges.Clear();
#if goletas
            _ocaMap.Clear();
#else
#if C5
            _ocaMap.Clear();
#else
            initIndex();
#endif
#endif
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
#if goletas
#else
#if C5
#else
#endif
#endif
            //throw new NotSupportedException();
            //return _index.Remove(e) && 
            //       _edges.Remove(e);

        }

        public IEnumerator<Edge<TCoordinate>> GetEnumerator()
        {
            return _edges.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _edges.GetEnumerator();
        }

        #endregion

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

        public void AddRange(IEnumerable<Edge<TCoordinate>> edges)
        {
            //_edges.AddRange(edges);
            foreach (Edge<TCoordinate> edge in edges)
                Add(edge);
        }

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
#if goletas
    OrientedCoordinateSequence<TCoordinate> oca = new OrientedCoordinateSequence<TCoordinate>(e.Coordinates);
    // will return null if no edge matches
    Edge<TCoordinate> matchEdge = _ocaMap[oca];
            return matchEdge;
#else
#if C5
            OrientedCoordinateSequence<TCoordinate> oca = new OrientedCoordinateSequence<TCoordinate>(e.Coordinates);
            // will return null if no edge matches
            Edge<TCoordinate> matchEdge = null;
            _ocaMap.Find(oca, out matchEdge);
            return matchEdge;
#else
            IEnumerable<Edge<TCoordinate>> result = _index.Query(e.Extents);
            foreach (Edge<TCoordinate> edge in result)
            {
                if (edge.Equals(e))
                {
                    return edge;
                }
            }

            return null;
#endif
#endif

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
            return _edges.FindIndex(e.Equals);
        }

#if goletas
#else
#if C5
#else
        private void initIndex()
        {
            // TODO: reevaluate whether a dynamic RTree would work better here.
            _index = new Quadtree<TCoordinate, Edge<TCoordinate>>(_geoFactory);
        }
#endif
#endif
    }
}