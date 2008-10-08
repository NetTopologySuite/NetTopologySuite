using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures.Collections.Generic;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Planargraph;
using GisSharpBlog.NetTopologySuite.Planargraph.Algorithm;
using NPack.Interfaces;
using GeoAPI.Diagnostics;
using GeoAPI.DataStructures;
#if DOTNET35
using System.Linq;
#endif

namespace GisSharpBlog.NetTopologySuite.Operation.Linemerge
{
    /// <summary>
    /// Builds a sequence from a set of <see cref="ILineString" />s,
    /// so that they are ordered end to end.
    /// A sequence is a complete non-repeating list of the linear
    /// components of the input.  Each linestring is oriented
    /// so that identical endpoints are adjacent in the list.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The input linestrings may form one or more connected sets.
    /// The input linestrings should be correctly noded, or the results may
    /// not be what is expected.
    /// The output of this method is a single <see cref="IMultiLineString" />,
    /// containing the ordered linestrings in the sequence.
    /// </para>
    /// <para>
    /// The sequencing employs the classic 'Eulerian path' graph algorithm.
    /// Since Eulerian paths are not uniquely determined, further rules are used to
    /// make the computed sequence preserve as much as possible of the input ordering.
    /// Within a connected subset of lines, the ordering rules are:    
    ///  - If there is degree-1 node which is the start
    /// node of an linestring, use that node as the start of the sequence.
    ///  - If there is a degree-1 node which is the end
    /// node of an linestring, use that node as the end of the sequence.
    ///  - If the sequence has no degree-1 nodes, use any node as the start
    /// </para>
    /// <para>
    /// Not all arrangements of lines can be sequenced.
    /// For a connected set of edges in a graph,
    /// Euler's Theorem states that there is a sequence containing each edge once
    /// if and only if there are no more than 2 nodes of odd degree.
    /// If it is not possible to find a sequence, the <see cref="IsSequenceable" /> 
    /// property will return <c>false</c>.
    /// </para>
    /// </remarks>
    public class LineSequencer<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// Tests whether a <see cref="Geometry{TCoordinate}" /> is sequenced correctly.
        /// <see cref="ILineString" />s are trivially sequenced.
        /// <see cref="IMultiLineString" />s are checked for correct sequencing.
        /// Otherwise, <c>IsSequenced</c> is defined
        /// to be <see langword="true"/> for geometries that are not lineal.
        /// </summary>
        /// <param name="geom">The <see cref="Geometry{TCoordinate}" /> to test.</param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="Geometry{TCoordinate}" /> is sequenced or is not lineal.
        /// </returns>
        public static Boolean IsSequenced(IGeometry<TCoordinate> geom)
        {
            if (!(geom is IMultiLineString<TCoordinate>))
            {
                return true;
            }

            IMultiLineString<TCoordinate> mls = geom as IMultiLineString<TCoordinate>;

            // The nodes in all subgraphs which have been completely scanned
            ISet<TCoordinate> prevSubgraphNodes = new SortedSet<TCoordinate>();

            TCoordinate lastNode = default(TCoordinate);
            List<TCoordinate> currNodes = new List<TCoordinate>();

            for (Int32 i = 0; i < mls.Count; i++)
            {
                ILineString<TCoordinate> line = mls[i];
                TCoordinate startNode = line.Coordinates[0];
                TCoordinate endNode = line.Coordinates[line.PointCount - 1];

                /*
                 * If this linestring is connected to a previous subgraph, geom is not sequenced
                 */
                if (prevSubgraphNodes.Contains(startNode))
                {
                    return false;
                }

                if (prevSubgraphNodes.Contains(endNode))
                {
                    return false;
                }

                if (!Coordinates<TCoordinate>.IsEmpty(lastNode) && !lastNode.Equals(startNode))
                {
                    // start new connected sequence
                    prevSubgraphNodes.AddRange(currNodes);
                    currNodes.Clear();
                }

                currNodes.Add(startNode);
                currNodes.Add(endNode);
                lastNode = endNode;
            }

            return true;
        }

        private readonly LineMergeGraph<TCoordinate> _graph = new LineMergeGraph<TCoordinate>();

        // Initialize with default, in case no lines are input        
        private IGeometryFactory<TCoordinate> _geoFactory;

        private IGeometry<TCoordinate> _sequencedGeometry = null;

        private Int32 _lineCount = 0;
        private Boolean _isRun = false;
        private Boolean _isSequenceable = false;

        /// <summary>
        /// Adds a <see cref="IEnumerable" /> of <see cref="Geometry{TCoordinate}" />s to be sequenced.
        /// May be called multiple times.
        /// Any dimension of Geometry may be added; the constituent linework will be extracted.
        /// </summary>
        /// <param name="geometries">A <see cref="IEnumerable" /> of geometries to add.</param>
        public void Add(IEnumerable<IGeometry> geometries)
        {
            foreach (IGeometry<TCoordinate> geometry in geometries)
            {
                Add(geometry);
            }
        }

        /// <summary>
        /// Adds a <see cref="Geometry{TCoordinate}" /> to be sequenced.
        /// May be called multiple times.
        /// Any dimension of <see cref="Geometry{TCoordinate}" /> may be added; 
        /// the constituent linework will be extracted.
        /// </summary>
        public void Add(IGeometry<TCoordinate> geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            if (geometry is IHasGeometryComponents<TCoordinate>)
            {
                IHasGeometryComponents<TCoordinate> container
                    = geometry as IHasGeometryComponents<TCoordinate>;

                foreach (ILineString<TCoordinate> s in container.Components)
                {
                    if (s != null)
                    {
                        addLine(s);
                    }
                }
            }
            else if (geometry is ILineString<TCoordinate>)
            {
                addLine(geometry as ILineString<TCoordinate>);
            }
        }
        
        /*
         * [codekaizen 2008-01-14]  removed during translation of visitor patterns
         *                          to enumeration / query patterns.
         */
        ///// <summary>
        ///// A private implementation for <see cref="IGeometryComponentFilter{TCoordinate}" />
        ///// </summary>
        //internal class GeometryComponentFilterImpl : IGeometryComponentFilter<TCoordinate>
        //{
        //    private readonly LineSequencer<TCoordinate> _sequencer = null;

        //    /// <summary>
        //    /// Initializes a new instance of the 
        //    /// <see cref="GeometryComponentFilterImpl"/> class.
        //    /// </summary>
        //    /// <param name="sequencer">The sequencer.</param>
        //    internal GeometryComponentFilterImpl(LineSequencer<TCoordinate> sequencer)
        //    {
        //        _sequencer = sequencer;
        //    }

        //    /// <summary>
        //    /// Performs an operation with or on <paramref name="component" />
        //    /// </summary>
        //    /// <param name="component">
        //    /// A <see cref="Geometry{TCoordinate}" /> to which the filter is applied.
        //    /// </param>
        //    public void Filter(IGeometry<TCoordinate> component)
        //    {
        //        if (component is ILineString<TCoordinate>)
        //        {
        //            _sequencer.AddLine(component as ILineString<TCoordinate>);
        //        }
        //    }
        //}

        private void addLine(ILineString<TCoordinate> lineString)
        {
            if (_geoFactory == null)
            {
                _geoFactory = lineString.Factory;
            }

            _graph.AddEdge(lineString);
            _lineCount++;
        }

        /// <summary>
        /// Tests whether the arrangement of linestrings has a valid sequence.
        /// </summary>
        /// <returns><see langword="true"/> if a valid sequence exists.</returns>
        public Boolean IsSequenceable()
        {
            computeSequence();
            return _isSequenceable;
        }

        /// <summary>
        /// Returns the <see cref="LineString{TCoordinate}" /> or <see cref="MultiLineString{TCoordinate}" />
        /// built by the sequencing process, if one exists.
        /// </summary>
        /// <returns>The sequenced linestrings,
        /// or <see langword="null" /> if a valid sequence does not exist.</returns>
        public IGeometry<TCoordinate> GetSequencedLineStrings()
        {
            computeSequence();
            return _sequencedGeometry;
        }

        private void computeSequence()
        {
            if (_isRun)
            {
                return;
            }

            _isRun = true;

            IEnumerable<IEnumerable<DirectedEdge<TCoordinate>>> sequences = findSequences();

            if (sequences == null)
            {
                return;
            }

            _sequencedGeometry = buildSequencedGeometry(sequences);
            _isSequenceable = true;

            Int32 finalLineCount = (_sequencedGeometry is IGeometryCollection)
                ? ((IGeometryCollection)_sequencedGeometry).Count
                : 1;

            Assert.IsTrue(_lineCount == finalLineCount, "Lines were missing from result");
            Assert.IsTrue(_sequencedGeometry is ILineString || _sequencedGeometry is IMultiLineString,
                          "Result is not lineal");
        }

        private IEnumerable<IEnumerable<DirectedEdge<TCoordinate>>> findSequences()
        {
            ConnectedSubgraphFinder<TCoordinate> csFinder = new ConnectedSubgraphFinder<TCoordinate>(_graph);
            IEnumerable<Subgraph<TCoordinate>> subgraphs = csFinder.FindConnectedSubgraphs();

            foreach (Subgraph<TCoordinate> subgraph in subgraphs)
            {
                if (hasSequence(subgraph))
                {
                    yield return findSequence(subgraph);
                }
                else
                {
                    // if any subgraph cannot be sequenced, abort
                    yield break;
                }
            }
        }

        /// <summary>
        /// Tests whether a complete unique path exists in a graph
        /// using Euler's Theorem.
        /// </summary>
        /// <param name="graph">The <see cref="Subgraph{TCoordinate}" /> containing the edges.</param>
        /// <returns><see langword="true"/> if a sequence exists.</returns>
        private Boolean hasSequence(Subgraph<TCoordinate> graph)
        {
            Int32 oddDegreeCount = 0;
            foreach (Node<TCoordinate> node in graph.Nodes)
            {
                if (node.Degree % 2 == 1)
                {
                    oddDegreeCount++;
                }
            }

            return oddDegreeCount <= 2;
        }

        private static IEnumerable<DirectedEdge<TCoordinate>> findSequence(Subgraph<TCoordinate> graph)
        {
            IEnumerable<GraphComponent<TCoordinate>> edges =
                Caster.Upcast<GraphComponent<TCoordinate>, Edge<TCoordinate>>(graph.Edges);

            GraphComponent<TCoordinate>.SetVisited(edges, false);

            Node<TCoordinate> startNode = findLowestDegreeNode(graph);

            // HACK: we need to reverse manually the order: maybe sorting error?
            //ArrayList list = (ArrayList)startNode.OutEdges.Edges;
            //list.Reverse();


            DirectedEdge<TCoordinate> startDE = Slice.GetFirst(startNode.OutEdges.Edges);
            DirectedEdge<TCoordinate> startDESym = startDE.Sym;

            LinkedList<DirectedEdge<TCoordinate>> seq = new LinkedList<DirectedEdge<TCoordinate>>();
            LinkedListNode<DirectedEdge<TCoordinate>> pos = addReverseSubpath(startDESym, null, seq, false);

            while (pos != null)
            {
                DirectedEdge<TCoordinate> prev = pos.Value;
                DirectedEdge<TCoordinate> unvisitedOutDE = findUnvisitedBestOrientedDE(prev.FromNode);

                if (unvisitedOutDE != null)
                {
                    DirectedEdge<TCoordinate> toInsert = unvisitedOutDE.Sym;
                    pos = addReverseSubpath(toInsert, pos, seq, true);
                }
                else
                {
                    pos = pos.Previous;
                }
            }

            /*
             * At this point, we have a valid sequence of graph DirectedEdges, but it
             * is not necessarily appropriately oriented relative to the underlying geometry.
             */
            IEnumerable<DirectedEdge<TCoordinate>> orientedSeq = orient(seq);
            return orientedSeq;
        }

        /// <summary>
        /// Finds an <see cref="DirectedEdge{TCoordinate}" /> for an unvisited edge (if any),
        /// choosing the <see cref="DirectedEdge{TCoordinate}" /> which preserves orientation, if possible.
        /// </summary>
        /// <param name="node">The <see cref="Node{TCoordinate}" /> to examine.</param>
        /// <returns>
        /// The <see cref="DirectedEdge{TCoordinate}" /> found, 
        /// or <see langword="null" /> if none were unvisited.
        /// </returns>
        private static DirectedEdge<TCoordinate> findUnvisitedBestOrientedDE(Node<TCoordinate> node)
        {
            DirectedEdge<TCoordinate> wellOrientedDE = null;
            DirectedEdge<TCoordinate> unvisitedDE = null;

            foreach (DirectedEdge<TCoordinate> de in node.OutEdges)
            {
                if (!de.Edge.IsVisited)
                {
                    unvisitedDE = de;
                    if (de.EdgeDirection)
                    {
                        wellOrientedDE = de;
                    }
                }
            }

            if (wellOrientedDE != null)
            {
                return wellOrientedDE;
            }

            return unvisitedDE;
        }

        private static LinkedListNode<DirectedEdge<TCoordinate>> addReverseSubpath(
            DirectedEdge<TCoordinate> de, LinkedListNode<DirectedEdge<TCoordinate>> pos,
            LinkedList<DirectedEdge<TCoordinate>> list, Boolean expectedClosed)
        {
            // trace an unvisited path *backwards* from this de
            Node<TCoordinate> endNode = de.ToNode;
            Node<TCoordinate> fromNode;

            while (true)
            {
                if (pos == null)
                {
                    pos = list.AddLast(de.Sym);
                }
                else
                {
                    pos = list.AddAfter(pos, de.Sym);
                }

                de.Edge.Visited = true;
                fromNode = de.FromNode;
                DirectedEdge<TCoordinate> unvisitedOutDE = findUnvisitedBestOrientedDE(fromNode);

                // this must terminate, since we are continually marking edges as visited
                if (unvisitedOutDE == null)
                {
                    break;
                }

                de = unvisitedOutDE.Sym;
            }

            if (expectedClosed)
            {
                // the path should end at the toNode of this de, otherwise we have an error
                Assert.IsTrue(fromNode == endNode, "path not contiguous");
            }

            return pos;
        }

        private static Node<TCoordinate> findLowestDegreeNode(PlanarGraph<TCoordinate> graph)
        {
            Int32 minDegree = Int32.MaxValue;
            Node<TCoordinate> minDegreeNode = null;

            foreach (Node<TCoordinate> node in graph.Nodes)
            {
                if (minDegreeNode == null || node.Degree < minDegree)
                {
                    minDegree = node.Degree;
                    minDegreeNode = node;
                }
            }

            return minDegreeNode;
        }

        /// <summary>
        /// Computes a version of the sequence which is optimally
        /// oriented relative to the underlying geometry.
        /// <para>
        /// Heuristics used are:   
        ///  - If the path has a degree-1 node which is the start
        /// node of an linestring, use that node as the start of the sequence.
        ///  - If the path has a degree-1 node which is the end
        /// node of an linestring, use that node as the end of the sequence.
        ///  - If the sequence has no degree-1 nodes, use any node as the start
        /// (NOTE: in this case could orient the sequence according to the majority of the
        /// linestring orientations).
        /// </para>
        /// </summary>
        /// <param name="seq">A set of <see cref="DirectedEdge{TCoordinate}" />s.</param>
        /// <returns>
        /// A set of <see cref="DirectedEdge{TCoordinate}" />s oriented appropriately.
        /// </returns>
        private static IEnumerable<DirectedEdge<TCoordinate>> orient(IEnumerable<DirectedEdge<TCoordinate>> seq)
        {
            DirectedEdge<TCoordinate> startEdge = Slice.GetFirst(seq);
            DirectedEdge<TCoordinate> endEdge = Slice.GetLast(seq);

            Node<TCoordinate> startNode = startEdge.FromNode;
            Node<TCoordinate> endNode = endEdge.ToNode;

            Boolean flipSeq = false;
            Boolean hasDegree1Node = (startNode.Degree == 1 || endNode.Degree == 1);

            if (hasDegree1Node)
            {
                Boolean hasObviousStartNode = false;

                // test end edge before start edge, to make result stable
                // (ie. if both are good starts, pick the actual start
                if (endEdge.ToNode.Degree == 1 && endEdge.EdgeDirection == false)
                {
                    hasObviousStartNode = true;
                    flipSeq = true;
                }

                if (startEdge.FromNode.Degree == 1 && startEdge.EdgeDirection)
                {
                    hasObviousStartNode = true;
                    flipSeq = false;
                }

                // since there is no obvious start node, use any node of degree 1
                if (!hasObviousStartNode)
                {
                    // check if the start node should actually be the end node
                    if (startEdge.FromNode.Degree == 1)
                    {
                        flipSeq = true;
                    }
                    // if the end node is of degree 1, it is properly the end node
                }
            }

            // if there is no degree 1 node, just use the sequence as is
            // (Could insert heuristic of taking direction of majority of lines as overall direction)

            if (flipSeq)
            {
                return Enumerable.Reverse(seq);
            }

            return seq;
        }

        ///// <summary>
        ///// Reverse the sequence.
        ///// This requires reversing the order of the <see cref="DirectedEdge" />s, 
        ///// and flipping each <see cref="DirectedEdge" /> as well.
        ///// </summary>
        ///// <param name="seq">
        ///// A <see cref="IList"/> of <see cref="DirectedEdge" />s, 
        ///// in sequential order.
        ///// </param>
        ///// <returns>The reversed sequence.</returns>
        //private IList Reverse(IList seq)
        //{
        //    LinkedList<DirectedEdge> newSeq = new LinkedList<DirectedEdge>();
        //    IEnumerator i = seq.GetEnumerator();
        //    while (i.MoveNext())
        //    {
        //        DirectedEdge de = (DirectedEdge)i.Current;
        //        newSeq.AddFirst(de.Sym);
        //    }
        //    return new ArrayList(newSeq);
        //}

        /// <summary>
        /// Builds a geometry (<see cref="ILineString{TCoordinate}" /> 
        /// or <see cref="IMultiLineString{TCoordinate}" />)
        /// representing the sequence.
        /// </summary>
        /// <param name="sequences">
        /// A set of sets of <see cref="DirectedEdge{TCoordinate}" />s
        /// with <see cref="LineMergeEdge{TCoordinate}" />s as their parent edges.
        /// </param>
        /// <returns>
        /// The sequenced geometry, or <see langword="null" /> if no sequence exists.
        /// </returns>
        private IGeometry<TCoordinate> buildSequencedGeometry(IEnumerable<IEnumerable<DirectedEdge<TCoordinate>>> sequences)
        {
            List<IGeometry<TCoordinate>> lines = new List<IGeometry<TCoordinate>>();

            foreach (IEnumerable<DirectedEdge<TCoordinate>> sequence in sequences)
            {
                foreach (DirectedEdge<TCoordinate> de in sequence)
                {
                    LineMergeEdge<TCoordinate> e = de.Edge as LineMergeEdge<TCoordinate>;
                    Debug.Assert(e != null);
                    ILineString<TCoordinate> line = e.Line;
                    ILineString<TCoordinate> lineToAdd = line;

                    if (!de.EdgeDirection && !line.IsClosed)
                    {
                        lineToAdd = line.Reverse();
                    }

                    lines.Add(lineToAdd);
                }
            }

            if (lines.Count == 0)
            {
                return _geoFactory.CreateMultiLineString();
            }

            return _geoFactory.BuildGeometry(lines);
        }

        //private static ILineString<TCoordinate> Reverse(ILineString<TCoordinate> line)
        //{
        //    return line.Reverse();
        //    //ICoordinate[] pts = line.Coordinates;
        //    //Array.Reverse(pts);
        //    //return line.Factory.CreateLineString(pts);
        //}
    }
}