using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Noding;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
    ///<summary>
    /// <see cref="ISpatialRelation{TCoordinate}.Intersects(GeoAPI.Geometries.IGeometry{TCoordinate})"/> operation for <see cref="PreparedLineString{TCoordinate}"/>.
    ///</summary>
    public class PreparedLineStringIntersects<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        protected PreparedLineString<TCoordinate> _prepLine;

        ///<summary>
        /// Creates an instance of this operation.
        ///</summary>
        ///<param name="prepLine">the target PreparedLineString</param>
        public PreparedLineStringIntersects(PreparedLineString<TCoordinate> prepLine)
        {
            _prepLine = prepLine;
        }

        ///<summary>
        /// Computes the intersects predicate between a <see cref="PreparedLineString{TCoordinate}"/>
        /// and a <see cref="IGeometry{TCoordinate}"/>.
        ///</summary>
        ///<param name="prep">the prepared linestring</param>
        ///<param name="geom">a test geometry</param>
        ///<returns>true if the linestring intersects the geometry</returns>
        public static Boolean Intersects(PreparedLineString<TCoordinate> prep, IGeometry<TCoordinate> geom)
        {
            PreparedLineStringIntersects<TCoordinate> op = new PreparedLineStringIntersects<TCoordinate>(prep);
            return op.Intersects(geom);
        }

        ///<summary>
        /// Tests whether this geometry intersects a given geometry.
        ///</summary>
        ///<param name="geom">the test geometry</param>
        ///<returns>true if the test geometry intersects</returns>
        public Boolean Intersects(IGeometry<TCoordinate> geom)
        {
            /**
             * If any segments intersect, obviously intersects = true
             */
            List<NodedSegmentString<TCoordinate>> lineSegStr = SegmentStringUtil<TCoordinate>.ExtractSegmentStrings(geom);
            Boolean segsIntersect = _prepLine.IntersectionFinder.Intersects(lineSegStr);
            // MD - performance testing
            //		boolean segsIntersect = false;
            if (segsIntersect)
                return true;

            /**
             * For L/L case we are done
             */
            if (geom.Dimension == Dimensions.Point) return false;

            /**
             * For L/A case, need to check for proper inclusion of the target in the test
             */
            if (geom.Dimension == Dimensions.Curve
                && _prepLine.IsAnyTargetComponentInTest(geom)) return true;

            /** 
             * For L/P case, need to check if any points lie on line(s)
             */
            if (geom.Dimension == Dimensions.Surface)
                return IsAnyTestPointInTarget(geom);

            //		return prepLine.getGeometry().intersects(geom);
            return false;
        }

        ///<summary>
        /// Tests whether any representative point of the test Geometry intersects
        /// the target geometry.
        /// Only handles test geometries which are <see cref="Dimensions.Point"/>.
        ///</summary>
        /// <param name="testGeom"><see cref="Dimensions.Point"/> geometry to test</param>
        /// <returns>true if any point of the argument intersects the prepared geometry</returns>
        protected Boolean IsAnyTestPointInTarget(IGeometry<TCoordinate> testGeom)
        {
            /**
             * This could be optimized by using the segment index on the lineal target.
             * However, it seems like the L/P case would be pretty rare in practice.
             */
            PointLocator<TCoordinate> locator = new PointLocator<TCoordinate>();
            foreach (TCoordinate p in testGeom.Coordinates)
            {
                if (locator.Intersects(p, _prepLine.Geometry))
                    return true;
            }
            return false;
        }
    }
}