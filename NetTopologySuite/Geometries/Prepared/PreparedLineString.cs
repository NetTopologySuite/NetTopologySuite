#if useFullGeoAPI
using GeoAPI.Geometries;
#else
using ICoordinate = NetTopologySuite.Geometries.Coordinate;
using IEnvelope = NetTopologySuite.Geometries.Envelope;
using IGeometry = NetTopologySuite.Geometries.Geometry;
using IPoint = NetTopologySuite.Geometries.Point;
using ILineString = NetTopologySuite.Geometries.LineString;
using ILinearRing = NetTopologySuite.Geometries.LinearRing;
using IPolygon = NetTopologySuite.Geometries.Polygon;
using IGeometryCollection = NetTopologySuite.Geometries.GeometryCollection;
using IMultiPoint = NetTopologySuite.Geometries.MultiPoint;
using IMultiLineString = NetTopologySuite.Geometries.MultiLineString;
using IMultiPolygon = NetTopologySuite.Geometries.MultiPolygon;
using IGeometryFactory = NetTopologySuite.Geometries.GeometryFactory;
using IPrecisionModel = NetTopologySuite.Geometries.PrecisionModel;
#endif
using NetTopologySuite.Noding;

namespace NetTopologySuite.Geometries.Prepared
{
    ///<summary>
    /// A prepared version for <see cref="ILineal"/> geometries.
    ///</summary>
    /// <author>mbdavis</author>
    public class PreparedLineString : BasicPreparedGeometry
    {
        private FastSegmentSetIntersectionFinder _segIntFinder;

        public PreparedLineString(ILineal line)
            : base((IGeometry)line)
        {
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
                    _segIntFinder =
                        new FastSegmentSetIntersectionFinder(SegmentStringUtil.ExtractSegmentStrings(Geometry));
                return _segIntFinder;
            }
        }

        public override bool Intersects(IGeometry g)
        {
            if (!EnvelopesIntersect(g)) return false;
            return PreparedLineStringIntersects.Intersects(this, g);
        }

        /*
         * There's not much point in trying to optimize contains, since 
         * contains for linear targets requires the entire test geometry 
         * to exactly match the target linework.
         */
    }
}
