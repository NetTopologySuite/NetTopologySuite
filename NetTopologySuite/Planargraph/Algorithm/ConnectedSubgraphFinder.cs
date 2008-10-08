using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using NPack.Interfaces;
#if DOTNET35
using System.Linq;
#endif

namespace GisSharpBlog.NetTopologySuite.Planargraph.Algorithm
{
    /// <summary>
    /// Finds all connected <see cref="Subgraph{TCoordinate}" />s of a 
    /// <see cref="PlanarGraph{TCoordinate}" />.
    /// </summary>
    public class ConnectedSubgraphFinder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible, 
                            IComputable<Double, TCoordinate>
    {
        private readonly PlanarGraph<TCoordinate> _graph;

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="ConnectedSubgraphFinder{TCoordinate}"/> class.
        /// </summary>
        /// <param name="graph">The <see cref="PlanarGraph{TCoordinate}" />.</param>
        public ConnectedSubgraphFinder(PlanarGraph<TCoordinate> graph)
        {
            _graph = graph;
        }

        public IEnumerable<Subgraph<TCoordinate>> FindConnectedSubgraphs()
        {
            IEnumerable<GraphComponent<TCoordinate>> components
                = Caster.Upcast<GraphComponent<TCoordinate>, Node<TCoordinate>>(_graph.Nodes);

            GraphComponent<TCoordinate>.SetVisited(components, false);

            foreach (Edge<TCoordinate> edge in _graph.Edges)
            {
                Node<TCoordinate> node = edge.GetDirectedEdge(0).FromNode;

                if (!node.IsVisited)
                {
                    yield return findSubgraph(node);
                }
            }
        }

        private Subgraph<TCoordinate> findSubgraph(Node<TCoordinate> node)
        {
            Subgraph<TCoordinate> subgraph = new Subgraph<TCoordinate>(_graph);
            addReachableToSubgraph(subgraph, node);
            return subgraph;
        }

        // Adds all nodes and edges reachable from this node to the subgraph.
        // Uses an explicit stack to avoid a large depth of recursion.
        private static void addReachableToSubgraph(Subgraph<TCoordinate> subgraph, Node<TCoordinate> startNode)
        {
            Stack<Node<TCoordinate>> nodeStack = new Stack<Node<TCoordinate>>();
            nodeStack.Push(startNode);

            while (!(nodeStack.Count == 0))
            {
                Node<TCoordinate> node = nodeStack.Pop();
                addEdgesToSubgraph(subgraph, node, nodeStack);
            }
        }

        // Adds the argument node and all its out edges to the subgraph.
        private static void addEdgesToSubgraph(Subgraph<TCoordinate> subgraph, Node<TCoordinate> node, Stack<Node<TCoordinate>> nodeStack)
        {
            node.Visited = true;
            IEnumerator<DirectedEdge<TCoordinate>> i = node.OutEdges.GetEnumerator();

            while (i.MoveNext())
            {
                DirectedEdge<TCoordinate> de = i.Current;
                subgraph.Add(de.Edge);
                Node<TCoordinate> toNode = de.ToNode;
                if (!toNode.IsVisited)
                {
                    nodeStack.Push(toNode);
                }
            }
        }
    }
}