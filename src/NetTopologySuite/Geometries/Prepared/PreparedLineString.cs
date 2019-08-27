using NetTopologySuite.Noding;

namespace NetTopologySuite.Geometries.Prepared
{
    /// <summary>
    /// A prepared version for <see cref="ILineal"/> geometries.
    /// <para>Instances of this class are thread-safe</para>.
    /// </summary>
    /// <author>mbdavis</author>
    public class PreparedLineString : BasicPreparedGeometry
    {
        private readonly object _lock = new object();
        private volatile FastSegmentSetIntersectionFinder _segIntFinder;

        public PreparedLineString(ILineal line)
            : base((Geometry)line)
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
                {
                    lock (_lock)
                    {
                        if (_segIntFinder == null)
                            _segIntFinder = new FastSegmentSetIntersectionFinder(SegmentStringUtil.ExtractSegmentStrings(Geometry));
                    }
                }

                return _segIntFinder;
            }
        }

        public override bool Intersects(Geometry g)
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
