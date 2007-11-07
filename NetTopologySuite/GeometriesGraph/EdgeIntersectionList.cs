using System;
using System.Collections;
using System.IO;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A list of edge intersections along an Edge.
    /// </summary>
    public class EdgeIntersectionList
    {
        // a list of EdgeIntersections      
        private IDictionary nodeMap = new SortedList();
        private Edge edge; // the parent edge

        public EdgeIntersectionList(Edge edge)
        {
            this.edge = edge;
        }

        public Int32 Count
        {
            get { return nodeMap.Count; }
        }

        /// <summary> 
        /// Adds an intersection into the list, if it isn't already there.
        /// The input segmentIndex and dist are expected to be normalized.
        /// </summary>
        /// <returns>The EdgeIntersection found or added.</returns>
        public EdgeIntersection Add(ICoordinate intPt, Int32 segmentIndex, Double dist)
        {
            EdgeIntersection eiNew = new EdgeIntersection(intPt, segmentIndex, dist);
            EdgeIntersection ei = (EdgeIntersection) nodeMap[eiNew];
            
            if (ei != null)
            {
                return ei;
            }

            nodeMap.Add(eiNew, eiNew);
            return eiNew;
        }

        /// <summary> 
        /// Returns an iterator of EdgeIntersections.
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return nodeMap.Values.GetEnumerator();
        }

        public Boolean IsIntersection(ICoordinate pt)
        {
            for (IEnumerator it = GetEnumerator(); it.MoveNext();)
            {
                EdgeIntersection ei = (EdgeIntersection) it.Current;
               
                if (ei.Coordinate.Equals(pt))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds entries for the first and last points of the edge to the list.
        /// </summary>
        public void AddEndpoints()
        {
            Int32 maxSegIndex = edge.Points.Length - 1;
            Add(edge.Points[0], 0, 0.0);
            Add(edge.Points[maxSegIndex], maxSegIndex, 0.0);
        }

        /// <summary> 
        /// Creates new edges for all the edges that the intersections in this
        /// list split the parent edge into.
        /// Adds the edges to the input list (this is so a single list
        /// can be used to accumulate all split edges for a Geometry).
        /// </summary>
        public void AddSplitEdges(IList edgeList)
        {
            // ensure that the list has entries for the first and last point of the edge
            AddEndpoints();

            IEnumerator it = GetEnumerator();
            it.MoveNext();

            // there should always be at least two entries in the list
            EdgeIntersection eiPrev = (EdgeIntersection) it.Current;

            while (it.MoveNext())
            {
                EdgeIntersection ei = (EdgeIntersection) it.Current;
                Edge newEdge = CreateSplitEdge(eiPrev, ei);
                edgeList.Add(newEdge);

                eiPrev = ei;
            }
        }

        /// <summary>
        /// Create a new "split edge" with the section of points between
        /// (and including) the two intersections.
        /// The label for the new edge is the same as the label for the parent edge.
        /// </summary>
        public Edge CreateSplitEdge(EdgeIntersection ei0, EdgeIntersection ei1)
        {
            Int32 npts = ei1.SegmentIndex - ei0.SegmentIndex + 2;
            ICoordinate lastSegStartPt = edge.Points[ei1.SegmentIndex];

            // if the last intersection point is not equal to the its segment start pt,
            // add it to the points list as well.
            // (This check is needed because the distance metric is not totally reliable!)
            // The check for point equality is 2D only - Z values are ignored
            Boolean useIntPt1 = ei1.Distance > 0.0 || ! ei1.Coordinate.Equals2D(lastSegStartPt);
            
            if (! useIntPt1)
            {
                npts--;
            }

            ICoordinate[] pts = new ICoordinate[npts];
            Int32 ipt = 0;
            pts[ipt++] = new Coordinate(ei0.Coordinate);

            for (Int32 i = ei0.SegmentIndex + 1; i <= ei1.SegmentIndex; i++)
            {
                pts[ipt++] = edge.Points[i];
            }

            if (useIntPt1)
            {
                pts[ipt] = ei1.Coordinate;
            }

            return new Edge(pts, new Label(edge.Label));
        }

        public void Write(StreamWriter outstream)
        {
            outstream.WriteLine("Intersections:");

            for (IEnumerator it = GetEnumerator(); it.MoveNext();)
            {
                EdgeIntersection ei = (EdgeIntersection) it.Current;
                ei.Write(outstream);
            }
        }
    }
}