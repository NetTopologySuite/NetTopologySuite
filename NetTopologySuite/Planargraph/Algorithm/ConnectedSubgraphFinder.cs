using System.Collections.Generic;

namespace NetTopologySuite.Planargraph.Algorithm
{
    /// <summary>
    /// Finds all connected <see cref="Subgraph" />s of a <see cref="PlanarGraph" />.
    /// </summary>
    public class ConnectedSubgraphFinder
    {
        private readonly PlanarGraph graph;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedSubgraphFinder"/> class.
        /// </summary>
        /// <param name="graph">The <see cref="PlanarGraph" />.</param>
        public ConnectedSubgraphFinder(PlanarGraph graph)
        {
            this.graph = graph;
        }

        public IList<Subgraph> GetConnectedSubgraphs()
        {
            IList<Subgraph> subgraphs = new List<Subgraph>();
            GraphComponent.SetVisited(graph.GetNodeEnumerator(), false);
            IEnumerator<Edge> ienum = graph.GetEdgeEnumerator();
            while(ienum.MoveNext())
            {
                Edge e = ienum.Current;
                Node node = e.GetDirEdge(0).FromNode;
                if (!node.IsVisited)
                    subgraphs.Add(FindSubgraph(node));                
            }
            return subgraphs;
        }

        private Subgraph FindSubgraph(Node node)
        {
            Subgraph subgraph = new Subgraph(graph);
            AddReachable(node, subgraph);
            return subgraph;
        }

        /// <summary>
        /// Adds all nodes and edges reachable from this node to the subgraph.
        /// Uses an explicit stack to avoid a large depth of recursion.
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="subgraph"></param>
        private void AddReachable(Node startNode, Subgraph subgraph)
        {
            Stack<Node> nodeStack = new Stack<Node>();
            nodeStack.Push(startNode);
            while (nodeStack.Count != 0)
            {
                Node node = nodeStack.Pop();
                AddEdges(node, nodeStack, subgraph);
            }
        }

        /// <summary>
        /// Adds the argument node and all its out edges to the subgraph.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeStack"></param>
        /// <param name="subgraph"></param>
        private static void AddEdges(Node node, Stack<Node> nodeStack, Subgraph subgraph)
        {
            node.Visited = true;
            foreach (DirectedEdge de in node.OutEdges)
            {
                subgraph.Add(de.Edge);
                Node toNode = de.ToNode;
                if (!toNode.IsVisited) 
                    nodeStack.Push(toNode);
            }
        }
    }
}
