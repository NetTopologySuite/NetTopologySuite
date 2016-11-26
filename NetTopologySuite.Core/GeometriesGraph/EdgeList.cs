using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Noding;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A EdgeList is a list of Edges.  It supports locating edges
    /// that are pointwise equals to a target edge.
    /// </summary>
    public class EdgeList
    {
        private readonly List<Edge> _edges = new List<Edge>();

        /// <summary>
        /// An index of the edges, for fast lookup.
        ///// a Quadtree is used, because this index needs to be dynamic
        ///// (e.g. allow insertions after queries).
        ///// An alternative would be to use an ordered set based on the values
        ///// of the edge coordinates.
        /// </summary>
        ////private readonly ISpatialIndex<Edge> _index = new Quadtree<Edge>();
        private readonly IDictionary<OrientedCoordinateArray, Edge> _ocaMap = new SortedDictionary<OrientedCoordinateArray, Edge>();


        /// <summary>
        /// Remove the selected Edge element from the list if present.
        /// </summary>
        /// <param name="e">Edge element to remove from list</param>
        public void Remove(Edge e)
        {
            _edges.Remove(e);
        }

        /// <summary> 
        /// Insert an edge unless it is already in the list.
        /// </summary>
        /// <param name="e"></param>
        public void Add(Edge e)
        {
            _edges.Add(e);
            var oca = new OrientedCoordinateArray(e.Coordinates);
            _ocaMap.Add(oca, e);
            //_index.Insert(e.Envelope, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edgeColl"></param>
        public void AddAll(IEnumerable<Edge> edgeColl)
        {
            for (var i = edgeColl.GetEnumerator(); i.MoveNext(); ) 
                Add(i.Current);
        }

        /// <summary>
        /// 
        /// </summary>
        public IList<Edge> Edges
        {
            get { return _edges; }
        }

        /// <summary>
        /// If there is an edge equal to e already in the list, return it.
        /// Otherwise return null.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>  
        /// equal edge, if there is one already in the list,
        /// null otherwise.
        /// </returns>
        public Edge FindEqualEdge(Edge e)
        {
            var oca = new OrientedCoordinateArray(e.Coordinates);
            // will return null if no edge matches
            Edge matchEdge;
            _ocaMap.TryGetValue(oca, out matchEdge);
            return matchEdge; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Edge> GetEnumerator() 
        { 
            return _edges.GetEnumerator(); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Edge this[int index]
        {
            get
            {
                return Get(index);
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Edge Get(int i) 
        {
            return _edges[i]; 
        }

        /// <summary>
        /// If the edge e is already in the list, return its index.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>  
        /// Index, if e is already in the list,
        /// -1 otherwise.
        /// </returns>
        public int FindEdgeIndex(Edge e)
        {
            for (var i = 0; i < _edges.Count; i++)
                if ((_edges[i]).Equals(e))
                    return i;            
            return -1;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="outstream"></param>
        public void Write(StreamWriter outstream)
        {
            outstream.Write("MULTILINESTRING ( ");
            for (var j = 0; j < _edges.Count; j++) 
            {
                var e = _edges[j];
                if (j > 0) 
                    outstream.Write(",");
                outstream.Write("(");
                var pts = e.Coordinates;
                for (var i = 0; i < pts.Length; i++)
                {
                    if (i > 0) 
                        outstream.Write(",");
                    outstream.Write(pts[i].X + " " + pts[i].Y);
                }
                outstream.WriteLine(")");
            }
            outstream.Write(")  ");
        }
    }
}
