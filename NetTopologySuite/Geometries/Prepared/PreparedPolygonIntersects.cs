using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Noding;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
    ///<summary>
    /// <see cref="ISpatialRelation{TCoordinate}.Intersects(GeoAPI.Geometries.IGeometry{TCoordinate})"/> operation for <see cref="PreparedPolygon{TCoordinate}"/>.
    ///</summary>
    public class PreparedPolygonIntersects<TCoordinate> : PreparedPolygonPredicate<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        ///<summary>
        /// Computes the intersects predicate between a <see cref="PreparedPolygon{TCoordinate}"/>
        /// and a <see cref="IGeometry{TCoordinate}"/>.
        ///</summary>
        ///<param name="prep">the prepared polygon</param>
        ///<param name="geom">a test geometry</param>
        ///<returns>true if the polygon intersects the geometry</returns>
        public static Boolean Intersects(PreparedPolygon<TCoordinate> prep, IGeometry<TCoordinate> geom)
        {
            PreparedPolygonIntersects<TCoordinate> polyInt = new PreparedPolygonIntersects<TCoordinate>(prep);
            return polyInt.Intersects(geom);
        }

        ///<summary>
        /// Creates an instance of this operation.
        ///</summary>
        ///<param name="prepPoly">the PreparedPolygon to evaluate</param>
        public PreparedPolygonIntersects(PreparedPolygon<TCoordinate> prepPoly)
            : base(prepPoly)
        {
        }

        ///<summary>
        /// Tests whether this PreparedPolygon intersects a given geometry.
        ///</summary>
        ///<param name="geom">the test geometry</param>
        ///<returns>true if the test geometry intersects</returns>
        public Boolean Intersects(IGeometry<TCoordinate> geom)
        {
            /**
             * Do point-in-poly tests first, since they are cheaper and may result
             * in a quick positive result.
             * 
             * If a point of any test components lie in target, result is true
             */
            if (IsAnyTestComponentInTarget(geom))
                return true;

            /**
             * If any segments intersect, result is true
             */
            EdgeList<TCoordinate> lineSegStr = SegmentStringUtil<TCoordinate>.ExtractSegmentStrings(geom);
            if (_prepPoly.IntersectionFinder.Intersects(lineSegStr))
                return true;

            /**
             * If the test has dimension = 2 as well, it is necessary to
             * test for proper inclusion of the target.
             * Since no segments intersect, it is sufficient to test representative points.
             */
            if (geom.Dimension == Dimensions.Curve)
            {
                // TODO: generalize this to handle GeometryCollections
                if (IsAnyTargetComponentInAreaTest(geom, _prepPoly.RepresentativePoints)) 
                    return true;
            }

            return false;
        }
    }
}
