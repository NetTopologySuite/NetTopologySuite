using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    ///<summary>
    /// An interface for classes which represent a sequence of contiguous line segments.
    /// SegmentStrings can carry a context object, which is useful
    /// for preserving topological or parentage information.
    ///</summary>
    public interface ISegmentString
    {
        ///<summary>
        /// Gets/Sets the user-defined data for this segment string.
        ///</summary>
        Object Context { get; set; }
        ///<summary>
        /// Points that make up ISegmentString
        ///</summary>
        ICoordinate[] Coordinates { get; }
        ///<summary>
        /// Size of Coordinate Sequence
        ///</summary>
        Int32 Count { get; }
        /// <summary>
        /// States whether ISegmentString is closed
        /// </summary>
        Boolean IsClosed { get; }

        LineSegment this[Int32 index] { get; set; }
    }
}