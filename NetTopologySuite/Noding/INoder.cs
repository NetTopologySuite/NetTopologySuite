using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// Computes all intersections between segments in a set of 
    /// <see cref="NodedSegmentString{TCoordinate}" />s. Intersections found 
    /// are represented as <see cref="SegmentNode{TCoordinate}" />s and added to the
    /// <see cref="NodedSegmentString{TCoordinate}" />s in which they occur.
    /// As a final step in the noding a new set of 
    /// <see cref="NodedSegmentString{TCoordinate}" />s split at 
    /// the nodes may be returned.
    /// </summary>
    /// <typeparam name="TCoordinate">The type of coordinate.</typeparam>
    public interface INoder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        /// <summary>
        /// Computes the noding for a collection of 
        /// <see cref="ISegmentString{TCoordinate}" />s.
        /// Some noders may add all these nodes to the input 
        /// <see cref="ISegmentString{TCoordinate}" />s;
        /// others may only add some or none at all.
        /// </summary>
        /// <param name="segStrings">
        /// The <see cref="ISegmentString{TCoordinate}"/>s to node.
        /// </param>
        IEnumerable<ISegmentString<TCoordinate>> Node(IEnumerable<ISegmentString<TCoordinate>> segStrings);

        ///<summary>
        /// Computes the noding for a collection of {@link SegmentString}s.
        /// Some Noders may add all these nodes to the input SegmentStrings;
        /// others may only add some or none at all.
        ///</summary>
        ///<param name="segStrings">segStrings an enumerable of <see cref="ISegmentString{TCoordinate}"/>s to node</param>
        void ComputeNodes(IEnumerable<ISegmentString<TCoordinate>> segStrings);

    }
}