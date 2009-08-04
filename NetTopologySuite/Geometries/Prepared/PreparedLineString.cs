using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Noding;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
    ///<summary>
    /// A prepared version for <see cref="ILineString{TCoordinate}"/> geometries.
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class PreparedLineString<TCoordinate> : BasicPreparedGeometry<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private FastSegmentSetIntersectionFinder<TCoordinate> _segIntFinder;

        ///<summary>
        /// Constructs a <see cref="PreparedLineString{TCoordinate}"/>.
        ///</summary>
        ///<param name="line"><see cref="ILineString{TCoordinate}"/> to prepare</param>
        public PreparedLineString(ILineString<TCoordinate> line)
            : base(line)
        {
        }

        /// <summary>
        /// Intersection Finder
        /// </summary>
        public FastSegmentSetIntersectionFinder<TCoordinate> IntersectionFinder
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
                        new FastSegmentSetIntersectionFinder<TCoordinate>(
                            Geometry.Factory,
                            SegmentStringUtil<TCoordinate>.ExtractSegmentStrings(Geometry));
                return _segIntFinder;
            }
        }

        public override Boolean Intersects(IGeometry<TCoordinate> g)
        {
            if (!EnvelopesIntersect(g))
                return false;

            return PreparedLineStringIntersects<TCoordinate>.Intersects(this, g);
        }

        /// <summary>
        /// There's not much point in trying to optimize contains, since 
        /// contains for linear targets requires the entire test geometry 
        /// to exactly match the target linework.
        /// </summary>
    }
}