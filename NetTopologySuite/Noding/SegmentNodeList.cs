using System;
using System.Collections;
using System.IO;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// A list of the <see cref="SegmentNode" />s present along a noded <see cref="SegmentString"/>.
    /// </summary>
    public class SegmentNodeList : IEnumerable
    {
        private IDictionary nodeMap = new SortedList();
        private SegmentString edge = null; // the parent edge

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentNodeList"/> class.
        /// </summary>
        /// <param name="edge">The edge.</param>
        public SegmentNodeList(SegmentString edge)
        {
            this.edge = edge;
        }

        public SegmentString Edge
        {
            get { return edge; }
        }

        /// <summary>
        /// Adds an intersection into the list, if it isn't already there.
        /// The input segmentIndex and dist are expected to be normalized.
        /// </summary>
        /// <returns>The SegmentIntersection found or added.</returns>
        public SegmentNode Add(ICoordinate intPt, Int32 segmentIndex)
        {
            SegmentNode eiNew = new SegmentNode(edge, intPt, segmentIndex, edge.GetSegmentOctant(segmentIndex));
            SegmentNode ei = (SegmentNode) nodeMap[eiNew];
            if (ei != null)
            {
                // debugging sanity check
                Assert.IsTrue(ei.Coordinate.Equals2D(intPt), "Found equal nodes with different coordinates");
                return ei;
            }
            // node does not exist, so create it
            nodeMap.Add(eiNew, eiNew);
            return eiNew;
        }

        /// <summary>
        /// Returns an iterator of SegmentNodes.
        /// </summary>
        /// <returns>An iterator of SegmentNodes.</returns>
        public IEnumerator GetEnumerator()
        {
            return nodeMap.Values.GetEnumerator();
        }

        /// <summary>
        /// Adds nodes for the first and last points of the edge.
        /// </summary>
        private void AddEndPoints()
        {
            Int32 maxSegIndex = edge.Count - 1;
            Add(edge.GetCoordinate(0), 0);
            Add(edge.GetCoordinate(maxSegIndex), maxSegIndex);
        }

        /// <summary>
        /// Adds nodes for any collapsed edge pairs.
        /// Collapsed edge pairs can be caused by inserted nodes, or they can be
        /// pre-existing in the edge vertex list.
        /// In order to provide the correct fully noded semantics,
        /// the vertex at the base of a collapsed pair must also be added as a node.
        /// </summary>
        private void AddCollapsedNodes()
        {
            IList collapsedVertexIndexes = new ArrayList();

            FindCollapsesFromInsertedNodes(collapsedVertexIndexes);
            FindCollapsesFromExistingVertices(collapsedVertexIndexes);

            // node the collapses
            foreach (object obj in collapsedVertexIndexes)
            {
                Int32 vertexIndex = (Int32) obj;
                Add(edge.GetCoordinate(vertexIndex), vertexIndex);
            }
        }

        /// <summary>
        /// Adds nodes for any collapsed edge pairs
        /// which are pre-existing in the vertex list.
        /// </summary>
        private void FindCollapsesFromExistingVertices(IList collapsedVertexIndexes)
        {
            for (Int32 i = 0; i < edge.Count - 2; i++)
            {
                ICoordinate p0 = edge.GetCoordinate(i);
                ICoordinate p1 = edge.GetCoordinate(i + 1);
                ICoordinate p2 = edge.GetCoordinate(i + 2);
                if (p0.Equals2D(p2)) // add base of collapse as node
                {
                    collapsedVertexIndexes.Add(i + 1);
                }
            }
        }

        /// <summary>
        /// Adds nodes for any collapsed edge pairs caused by inserted nodes
        /// Collapsed edge pairs occur when the same coordinate is inserted as a node
        /// both before and after an existing edge vertex.
        /// To provide the correct fully noded semantics,
        /// the vertex must be added as a node as well.
        /// </summary>
        private void FindCollapsesFromInsertedNodes(IList collapsedVertexIndexes)
        {
            Int32[] collapsedVertexIndex = new Int32[1];

            IEnumerator ie = GetEnumerator();
            ie.MoveNext();

            // there should always be at least two entries in the list, since the endpoints are nodes
            SegmentNode eiPrev = (SegmentNode) ie.Current;
            while (ie.MoveNext())
            {
                SegmentNode ei = (SegmentNode) ie.Current;
                Boolean isCollapsed = FindCollapseIndex(eiPrev, ei, collapsedVertexIndex);
                if (isCollapsed)
                {
                    collapsedVertexIndexes.Add(collapsedVertexIndex[0]);
                }
                eiPrev = ei;
            }
        }

        private Boolean FindCollapseIndex(SegmentNode ei0, SegmentNode ei1, Int32[] collapsedVertexIndex)
        {
            // only looking for equal nodes
            if (!ei0.Coordinate.Equals2D(ei1.Coordinate))
            {
                return false;
            }
            Int32 numVerticesBetween = ei1.SegmentIndex - ei0.SegmentIndex;
            if (!ei1.IsInterior)
            {
                numVerticesBetween--;
            }
            // if there is a single vertex between the two equal nodes, this is a collapse
            if (numVerticesBetween == 1)
            {
                collapsedVertexIndex[0] = ei0.SegmentIndex + 1;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates new edges for all the edges that the intersections in this
        /// list split the parent edge into.
        /// Adds the edges to the provided argument list
        /// (this is so a single list can be used to accumulate all split edges
        /// for a set of <see cref="SegmentString" />s).
        /// </summary>
        public void AddSplitEdges(IList edgeList)
        {
            // ensure that the list has entries for the first and last point of the edge
            AddEndPoints();
            AddCollapsedNodes();

            IEnumerator ie = GetEnumerator();
            ie.MoveNext();

            // there should always be at least two entries in the list, since the endpoints are nodes
            SegmentNode eiPrev = (SegmentNode) ie.Current;
            while (ie.MoveNext())
            {
                SegmentNode ei = (SegmentNode) ie.Current;
                SegmentString newEdge = CreateSplitEdge(eiPrev, ei);
                edgeList.Add(newEdge);
                eiPrev = ei;
            }
        }

        private void CheckSplitEdgesCorrectness(IList splitEdges)
        {
            ICoordinate[] edgePts = edge.Coordinates;

            // check that first and last points of split edges are same as endpoints of edge
            SegmentString split0 = (SegmentString) splitEdges[0];
            ICoordinate pt0 = split0.GetCoordinate(0);
            if (!pt0.Equals2D(edgePts[0]))
            {
                throw new Exception("bad split edge start point at " + pt0);
            }

            SegmentString splitn = (SegmentString) splitEdges[splitEdges.Count - 1];
            ICoordinate[] splitnPts = splitn.Coordinates;
            ICoordinate ptn = splitnPts[splitnPts.Length - 1];
            if (!ptn.Equals2D(edgePts[edgePts.Length - 1]))
            {
                throw new Exception("bad split edge end point at " + ptn);
            }
        }

        /// <summary>
        ///  Create a new "split edge" with the section of points between
        /// (and including) the two intersections.
        /// The label for the new edge is the same as the label for the parent edge.
        /// </summary>
        private SegmentString CreateSplitEdge(SegmentNode ei0, SegmentNode ei1)
        {
            Int32 npts = ei1.SegmentIndex - ei0.SegmentIndex + 2;

            ICoordinate lastSegStartPt = edge.GetCoordinate(ei1.SegmentIndex);
            // if the last intersection point is not equal to the its segment start pt, add it to the points list as well.
            // (This check is needed because the distance metric is not totally reliable!)
            // The check for point equality is 2D only - Z values are ignored
            Boolean useIntPt1 = ei1.IsInterior || !ei1.Coordinate.Equals2D(lastSegStartPt);
            if (!useIntPt1)
            {
                npts--;
            }

            ICoordinate[] pts = new ICoordinate[npts];
            Int32 ipt = 0;
            pts[ipt++] = new Coordinate(ei0.Coordinate);
            for (Int32 i = ei0.SegmentIndex + 1; i <= ei1.SegmentIndex; i++)
            {
                pts[ipt++] = edge.GetCoordinate(i);
            }
            if (useIntPt1)
            {
                pts[ipt] = ei1.Coordinate;
            }

            return new SegmentString(pts, edge.Data);
        }

        public void Write(StreamWriter outstream)
        {
            outstream.Write("Intersections:");
            foreach (object obj in this)
            {
                SegmentNode ei = (SegmentNode) obj;
                ei.Write(outstream);
            }
        }
    }

    internal class NodeVertexIterator : IEnumerator
    {
        private SegmentNodeList nodeList;
        private SegmentString edge;
        private IEnumerator nodeIt;
        private SegmentNode currNode = null;
        private SegmentNode nextNode = null;
        private Int32 currSegIndex = 0;

        private NodeVertexIterator(SegmentNodeList nodeList)
        {
            this.nodeList = nodeList;
            edge = nodeList.Edge;
            nodeIt = nodeList.GetEnumerator();
        }

        private void ReadNextNode()
        {
            if (nodeIt.MoveNext())
            {
                nextNode = (SegmentNode) nodeIt.Current;
            }
            else
            {
                nextNode = null;
            }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <exception cref="NotSupportedException">This method is not implemented.</exception>
        [Obsolete("Not implemented!")]
        public void Remove()
        {
            throw new NotSupportedException(GetType().Name);
        }

        public object Current
        {
            get { return currNode; }
        }

        public Boolean MoveNext()
        {
            if (currNode == null)
            {
                currNode = nextNode;
                currSegIndex = currNode.SegmentIndex;
                ReadNextNode();
                return true;
            }
            // check for trying to read too far
            if (nextNode == null)
            {
                return false;
            }

            if (nextNode.SegmentIndex == currNode.SegmentIndex)
            {
                currNode = nextNode;
                currSegIndex = currNode.SegmentIndex;
                ReadNextNode();
                return true;
            }

            if (nextNode.SegmentIndex > currNode.SegmentIndex) {}
            return false;
        }

        public void Reset()
        {
            nodeIt.Reset();
        }
    }
}