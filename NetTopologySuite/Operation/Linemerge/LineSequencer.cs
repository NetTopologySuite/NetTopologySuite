using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Planargraph;
using GisSharpBlog.NetTopologySuite.Planargraph.Algorithm;
using GisSharpBlog.NetTopologySuite.Utilities;
using Iesi_NTS.Collections.Generic;

namespace GisSharpBlog.NetTopologySuite.Operation.Linemerge
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
        public static bool IsSequenced(IGeometry geom)
        {
            if (!(geom is IMultiLineString)) 
                return true;
        
            IMultiLineString mls = geom as IMultiLineString;

            // The nodes in all subgraphs which have been completely scanned
            ISet<ICoordinate> prevSubgraphNodes = new SortedSet<ICoordinate>();

            ICoordinate lastNode = null;
            IList<ICoordinate> currNodes = new List<ICoordinate>();
            for (int i = 0; i < mls.NumGeometries; i++) 
            {
                ILineString line = (ILineString) mls.GetGeometryN(i);
                ICoordinate startNode = line.GetCoordinateN(0);
                ICoordinate endNode   = line.GetCoordinateN(line.NumPoints - 1);

                /*
                 * If this linestring is connected to a previous subgraph, geom is not sequenced
                 */
                if (prevSubgraphNodes.Contains(startNode)) 
                    return false;
                if (prevSubgraphNodes.Contains(endNode)) 
                    return false;

                if (lastNode != null && startNode != lastNode) 
                {
                    // start new connected sequence
                    prevSubgraphNodes.AddAll(currNodes);
                    currNodes.Clear();
                }                

                currNodes.Add(startNode);
                currNodes.Add(endNode);
                lastNode = endNode;
            }
            return true;
        }

        private LineMergeGraph graph = new LineMergeGraph();

        // Initialize with default, in case no lines are input        
        private IGeometryFactory factory = GeometryFactory.Default;

        private IGeometry sequencedGeometry = null;
        
        private int lineCount = 0;
        private bool isRun = false;
        private bool isSequenceable = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="LineSequencer"/> class.
        /// </summary>
        public LineSequencer() { }
       
        /// <summary>
        /// Adds a <see cref="IEnumerable" /> of <see cref="Geometry" />s to be sequenced.
        /// May be called multiple times.
        /// Any dimension of Geometry may be added; the constituent linework will be extracted.
        /// </summary>
        /// <param name="geometries">A <see cref="IEnumerable" /> of geometries to add.</param>
        public void Add(IEnumerable<IGeometry> geometries)
        {
            foreach(IGeometry geometry in geometries)
                Add(geometry);            
        }

        /// <summary>
        /// Adds a <see cref="Geometry" /> to be sequenced.
        /// May be called multiple times.
        /// Any dimension of <see cref="Geometry" /> may be added; 
        /// the constituent linework will be extracted.
        /// </summary>
        /// <param name="geometry"></param>
        public void Add(IGeometry geometry) 
        {
            geometry.Apply(new GeometryComponentFilterImpl(this));             
        }

        /// <summary>
        /// A private implementation for <see cref="IGeometryComponentFilter" />
        /// </summary>
        internal class GeometryComponentFilterImpl : IGeometryComponentFilter
        {
            private LineSequencer sequencer = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="GeometryComponentFilterImpl"/> class.
            /// </summary>
            /// <param name="sequencer">The sequencer.</param>
            internal GeometryComponentFilterImpl(LineSequencer sequencer)
            {
                this.sequencer = sequencer;
            }

            /// <summary>
            /// Performs an operation with or on <paramref name="component" />
            /// </summary>
            /// <param name="component">
            /// A <see cref="Geometry" /> to which the filter is applied.
            /// </param>
            public void Filter(IGeometry component)
            {
                if (component is ILineString)
                    sequencer.AddLine(component as ILineString);                    
            }         
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineString"></param>
        internal void AddLine(ILineString lineString)
        {
            if (factory == null)
                factory = lineString.Factory;
            
            graph.AddEdge(lineString);
            lineCount++;
        }

        /// <summary>
        /// Tests whether the arrangement of linestrings has a valid sequence.
        /// </summary>
        /// <returns><c>true</c> if a valid sequence exists.</returns>
        public bool IsSequenceable()
        {            
            ComputeSequence();
            return isSequenceable;         
        }

        /// <summary>
        /// Returns the <see cref="LineString" /> or <see cref="MultiLineString" />
        /// built by the sequencing process, if one exists.
        /// </summary>
        /// <returns>The sequenced linestrings,
        /// or <c>null</c> if a valid sequence does not exist.</returns>
        public IGeometry GetSequencedLineStrings()
        {
            ComputeSequence();
            return sequencedGeometry;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ComputeSequence() 
        {
            if (isRun) 
                return;
            isRun = true;

            IList sequences = FindSequences();
            if (sequences == null)
                return;

            sequencedGeometry = BuildSequencedGeometry(sequences);
            isSequenceable = true;

            int finalLineCount = sequencedGeometry.NumGeometries;
            Assert.IsTrue(lineCount == finalLineCount, "Lines were missing from result");
            Assert.IsTrue(sequencedGeometry is ILineString || sequencedGeometry is IMultiLineString,
                "Result is not lineal");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IList FindSequences()
        {
            IList sequences = new ArrayList();
            ConnectedSubgraphFinder csFinder = new ConnectedSubgraphFinder(graph);
            IList subgraphs = csFinder.GetConnectedSubgraphs();
            foreach(Subgraph subgraph in subgraphs)
            {                
                if (HasSequence(subgraph))
                {
                    IList seq = FindSequence(subgraph);
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
        private bool HasSequence(Subgraph graph)
        {
            int oddDegreeCount = 0;
            IEnumerator i = graph.GetNodeEnumerator();
            while(i.MoveNext())
            {
                Node node = (Node) i.Current;
                if (node.Degree % 2 == 1)
                    oddDegreeCount++;
            }
            return oddDegreeCount <= 2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        private IList FindSequence(Subgraph graph)
        {            
            GraphComponent.SetVisited(graph.GetEdgeEnumerator(), false);

            Node startNode = FindLowestDegreeNode(graph);                        
            
            // HACK: we need to reverse manually the order: maybe sorting error?
            ArrayList list = (ArrayList) startNode.OutEdges.Edges;
            list.Reverse();

            IEnumerator ie = list.GetEnumerator();                        
            ie.MoveNext();

            DirectedEdge startDE = (DirectedEdge) ie.Current;            
            DirectedEdge startDESym = startDE.Sym;
            
            LinkedList<DirectedEdge> seq = new LinkedList<DirectedEdge>();
            LinkedListNode<DirectedEdge> pos = AddReverseSubpath(startDESym, null, seq, false);            
            while (pos != null)
            {
                DirectedEdge prev = pos.Value;
                DirectedEdge unvisitedOutDE = FindUnvisitedBestOrientedDE(prev.FromNode);
                if (unvisitedOutDE != null)
                {
                    DirectedEdge toInsert = unvisitedOutDE.Sym;
                    pos = AddReverseSubpath(toInsert, pos, seq, true);
                }
                else pos = pos.Previous;                
            }                       

            /*
             * At this point, we have a valid sequence of graph DirectedEdges, but it
             * is not necessarily appropriately oriented relative to the underlying geometry.
             */
            IList orientedSeq = Orient(new ArrayList(seq));
            return orientedSeq;
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
                DirectedEdge de = (DirectedEdge) obj;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="de"></param>
        /// <param name="pos"></param>
        /// <param name="list"></param>
        /// <param name="expectedClosed"></param>
        /// <returns></returns>
        private LinkedListNode<DirectedEdge> AddReverseSubpath(DirectedEdge de, LinkedListNode<DirectedEdge> pos,
                                                LinkedList<DirectedEdge> list,  bool expectedClosed)
        {
            // trace an unvisited path *backwards* from this de
            Node endNode = de.ToNode;
            Node fromNode = null;
            while (true)
            {
                if (pos == null)
                     pos = list.AddLast(de.Sym);
                else pos = list.AddAfter(pos, de.Sym);
                de.Edge.Visited = true;
                fromNode = de.FromNode;
                DirectedEdge unvisitedOutDE = FindUnvisitedBestOrientedDE(fromNode);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        private static Node FindLowestDegreeNode(Subgraph graph)
        {
            int minDegree = Int32.MaxValue;
            Node minDegreeNode = null;            
            IEnumerator i = graph.GetNodeEnumerator();
            while (i.MoveNext())
            {
                Node node = (Node) i.Current;
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
        /// <param name="seq">A <see cref="IList" /> of <see cref="DirectedEdge" />s.</param>
        /// <returns>
        /// A <see cref="IList" /> of <see cref="DirectedEdge" />s oriented appropriately.
        /// </returns>
        private IList Orient(IList seq)
        {
            DirectedEdge startEdge = (DirectedEdge) seq[0];
            DirectedEdge endEdge = (DirectedEdge) seq[seq.Count - 1];
            Node startNode = startEdge.FromNode;
            Node endNode = endEdge.ToNode;

            bool flipSeq = false;
            bool hasDegree1Node = (startNode.Degree == 1 || endNode.Degree == 1);

            if (hasDegree1Node)
            {
                bool hasObviousStartNode = false;

                // test end edge before start edge, to make result stable
                // (ie. if both are good starts, pick the actual start
                if (endEdge.ToNode.Degree == 1 && endEdge.EdgeDirection == false)
                {
                    hasObviousStartNode = true;
                    flipSeq = true;
                }
                if (startEdge.FromNode.Degree == 1 && startEdge.EdgeDirection == true)
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
            return seq;
        }

        /// <summary>
        /// Reverse the sequence.
        /// This requires reversing the order of the <see cref="DirectedEdge" />s, 
        /// and flipping each <see cref="DirectedEdge" /> as well.
        /// </summary>
        /// <param name="seq">
        /// A <see cref="IList"/> of <see cref="DirectedEdge" />s, 
        /// in sequential order.
        /// </param>
        /// <returns>The reversed sequence.</returns>
        private IList Reverse(IEnumerable seq)
        {
            LinkedList<DirectedEdge> newSeq = new LinkedList<DirectedEdge>();
            IEnumerator i = seq.GetEnumerator();
            while (i.MoveNext())
            {
                DirectedEdge de = (DirectedEdge) i.Current;
                newSeq.AddFirst(de.Sym);
            }
            return new ArrayList(newSeq);
        }

        /// <summary>
        /// Builds a geometry (<see cref="LineString" /> or <see cref="MultiLineString" />)
        /// representing the sequence.
        /// </summary>
        /// <param name="sequences">
        /// A <see cref="IList" /> of <see cref="IList" />s of <see cref="DirectedEdge" />s
        /// with <see cref="LineMergeEdge" />s as their parent edges.
        /// </param>
        /// <returns>
        /// The sequenced geometry, or <c>null</c> if no sequence exists.
        /// </returns>
        private IGeometry BuildSequencedGeometry(IEnumerable sequences)
        {
            IList lines = new ArrayList();

            IEnumerator i1 = sequences.GetEnumerator();
            while (i1.MoveNext())
            {
                IList seq = (IList) i1.Current;
                IEnumerator i2 = seq.GetEnumerator();
                while(i2.MoveNext())
                {
                    DirectedEdge de = (DirectedEdge) i2.Current;
                    LineMergeEdge e = (LineMergeEdge) de.Edge;
                    ILineString line = e.Line;

                    ILineString lineToAdd = line;
                    if (!de.EdgeDirection && !line.IsClosed)
                        lineToAdd = Reverse(line);

                    lines.Add(lineToAdd);
                }
            }

            if (lines.Count == 0)
                return factory.CreateMultiLineString(new ILineString[] { });
            return factory.BuildGeometry(lines);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static ILineString Reverse(ILineString line)
        {
            ICoordinate[] pts = line.Coordinates;                     
            Array.Reverse(pts);
            ILineString rev = line.Factory.CreateLineString(pts);
            rev.UserData = line.UserData; // Maintain UserData in reverse process
            return rev;
        }
    }    
}
