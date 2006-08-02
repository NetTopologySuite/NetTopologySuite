using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Nodes a set of SegmentStrings completely.
    /// The set of segmentStrings is fully noded;
    /// i.e. noding is repeated until no further
    /// intersections are detected.
    /// Iterated noding using a Floating precision model is not guaranteed to converge,
    /// due to roundoff error.  This problem is detected and an exception is thrown.
    /// Clients can choose to rerun the noding using a lower precision model.
    /// </summary>
    public class IteratedNoder
    {
        private PrecisionModel pm;
        private LineIntersector li;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pm"></param>
        public IteratedNoder(PrecisionModel pm)
        {
            li = new RobustLineIntersector();
            this.pm = pm;
            li.PrecisionModel = pm;
        }

        /// <summary>
        /// Fully nodes a list of <c>SegmentString</c>s, i.e. peforms noding iteratively
        /// until no intersections are found between segments.
        /// Maintains labelling of edges correctly through
        /// the noding.
        /// </summary>
        /// <param name="segStrings">A collection of SegmentStrings to be noded.</param>
        /// <returns>A collection of the noded SegmentStrings.</returns>
        public virtual IList Node(IList segStrings)            
        {
            int[] numInteriorIntersections = new int[1];
            IList nodedEdges = segStrings;
            int nodingIterationCount = 0;
            int lastNodesCreated = -1;
            do
            {                
                nodedEdges = Node(nodedEdges, numInteriorIntersections);             
                nodingIterationCount++;
                int nodesCreated = numInteriorIntersections[0];                
                if (lastNodesCreated > 0 && nodesCreated > lastNodesCreated)                 
                    throw new TopologyException("Iterated noding failed to converge after "
                                                + nodingIterationCount + " iterations");            
                lastNodesCreated = nodesCreated;                
            }             
            while (lastNodesCreated > 0);
            return nodedEdges;
        }

        /// <summary>
        /// Node the input segment strings once
        /// and create the split edges between the nodes.
        /// </summary>
        /// <param name="segStrings"></param>
        /// <param name="numInteriorIntersections"></param>
        public virtual IList Node(IList segStrings, int[] numInteriorIntersections)
        {
            SegmentIntersector si = new SegmentIntersector(li);
            MCQuadtreeNoder noder = new MCQuadtreeNoder();
            noder.SegmentIntersector = si;

            // perform the noding
            IList nodedSegStrings = noder.Node(segStrings);
            numInteriorIntersections[0] = si.numInteriorIntersections;       
            return nodedSegStrings;
        }
    }
}
