using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
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
        /// <see cref="NodedSegmentString{TCoordinate}" />s.
        /// Some noders may add all these nodes to the input 
        /// <see cref="NodedSegmentString{TCoordinate}" />s;
        /// others may only add some or none at all.
        /// </summary>
        /// <param name="segStrings">
        /// The <see cref="NodedSegmentString{TCoordinate}"/>s to node.
        /// </param>
        IEnumerable<NodedSegmentString<TCoordinate>> Node(IEnumerable<NodedSegmentString<TCoordinate>> segStrings);
    }
}