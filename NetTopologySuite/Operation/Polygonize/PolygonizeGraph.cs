using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures.Collections.Generic;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Planargraph;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// Represents a planar graph of edges that can be used to compute a
    /// polygonization, and implements the algorithms to compute the
    /// EdgeRings formed by the graph.
    /// The marked flag on DirectedEdges is used to indicate that a directed edge
    /// has be logically deleted from the graph.
    /// </summary>
    public class PolygonizeGraph<TCoordinate> : PlanarGraph<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// Deletes all edges at a node.
        /// </summary>
        public static void DeleteAllEdges(Node<TCoordinate> node)
        {
            foreach (DirectedEdge<TCoordinate> de in node.OutEdges.Edges)
            {
                de.Marked = true;

                PolygonizeDirectedEdge<TCoordinate> sym = de.Sym as PolygonizeDirectedEdge<TCoordinate>;

                if (sym != null)
                {
                    sym.Marked = true;
                }
            }
        }

        private readonly IGeometryFactory<TCoordinate> _factory;

        /// <summary>
        /// Create a new polygonization graph.
        /// </summary>
        public PolygonizeGraph(IGeometryFactory<TCoordinate> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Add a <c>LineString</c> forming an edge of the polygon graph.
        /// </summary>
        /// <param name="line">The line to add.</param>
        public void AddEdge(ILineString<TCoordinate> line)
        {
            if (line.IsEmpty)
            {
                return;
            }

            ICoordinateSequence<TCoordinate> linePts = line.Coordinates.WithoutRepeatedPoints();

            TCoordinate startPt = Slice.GetFirst(linePts);
            TCoordinate endPt = Slice.GetLast(linePts);

            Node<TCoordinate> nStart = GetNode(startPt);
            Node<TCoordinate> nEnd = GetNode(endPt);

            DirectedEdge<TCoordinate> de0 = new PolygonizeDirectedEdge<TCoordinate>(nStart, nEnd, linePts[1], true);
            DirectedEdge<TCoordinate> de1 =
                new PolygonizeDirectedEdge<TCoordinate>(nEnd, nStart, linePts[linePts.Count - 2], false);
            Edge<TCoordinate> edge = new PolygonizeEdge<TCoordinate>(line);
            edge.SetDirectedEdges(de0, de1);
            Add(edge);
        }

        /// <summary>
        /// Computes the EdgeRings formed by the edges in this graph.        
        /// </summary>
        /// <returns>A list of the{EdgeRings found by the polygonization process.</returns>
        public IEnumerable<EdgeRing<TCoordinate>> GetEdgeRings()
        {
            // maybe could optimize this, since most of these pointers should be set correctly already by deleteCutEdges()
            computeNextCWEdges();

            // clear labels of all edges in graph
            IEnumerable<PolygonizeDirectedEdge<TCoordinate>> directedEdges
                =
                Enumerable.Downcast<PolygonizeDirectedEdge<TCoordinate>, DirectedEdge<TCoordinate>>(
                    DirectedEdges);

            label(directedEdges, -1);
            IEnumerable<PolygonizeDirectedEdge<TCoordinate>> maximalRings = findLabeledEdgeRings(directedEdges);
            ConvertMaximalToMinimalEdgeRings(maximalRings);

            // find all edgerings
            foreach (PolygonizeDirectedEdge<TCoordinate> de in directedEdges)
            {
                if (de.IsMarked)
                {
                    continue;
                }

                if (de.IsInRing)
                {
                    continue;
                }

                yield return findEdgeRing(de);
            }
        }

        /// <summary>
        /// Finds and removes all cut edges from the graph.
        /// </summary>
        /// <returns>
        /// A set of the <see cref="ILineString{TCoordinate}"/>s 
        /// forming the removed cut edges.
        /// </returns>
        public IEnumerable<ILineString<TCoordinate>> DeleteCutEdges()
        {
            computeNextCWEdges();

            IEnumerable<PolygonizeDirectedEdge<TCoordinate>> directedEdges
                =
                Enumerable.Downcast<PolygonizeDirectedEdge<TCoordinate>, DirectedEdge<TCoordinate>>(
                    DirectedEdges);

            // label the current set of edgerings
            findLabeledEdgeRings(directedEdges);

            /*
            * Cut Edges are edges where both dirEdges have the same label.
            * Delete them, and record them
            */
            foreach (PolygonizeDirectedEdge<TCoordinate> de in directedEdges)
            {
                if (de.IsMarked)
                {
                    continue;
                }

                PolygonizeDirectedEdge<TCoordinate> sym = de.Sym as PolygonizeDirectedEdge<TCoordinate>;

                Debug.Assert(sym != null);

                if (de.Label == sym.Label)
                {
                    de.Marked = true;
                    sym.Marked = true;

                    // save the line as a cut edge
                    PolygonizeEdge<TCoordinate> e = de.Edge as PolygonizeEdge<TCoordinate>;
                    Debug.Assert(e != null);
                    yield return e.Line;
                }
            }
        }

        /// <summary>
        /// Marks all edges from the graph which are "dangles".
        /// Dangles are edges which are incident on a node with degree 1.
        /// This process is recursive, since removing a dangling edge
        /// may result in another edge becoming a dangle.
        /// In order to handle large recursion depths efficiently,
        /// an explicit recursion stack is used.
        /// </summary>
        /// <returns>
        /// A set containing the <see cref="ILineString{TCoordinate}"/>s 
        /// that formed dangles.
        /// </returns>
        public IEnumerable<ILineString<TCoordinate>> DeleteDangles()
        {
            IEnumerable<Node<TCoordinate>> nodesToRemove = FindNodesOfDegree(1);
            ISet<ILineString<TCoordinate>> dangleLines = new HashedSet<ILineString<TCoordinate>>();

            Stack<Node<TCoordinate>> nodeStack = new Stack<Node<TCoordinate>>(nodesToRemove);

            while (nodeStack.Count != 0)
            {
                Node<TCoordinate> node = nodeStack.Pop();

                DeleteAllEdges(node);

                foreach (PolygonizeDirectedEdge<TCoordinate> de in node.OutEdges.Edges)
                {
                    // delete this edge and its sym
                    de.Marked = true;

                    PolygonizeDirectedEdge<TCoordinate> sym = de.Sym as PolygonizeDirectedEdge<TCoordinate>;

                    if (sym != null)
                    {
                        sym.Marked = true;
                    }

                    // save the line as a dangle
                    PolygonizeEdge<TCoordinate> e = de.Edge as PolygonizeEdge<TCoordinate>;
                    Debug.Assert(e != null);
                    dangleLines.Add(e.Line);

                    Node<TCoordinate> toNode = de.ToNode;

                    // add the toNode to the list to be processed, if it is now a dangle
                    if (getDegreeNonDeleted(toNode) == 1)
                    {
                        nodeStack.Push(toNode);
                    }
                }
            }

            return dangleLines;
        }

        /// <param name="dirEdges">A List of the DirectedEdges in the graph.</param>
        /// <returns>A set of DirectedEdges, one for each edge ring found.</returns>
        private static IEnumerable<PolygonizeDirectedEdge<TCoordinate>> findLabeledEdgeRings(
            IEnumerable<PolygonizeDirectedEdge<TCoordinate>> dirEdges)
        {
            // label the edge rings formed
            Int64 currLabel = 1;

            foreach (PolygonizeDirectedEdge<TCoordinate> de in dirEdges)
            {
                if (de.IsMarked)
                {
                    continue;
                }

                if (de.Label >= 0)
                {
                    continue;
                }

                yield return de;
                IEnumerable<PolygonizeDirectedEdge<TCoordinate>> edges = findDirEdgesInRing(de);

                label(edges, currLabel);
                currLabel++;
            }
        }

        private static void label(IEnumerable<PolygonizeDirectedEdge<TCoordinate>> directedEdges, Int64 label)
        {
            foreach (PolygonizeDirectedEdge<TCoordinate> directedEdge in directedEdges)
            {
                directedEdge.Label = label;
            }
        }

        private static void computeNextCWEdges(Node<TCoordinate> node)
        {
            DirectedEdgeStar<TCoordinate> deStar = node.OutEdges;
            PolygonizeDirectedEdge<TCoordinate> startDE = null;
            PolygonizeDirectedEdge<TCoordinate> prevDE = null;

            // the edges are stored in CCW order around the star
            foreach (PolygonizeDirectedEdge<TCoordinate> outDE in deStar.Edges)
            {
                if (outDE.IsMarked)
                {
                    continue;
                }

                if (startDE == null)
                {
                    startDE = outDE;
                }

                if (prevDE != null)
                {
                    PolygonizeDirectedEdge<TCoordinate> sym = prevDE.Sym as PolygonizeDirectedEdge<TCoordinate>;
                    Debug.Assert(sym != null);
                    sym.Next = outDE;
                }

                prevDE = outDE;
            }

            if (prevDE != null)
            {
                PolygonizeDirectedEdge<TCoordinate> sym = prevDE.Sym as PolygonizeDirectedEdge<TCoordinate>;
                Debug.Assert(sym != null);
                sym.Next = startDE;
            }
        }

        /// <summary>
        /// Computes the next edge pointers going CCW around the given node, for the
        /// given edgering label.
        /// This algorithm has the effect of converting maximal edgerings into minimal edgerings
        /// </summary>
        private static void computeNextCCWEdges(Node<TCoordinate> node, Int64 label)
        {
            DirectedEdgeStar<TCoordinate> deStar = node.OutEdges;
            //PolyDirectedEdge lastInDE = null;
            PolygonizeDirectedEdge<TCoordinate> firstOutDE = null;
            PolygonizeDirectedEdge<TCoordinate> prevInDE = null;

            // the edges are stored in CCW order around the star
            IList<DirectedEdge<TCoordinate>> edges = deStar.Edges;

            //for (IEnumerator i = deStar.Edges.GetEnumerator(); i.MoveNext(); ) {
            for (Int32 i = edges.Count - 1; i >= 0; i--)
            {
                PolygonizeDirectedEdge<TCoordinate> de = edges[i] as PolygonizeDirectedEdge<TCoordinate>;
                Debug.Assert(de != null);
                PolygonizeDirectedEdge<TCoordinate> sym = de.Sym as PolygonizeDirectedEdge<TCoordinate>;
                Debug.Assert(sym != null);

                PolygonizeDirectedEdge<TCoordinate> outDE = null;

                if (de.Label == label)
                {
                    outDE = de;
                }

                PolygonizeDirectedEdge<TCoordinate> inDE = null;

                if (sym.Label == label)
                {
                    inDE = sym;
                }

                if (outDE == null && inDE == null)
                {
                    continue; // this edge is not in edgering
                }

                if (inDE != null)
                {
                    prevInDE = inDE;
                }

                if (outDE != null)
                {
                    if (prevInDE != null)
                    {
                        prevInDE.Next = outDE;
                        prevInDE = null;
                    }

                    if (firstOutDE == null)
                    {
                        firstOutDE = outDE;
                    }
                }
            }

            if (prevInDE != null)
            {
                Assert.IsTrue(firstOutDE != null);
                prevInDE.Next = firstOutDE;
            }
        }

        private Node<TCoordinate> GetNode(TCoordinate pt)
        {
            Node<TCoordinate> node = FindNode(pt);

            if (node == null)
            {
                node = new Node<TCoordinate>(pt);
                // ensure node is only added once to graph
                Add(node);
            }

            return node;
        }

        private void computeNextCWEdges()
        {
            // set the next pointers for the edges around each node
            foreach (Node<TCoordinate> node in Nodes)
            {
                computeNextCWEdges(node);
            }
        }

        /// <summary>
        /// Finds all nodes in a maximal edgering which are self-intersection nodes
        /// </summary>
        /// <returns> 
        /// The list of intersection nodes found,
        /// or null if no intersection nodes were found.       
        /// </returns>
        private static IEnumerable<Node<TCoordinate>> findIntersectionNodes(PolygonizeDirectedEdge<TCoordinate> startDE,
                                                                            Int64 label)
        {
            PolygonizeDirectedEdge<TCoordinate> de = startDE;

            do
            {
                Node<TCoordinate> node = de.FromNode;

                if (getDegree(node, label) > 1)
                {
                    yield return node;
                }

                de = de.Next;
                Assert.IsTrue(de != null, "Found null DE in ring.");
                Debug.Assert(de != null);
                Assert.IsTrue(de == startDE || !de.IsInRing, "Found DE already in ring.");
            } while (de != startDE);
        }

        /// <summary>
        /// Convert the maximal edge rings found by the initial graph traversal
        /// into the minimal edge rings required by NTS polygon topology rules.
        /// </summary>
        /// <param name="ringEdges">The list of start edges for the edgeRings to convert.</param>
        private void ConvertMaximalToMinimalEdgeRings(IEnumerable<PolygonizeDirectedEdge<TCoordinate>> ringEdges)
        {
            foreach (PolygonizeDirectedEdge<TCoordinate> de in ringEdges)
            {
                Int64 label = de.Label;
                IEnumerable<Node<TCoordinate>> intersectionNodes = findIntersectionNodes(de, label);

                Debug.Assert(intersectionNodes != null);

                // flip the next pointers on the intersection nodes to create minimal edge rings
                foreach (Node<TCoordinate> node in intersectionNodes)
                {
                    computeNextCCWEdges(node, label);
                }
            }
        }

        /// <summary>
        /// Traverse a ring of DirectedEdges, accumulating them into a list.
        /// This assumes that all dangling directed edges have been removed
        /// from the graph, so that there is always a next dirEdge.
        /// </summary>
        /// <param name="startDE">The DirectedEdge to start traversing at.</param>
        /// <returns>A List of DirectedEdges that form a ring.</returns>
        private static IEnumerable<PolygonizeDirectedEdge<TCoordinate>> findDirEdgesInRing(
            PolygonizeDirectedEdge<TCoordinate> startDE)
        {
            PolygonizeDirectedEdge<TCoordinate> de = startDE;

            do
            {
                yield return de;
                de = de.Next;
                Assert.IsTrue(de != null, "found null DE in ring");
                Debug.Assert(de != null);
                Assert.IsTrue(de == startDE || ! de.IsInRing, "found DE already in ring");
            } while (de != startDE);
        }

        private EdgeRing<TCoordinate> findEdgeRing(PolygonizeDirectedEdge<TCoordinate> startDE)
        {
            PolygonizeDirectedEdge<TCoordinate> de = startDE;
            EdgeRing<TCoordinate> er = new EdgeRing<TCoordinate>(_factory);

            do
            {
                er.Add(de);
                de.Ring = er;
                de = de.Next;
                Assert.IsTrue(de != null, "found null DE in ring");
                Debug.Assert(de != null);
                Assert.IsTrue(de == startDE || ! de.IsInRing, "found DE already in ring");
            } while (de != startDE);

            return er;
        }

        private static Int32 getDegreeNonDeleted(Node<TCoordinate> node)
        {
            IEnumerable<DirectedEdge<TCoordinate>> edges = node.OutEdges.Edges;
            Int32 degree = 0;

            foreach (PolygonizeDirectedEdge<TCoordinate> de in edges)
            {
                if (!de.IsMarked)
                {
                    degree++;
                }
            }

            return degree;
        }

        private static Int32 getDegree(Node<TCoordinate> node, Int64 label)
        {
            IEnumerable<DirectedEdge<TCoordinate>> edges = node.OutEdges.Edges;
            Int32 degree = 0;

            foreach (PolygonizeDirectedEdge<TCoordinate> de in edges)
            {
                if (de.Label == label)
                {
                    degree++;
                }
            }

            return degree;
        }
    }
}