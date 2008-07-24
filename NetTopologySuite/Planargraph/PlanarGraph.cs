using System.Collections;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Planargraph
{
    /// <summary>
    /// Represents a directed graph which is embeddable in a planar surface.
    /// This class and the other classes in this package serve as a framework for
    /// building planar graphs for specific algorithms. This class must be
    /// subclassed to expose appropriate methods to construct the graph. This allows
    /// controlling the types of graph components ({DirectedEdge}s,
    /// <c>Edge</c>s and <c>Node</c>s) which can be added to the graph. An
    /// application which uses the graph framework will almost always provide
    /// subclasses for one or more graph components, which hold application-specific
    /// data and graph algorithms.
    /// </summary>
    public abstract class PlanarGraph
    {
        /// <summary>
        /// 
        /// </summary>
        protected IList edges = new ArrayList();
        
        /// <summary>
        /// 
        /// </summary>
        protected IList dirEdges = new ArrayList();
        
        /// <summary>
        /// 
        /// </summary>
        protected NodeMap nodeMap = new NodeMap();

        /// <summary>
        /// Constructs a PlanarGraph without any Edges, DirectedEdges, or Nodes.
        /// </summary>
        public PlanarGraph() { }

        /// <summary>
        /// Returns the Node at the given location, or null if no Node was there.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public Node FindNode(ICoordinate pt)
        {
            return nodeMap.Find(pt);
        }

        /// <summary>
        /// Adds a node to the map, replacing any that is already at that location.
        /// Only subclasses can add Nodes, to ensure Nodes are of the right type.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>The added node.</returns>
        protected void Add(Node node)
        {
            nodeMap.Add(node);
        }

        /// <summary>
        /// Adds the Edge and its DirectedEdges with this PlanarGraph.
        /// Assumes that the Edge has already been created with its associated DirectEdges.
        /// Only subclasses can add Edges, to ensure the edges added are of the right class.
        /// </summary>
        /// <param name="edge"></param>
        protected void Add(Edge edge)
        {
            edges.Add(edge);
            Add(edge.GetDirEdge(0));
            Add(edge.GetDirEdge(1));
        }

        /// <summary>
        /// Adds the Edge to this PlanarGraph; only subclasses can add DirectedEdges,
        /// to ensure the edges added are of the right class.
        /// </summary>
        /// <param name="dirEdge"></param>
        protected void Add(DirectedEdge dirEdge)
        {
            dirEdges.Add(dirEdge);
        }

        /// <summary>
        /// Returns an IEnumerator over the Nodes in this PlanarGraph.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetNodeEnumerator()
        {            
            return nodeMap.GetEnumerator();          
        }

        /// <summary>
        /// Returns the Nodes in this PlanarGraph.
        /// </summary>
        public ICollection Nodes
        {
            get { return nodeMap.Values; }
        }

        /// <summary> 
        /// Returns an Iterator over the DirectedEdges in this PlanarGraph, in the order in which they
        /// were added.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetDirEdgeEnumerator() 
        {            
            return dirEdges.GetEnumerator();          
        }

        /// <summary>
        /// Returns an Iterator over the Edges in this PlanarGraph, in the order in which they
        /// were added.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEdgeEnumerator()
        {
            return edges.GetEnumerator(); 
        }

        /// <summary>
        /// Returns the Edges that have been added to this PlanarGraph.
        /// </summary>
        public IList Edges
        {
            get { return edges; }
        }

        /// <summary>
        /// Removes an Edge and its associated DirectedEdges from their from-Nodes and
        /// from this PlanarGraph. Note: This method does not remove the Nodes associated
        /// with the Edge, even if the removal of the Edge reduces the degree of a
        /// Node to zero.
        /// </summary>
        /// <param name="edge"></param>
        public void Remove(Edge edge)
        {
            Remove(edge.GetDirEdge(0));
            Remove(edge.GetDirEdge(1));
            edges.Remove(edge);
            edge.Remove();
        }

        /// <summary> 
        /// Removes DirectedEdge from its from-Node and from this PlanarGraph. Note:
        /// This method does not remove the Nodes associated with the DirectedEdge,
        /// even if the removal of the DirectedEdge reduces the degree of a Node to
        /// zero.
        /// </summary>
        /// <param name="de"></param>
        public void Remove(DirectedEdge de)
        {
            DirectedEdge sym = de.Sym;
            if (sym != null) 
                sym.Sym = null;
            de.FromNode.OutEdges.Remove(de);
            de.Remove();
            dirEdges.Remove(de);
        }

        /// <summary>
        /// Removes a node from the graph, along with any associated DirectedEdges and
        /// Edges.
        /// </summary>
        /// <param name="node"></param>
        public void Remove(Node node)
        {
            // unhook all directed edges
            IList outEdges = node.OutEdges.Edges;
            for (IEnumerator i = outEdges.GetEnumerator(); i.MoveNext(); )
            {
                DirectedEdge de = (DirectedEdge) i.Current;
                DirectedEdge sym = de.Sym;
                // remove the diredge that points to this node
                if (sym != null) 
                    Remove(sym);
                // remove this diredge from the graph collection
                dirEdges.Remove(de);

                Edge edge = de.Edge;
                if (edge != null)                
                    edges.Remove(edge);                
            }
            // remove the node from the graph
            nodeMap.Remove(node.Coordinate);
            node.Remove();
        }

        /// <summary>
        /// Returns all Nodes with the given number of Edges around it.
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        public IList FindNodesOfDegree(int degree)
        {
            IList nodesFound = new ArrayList();
            for (IEnumerator i = this.GetNodeEnumerator(); i.MoveNext(); )
            {
                Node node = (Node) i.Current;
                if (node.Degree == degree)
                    nodesFound.Add(node);
            }
            return nodesFound;
        }
    }
}
