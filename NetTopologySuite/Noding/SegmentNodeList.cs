using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// A list of the <see cref="SegmentNode" />s present along a noded <see cref="ISegmentString"/>.
    /// </summary>
    public class SegmentNodeList : IEnumerable<object>
    {
        private readonly IDictionary<SegmentNode, Object> _nodeMap = new SortedDictionary<SegmentNode, Object>();
        private readonly NodedSegmentString _edge;  // the parent edge

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentNodeList"/> class.
        /// </summary>
        /// <param name="edge">The edge.</param>
        public SegmentNodeList(NodedSegmentString edge)
        {
            _edge = edge;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public NodedSegmentString Edge 
        {
            get { return _edge; }
        }

        /// <summary>
        /// Adds an intersection into the list, if it isn't already there.
        /// The input segmentIndex and dist are expected to be normalized.
        /// </summary>
        /// <param name="intPt"></param>
        /// <param name="segmentIndex"></param>
        /// <returns>The SegmentIntersection found or added.</returns>
        public SegmentNode Add(Coordinate intPt, int segmentIndex)
        {
            var eiNew = new SegmentNode(_edge, intPt, segmentIndex, _edge.GetSegmentOctant(segmentIndex));
            object eiObj;
            if (_nodeMap.TryGetValue(eiNew, out eiObj))
            {
                var ei = (SegmentNode)eiObj;
                // debugging sanity check
                Assert.IsTrue(ei.Coord.Equals2D(intPt), "Found equal nodes with different coordinates");               
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
        public IEnumerator<object>GetEnumerator() 
        { 
            return _nodeMap.Values.GetEnumerator(); 
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Adds nodes for the first and last points of the edge.
        /// </summary>
        private void AddEndpoints()
        {
            var maxSegIndex = _edge.Count - 1;
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
        private void AddCollapsedNodes()
        {
            IList<int> collapsedVertexIndexes = new List<int>();

            FindCollapsesFromInsertedNodes(collapsedVertexIndexes);
            FindCollapsesFromExistingVertices(collapsedVertexIndexes);

            // node the collapses
            foreach(var vertexIndex in collapsedVertexIndexes)
                Add(_edge.GetCoordinate(vertexIndex), vertexIndex);
        }

        /// <summary>
        /// Adds nodes for any collapsed edge pairs
        /// which are pre-existing in the vertex list.
        /// </summary>
        /// <param name="collapsedVertexIndexes"></param>
        private void FindCollapsesFromExistingVertices(IList<int> collapsedVertexIndexes)
        {
            for (var i = 0; i < _edge.Count - 2; i++)
            {
                var p0 = _edge.GetCoordinate(i);
                //var p1 = _edge.GetCoordinate(i + 1);
                var p2 = _edge.GetCoordinate(i + 2);
                if (p0.Equals2D(p2))    // add base of collapse as node
                    collapsedVertexIndexes.Add(i + 1);                
            }
        }

        /// <summary>
        /// Adds nodes for any collapsed edge pairs caused by inserted nodes
        /// Collapsed edge pairs occur when the same coordinate is inserted as a node
        /// both before and after an existing edge vertex.
        /// To provide the correct fully noded semantics,
        /// the vertex must be added as a node as well.
        /// </summary>
        /// <param name="collapsedVertexIndexes"></param>
        private void FindCollapsesFromInsertedNodes(IList<int> collapsedVertexIndexes)
        {
            var collapsedVertexIndex = new int[1];
            
	        var ie = GetEnumerator();
	        ie.MoveNext();

            // there should always be at least two entries in the list, since the endpoints are nodes
            var eiPrev = (SegmentNode) ie.Current;
            while (ie.MoveNext())
            {
                var ei = (SegmentNode) ie.Current;
                var isCollapsed = FindCollapseIndex(eiPrev, ei, collapsedVertexIndex);
                if (isCollapsed)
                    collapsedVertexIndexes.Add(collapsedVertexIndex[0]);
                eiPrev = ei;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ei0"></param>
        /// <param name="ei1"></param>
        /// <param name="collapsedVertexIndex"></param>
        /// <returns></returns>
        private static bool FindCollapseIndex(SegmentNode ei0, SegmentNode ei1, int[] collapsedVertexIndex)
        {
            // only looking for equal nodes
            if (!ei0.Coord.Equals2D(ei1.Coord)) 
                return false;
            var numVerticesBetween = ei1.SegmentIndex - ei0.SegmentIndex;
            if (!ei1.IsInterior)
                numVerticesBetween--;
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
        /// for a set of <see cref="ISegmentString" />s).
        /// </summary>
        /// <param name="edgeList"></param>
        public void AddSplitEdges(IList<ISegmentString> edgeList)
        {
            // ensure that the list has entries for the first and last point of the edge
            AddEndpoints();
            AddCollapsedNodes();

            // there should always be at least two entries in the list, since the endpoints are nodes
            var ie = GetEnumerator();
	        ie.MoveNext();            
            var eiPrev = (SegmentNode) ie.Current;
            while (ie.MoveNext())
            {
                var ei = (SegmentNode)ie.Current;
                var newEdge = CreateSplitEdge(eiPrev, ei);
                /*
                if (newEdge.Count < 2)
                  throw new Exception("created single point edge: " + newEdge);
                 */
                edgeList.Add(newEdge);
                eiPrev = ei;
            }
            //CheckSplitEdgesCorrectness(testingSplitEdges);
        }

        /*
        /// <summary>
        /// Checks the correctness of the set of split edges corresponding to this edge.
        /// </summary>
        /// <param name="splitEdges"></param>
        private void CheckSplitEdgesCorrectness(IList<ISegmentString> splitEdges)
        {
            var edgePts = _edge.Coordinates;

            // check that first and last points of split edges are same as endpoints of edge
            var split0 = (ISegmentString) splitEdges[0];
            var pt0 = split0.Coordinates[0];
            if (!pt0.Equals2D(edgePts[0]))
                throw new Exception("bad split edge start point at " + pt0);

            var splitn = (ISegmentString)splitEdges[splitEdges.Count - 1];
            var splitnPts = splitn.Coordinates;
            var ptn = splitnPts[splitnPts.Length - 1];
            if (!ptn.Equals2D(edgePts[edgePts.Length - 1]))
                throw new Exception("bad split edge end point at " + ptn);
        }
        */

        /// <summary>
        ///  Create a new "split edge" with the section of points between
        /// (and including) the two intersections.
        /// The label for the new edge is the same as the label for the parent edge.
        /// </summary>
        /// <param name="ei0"></param>
        /// <param name="ei1"></param>
        /// <returns></returns>
        ISegmentString CreateSplitEdge(SegmentNode ei0, SegmentNode ei1)
        {
            var npts = ei1.SegmentIndex - ei0.SegmentIndex + 2;

            var lastSegStartPt = _edge.GetCoordinate(ei1.SegmentIndex);
            // if the last intersection point is not equal to the its segment start pt, add it to the points list as well.
            // (This check is needed because the distance metric is not totally reliable!)
            // The check for point equality is 2D only - Z values are ignored
            var useIntPt1 = ei1.IsInterior || !ei1.Coord.Equals2D(lastSegStartPt);
            if(!useIntPt1)
                npts--;

            var pts = new Coordinate[npts];
            var ipt = 0;
            pts[ipt++] = new Coordinate(ei0.Coord);
            for (var i = ei0.SegmentIndex + 1; i <= ei1.SegmentIndex; i++)
                pts[ipt++] = _edge.GetCoordinate(i);            
            if (useIntPt1) 
                pts[ipt] = new Coordinate(ei1.Coord);

            return new NodedSegmentString(pts, _edge.Context);
        }

        /// <summary>Gets the list of coordinates for the fully noded segment string,
        /// including all original segment string vertices and vertices
        /// introduced by nodes in this list.
        /// Repeated coordinates are collapsed.
        /// </summary>
        /// <returns>An array of <see cref="Coordinate"/>s</returns>
        public Coordinate[] GetSplitCoordinates()
        {
            var coordList = new CoordinateList();
            // ensure that the list has entries for the first and last point of the edge
            AddEndpoints();

            var it = GetEnumerator();
            it.MoveNext();
            // there should always be at least two entries in the list, since the endpoints are nodes
            var eiPrev = (SegmentNode)it.Current;
            while (it.MoveNext())
            {
                var ei = (SegmentNode)it.Current;
                AddEdgeCoordinates(eiPrev, ei, coordList);
                eiPrev = ei;
            }
            return coordList.ToCoordinateArray();
        }

        private void AddEdgeCoordinates(SegmentNode ei0, SegmentNode ei1,
            CoordinateList coordList)
        {
            var npts = ei1.SegmentIndex - ei0.SegmentIndex + 2;

            var lastSegStartPt = _edge.GetCoordinate(ei1.SegmentIndex);
            // if the last intersection point is not equal to the its segment start pt,
            // add it to the points list as well.
            // (This check is needed because the distance metric is not totally reliable!)
            // The check for point equality is 2D only - Z values are ignored
            var useIntPt1 = ei1.IsInterior || !ei1.Coord.Equals2D(lastSegStartPt);
            if (!useIntPt1)
            {
                npts--;
            }

            var ipt = 0;
            coordList.Add(new Coordinate(ei0.Coord), false);
            for (var i = ei0.SegmentIndex + 1; i <= ei1.SegmentIndex; i++)
            {
                coordList.Add(_edge.GetCoordinate(i));
            }
            if (useIntPt1)
            {
                coordList.Add(new Coordinate(ei1.Coord));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outstream"></param>
        public void Write(StreamWriter outstream)
        {
            outstream.Write("Intersections:");
            foreach(var obj in this)
            {
                var ei = (SegmentNode) obj;
                ei.Write(outstream);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class NodeVertexIterator : IEnumerator<object>
    {
        private SegmentNodeList _nodeList;
        private ISegmentString _edge;
        private readonly IEnumerator<object> _nodeIt;
        private SegmentNode _currNode;
        private SegmentNode _nextNode;
        private int _currSegIndex;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeList"></param>
        NodeVertexIterator(SegmentNodeList nodeList)
        {
            _nodeList = nodeList;
            _edge = nodeList.Edge;
            _nodeIt = nodeList.GetEnumerator();            
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReadNextNode()
        {
            if (_nodeIt.MoveNext())
                 _nextNode = (SegmentNode) _nodeIt.Current;
            else _nextNode = null;
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

        /// <summary>
        /// 
        /// </summary>
        public object Current
        {
            get  { return _currNode; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (_currNode == null)
            {
                _currNode = _nextNode;
                _currSegIndex = _currNode.SegmentIndex;
                ReadNextNode();
                return true;
            }

            // check for trying to read too far
            if (_nextNode == null) 
                return false;

            if (_nextNode.SegmentIndex == _currNode.SegmentIndex)
            {
                _currNode = _nextNode;
                _currSegIndex = _currNode.SegmentIndex;
                ReadNextNode();
                return true;
            }

            if (_nextNode.SegmentIndex > _currNode.SegmentIndex)
            {

            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            _nodeIt.Reset();            
        }
    }
}
