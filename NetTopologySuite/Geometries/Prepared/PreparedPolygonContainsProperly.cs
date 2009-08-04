using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Noding;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
    ///<summary>
    /// <see>ContainsProperly</see> operation for <see cref="PreparedPolygon{TCoordinate}"/>.
    ///</summary>
    public class PreparedPolygonContainsProperly<TCoordinate> : PreparedPolygonPredicate<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        ///<summary>
        /// Creates an instance of this operation.
        ///</summary>
        ///<param name="prepPoly">the PreparedPolygon to evaluate</param>
        public PreparedPolygonContainsProperly(PreparedPolygon<TCoordinate> prepPoly)
            : base(prepPoly)
        {
        }

        ///<summary>
        /// Computes the </see>ContainsProperly</see> predicate between a <see cref="PreparedPolygon{TCoordinate}"/> and a <see cref="IGeometry{TCoordinate}"/>.
        ///</summary>
        ///<param name="prep">the prepared Polygon</param>
        ///<param name="geom">the geometry to test</param>
        ///<returns>true if the test geometry is properly contained from the prepared geometry</returns>
        public static Boolean ContainsProperly(PreparedPolygon<TCoordinate> prep, IGeometry<TCoordinate> geom)
        {
            PreparedPolygonContainsProperly<TCoordinate> polyInt = new PreparedPolygonContainsProperly<TCoordinate>(prep);
            return polyInt.ContainsProperly(geom);
        }

        ///<summary>
        /// Tests whether this PreparedPolygon containsProperly a given geometry.
        ///</summary>
        ///<param name="geom">the test geometry</param>
        ///<returns>true if the test geometry is contained properly</returns>
        public Boolean ContainsProperly(IGeometry<TCoordinate> geom)
        {
            /**
             * Do point-in-poly tests first, since they are cheaper and may result
             * in a quick negative result.
             * 
             * If a point of any test components does not lie in the target interior, result is false
             */
            if (!IsAllTestComponentsInTargetInterior(geom))
                return false;

            /**
             * If any segments intersect, result is false.
             */
            EdgeList<TCoordinate> lineSegStr = SegmentStringUtil<TCoordinate>.ExtractSegmentStrings(geom);
            Boolean segsIntersect = _prepPoly.IntersectionFinder.Intersects(lineSegStr);
            if (segsIntersect)
                return false;

            /**
             * Given that no segments intersect, if any vertex of the target
             * is contained in some test component.
             * the test is NOT properly contained.
             */
            if (geom is IPolygon<TCoordinate>)
            {
                // TODO: generalize this to handle GeometryCollections
                Boolean isTargetGeomInTestArea = IsAnyTargetComponentInAreaTest(geom, _prepPoly.RepresentativePoints);
                if (isTargetGeomInTestArea)
                    return false;
            }

            return true;
        }
    }
}