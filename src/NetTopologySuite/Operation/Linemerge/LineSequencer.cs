using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Planargraph;
using NetTopologySuite.Planargraph.Algorithm;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.Linemerge
{
    /// <summary>
    /// <para>
    /// Builds a sequence from a set of <see cref="LineString" />s,
    /// so that they are ordered end to end.
    /// A sequence is a complete non-repeating list of the linear
    /// components of the input.  Each linestring is oriented
    /// so that identical endpoints are adjacent in the list.
    /// </para>
    /// <para>
    /// The input linestrings may form one or more connected sets.
    /// The input linestrings should be correctly noded, or the results may
    /// not be what is expected.
    /// The output of this method is a single <see cref="MultiLineString" />,
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
    /// </summary>
    public class LineSequencer
    {
        /// <summary>
        /// Tests whether a <see cref="Geometry" /> is sequenced correctly.
        /// <see cref="LineString" />s are trivially sequenced.
        /// <see cref="MultiLineString" />s are checked for correct sequencing.
        /// Otherwise, <c>IsSequenced</c> is defined
        /// to be <c>true</c> for geometries that are not lineal.
        /// </summary>
        /// <param name="geom">The <see cref="Geometry" /> to test.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="Geometry" /> is sequenced or is not lineal.
        /// </returns>
        public static bool IsSequenced(Geometry geom)
        {
            if (!(geom is MultiLineString))
                return true;

            var mls = geom as MultiLineString;

            // The nodes in all subgraphs which have been completely scanned
            var prevSubgraphNodes = new HashSet<Coordinate>();

            Coordinate lastNode = null;
            var currNodes = new List<Coordinate>();
            for (int i = 0; i < mls.NumGeometries; i++)
            {
                var line = (LineString) mls.GetGeometryN(i);
                var startNode = line.GetCoordinateN(0);
                var endNode   = line.GetCoordinateN(line.NumPoints - 1);

                /*
                 * If this linestring is connected to a previous subgraph, geom is not sequenced
                 */
                if (prevSubgraphNodes.Contains(startNode))
                    return false;
                if (prevSubgraphNodes.Contains(endNode))
                    return false;

                if (lastNode != null && !startNode.Equals(lastNode))
                {
                    // start new connected sequence
                    prevSubgraphNodes.UnionWith(currNodes);
                    currNodes.Clear();
                }

                currNodes.Add(startNode);
                currNodes.Add(endNode);
                lastNode = endNode;
            }
            return true;
        }

        private readonly LineMergeGraph _graph = new LineMergeGraph();

        // Initialize with default, in case no lines are input
        private GeometryFactory _factory = GeometryFactory.Default;

        private Geometry _sequencedGeometry;

        private int _lineCount;
        private bool _isRun;
        private bool _isSequenceable;

        /// <summary>
        /// Adds a <see cref="IEnumerable{T}" /> of <see cref="Geometry" />s to be sequenced.
        /// May be called multiple times.
        /// Any dimension of Geometry may be added; the constituent linework will be extracted.
        /// </summary>
        /// <param name="geometries">A <see cref="IEnumerable{T}" /> of geometries to add.</param>
        public void Add(IEnumerable<Geometry> geometries)
        {
            foreach(var geometry in geometries)
                Add(geometry);
        }

        /// <summary>
        /// Adds a <see cref="Geometry" /> to be sequenced.
        /// May be called multiple times.
        /// Any dimension of <see cref="Geometry" /> may be added;
        /// the constituent linework will be extracted.
        /// </summary>
        /// <param name="geometry"></param>
        public void Add(Geometry geometry)
        {
            geometry.Apply(new GeometryComponentFilterImpl(this));
        }

        /// <summary>
        /// A private implementation for <see cref="IGeometryComponentFilter" />
        /// </summary>
        internal class GeometryComponentFilterImpl : IGeometryComponentFilter
        {
            private readonly LineSequencer _sequencer;

            /// <summary>
            /// Initializes a new instance of the <see cref="GeometryComponentFilterImpl"/> class.
            /// </summary>
            /// <param name="sequencer">The sequencer.</param>
            internal GeometryComponentFilterImpl(LineSequencer sequencer)
            {
                _sequencer = sequencer;
            }

            /// <summary>
            /// Performs an operation with or on <paramref name="component" />
            /// </summary>
            /// <param name="component">
            /// A <see cref="Geometry" /> to which the filter is applied.
            /// </param>
            public void Filter(Geometry component)
            {
                if (component is LineString)
                    _sequencer.AddLine(component as LineString);
            }
        }

        internal void AddLine(LineString lineString)
        {
            if (_factory == null)
                _factory = lineString.Factory;

            _graph.AddEdge(lineString);
            _lineCount++;
        }

        /// <summary>
        /// Tests whether the arrangement of linestrings has a valid sequence.
        /// </summary>
        /// <returns><c>true</c> if a valid sequence exists.</returns>
        public bool IsSequenceable()
        {
            ComputeSequence();
            return _isSequenceable;
        }

        /// <summary>
        /// Returns the <see cref="LineString" /> or <see cref="MultiLineString" />
        /// built by the sequencing process, if one exists.
        /// </summary>
        /// <returns>The sequenced linestrings,
        /// or <c>null</c> if a valid sequence does not exist.</returns>
        public Geometry GetSequencedLineStrings()
        {
            ComputeSequence();
            return _sequencedGeometry;
        }

        private void ComputeSequence()
        {
            if (_isRun)
                return;
            _isRun = true;

            var sequences = FindSequences();
            if (sequences == null)
                return;

            _sequencedGeometry = BuildSequencedGeometry(sequences);
            _isSequenceable = true;

            int finalLineCount = _sequencedGeometry.NumGeometries;
            Assert.IsTrue(_lineCount == finalLineCount, "Lines were missing from result");
            Assert.IsTrue(_sequencedGeometry is LineString || _sequencedGeometry is MultiLineString, "Result is not lineal");
        }

        private IList<IEnumerable<DirectedEdge>> FindSequences()
        {
            var sequences = new List<IEnumerable<DirectedEdge>>();
            var csFinder = new ConnectedSubgraphFinder(_graph);
            var subgraphs = csFinder.GetConnectedSubgraphs();
            foreach(var subgraph in subgraphs)
            {
                if (HasSequence(subgraph))
                {
                    var seq = FindSequence(subgraph);
                    sequences.Add(seq);
                }
                else
                {
                    // if any subgraph cannot be sequenced, abort
                    return null;
                }
            }
            return sequences;
        }

        /// <summary>
        /// Tests whether a complete unique path exists in a graph
        /// using Euler's Theorem.
        /// </summary>
        /// <param name="graph">The <see cref="Subgraph" /> containing the edges.</param>
        /// <returns><c>true</c> if a sequence exists.</returns>
        private static bool HasSequence(Subgraph graph)
        {
            int oddDegreeCount = 0;
            var i = graph.GetNodeEnumerator();
            while(i.MoveNext())
            {
                var node = (Node) i.Current;
                if (node.Degree % 2 == 1)
                    oddDegreeCount++;
            }
            return oddDegreeCount <= 2;
        }

        private static IEnumerable<DirectedEdge> FindSequence(Subgraph graph)
        {
            GraphComponent.SetVisited(graph.GetEdgeEnumerator(), false);

            var startNode = FindLowestDegreeNode(graph);

            var list = startNode.OutEdges.Edges;

            var ie = list.GetEnumerator();
            ie.MoveNext();

            var startDE = ie.Current;
            var startDESym = startDE.Sym;

            var seq = new LinkedList<DirectedEdge>();
            var pos = AddReverseSubpath(startDESym, null, seq, false);
            while (pos != null)
            {
                var prev = pos.Value;
                var unvisitedOutDE = FindUnvisitedBestOrientedDE(prev.FromNode);
                if (unvisitedOutDE != null)
                {
                    var toInsert = unvisitedOutDE.Sym;
                    pos = AddReverseSubpath(toInsert, pos, seq, true);
                }
                else pos = pos.Previous;
            }

            /*
             * At this point, we have a valid sequence of graph DirectedEdges, but it
             * is not necessarily appropriately oriented relative to the underlying geometry.
             */
            return Orient(seq/*new ArrayList(seq.CastPlatform())*/);
        }

        /// <summary>
        /// Finds an <see cref="DirectedEdge" /> for an unvisited edge (if any),
        /// choosing the <see cref="DirectedEdge" /> which preserves orientation, if possible.
        /// </summary>
        /// <param name="node">The <see cref="Node" /> to examine.</param>
        /// <returns>
        /// The <see cref="DirectedEdge" /> found,
        /// or <c>null</c> if none were unvisited.
        /// </returns>
        private static DirectedEdge FindUnvisitedBestOrientedDE(Node node)
        {
            DirectedEdge wellOrientedDE = null;
            DirectedEdge unvisitedDE = null;
            foreach(object obj in node.OutEdges)
            {
                var de = (DirectedEdge) obj;
                if (!de.Edge.IsVisited)
                {
                    unvisitedDE = de;
                    if (de.EdgeDirection)
                        wellOrientedDE = de;
                }
            }
            if (wellOrientedDE != null)
                return wellOrientedDE;
            return unvisitedDE;
        }

       private static LinkedListNode<DirectedEdge> AddReverseSubpath(
            DirectedEdge de, LinkedListNode<DirectedEdge> pos,
            LinkedList<DirectedEdge> list,  bool expectedClosed)
        {
            // trace an unvisited path *backwards* from this de
            var endNode = de.ToNode;
            Node fromNode;
            while (true)
            {
                if (pos == null)
                     pos = list.AddLast(de.Sym);
                else pos = list.AddAfter(pos, de.Sym);
                de.Edge.Visited = true;
                fromNode = de.FromNode;
                var unvisitedOutDE = FindUnvisitedBestOrientedDE(fromNode);
                // this must terminate, since we are continually marking edges as visited
                if (unvisitedOutDE == null)
                    break;
                de = unvisitedOutDE.Sym;
            }
            if (expectedClosed)
            {
                // the path should end at the toNode of this de, otherwise we have an error
                Assert.IsTrue(fromNode == endNode, "path not contiguous");
            }
            return pos;
        }

        private static Node FindLowestDegreeNode(Subgraph graph)
        {
            int minDegree = int.MaxValue;
            Node minDegreeNode = null;
            var i = graph.GetNodeEnumerator();
            while (i.MoveNext())
            {
                var node = i.Current;
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
        /// <param name="seq">A <see cref="IList{DirectedEdge}" /> of <see cref="DirectedEdge" />s.</param>
        /// <returns>
        /// A <see cref="IList{DirectedEdge}" /> of <see cref="DirectedEdge" />s oriented appropriately.
        /// </returns>
        private static List<DirectedEdge> Orient(LinkedList<DirectedEdge> seq)
        {
            var startEdge = seq.First.Value;
            var endEdge = seq.Last.Value;

            var startNode = startEdge.FromNode;
            var endNode = endEdge.ToNode;

            bool flipSeq = false;
            bool hasDegree1Node = (startNode.Degree == 1 || endNode.Degree == 1);

            if (hasDegree1Node)
            {
                bool hasObviousStartNode = false;

                // test end edge before start edge, to make result stable
                // (ie. if both are good starts, pick the actual start
                if (endEdge.ToNode.Degree == 1 && !endEdge.EdgeDirection)
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
                        flipSeq = true;
                    // if the end node is of degree 1, it is properly the end node
                }
            }

            // if there is no degree 1 node, just use the sequence as is
            // (Could insert heuristic of taking direction of majority of lines as overall direction)
            if (flipSeq)
                return Reverse(seq);
            return new List<DirectedEdge>(seq);
        }

        /// <summary>
        /// Reverse the sequence.
        /// This requires reversing the order of the <see cref="DirectedEdge" />s,
        /// and flipping each <see cref="DirectedEdge" /> as well.
        /// </summary>
        /// <param name="seq">
        /// A enumeration of <see cref="DirectedEdge" />s,
        /// in sequential order.
        /// </param>
        /// <returns>The reversed sequence.</returns>
        private static List<DirectedEdge> Reverse(IEnumerable<DirectedEdge> seq)
        {
            var tmp = new Stack<DirectedEdge>();

            foreach (var edge in seq)
            {
                // Add Edge that has the other direction instead of copying it.
                tmp.Push(edge.Sym);
            }

            return new List<DirectedEdge>(tmp);
            /*
            LinkedList<DirectedEdge> newSeq = new LinkedList<DirectedEdge>();
            IEnumerator i = seq.GetEnumerator();
            while (i.MoveNext())
            {
                DirectedEdge de = (DirectedEdge) i.Current;
                newSeq.AddFirst(de.Sym);
            }
            return new List<DirectedEdge>(newSeq);
             */
        }

        /// <summary>
        /// Builds a geometry (<see cref="LineString" /> or <see cref="MultiLineString" />)
        /// representing the sequence.
        /// </summary>
        /// <param name="sequences">
        /// An enumeration of  <see cref="IList{DirectedEdge}" />s of <see cref="DirectedEdge" />s
        /// with <see cref="LineMergeEdge" />s as their parent edges.
        /// </param>
        /// <returns>
        /// The sequenced geometry, or <c>null</c> if no sequence exists.
        /// </returns>
        private Geometry BuildSequencedGeometry(IEnumerable<IEnumerable<DirectedEdge>> sequences)
        {
            var lines = new List<Geometry>();

            foreach (IList<DirectedEdge> seq in sequences)
            {
                foreach (var de in seq)
                {
                    var e = (LineMergeEdge) de.Edge;
                    var line = e.Line;

                    var lineToAdd = line;
                    if (!de.EdgeDirection && !line.IsClosed)
                        lineToAdd = Reverse(line);

                    lines.Add(lineToAdd);
                }
            }

            return lines.Count == 0 ? _factory.CreateMultiLineString(new LineString[] { }) : _factory.BuildGeometry(lines);
        }

        private static LineString Reverse(LineString line)
        {
            // Use a new CoordinateSequence to avoiding reversing the Coordinates of the line
            var rev = line.Factory.CreateLineString(line.CoordinateSequence.Reversed());
            rev.UserData = line.UserData; // Maintain UserData in reverse process
            return rev;
        }
    }
}
