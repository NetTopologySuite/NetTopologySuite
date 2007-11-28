using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{

    /// <summary>
    /// Computes all intersections between segments in a set of 
    /// <see cref="SegmentString{TCoordinate}" />s. Intersections found 
    /// are represented as <see cref="SegmentNode{TCoordinate}" />s and added to the
    /// <see cref="SegmentString{TCoordinate}" />s in which they occur.
    /// As a final step in the noding a new set of segment strings split at 
    /// the nodes may be returned.
    /// </summary>
    public interface INoder<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {

        /// <summary>
        /// Computes the noding for a collection of <see cref="SegmentString{TCoordinate}" />s.
        /// Some Noders may add all these nodes to the input <see cref="SegmentString{TCoordinate}" />s;
        /// others may only add some or none at all.
        /// </summary>
        /// <param name="segStrings"></param>
        void ComputeNodes(IEnumerable<SegmentString<TCoordinate>> segStrings);

        /// <summary>
        /// Returns a set of fully noded <see cref="SegmentString{TCoordinate}" />s.
        /// The <see cref="SegmentString{TCoordinate}" />s have the same 
        /// context as their parent.
        /// </summary>
        /// <returns></returns>
        IEnumerable<SegmentString<TCoordinate>> GetNodedSubstrings();

    }
}
