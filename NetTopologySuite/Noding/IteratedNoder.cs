using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Nodes a set of <see cref="SegmentString{TCoordinate}" />s completely.
    /// The set of <see cref="SegmentString{TCoordinate}" />s is fully noded;
    /// i.e. noding is repeated until no further intersections are detected.
    /// <para>
    /// Iterated noding using a <see cref="PrecisionModels.Floating" /> precision model is not guaranteed to converge,
    /// due to roundoff error. This problem is detected and an exception is thrown.
    /// Clients can choose to rerun the noding using a lower precision model.
    /// </para>
    /// </summary>
    public class IteratedNoder<TCoordinate> : INoder<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        public const Int32 DefaultMaxIterations = 5;

        private readonly LineIntersector<TCoordinate> _li = null;
        private IEnumerable<SegmentString<TCoordinate>> _nodedSegStrings = null;
        private Int32 _maxIter = DefaultMaxIterations;

        /// <summary>
        /// Initializes a new instance of the <see cref="IteratedNoder{TCoordinate}"/> class.
        /// </summary>
        public IteratedNoder(IPrecisionModel<TCoordinate> pm)
        {
            _li = new RobustLineIntersector<TCoordinate>();
            _li.PrecisionModel = pm;
        }

        /// <summary>
        /// Gets or sets the maximum number of noding iterations performed before
        /// the noding is aborted. Experience suggests that this should rarely need to be changed
        /// from the default. The default is <see cref="DefaultMaxIterations" />.
        /// </summary>
        public Int32 MaximumIterations
        {
            get { return _maxIter; }
            set { _maxIter = value; }
        }

        /// <summary>
        /// Returns a set of fully noded <see cref="SegmentString{TCoordinate}" />s.
        /// The <see cref="SegmentString{TCoordinate}" />s have the same context as their parent.
        /// </summary>
        public IEnumerable<SegmentString<TCoordinate>> GetNodedSubstrings()
        {
            return _nodedSegStrings;
        }

        /// <summary>
        /// Fully nodes a list of <see cref="SegmentString{TCoordinate}" />s, i.e. peforms noding iteratively
        /// until no intersections are found between segments.
        /// Maintains labeling of edges correctly through the noding.
        /// </summary>
        /// <param name="segStrings">A collection of SegmentStrings to be noded.</param>
        /// <exception cref="TopologyException">If the iterated noding fails to converge.</exception>
        public void ComputeNodes(IEnumerable<SegmentString<TCoordinate>> segStrings)
        {
            _nodedSegStrings = segStrings;
            Int32 nodingIterationCount = 0;
            Int32 lastNodesCreated = -1;

            do
            {
                Int32 numInteriorIntersections;
                node(_nodedSegStrings, out numInteriorIntersections);
                nodingIterationCount++;
                Int32 nodesCreated = numInteriorIntersections;

               /*
                * Fail if the number of nodes created is not declining.
                * However, allow a few iterations at least before doing this
                */
                if (lastNodesCreated > 0
                    && nodesCreated >= lastNodesCreated
                    && nodingIterationCount > _maxIter)
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
        private void node(IEnumerable<SegmentString<TCoordinate>> segStrings, out Int32 numInteriorIntersections)
        {
            IntersectionAdder<TCoordinate> si = new IntersectionAdder<TCoordinate>(_li);
            MonotoneChainIndexNoder<TCoordinate> noder = new MonotoneChainIndexNoder<TCoordinate>(si);
            noder.ComputeNodes(segStrings);
            _nodedSegStrings = noder.GetNodedSubstrings();
            numInteriorIntersections = si.InteriorIntersectionCount;
        }
    }
}