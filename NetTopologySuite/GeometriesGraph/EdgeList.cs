using System;
using System.Collections;
using System.IO;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A EdgeList is a list of Edges.  It supports locating edges
    /// that are pointwise equals to a target edge.
    /// </summary>
    public class EdgeList
    {
        private IList edges = new ArrayList();

        /// <summary>
        /// An index of the edges, for fast lookup.
        /// a Quadtree is used, because this index needs to be dynamic
        /// (e.g. allow insertions after queries).
        /// An alternative would be to use an ordered set based on the values
        /// of the edge coordinates.
        /// </summary>
        private ISpatialIndex index = new Quadtree();

        public EdgeList() {}


        /// <summary>
        /// Remove the selected Edge element from the list if present.
        /// </summary>
        /// <param name="e">Edge element to remove from list</param>
        public void Remove(Edge e)
        {
            edges.Remove(e);
        }

        /// <summary> 
        /// Insert an edge unless it is already in the list.
        /// </summary>
        public void Add(Edge e)
        {
            edges.Add(e);
            index.Insert(e.Envelope, e);
        }

        public void AddAll(ICollection edgeColl)
        {
            for (IEnumerator i = edgeColl.GetEnumerator(); i.MoveNext();)
            {
                Add((Edge) i.Current);
            }
        }

        public IList Edges
        {
            get { return edges; }
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
        public Edge FindEqualEdge(Edge e)
        {
            ICollection testEdges = index.Query(e.Envelope);

            for (IEnumerator i = testEdges.GetEnumerator(); i.MoveNext();)
            {
                Edge testEdge = (Edge) i.Current;

                if (testEdge.Equals(e))
                {
                    return testEdge;
                }
            }

            return null;
        }

        public IEnumerator GetEnumerator()
        {
            return edges.GetEnumerator();
        }

        public Edge this[Int32 index]
        {
            get { return Get(index); }
        }

        public Edge Get(Int32 i)
        {
            return (Edge) edges[i];
        }

        /// <summary>
        /// If the edge e is already in the list, return its index.
        /// </summary>
        /// <returns>  
        /// Index, if e is already in the list,
        /// -1 otherwise.
        /// </returns>
        public Int32 FindEdgeIndex(Edge e)
        {
            for (Int32 i = 0; i < edges.Count; i++)
            {
                if (((Edge) edges[i]).Equals(e))
                {
                    return i;
                }
            }

            return -1;
        }

        public void Write(StreamWriter outstream)
        {
            outstream.Write("MULTILINESTRING ( ");

            for (Int32 j = 0; j < edges.Count; j++)
            {
                Edge e = (Edge) edges[j];

                if (j > 0)
                {
                    outstream.Write(",");
                }

                outstream.Write("(");
                ICoordinate[] pts = e.Coordinates;
                
                for (Int32 i = 0; i < pts.Length; i++)
                {
                    if (i > 0)
                    {
                        outstream.Write(",");
                    }

                    outstream.Write(pts[i].X + " " + pts[i].Y);
                }

                outstream.WriteLine(")");
            }

            outstream.Write(")  ");
        }
    }
}