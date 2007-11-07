using System;
using System.Collections;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Nodes a set of <see cref="SegmentString" />s completely.
    /// The set of <see cref="SegmentString" />s is fully noded;
    /// i.e. noding is repeated until no further intersections are detected.
    /// <para>
    /// Iterated noding using a <see cref="PrecisionModels.Floating" /> precision model is not guaranteed to converge,
    /// due to roundoff error. This problem is detected and an exception is thrown.
    /// Clients can choose to rerun the noding using a lower precision model.
    /// </para>
    /// </summary>
    public class IteratedNoder : INoder
    {
        public const Int32 MaxIterations = 5;

        private LineIntersector li = null;
        private IList nodedSegStrings = null;
        private Int32 maxIter = MaxIterations;

        /// <summary>
        /// Initializes a new instance of the <see cref="IteratedNoder"/> class.
        /// </summary>
        public IteratedNoder(PrecisionModel pm)
        {
            li = new RobustLineIntersector();
            li.PrecisionModel = pm;
        }

        /// <summary>
        /// Gets or sets the maximum number of noding iterations performed before
        /// the noding is aborted. Experience suggests that this should rarely need to be changed
        /// from the default. The default is <see cref="MaxIterations" />.
        /// </summary>
        public Int32 MaximumIterations
        {
            get { return maxIter; }
            set { maxIter = value; }
        }

        /// <summary>
        /// Returns a <see cref="IList"/> of fully noded <see cref="SegmentString"/>s.
        /// The <see cref="SegmentString"/>s have the same context as their parent.
        /// </summary>
        public IList GetNodedSubstrings()
        {
            return nodedSegStrings;
        }

        /// <summary>
        /// Fully nodes a list of <see cref="SegmentString" />s, i.e. peforms noding iteratively
        /// until no intersections are found between segments.
        /// Maintains labelling of edges correctly through the noding.
        /// </summary>
        /// <param name="segStrings">A collection of SegmentStrings to be noded.</param>
        /// <exception cref="TopologyException">If the iterated noding fails to converge.</exception>
        public void ComputeNodes(IList segStrings)
        {
            Int32[] numInteriorIntersections = new Int32[1];
            nodedSegStrings = segStrings;
            Int32 nodingIterationCount = 0;
            Int32 lastNodesCreated = -1;
            do
            {
                Node(nodedSegStrings, numInteriorIntersections);
                nodingIterationCount++;
                Int32 nodesCreated = numInteriorIntersections[0];

                /*
               * Fail if the number of nodes created is not declining.
               * However, allow a few iterations at least before doing this
               */
                if (lastNodesCreated > 0
                    && nodesCreated >= lastNodesCreated
                    && nodingIterationCount > maxIter)
                {
                    throw new TopologyException("Iterated noding failed to converge after "
                                                + nodingIterationCount + " iterations");
                }
                lastNodesCreated = nodesCreated;
            } while (lastNodesCreated > 0);
        }

        /// <summary>
        /// Node the input segment strings once
        /// and create the split edges between the nodes.
        /// </summary>
        private void Node(IList segStrings, Int32[] numInteriorIntersections)
        {
            IntersectionAdder si = new IntersectionAdder(li);
            MCIndexNoder noder = new MCIndexNoder(si);
            noder.ComputeNodes(segStrings);
            nodedSegStrings = noder.GetNodedSubstrings();
            numInteriorIntersections[0] = si.NumInteriorIntersections;
        }
    }
}