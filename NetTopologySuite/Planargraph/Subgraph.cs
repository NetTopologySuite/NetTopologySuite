//using System.Collections;
using System.Collections.Generic;

namespace NetTopologySuite.Planargraph
{
    /// <summary>
    /// A subgraph of a <see cref="PlanarGraph" />.
    /// A subgraph may contain any subset of <see cref="Edge" />s
    /// from the parent graph.
    /// It will also automatically contain all <see cref="DirectedEdge" />s
    /// and <see cref="Node" />s associated with those edges.
    /// No new objects are created when edges are added -
    /// all associated components must already exist in the parent graph.
    /// </summary>
    public class Subgraph
    {
        /// <summary>
        ///
        /// </summary>
        protected PlanarGraph parentGraph;

        /// <summary>
        ///
        /// </summary>
        protected HashSet<Edge> edges = new HashSet<Edge>();

        /// <summary>
        ///
        /// </summary>
        protected IList<DirectedEdge> dirEdges = new List<DirectedEdge>();

        /// <summary>
        ///
        /// </summary>
        protected NodeMap nodeMap = new NodeMap();

        /// <summary>
        /// Creates a new subgraph of the given <see cref="PlanarGraph" />.
        /// </summary>
        /// <param name="parentGraph"></param>
        public Subgraph(PlanarGraph parentGraph)
        {
            this.parentGraph = parentGraph;
        }

        /// <summary>
        ///  Gets the <see cref="PlanarGraph" /> which this subgraph is part of.
        /// </summary>
        /// <returns></returns>
        public PlanarGraph GetParent()
        {
            return parentGraph;
        }

        /// <summary>
        /// Adds an <see cref="Edge" /> to the subgraph.
        /// The associated <see cref="DirectedEdge" />s and <see cref="Node" />s are also added.
        /// </summary>
        /// <param name="e">The <see cref="Edge" /> to add.</param>
        public void Add(Edge e)
        {
            if (edges.Contains(e))
                return;

            edges.Add(e);

            dirEdges.Add(e.GetDirEdge(0));
            dirEdges.Add(e.GetDirEdge(1));

            nodeMap.Add(e.GetDirEdge(0).FromNode);
            nodeMap.Add(e.GetDirEdge(1).FromNode);
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator{DirectedEdge}" /> over the <see cref="DirectedEdge" />s in this graph,
        /// in the order in which they were added.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<DirectedEdge> GetDirEdgeEnumerator()
        {
            return dirEdges.GetEnumerator();
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator{Edge}" /> over the <see cref="Edge" />s in this graph,
        /// in the order in which they were added.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Edge> GetEdgeEnumerator()
        {
            return edges.GetEnumerator();
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator{Node}" /> over the <see cref="Node" />s in this graph.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Node> GetNodeEnumerator()
        {
            return nodeMap.GetEnumerator();
        }

        /// <summary>
        /// Tests whether an <see cref="Edge" /> is contained in this subgraph.
        /// </summary>
        /// <param name="e">The <see cref="Edge" /> to test.</param>
        /// <returns><c>true</c> if the <see cref="Edge" /> is contained in this subgraph.</returns>
        public bool Contains(Edge e)
        {
            return edges.Contains(e);
        }
    }
}
