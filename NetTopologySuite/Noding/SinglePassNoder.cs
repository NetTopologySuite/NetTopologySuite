using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// Base class for <see cref="INoder{TCoordinate}" />s which make a single pass to find intersections.
    /// This allows using a custom <see cref="ISegmentIntersector{TCoordinate}" />
    /// (which for instance may simply identify intersections, rather than insert them).
    /// </summary>
    public abstract class SinglePassNoder<TCoordinate> : INoder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly ISegmentIntersector<TCoordinate> _segInt;

        /// <summary>
        /// Initializes a new instance of the <see cref="SinglePassNoder{TCoordinate}"/> class.
        /// </summary>
        /// <param name="segInt">The <see cref="ISegmentIntersector{TCoordinate}" /> to use.</param>
        public SinglePassNoder(ISegmentIntersector<TCoordinate> segmentIntersector)
        {
            _segInt = segmentIntersector;
        }

        /// <summary>
        /// Gets/sets the <see cref="ISegmentIntersector{TCoordinate}" /> to use with this noder.
        /// A <see cref="ISegmentIntersector{TCoordinate}" />  will normally add intersection nodes
        /// to the input segment strings, but it may not - it may
        /// simply record the presence of intersections.
        /// However, some <see cref="INoder{TCoordinate}" />s may require that intersections be added.
        /// </summary>
        public ISegmentIntersector<TCoordinate> SegmentIntersector
        {
            get { return _segInt; }
        }

        #region INoder<TCoordinate> Members

        /// <summary>
        /// Returns a set of fully noded <see cref="NodedSegmentString{TCoordinate}"/>s.
        /// The <see cref="NodedSegmentString{TCoordinate}"/>s have the same context as their parent.
        /// </summary>
        /// <remarks>
        /// Computes the noding for a collection of <see cref="NodedSegmentString{TCoordinate}"/>s.
        /// Some Noders may add all these nodes to the input <see cref="NodedSegmentString{TCoordinate}"/>s;
        /// others may only add some or none at all.
        /// </remarks>
        public abstract IEnumerable<ISegmentString<TCoordinate>> Node(
            IEnumerable<ISegmentString<TCoordinate>> segmentStrings);

        public abstract void ComputeNodes(IEnumerable<ISegmentString<TCoordinate>> segmentStrings);
        #endregion

        public abstract IEnumerable<TNodingResult> Node<TNodingResult>(
            IEnumerable<ISegmentString<TCoordinate>> segmentStrings,
            Func<ISegmentString<TCoordinate>, TNodingResult> generator);
    }
}