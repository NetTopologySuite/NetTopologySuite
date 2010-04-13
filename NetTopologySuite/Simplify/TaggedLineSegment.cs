using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Simplify
{
    /// <summary>
    /// A LineSegment which is tagged with its location in a <see cref="Geometry{TCoordinate}"/>.
    /// Used to index the segments in a point and recover the segment locations
    /// from the index.
    /// </summary>
    public class TaggedLineSegment<TCoordinate> : IBoundable<IExtents<TCoordinate>>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        
        private readonly LineSegment<TCoordinate> _segment;
        private readonly IGeometry<TCoordinate> _parent;
        private readonly Int32 _index;

        public TaggedLineSegment(TCoordinate p0, TCoordinate p1, IGeometry<TCoordinate> parent, Int32 index)
        {
            _segment = new LineSegment<TCoordinate>(p0, p1);
            _parent = parent;
            _index = index;
        }

        public TaggedLineSegment(TCoordinate p0, TCoordinate p1)
            : this(p0, p1, null, -1) {}

        public IGeometry<TCoordinate> Parent
        {
            get { return _parent; }
        }

        public Int32 Index
        {
            get { return _index; }
        }

        public LineSegment<TCoordinate> LineSegment
        {
            get { return _segment; }
        }

        public bool Intersects(IExtents<TCoordinate> other)
        {
            return Bounds.Intersects(other);
        }

        public IExtents<TCoordinate> Bounds
        {
            get
            {
                return TopologyPreservingSimplifier<TCoordinate>.GeometryFactory.CreateExtents(LineSegment.P0, LineSegment.P1);
            }
        }
    }
}