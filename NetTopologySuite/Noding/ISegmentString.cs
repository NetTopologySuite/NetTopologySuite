using System;
using GeoAPI.Coordinates;
using NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Noding
{
    ///<summary>
    /// An interface for classes which represent a sequence of contiguous line segments.
    /// SegmentStrings can carry a context object, which is useful
    /// for preserving topological or parentage information.
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public interface ISegmentString<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        ///<summary>
        /// Gets/Sets the user-defined data for this segment string.
        ///</summary>
        Object Context { get; set; }
        ///<summary>
        /// Points that make up SegmentString
        ///</summary>
        ICoordinateSequence<TCoordinate> Coordinates { get; }
        ///<summary>
        /// Size of Coordinate Sequence
        ///</summary>
        Int32 Count { get; }
        /// <summary>
        /// States whether SegmentString is closed
        /// </summary>
        Boolean IsClosed { get; }

        LineSegment<TCoordinate> this[Int32 index] { get; set; }
    }

    ///<summary>An interface for classes which support adding nodes to a segment string.
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public interface INodableSegmentString<TCoordinate>:ISegmentString<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        ///<summary>Adds an intersection node for a given point and segment to this segment string.
        ///</summary>
        ///<param name="intPt">the location of the intersection</param>
        ///<param name="segmentIndex">the index of the segment containing the intersection</param>
        void AddIntersection(TCoordinate intPt, Int32 segmentIndex);
        
    }
}