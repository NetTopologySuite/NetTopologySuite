using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
    ///<summary>
    /// Tests whether this point intersects a <see cref="IGeometry{TCoordinate}"/>.
    /// The optimization here is that computing topology for the test geometry
    /// is avoided. This can be significant for large geometries.
    ///</summary>
    public class PreparedPoint<TCoordinate> : BasicPreparedGeometry<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        ///<summary>
        /// Constructs an instance of <see cref="PreparedPoint{TCoordinate}"/>
        ///</summary>
        ///<param name="point">base geometry</param>
        public PreparedPoint(IPoint<TCoordinate> point)
            : base(point)
        {
        }

        public override Boolean Intersects(IGeometry<TCoordinate> g)
        {
            if (!EnvelopesIntersect(g))
                return false;

            /**
             * This avoids computing topology for the test geometry
             */
            return IsAnyTargetComponentInTest(g);
        }
    }
}