using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Noding;
using NetTopologySuite.Operation.Predicate;

namespace NetTopologySuite.Geometries.Prepared
{
    /// <summary>
    /// A prepared version for <see cref="IPolygonal"/> geometries.
    /// This class supports both <see cref="Polygon"/>s and <see cref="MultiPolygon"/>s.
    /// <para>This class does <b>not</b> support MultiPolygons which are non-valid
    /// (e.g. with overlapping elements).
    /// </para>
    /// <para/>
    /// Instances of this class are thread-safe and immutable.
    /// </summary>
    /// <author>mbdavis</author>
    public class PreparedPolygon : BasicPreparedGeometry
    {
        private readonly bool _isRectangle;
        // create these lazily, since they are expensive
        private readonly object _lockSif = new object(), _lockPia = new object();
        private volatile FastSegmentSetIntersectionFinder _segIntFinder;
        private volatile IPointOnGeometryLocator _pia;

        public PreparedPolygon(IPolygonal poly)
            : base((Geometry)poly)
        {
            _isRectangle = Geometry.IsRectangle;
        }

        public FastSegmentSetIntersectionFinder IntersectionFinder
        {
            get
            {
                /*
                 * MD - Another option would be to use a simple scan for
                 * segment testing for small geometries.
                 * However, testing indicates that there is no particular advantage
                 * to this approach.
                 */
                if (_segIntFinder == null)
                {
                    lock (_lockSif)
                    {
                        if (_segIntFinder == null)
                            _segIntFinder = new FastSegmentSetIntersectionFinder(SegmentStringUtil.ExtractSegmentStrings(Geometry));
                    }
                }

                return _segIntFinder;
            }
        }

        public IPointOnGeometryLocator PointLocator
        {
            get
            {
                if (_pia == null)
                {
                    lock (_lockPia)
                    {
                        if (_pia == null)
                            _pia = new IndexedPointInAreaLocator(Geometry);
                    }
                }

                return _pia;
            }
        }

        public override bool Intersects(Geometry g)
        {
            // envelope test
            if (!EnvelopesIntersect(g)) return false;

            // optimization for rectangles
            if (_isRectangle)
            {
                return RectangleIntersects.Intersects((Polygon)Geometry, g);
            }

            return PreparedPolygonIntersects.Intersects(this, g);
        }

        public override bool Contains(Geometry g)
        {
            // short-circuit test
            if (!EnvelopeCovers(g))
                return false;

            // optimization for rectangles
            if (_isRectangle)
            {
                return RectangleContains.Contains((Polygon)Geometry, g);
            }

            return PreparedPolygonContains.Contains(this, g);
        }

        public override bool ContainsProperly(Geometry g)
        {
            // short-circuit test
            if (!EnvelopeCovers(g))
                return false;
            return PreparedPolygonContainsProperly.ContainsProperly(this, g);
        }

        public override bool Covers(Geometry g)
        {
            // short-circuit test
            if (!EnvelopeCovers(g))
                return false;
            // optimization for rectangle arguments
            if (_isRectangle)
            {
                return true;
            }
            return PreparedPolygonCovers.Covers(this, g);
        }
    }
}