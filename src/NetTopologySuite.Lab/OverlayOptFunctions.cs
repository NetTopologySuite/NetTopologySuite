using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;

namespace NetTopologySuite
{
    /// <summary>
    /// Functions to test using spatial predicates
    /// as a filter in front of overlay operations
    /// to optimize performance.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class OverlayOptFunctions
    {
        /// <summary>
        /// Use spatial predicates as a filter
        /// in front of intersection.
        /// </summary>
        /// <param name="a">A geometry</param>
        /// <param name="b">A geometry</param>
        /// <returns>The intersection of the geometries</returns>
        public static Geometry IntersectionOpt(Geometry a, Geometry b)
        {
            if (!a.Intersects(b)) return null;
            if (a.Covers(b)) return b.Copy();
            if (b.Covers(a)) return a.Copy();
            return a.Intersection(b);
        }

        /// <summary>
        /// Use prepared geometry spatial predicates as a filter
        /// in front of intersection,
        /// with the first operand prepared.
        /// </summary>
        /// <param name="a">A geometry</param>
        /// <param name="b">A geometry</param>
        /// <returns>The intersection of the geometries</returns>
        public static Geometry IntersectionOptPrep(Geometry a, Geometry b)
        {
            var pg = CacheFetch(a);
            if (!pg.Intersects(b)) return null;
            if (pg.Covers(b)) return b.Copy();
            return a.Intersection(b);
        }

        private static Geometry _cacheKey;
        private static IPreparedGeometry _cache;

        private static IPreparedGeometry CacheFetch(Geometry g)
        {
            if (g != _cacheKey)
            {
                _cacheKey = g;
                _cache = new PreparedGeometryFactory().Create(g);
            }
            return _cache;
        }
    }
}
