using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Noding;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
    /**
     * A prepared version for {@link Lineal} geometries.
     * 
     * @author mbdavis
     *
     */
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
                /**
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

        /**
         * There's not much point in trying to optimize contains, since 
         * contains for linear targets requires the entire test geometry 
         * to exactly match the target linework.
         */
    }
}