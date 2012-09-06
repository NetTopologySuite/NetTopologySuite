using System.Collections.Generic;
using GeoAPI.Geometries;
using Wintellect.PowerCollections;

namespace NetTopologySuite.Planargraph
{
    /// <summary>
    /// A sorted collection of <c>DirectedEdge</c>s which leave a <c>Node</c>
    /// in a <c>PlanarGraph</c>.
    /// </summary>
    public class DirectedEdgeStar
    {
        /// <summary>
        /// The underlying list of outgoing DirectedEdges.
        /// </summary>
        private readonly BigList<DirectedEdge> outEdges = new BigList<DirectedEdge>();

        private bool _sorted;

        /*
        /// <summary>
        /// Constructs a DirectedEdgeStar with no edges.
        /// </summary>
        public DirectedEdgeStar() { }
        */
        /// <summary>
        /// Adds a new member to this DirectedEdgeStar.
        /// </summary>
        /// <param name="de"></param>
        public void Add(DirectedEdge de)
        {            
            outEdges.Add(de);
            _sorted = false;
        }

        /// <summary>
        /// Drops a member of this DirectedEdgeStar.
        /// </summary>
        /// <param name="de"></param>
        public void Remove(DirectedEdge de)
        {
            outEdges.Remove(de);
        }

        /// <summary>
        /// Returns an Iterator over the DirectedEdges, in ascending order by angle with the positive x-axis.
        /// </summary>
        public IEnumerator<DirectedEdge> GetEnumerator()
        {            
            SortEdges();
            return outEdges.GetEnumerator();
        }
        
        /// <summary>
        /// Returns the number of edges around the Node associated with this DirectedEdgeStar.
        /// </summary>
        public int Degree
        {
            get
            {
                return outEdges.Count;
            }
        }

        /// <summary>
        /// Returns the coordinate for the node at wich this star is based.
        /// </summary>
        public Coordinate Coordinate
        {
            get
            {
                if (outEdges.Count == 0 || outEdges[0] == null)
                    return null;
                return outEdges[0].Coordinate;
            }
        }

        /// <summary>
        /// Returns the DirectedEdges, in ascending order by angle with the positive x-axis.
        /// </summary>
        public IList<DirectedEdge> Edges
        {
            get
            {
                SortEdges();
                return outEdges;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SortEdges()
        {
            if (!_sorted)
            {
                outEdges.Sort();
                _sorted = true;                
            }
        }

        /// <summary>
        /// Returns the zero-based index of the given Edge, after sorting in ascending order
        /// by angle with the positive x-axis.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public int GetIndex(Edge edge)
        {
            SortEdges();
            for (int i = 0; i < outEdges.Count; i++)
            {
                DirectedEdge de = outEdges[i];
                if (de.Edge == edge)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Returns the zero-based index of the given DirectedEdge, after sorting in ascending order
        /// by angle with the positive x-axis.
        /// </summary>
        /// <param name="dirEdge"></param>
        /// <returns></returns>
        public int GetIndex(DirectedEdge dirEdge)
        {
            SortEdges();
            for (int i = 0; i < outEdges.Count; i++)
            {
                DirectedEdge de = outEdges[i];
                if (de == dirEdge)
                    return i;
            }
            return -1;
        }

        /// <summary> 
        /// Returns the remainder when i is divided by the number of edges in this
        /// DirectedEdgeStar. 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int GetIndex(int i)
        {
            int modi = i % outEdges.Count;
            //I don't think modi can be 0 (assuming i is positive) [Jon Aquino 10/28/2003] 
            if (modi < 0) 
                modi += outEdges.Count;
            return modi;
        }

        /// <summary>
        /// Returns the DirectedEdge on the left-hand side of the given DirectedEdge (which
        /// must be a member of this DirectedEdgeStar). 
        /// </summary>
        /// <param name="dirEdge"></param>
        /// <returns></returns>
        public DirectedEdge GetNextEdge(DirectedEdge dirEdge)
        {
            int i = GetIndex(dirEdge);
            return outEdges[GetIndex(i + 1)];
        }
    }
}
