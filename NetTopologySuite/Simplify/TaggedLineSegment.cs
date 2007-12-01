using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Simplify
{
    /// <summary>
    /// A LineSegment which is tagged with its location in a <see cref="Geometry{TCoordinate}"/>.
    /// Used to index the segments in a point and recover the segment locations
    /// from the index.
    /// </summary>
    public class TaggedLineSegment<TCoordinate> : LineSegment<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        private readonly IGeometry<TCoordinate> _parent;
        private readonly Int32 _index;

        public TaggedLineSegment(TCoordinate p0, TCoordinate p1, IGeometry<TCoordinate> parent, Int32 index)
            : base(p0, p1)
        {
            _parent = parent;
            _index = index;
        }

        public TaggedLineSegment(TCoordinate p0, TCoordinate p1)
            : this(p0, p1, null, -1) {}

        public IGeometry Parent
        {
            get { return _parent; }
        }

        public Int32 Index
        {
            get { return _index; }
        }
    }
}