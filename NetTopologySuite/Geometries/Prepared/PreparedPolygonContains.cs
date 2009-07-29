using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
    ///<summary>
    /// <see cref="ISpatialRelation{TCoordinate}.Contains(GeoAPI.Geometries.IGeometry{TCoordinate})"/> operation for <see cref="PreparedPolygon{TCoordinate}"/>.
    ///</summary>
    public class PreparedPolygonContains<TCoordinate> : AbstractPreparedPolygonContains<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
   {
        ///<summary>
       /// Computes the <see cref="ISpatialRelation{TCoordinate}.Contains(GeoAPI.Geometries.IGeometry{TCoordinate})"/> predicate between a <see cref="PreparedPolygon{TCoordinate}"/>
       /// and a <see cref="IGeometry{TCoordinate}"/>.
        ///</summary>
       ///<param name="prep">the prepared polygon</param>
       ///<param name="geom"> a test geometry</param>
       ///<returns>true if the polygon contains the geometry</returns>
        public static Boolean Contains(PreparedPolygon<TCoordinate> prep, IGeometry<TCoordinate> geom)
        {
            PreparedPolygonContains<TCoordinate> polyInt = new PreparedPolygonContains<TCoordinate>(prep);
            return polyInt.Contains(geom);
        }

        ///<summary>
        /// Creates an instance of this operation.
        ///</summary>
        ///<param name="prepPoly">the PreparedPolygon to evaluate</param>
        public PreparedPolygonContains(PreparedPolygon<TCoordinate> prepPoly)
            : base(prepPoly)
        {
        }

        ///<summary>
        /// Tests whether this PreparedPolygon <see cref="ISpatialRelation{TCoordinate}.Contains(GeoAPI.Geometries.IGeometry{TCoordinate})"/> a given geometry.
        ///</summary>
        ///<param name="geom">the test geometry</param>
        ///<returns>true if the test geometry is contained</returns>
        public Boolean Contains(IGeometry<TCoordinate> geom)
        {
            return Eval(geom);
        }

        ///<summary>
        /// Computes the full topological <see cref="ISpatialRelation{TCoordinate}.Contains(GeoAPI.Geometries.IGeometry{TCoordinate})"/> predicate.
        /// Used when short-circuit tests are not conclusive.
        ///</summary>
        ///<param name="geom">the test geometry</param>
        ///<returns>true if the test geometry is contained</returns>
        protected override Boolean FullTopologicalPredicate(IGeometry<TCoordinate> geom)
        {
            return _prepPoly.Geometry.Contains(geom);
        }

   }
}
