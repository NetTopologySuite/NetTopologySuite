using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Base class for <see cref="INoder{TCoordinate}" />s which make a single pass to find intersections.
    /// This allows using a custom <see cref="ISegmentIntersector{TCoordinate}" />
    /// (which for instance may simply identify intersections, rather than insert them).
    /// </summary>
    public abstract class SinglePassNoder<TCoordinate> : INoder<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private ISegmentIntersector<TCoordinate> _segInt = null;
       
        /// <summary>
        /// Initializes a new instance of the <see cref="SinglePassNoder{TCoordinate}"/> class.
        /// </summary>
        public SinglePassNoder() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SinglePassNoder{TCoordinate}"/> class.
        /// </summary>
        /// <param name="segInt">The <see cref="ISegmentIntersector{TCoordinate}" /> to use.</param>
        public SinglePassNoder(ISegmentIntersector<TCoordinate> segInt)
        {
            _segInt = segInt;
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
            get 
            { 
                return _segInt; 
            }
            set 
            { 
                _segInt = value; 
            }
        }


        /// <summary>
        /// Computes the noding for a collection of <see cref="SegmentString{TCoordinate}"/>s.
        /// Some Noders may add all these nodes to the input <see cref="SegmentString{TCoordinate}"/>s;
        /// others may only add some or none at all.
        /// </summary>
        public abstract void ComputeNodes(IEnumerable<SegmentString<TCoordinate>> segStrings);

        /// <summary>
        /// Returns a set of fully noded <see cref="SegmentString{TCoordinate}"/>s.
        /// The <see cref="SegmentString{TCoordinate}"/>s have the same context as their parent.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<SegmentString<TCoordinate>> GetNodedSubstrings();

    }
}
