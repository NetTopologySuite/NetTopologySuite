using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// An ordered enumeration of the <see cref="SegmentNode{TCoordinate}" />s 
    /// present along a noded <see cref="NodedSegmentString{TCoordinate}"/>.
    /// </summary>
    public class SegmentNodeList<TCoordinate> : IEnumerable<SegmentNode<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly SortedList<SegmentNode<TCoordinate>, Object> _nodeList
            = new SortedList<SegmentNode<TCoordinate>, Object>();
        private readonly NodedSegmentString<TCoordinate> _segments; // the parent edge

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentNodeList{TCoordinate}"/> class.
        /// </summary>
        /// <param name="segments">The edge.</param>
        public SegmentNodeList(NodedSegmentString<TCoordinate> segments)
        {
            _segments = segments; 
        }

        public NodedSegmentString<TCoordinate> ParentSegments
        {
            get { return _segments; }
        }

        /// <summary>
        /// Adds an intersection into the list, if it isn't already there.
        /// The input <paramref name="segmentIndex"/> is expected to be normalized.
        /// </summary>
        /// <returns>The <see cref="SegmentNode{TCoordinate}"/> found or added.</returns>
        public SegmentNode<TCoordinate> Add(TCoordinate intersectionPoint, Int32 segmentIndex)
        {
            SegmentNode<TCoordinate> node = new SegmentNode<TCoordinate>(
                _segments, intersectionPoint, segmentIndex, _segments.GetSegmentOctant(segmentIndex));

            if (!_nodeList.ContainsKey(node))
            {
                // node does not exist, so create it
                _nodeList.Add(node, null);
            }

            return node;
        }

        /// <summary>
        /// Returns an iterator of SegmentNodes.
        /// </summary>
        /// <returns>An iterator of SegmentNodes.</returns>
        public IEnumerator<SegmentNode<TCoordinate>> GetEnumerator()
        {
            return _nodeList.Keys.GetEnumerator();
        }

        /// <summary>
        /// Creates new edges for all the edges that the intersections in this
        /// list split the parent edge into.
        /// </summary>
        public IEnumerable<NodedSegmentString<TCoordinate>> CreateSplitEdges()
        {
            // ensure that the list has entries for the first and last point of the edge
            addEndPoints();
            addCollapsedNodes();

            // there should always be at least two entries in the list, 
            // since the endpoints are nodes
            SegmentNode<TCoordinate> previousNode = Slice.GetFirst(this);

            foreach (SegmentNode<TCoordinate> node in Slice.StartAt(this, 1))
            {
                yield return createSplitEdge(previousNode, node);
            }
        }

        public void Write(StreamWriter outstream)
        {
            outstream.Write("Intersections:");

            foreach (SegmentNode<TCoordinate> edgeIntersection in this)
            {
                edgeIntersection.Write(outstream);
            }
        }

        // Adds nodes for the first and last points of the edge.
        private void addEndPoints()
        {
            Int32 maxSegIndex = _segments.Count - 1;
            Add(_segments.Coordinates[0], 0);
            Add(_segments.Coordinates[maxSegIndex], maxSegIndex);
        }

        // Adds nodes for any collapsed edge pairs.
        // Collapsed edge pairs can be caused by inserted nodes, or they can be
        // pre-existing in the edge vertex list.
        // In order to provide the correct fully noded semantics,
        // the vertex at the base of a collapsed pair must also be added as a node.
        private void addCollapsedNodes()
        {
            IEnumerable<Int32> collapseIndexes = findCollapsesFromInsertedNodes();
            collapseIndexes = Slice.Append(collapseIndexes, findCollapsesFromExistingVertices());

            // node the collapses
            foreach (Int32 vertexIndex in collapseIndexes)
            {
                Add(_segments.Coordinates[vertexIndex], vertexIndex);
            }
        }

        // Adds nodes for any collapsed edge pairs
        // which are pre-existing in the vertex list.
        private IEnumerable<Int32> findCollapsesFromExistingVertices()
        {
            Int32 i = 0;

            foreach (Triple<TCoordinate> triple in Slice.GetOverlappingTriples(_segments.Coordinates))
            {
                if (triple.First.Equals(triple.Third)) // add base of collapse as node
                {
                    yield return i + 1;
                }

                i += 1;
            }
        }

        // Adds nodes for any collapsed edge pairs caused by inserted nodes
        // Collapsed edge pairs occur when the same coordinate is inserted as a node
        // both before and after an existing edge vertex.
        // To provide the correct fully noded semantics,
        // the vertex must be added as a node as well.
        private IEnumerable<Int32> findCollapsesFromInsertedNodes()
        {
            SegmentNode<TCoordinate> previousEdgeIntersection = Slice.GetFirst(this);
             
            // there should always be at least two entries in the list, since the endpoints are nodes
            foreach (SegmentNode<TCoordinate> edgeIntersection in Slice.StartAt(this, 1))
            {
                Int32 collapsedVertexIndex;

                Boolean isCollapsed = findCollapseIndex(
                    previousEdgeIntersection, edgeIntersection, out collapsedVertexIndex);

                if (isCollapsed)
                {
                    yield return collapsedVertexIndex;
                }

                previousEdgeIntersection = edgeIntersection;
            }
        }

        private static Boolean findCollapseIndex(
            SegmentNode<TCoordinate> edgeIntersection0, SegmentNode<TCoordinate> edgeIntersection1, 
            out Int32 collapsedVertexIndex)
        {
            collapsedVertexIndex = -1;

            // only looking for equal nodes
            if (!edgeIntersection0.Coordinate.Equals(edgeIntersection1.Coordinate))
            {
                return false;
            }

            Int32 interiorVertexesCount = edgeIntersection1.SegmentIndex - edgeIntersection0.SegmentIndex;

            if (!edgeIntersection1.IsInterior)
            {
                interiorVertexesCount--;
            }

            // if there is a single vertex between the two equal nodes, 
            // it is a collapsed node
            if (interiorVertexesCount == 1)
            {
                collapsedVertexIndex = edgeIntersection0.SegmentIndex + 1;
                return true;
            }

            return false;
        }

        // Create a new "split edge" with the section of points between
        // (and including) the two intersections.
        // The label for the new edge is the same as the label for the parent edge.
        private NodedSegmentString<TCoordinate> createSplitEdge(
            SegmentNode<TCoordinate> edgeIntersection0, SegmentNode<TCoordinate> edgeIntersection1)
        {
            TCoordinate lastSegStartPt = ParentSegments.Coordinates[edgeIntersection1.SegmentIndex];

            // if the last intersection point is not equal to the its segment start pt, 
            // add it to the points list as well.
            // (This check is needed because the distance metric is not totally reliable!)
            // The check for point equality is 2D only - Z values are ignored
            Boolean useIntersectionPoint1 = edgeIntersection1.IsInterior || 
                                            !edgeIntersection1.Coordinate.Equals(lastSegStartPt);

            ICoordinateSequence<TCoordinate> pts;
            Int32 start = edgeIntersection0.SegmentIndex + 1;
            Int32 end = edgeIntersection1.SegmentIndex;

            pts = ParentSegments.Coordinates.Slice(start, end);

            Slice.Prepend(pts, edgeIntersection0.Coordinate);

            if (useIntersectionPoint1)
            {
                Slice.Append(pts, edgeIntersection1.Coordinate);
            }

            return new NodedSegmentString<TCoordinate>(pts, ParentSegments.Context);
        }

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
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

    /*
    internal class NodeVertexIterator<TCoordinate> : IEnumerator<SegmentNode<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
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