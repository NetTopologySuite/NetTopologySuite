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
    /// Nodes a set of <see cref="NodedSegmentString{TCoordinate}" />s completely.
    /// The set of <see cref="NodedSegmentString{TCoordinate}" />s is fully noded;
    /// i.e. noding is repeated until no further intersections are detected.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Iterated noding using a <see cref="PrecisionModelType.Floating" /> precision model 
    /// is not guaranteed to converge, due to roundoff error. This problem is detected 
    /// and an exception is thrown.
    /// Clients can choose to rerun the noding using a lower precision model.
    /// </para>
    /// </remarks>
    public class IteratedNoder<TCoordinate> : INoder<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        public static readonly Int32 DefaultMaxIterations = 5;

        private readonly LineIntersector<TCoordinate> _li = null;
        //private IEnumerable<SegmentString<TCoordinate>> _nodedSegStrings = null;
        private Int32 _maxIter = DefaultMaxIterations;

        /// <summary>
        /// Initializes a new instance of the <see cref="IteratedNoder{TCoordinate}"/> class.
        /// </summary>
        public IteratedNoder(IPrecisionModel<TCoordinate> pm)
        {
            _li = CGAlgorithms<TCoordinate>.CreateRobustLineIntersector();
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
        /// Fully nodes a set of <see cref="NodedSegmentString{TCoordinate}" />s, 
        /// i.e. peforms noding iteratively until no intersections are found between 
        /// segments.
        /// Maintains labeling of edges correctly through the noding.
        /// The <see cref="NodedSegmentString{TCoordinate}" />s have the same context as their parent.
        /// </summary>
        /// <param name="segStrings">
        /// An enumeration of <see cref="NodedSegmentString{TCoordinate}"/>s to be noded.
        /// </param>
        /// <exception cref="TopologyException">If the iterated noding fails to converge.</exception>
        public IEnumerable<NodedSegmentString<TCoordinate>> Node(IEnumerable<NodedSegmentString<TCoordinate>> segStrings)
        {
            //_nodedSegStrings = segStrings;
            Int32 nodingIterationCount = 0;
            Int32 lastNodesCreated = -1;

            do
            {
                Int32 numInteriorIntersections;
                segStrings = node(segStrings, out numInteriorIntersections);
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

            return segStrings;
        }

        /// <summary>
        /// Node the input segment strings once
        /// and create the split edges between the nodes.
        /// </summary>
        private IEnumerable<NodedSegmentString<TCoordinate>> node(IEnumerable<NodedSegmentString<TCoordinate>> segStrings, out Int32 interiorIntersectionsCount)
        {
            IntersectionAdder<TCoordinate> si = new IntersectionAdder<TCoordinate>(_li);
            MonotoneChainIndexNoder<TCoordinate> noder = new MonotoneChainIndexNoder<TCoordinate>(si);
            IEnumerable<NodedSegmentString<TCoordinate>> nodedSegments = noder.Node(segStrings);
            interiorIntersectionsCount = si.InteriorIntersectionCount;
            return nodedSegments;
        }
    }
}