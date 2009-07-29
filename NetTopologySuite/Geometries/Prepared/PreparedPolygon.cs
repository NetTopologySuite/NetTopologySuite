using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm.Locate;
using GisSharpBlog.NetTopologySuite.Noding;
using GisSharpBlog.NetTopologySuite.Operation.Predicate;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
    ///<summary>
    /// A prepared version for <see cref="IPolygon{TCoordinate}"/> geometries.
    ///</summary>
    public class PreparedPolygon<TCoordinate> : BasicPreparedGeometry<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly Boolean _isRectangle = false;
        // create these lazily, since they are expensive
        private FastSegmentSetIntersectionFinder<TCoordinate> _segIntFinder = null;
        private IPointOnGeometryLocator<TCoordinate> _pia = null;

        ///<summary>
        /// Constructs an instance of <see cref="PreparedPolygon{TCoordinate}"/>.
        ///</summary>
        ///<param name="polygon">the polygon to prepare</param>
        public PreparedPolygon(IPolygon<TCoordinate> polygon)
            :base(polygon)
        {
            _isRectangle = polygon.IsRectangle;
        }

        ///<summary>
        /// Intersection Finder
        ///</summary>
        public FastSegmentSetIntersectionFinder<TCoordinate> IntersectionFinder
        {
            get
            {
                if (_segIntFinder == null)
                    _segIntFinder = new FastSegmentSetIntersectionFinder<TCoordinate>(
                        Geometry.Factory,
                        SegmentStringUtil<TCoordinate>.ExtractSegmentStrings(Geometry));

                return _segIntFinder;
            }
        }

        ///<summary>
        /// Point Locator
        ///</summary>
        public IPointOnGeometryLocator<TCoordinate> PointLocator
        {
            get
            {
                if (_pia == null)
                    _pia = new IndexedPointInAreaLocator<TCoordinate>(Geometry);

                return _pia;
            }
        }

        public override Boolean Intersects(IGeometry<TCoordinate> g)
        {
            // envelope test
            if (!EnvelopesIntersect(g)) return false;

            // optimization for rectangles
            if (_isRectangle)
                return RectangleIntersects<TCoordinate>.Intersects((IPolygon<TCoordinate>)Geometry, g);

            return PreparedPolygonIntersects<TCoordinate>.Intersects(this, g);
        }

        public override Boolean Contains(IGeometry<TCoordinate> g)
        {
            // short-circuit test
            if (!EnvelopeCovers(g))
                return false;

            // optimization for rectangles
            if (_isRectangle)
                return RectangleContains<TCoordinate>.Contains((IPolygon<TCoordinate>) Geometry, g);

            return PreparedPolygonContains<TCoordinate>.Contains(this, g);
        }

        public override Boolean ContainsProperly(IGeometry<TCoordinate> g)
        {
            // short-circuit test
            if (!EnvelopeCovers(g))
                return false;

            return PreparedPolygonContainsProperly<TCoordinate>.ContainsProperly(this, g);
        }

        public override Boolean Covers(IGeometry<TCoordinate> g)
        {
            // short-circuit test
            if (!EnvelopeCovers(g))
                return false;
            // optimization for rectangle arguments
            if (_isRectangle)
                return true;

            return PreparedPolygonCovers<TCoordinate>.Covers(this, g);
        }
    }
}
