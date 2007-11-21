using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// A list of the <see cref="SegmentNode{TCoordinate}" />s 
    /// present along a noded <see cref="SegmentString{TCoordinate}"/>.
    /// </summary>
    public class SegmentNodeList<TCoordinate> : IEnumerable<SegmentNode<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly SortedList<SegmentNode<TCoordinate>, SegmentNode<TCoordinate>> _nodeMap 
            = new SortedList<SegmentNode<TCoordinate>, SegmentNode<TCoordinate>>();
        private readonly SegmentString<TCoordinate> _edge = null; // the parent edge

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentNodeList{TCoordinate}"/> class.
        /// </summary>
        /// <param name="edge">The edge.</param>
        public SegmentNodeList(SegmentString<TCoordinate> edge)
        {
            _edge = edge;
        }

        public SegmentString<TCoordinate> Edge
        {
            get { return _edge; }
        }

        /// <summary>
        /// Adds an intersection into the list, if it isn't already there.
        /// The input segmentIndex and dist are expected to be normalized.
        /// </summary>
        /// <returns>The SegmentIntersection found or added.</returns>
        public SegmentNode<TCoordinate> Add(TCoordinate intPt, Int32 segmentIndex)
        {
            SegmentNode<TCoordinate> eiNew = new SegmentNode<TCoordinate>(
                _edge, intPt, segmentIndex, _edge.GetSegmentOctant(segmentIndex));
            SegmentNode<TCoordinate> ei;

            if (_nodeMap.TryGetValue(eiNew, out ei))
            {
                // debugging sanity check
                Assert.IsTrue(ei.Coordinate.Equals(intPt), 
                    "Found equal nodes with different coordinates");

                return ei;
            }

            // node does not exist, so create it
            _nodeMap.Add(eiNew, eiNew);
            return eiNew;
        }

        /// <summary>
        /// Returns an iterator of SegmentNodes.
        /// </summary>
        /// <returns>An iterator of SegmentNodes.</returns>
        public IEnumerator<SegmentNode<TCoordinate>> GetEnumerator()
        {
            return _nodeMap.Values.GetEnumerator();
        }

        /// <summary>
        /// Adds nodes for the first and last points of the edge.
        /// </summary>
        private void addEndPoints()
        {
            Int32 maxSegIndex = _edge.Count - 1;
            Add(_edge.GetCoordinate(0), 0);
            Add(_edge.GetCoordinate(maxSegIndex), maxSegIndex);
        }

        /// <summary>
        /// Adds nodes for any collapsed edge pairs.
        /// Collapsed edge pairs can be caused by inserted nodes, or they can be
        /// pre-existing in the edge vertex list.
        /// In order to provide the correct fully noded semantics,
        /// the vertex at the base of a collapsed pair must also be added as a node.
        /// </summary>
        private void addCollapsedNodes()
        {
            List<Int32> collapsedVertexIndexes = new List<Int32>();

            findCollapsesFromInsertedNodes(collapsedVertexIndexes);
            findCollapsesFromExistingVertices(collapsedVertexIndexes);

            // node the collapses
            foreach (object obj in collapsedVertexIndexes)
            {
                Int32 vertexIndex = (Int32) obj;
                Add(_edge.GetCoordinate(vertexIndex), vertexIndex);
            }
        }

        /// <summary>
        /// Adds nodes for any collapsed edge pairs
        /// which are pre-existing in the vertex list.
        /// </summary>
        private void findCollapsesFromExistingVertices(List<Int32> collapsedVertexIndexes)
        {
            for (Int32 i = 0; i < _edge.Count - 2; i++)
            {
                TCoordinate p0 = _edge.GetCoordinate(i);
                TCoordinate p1 = _edge.GetCoordinate(i + 1);
                TCoordinate p2 = _edge.GetCoordinate(i + 2);

                if (p0.Equals(p2)) // add base of collapse as node
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
        private void findCollapsesFromInsertedNodes(List<Int32> collapsedVertexIndexes)
        {
            Int32[] collapsedVertexIndex = new Int32[1];

            SegmentNode<TCoordinate> eiPrev = Slice.GetFirst(this);
            
            // there should always be at least two entries in the list, since the endpoints are nodes
            foreach (SegmentNode<TCoordinate> ei in Slice.StartAt(1, this))
            {
                Boolean isCollapsed = findCollapseIndex(eiPrev, ei, collapsedVertexIndex);
                
                if (isCollapsed)
                {
                    collapsedVertexIndexes.Add(collapsedVertexIndex[0]);
                }

                eiPrev = ei;
            }
        }

        private static Boolean findCollapseIndex(SegmentNode<TCoordinate> ei0, SegmentNode<TCoordinate> ei1, Int32[] collapsedVertexIndex)
        {
            // only looking for equal nodes
            if (!ei0.Coordinate.Equals(ei1.Coordinate))
            {
                return false;
            }

            Int32 numVerticesBetween = ei1.SegmentIndex - ei0.SegmentIndex;

            if (!ei1.IsInterior)
            {
                numVerticesBetween--;
            }

            // if there is a single vertex between the two equal nodes, 
            // it is a collapsed node
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
        /// for a set of <see cref="SegmentString{TCoordinate}" />s).
        /// </summary>
        public IEnumerable<SegmentString<TCoordinate>> GetSplitEdges()
        {
            // ensure that the list has entries for the first and last point of the edge
            addEndPoints();
            addCollapsedNodes();



            // there should always be at least two entries in the list, 
            // since the endpoints are nodes
            SegmentNode<TCoordinate> eiPrev = Slice.GetFirst(this);

            foreach (SegmentNode<TCoordinate> node in this)
            {
                yield return createSplitEdge(eiPrev, node);
            }
        }

        //private void checkSplitEdgesCorrectness(IEnumerable<SegmentString<TCoordinate>> splitEdges)
        //{
        //    IEnumerable<TCoordinate> edgePts = Edge.Coordinates;

        //    // check that first and last points of split edges are same as endpoints of edge
        //    SegmentString<TCoordinate> split0 = (SegmentString<TCoordinate>)splitEdges[0];
        //    TCoordinate pt0 = split0.GetCoordinate(0);

        //    if (!pt0.Equals(edgePts[0]))
        //    {
        //        throw new Exception("bad split edge start point at " + pt0);
        //    }

        //    SegmentString<TCoordinate> splitn = (SegmentString<TCoordinate>)splitEdges[splitEdges.Count - 1];
        //    IEnumerable<TCoordinate> splitnPts = splitn.Coordinates;
        //    ICoordinate ptn = splitnPts[splitnPts.Length - 1];

        //    if (!ptn.Equals(edgePts[edgePts.Length - 1]))
        //    {
        //        throw new Exception("bad split edge end point at " + ptn);
        //    }
        //}

        /// <summary>
        /// Create a new "split edge" with the section of points between
        /// (and including) the two intersections.
        /// The label for the new edge is the same as the label for the parent edge.
        /// </summary>
        private SegmentString<TCoordinate> createSplitEdge(SegmentNode<TCoordinate> ei0, SegmentNode<TCoordinate> ei1)
        {
            Int32 npts = ei1.SegmentIndex - ei0.SegmentIndex + 2;

            TCoordinate lastSegStartPt = Edge.GetCoordinate(ei1.SegmentIndex);

            // if the last intersection point is not equal to the its segment start pt, 
            // add it to the points list as well.
            // (This check is needed because the distance metric is not totally reliable!)
            // The check for point equality is 2D only - Z values are ignored
            Boolean useIntPt1 = ei1.IsInterior || !ei1.Coordinate.Equals(lastSegStartPt);
            
            if (!useIntPt1)
            {
                npts--;
            }

            TCoordinate[] pts = new TCoordinate[npts];
            Int32 ipt = 0;
            pts[ipt++] = new TCoordinate(ei0.Coordinate);

            for (Int32 i = ei0.SegmentIndex + 1; i <= ei1.SegmentIndex; i++)
            {
                pts[ipt++] = Edge.GetCoordinate(i);
            }

            if (useIntPt1)
            {
                pts[ipt] = ei1.Coordinate;
            }

            return new SegmentString<TCoordinate>(pts, Edge.Data);
        }

        public void Write(StreamWriter outstream)
        {
            outstream.Write("Intersections:");

            foreach (SegmentNode<TCoordinate> ei in this)
            {
                ei.Write(outstream);
            }
        }

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /*
    internal class NodeVertexIterator<TCoordinate> : IEnumerator<SegmentNode<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private SegmentNodeList<TCoordinate> _nodeList;
        private SegmentString<TCoordinate> _edge;
        private IEnumerator<SegmentNode<TCoordinate>> _nodeIt;
        private SegmentNode<TCoordinate> _currNode = null;
        private SegmentNode<TCoordinate> _nextNode = null;
        private Int32 _currSegIndex = 0;
        private Boolean _isDisposed = false;

        private NodeVertexIterator(SegmentNodeList<TCoordinate> nodeList)
        {
            _nodeList = nodeList;
            _edge = nodeList.Edge;
            _nodeIt = nodeList.GetEnumerator();
        }

        public SegmentNode<TCoordinate> Current
        {
            get { checkDisposed(); return _currNode; }
        }

        public Boolean MoveNext()
        {
            checkDisposed();

            if (_currNode == null)
            {
                _currNode = _nextNode;
                _currSegIndex = _currNode.SegmentIndex;
                readNextNode();
                return true;
            }

            // check for trying to read too far
            if (_nextNode == null)
            {
                return false;
            }

            if (_nextNode.SegmentIndex == _currNode.SegmentIndex)
            {
                _currNode = _nextNode;
                _currSegIndex = _currNode.SegmentIndex;
                readNextNode();
                return true;
            }

            if (_nextNode.SegmentIndex > _currNode.SegmentIndex) {}
            return false;
        }

        public void Reset()
        {
            checkDisposed();
            _nodeIt.Reset();
        }

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _isDisposed = true;
        }

        #endregion

        private void checkDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private void readNextNode()
        {
            if (_nodeIt.MoveNext())
            {
                _nextNode = _nodeIt.Current;
            }
            else
            {
                _nextNode = null;
            }
        }

        #region IEnumerator Members

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion
    }
     */
}