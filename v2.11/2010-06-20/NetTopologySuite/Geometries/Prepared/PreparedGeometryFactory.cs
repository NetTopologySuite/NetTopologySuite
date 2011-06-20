using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
    ///</summary>
    /// A factory for creating <see cref="IPreparedGeometry{TCoordinate}"/>s.
    /// It chooses an appropriate implementation of PreparedGeometry
    /// based on the geoemtric type of the input geometry.
    ///
    /// In the future, the factory may accept hints that indicate
    /// special optimizations which can be performed.
    ///</summary>
    public class PreparedGeometryFactory<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        ///</summary>
        ///Creates a new <see cref="IPreparedGeometry{TCoordinate}"/> appropriate for the argument <see cref="IGeometry{TCoordinate}"/> 
        ///</summary>
        ///<param name="geom"> the geometry to prepare</param>
        ///<returns>the prepared geometry</returns>
        public static IPreparedGeometry<TCoordinate> Prepare(IGeometry<TCoordinate> geom)
        {
            return (new PreparedGeometryFactory<TCoordinate>()).Create(geom);
        }

        ///</summary>
        /// Creates a new <see cref="IPreparedGeometry{TCoordinate}"/> appropriate for the argument <see cref="IGeometry{TCoordinate}"/>.
        ///</summary>
        ///<param name="geom">the geometry to prepare</param>
        ///<returns>the prepared geometry</returns>
        public IPreparedGeometry<TCoordinate> Create(IGeometry<TCoordinate> geom)
        {
            if (geom is IPolygonal<TCoordinate>)
                return new PreparedPolygon<TCoordinate>((IPolygonal<TCoordinate>) geom);
            if (geom is ILineal<TCoordinate>)
                return new PreparedLineString<TCoordinate>((ILineal<TCoordinate>) geom);
            if (geom is IPuntal<TCoordinate>)
                return new PreparedPoint<TCoordinate>((IPuntal<TCoordinate>) geom);

            ///</summary>
            ///Default representation.
            ///</summary>
            return new BasicPreparedGeometry<TCoordinate>(geom);
        }
    }
}