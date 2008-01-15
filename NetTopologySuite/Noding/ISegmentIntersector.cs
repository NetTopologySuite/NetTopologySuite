using System;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Computes the intersections between two line segments in <see cref="NodedSegmentString{TCoordinate}" />s
    /// and adds them to each string.
    /// The <see cref="ISegmentIntersector{TCoordinate}" /> is passed to a <see cref="INoder{TCoordinate}" />.
    /// The <see cref="NodedSegmentString{TCoordinate}.AddIntersections" />  method is called whenever the <see cref="INoder{TCoordinate}" />
    /// detects that two <see cref="NodedSegmentString{TCoordinate}" /> s might intersect.
    /// This class is an example of the Strategy pattern.
    /// </summary>
    public interface ISegmentIntersector<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// This method is called by clients
        /// of the <see cref="ISegmentIntersector{TCoordinate}" /> interface to process
        /// intersections for two segments of the <see cref="NodedSegmentString{TCoordinate}" />s being intersected.
        /// </summary>
        void ProcessIntersections(NodedSegmentString<TCoordinate> e0, Int32 segIndex0, NodedSegmentString<TCoordinate> e1, Int32 segIndex1);
    }
}