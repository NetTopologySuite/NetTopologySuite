using System;
using System.Collections;
using System.Text;
using System.IO;

using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// A list of the <c>SegmentNode</c>s present along a noded <c>SegmentString</c>.
    /// </summary>
    public class SegmentNodeList
    {
        // a list of SegmentNodes        
        private IDictionary nodeMap = new SortedList();    
        private SegmentString edge;  // the parent edge

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        public SegmentNodeList(SegmentString edge)
        {
            this.edge = edge;
        }

        /// <summary>
        /// Adds an intersection into the list, if it isn't already there.
        /// The input segmentIndex and dist are expected to be normalized.
        /// </summary>
        /// <param name="intPt"></param>
        /// <param name="segmentIndex"></param>
        /// <param name="dist"></param>
        /// <returns>The SegmentIntersection found or added.</returns>
        public virtual SegmentNode Add(Coordinate intPt, int segmentIndex, double dist)
        {
            SegmentNode eiNew = new SegmentNode(intPt, segmentIndex, dist);
            object obj = nodeMap[eiNew];    // Line with no sense...
            SegmentNode ei = (SegmentNode)nodeMap[eiNew];
            if (ei != null) 
                return ei;            
            nodeMap.Add(eiNew, eiNew);
            return eiNew;            
        }
        
        /// <summary>
        /// Returns an iterator of SegmentNodes.
        /// </summary>
        public virtual IEnumerator GetEnumerator() 
        {            
            return nodeMap.Values.GetEnumerator(); 
        }

        /// <summary>
        /// Adds entries for the first and last points of the edge to the list
        /// </summary>
        public virtual void AddEndpoints()
        {
            int maxSegIndex = edge.Count - 1;
            Add(edge.GetCoordinate(0), 0, 0.0);
            Add(edge.GetCoordinate(maxSegIndex), maxSegIndex, 0.0);
        }

        /// <summary> 
        /// Creates new edges for all the edges that the intersections in this
        /// list split the parent edge into.
        /// Adds the edges to the input list (this is so a single list
        /// can be used to accumulate all split edges for a Geometry).
        /// </summary>
        /// <param name="edgeList"></param>
        public virtual void AddSplitEdges(IList edgeList)
        {
            // testingOnly
            IList testingSplitEdges = new ArrayList();
            // ensure that the list has entries for the first and last point of the edge
            AddEndpoints();

            // This code works correctly if i calls first MoveNext(),
            // otherwise nodeMap.Values.GetEnumerator() throws an InvalidOperationException.
            // Calls to MoveNext() semms to be init IEnumerator to first value.            
            
            IEnumerator it = GetEnumerator();
            it.MoveNext();
            // there should always be at least two entries in the list
            SegmentNode eiPrev = (SegmentNode) it.Current;            
            while (it.MoveNext()) 
            {
                SegmentNode ei = (SegmentNode) it.Current;
                SegmentString newEdge = CreateSplitEdge(eiPrev, ei);             
                edgeList.Add(newEdge);                
                testingSplitEdges.Add(newEdge);
                eiPrev = ei;
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="splitEdges"></param>
        private void CheckSplitEdgesCorrectness(IList splitEdges)
        {
            Coordinate[] edgePts = edge.Coordinates;
            // check that first and last points of split edges are same as endpoints of edge
            SegmentString split0 = (SegmentString) splitEdges[0];
            Coordinate pt0 = split0.GetCoordinate(0);
            if (! pt0.Equals(edgePts[0]))
                throw new ApplicationException("bad split edge start point at " + pt0);
            SegmentString splitn = (SegmentString) splitEdges[splitEdges.Count - 1];
            Coordinate[] splitnPts = splitn.Coordinates;
            Coordinate ptn = splitnPts[splitnPts.Length - 1];
            if (!ptn.Equals(edgePts[edgePts.Length - 1]))
                throw new ApplicationException("bad split edge end point at " + ptn);

        }

        /// <summary> 
        /// Create a new "split edge" with the section of points between
        /// (and including) the two intersections.
        /// The label for the new edge is the same as the label for the parent edge.
        /// </summary>
        /// <param name="ei0"></param>
        /// <param name="ei1"></param>
        public virtual SegmentString CreateSplitEdge(SegmentNode ei0, SegmentNode ei1)
        {
            int npts = ei1.SegmentIndex - ei0.SegmentIndex + 2;
            Coordinate lastSegStartPt = edge.GetCoordinate(ei1.SegmentIndex);
            // if the last intersection point is not equal to the its segment start pt,
            // add it to the points list as well.
            // (This check is needed because the distance metric is not totally reliable!)
            // The check for point equality is 2D only - Z values are ignored
            bool useIntPt1 = ei1.Distance > 0.0 || ! ei1.Coordinate.Equals2D(lastSegStartPt);
            if (!useIntPt1) 
                npts--;            
            Coordinate[] pts = new Coordinate[npts];
            int ipt = 0;
            pts[ipt++] = new Coordinate(ei0.Coordinate);
            for (int i = ei0.SegmentIndex + 1; i <= ei1.SegmentIndex; i++) 
                pts[ipt++] = edge.GetCoordinate(i);            
            if (useIntPt1) 
                pts[ipt] = ei1.Coordinate;
            return new SegmentString(pts, edge.Context);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="outstream"></param>
        public virtual void Write(StreamWriter outstream)
        {
            outstream.WriteLine("Intersections:");
            for (IEnumerator it = GetEnumerator(); it.MoveNext(); ) 
            {
                SegmentNode ei = (SegmentNode) it.Current;
                ei.Write(outstream);
            }
        }
    }
}
