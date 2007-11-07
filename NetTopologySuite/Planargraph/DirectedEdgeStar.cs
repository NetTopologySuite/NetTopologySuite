using System;
using System.Collections;

namespace GisSharpBlog.NetTopologySuite.Planargraph
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
        protected IList outEdges = new ArrayList();

        private Boolean sorted = false;

        /// <summary>
        /// Constructs a DirectedEdgeStar with no edges.
        /// </summary>
        public DirectedEdgeStar() { }

        /// <summary>
        /// Adds a new member to this DirectedEdgeStar.
        /// </summary>
        public void Add(DirectedEdge de)
        {
            outEdges.Add(de);
            sorted = false;
        }

        /// <summary>
        /// Drops a member of this DirectedEdgeStar.
        /// </summary>
        public void Remove(DirectedEdge de)
        {
            outEdges.Remove(de);
        }

        /// <summary>
        /// Returns an Iterator over the DirectedEdges, in ascending order by 
        /// angle with the positive x-axis.
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            SortEdges();
            return outEdges.GetEnumerator();
        }

        /// <summary>
        /// Returns the number of edges around the Node associated with this 
        /// DirectedEdgeStar.
        /// </summary>
        public Int32 Degree
        {
            get { return outEdges.Count; }
        }

        /// <summary>
        /// Returns the coordinate for the node at wich this star is based.
        /// </summary>
        public ICoordinate Coordinate
        {
            get
            {
                IEnumerator it = GetEnumerator();
                if (!it.MoveNext())
                {
                    return null;
                }
                DirectedEdge e = (DirectedEdge)it.Current;
                return e.Coordinate;
            }
        }

        /// <summary>
        /// Returns the DirectedEdges, in ascending order by angle with the positive 
        /// x-axis.
        /// </summary>
        public IList Edges
        {
            get
            {
                SortEdges();
                return outEdges;
            }
        }

        private void SortEdges()
        {
            if (!sorted)
            {
                ArrayList list = (ArrayList)outEdges;
                list.Sort();
                sorted = true;
            }
        }

        /// <summary>
        /// Returns the zero-based index of the given Edge, after sorting in 
        /// ascending order by angle with the positive x-axis.
        /// </summary>
        public Int32 GetIndex(Edge edge)
        {
            SortEdges();
            for (Int32 i = 0; i < outEdges.Count; i++)
            {
                DirectedEdge de = (DirectedEdge)outEdges[i];
                if (de.Edge == edge)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the zero-based index of the given DirectedEdge, after sorting in ascending order
        /// by angle with the positive x-axis.
        /// </summary>
        public Int32 GetIndex(DirectedEdge dirEdge)
        {
            SortEdges();
            for (Int32 i = 0; i < outEdges.Count; i++)
            {
                DirectedEdge de = (DirectedEdge)outEdges[i];
                if (de == dirEdge)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary> 
        /// Returns the remainder when i is divided by the number of edges in this
        /// DirectedEdgeStar. 
        /// </summary>
        public Int32 GetIndex(Int32 i)
        {
            Int32 modi = i % outEdges.Count;

            //I don't think modi can be 0 (assuming i is positive) [Jon Aquino 10/28/2003] 
            if (modi < 0)
            {
                modi += outEdges.Count;
            }
            return modi;
        }

        /// <summary>
        /// Returns the DirectedEdge on the left-hand side of the given DirectedEdge (which
        /// must be a member of this DirectedEdgeStar). 
        /// </summary>
        public DirectedEdge GetNextEdge(DirectedEdge dirEdge)
        {
            Int32 i = GetIndex(dirEdge);
            return (DirectedEdge)outEdges[GetIndex(i + 1)];
        }
    }
}