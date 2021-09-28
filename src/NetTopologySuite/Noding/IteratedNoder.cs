using System.Collections;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Noding
{

    /// <summary>
    /// Nodes a set of <see cref="ISegmentString" />s completely.
    /// The set of <see cref="ISegmentString" />s is fully noded;
    /// i.e. noding is repeated until no further intersections are detected.
    /// <para>
    /// Iterated noding using a <see cref="PrecisionModels.Floating" /> precision model is not guaranteed to converge,
    /// due to round off error. This problem is detected and an exception is thrown.
    /// Clients can choose to rerun the noding using a lower precision model.
    /// </para>
    /// </summary>
    public class IteratedNoder : INoder
    {

        /// <summary>
        ///
        /// </summary>
        public const int MaxIterations = 5;

        private readonly LineIntersector _li;
        private IList<ISegmentString> _nodedSegStrings;
        private int _maxIter = MaxIterations;

        /// <summary>
        /// Initializes a new instance of the <see cref="IteratedNoder"/> class.
        /// </summary>
        /// <param name="pm"></param>
        public IteratedNoder(PrecisionModel pm)
        {
            _li = new RobustLineIntersector {PrecisionModel = pm};
        }

        /// <summary>
        /// Gets/Sets the maximum number of noding iterations performed before
        /// the noding is aborted. Experience suggests that this should rarely need to be changed
        /// from the default. The default is <see cref="MaxIterations" />.
        /// </summary>
        public int MaximumIterations
        {
            get => _maxIter;
            set => _maxIter = value;
        }

        /// <summary>
        /// Returns a <see cref="IList"/> of fully noded <see cref="ISegmentString"/>s.
        /// The <see cref="ISegmentString"/>s have the same context as their parent.
        /// </summary>
        /// <returns></returns>
        public IList<ISegmentString> GetNodedSubstrings()
        {
            return _nodedSegStrings;
        }

        /// <summary>
        /// Fully nodes a list of <see cref="ISegmentString" />s, i.e. performs noding iteratively
        /// until no intersections are found between segments.
        /// Maintains labelling of edges correctly through the noding.
        /// </summary>
        /// <param name="segStrings">A collection of SegmentStrings to be noded.</param>
        /// <exception cref="TopologyException">If the iterated noding fails to converge.</exception>
        public void ComputeNodes(IList<ISegmentString> segStrings)
        {
            int[] numInteriorIntersections = new int[1];
            _nodedSegStrings = segStrings;
            int nodingIterationCount = 0;
            int lastNodesCreated = -1;
            do
            {
              Node(_nodedSegStrings, numInteriorIntersections);
              nodingIterationCount++;
              int nodesCreated = numInteriorIntersections[0];

              /*
               * Fail if the number of nodes created is not declining.
               * However, allow a few iterations at least before doing this
               */
              if (lastNodesCreated > 0
                  && nodesCreated >= lastNodesCreated
                  && nodingIterationCount > _maxIter)
                throw new TopologyException("Iterated noding failed to converge after "
                                            + nodingIterationCount + " iterations");
              lastNodesCreated = nodesCreated;

            }
            while (lastNodesCreated > 0);
        }

        /// <summary>
        /// Node the input segment strings once
        /// and create the split edges between the nodes.
        /// </summary>
        /// <param name="segStrings"></param>
        /// <param name="numInteriorIntersections"></param>
        private void Node(IList<ISegmentString> segStrings, int[] numInteriorIntersections)
        {
            var si = new IntersectionAdder(_li);
            var noder = new MCIndexNoder(si);
            noder.ComputeNodes(segStrings);
            _nodedSegStrings = noder.GetNodedSubstrings();
            numInteriorIntersections[0] = si.NumInteriorIntersections;
        }

    }
}
