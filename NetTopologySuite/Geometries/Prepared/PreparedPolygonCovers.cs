using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Geometries.Prepared
{
    ///<summary>
    /// <see cref="ISpatialRelation{TCoordinate}.Covers(GeoAPI.Geometries.IGeometry{TCoordinate})"/> operation for <see cref="PreparedPolygon{TCoordinate}"/>.
    ///</summary>
    public class PreparedPolygonCovers<TCoordinate> : AbstractPreparedPolygonContains<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        ///<summary>
        /// Creates an instance of this operation.
        ///</summary>
        ///<param name="prepPoly">the PreparedPolygon to evaluate</param>
        public PreparedPolygonCovers(PreparedPolygon<TCoordinate> prepPoly)
            : base(prepPoly)
        {
            RequireSomePointInInterior = false;
        }

        ///<summary>
        /// Computes the <see cref="ISpatialRelation.Covers(GeoAPI.Geometries.IGeometry)"/> predicate between a <see cref="PreparedPolygon{TCoordinate}"/> and a <see cref="IGeometry{TCoordinate}"/>.
        ///</summary>
        ///<param name="prep">the prepared polygon</param>
        ///<param name="geom">a test geometry</param>
        ///<returns>true if the polygon covers the geometry</returns>
        public static Boolean Covers(PreparedPolygon<TCoordinate> prep, IGeometry<TCoordinate> geom)
        {
            PreparedPolygonCovers<TCoordinate> polyInt = new PreparedPolygonCovers<TCoordinate>(prep);
            return polyInt.Covers(geom);
        }

        ///<summary>
        /// Tests whether this PreparedPolygon <tt>covers</tt> a given geometry.
        ///</summary>
        ///<param name="geom"></param>
        ///<returns>true if the test geometry is covered</returns>
        public Boolean Covers(IGeometry<TCoordinate> geom)
        {
            return Eval(geom);
        }

        ///<summary>
        /// Computes the full topological <see cref="ISpatialRelation.Covers(GeoAPI.Geometries.IGeometry)"/> predicate.
        ///</summary>
        /// <param name="geom">the test geometry</param>
        /// <returns>true if this prepared polygon covers the test geometry</returns>
        protected override Boolean FullTopologicalPredicate(IGeometry<TCoordinate> geom)
        {
            return _prepPoly.Geometry.Covers(geom);
        }
    }
}