using GeoAPI.Geometries;
using GeoAPI.Geometries.Prepared;

namespace NetTopologySuite.Geometries.Prepared
{
    ///<summary>
    /// A factory for creating <see cref="IPreparedGeometry"/>s. It chooses an appropriate implementation of PreparedGeometry
    /// based on the geometric type of the input geometry.
    ///</summary>
    /// <remarks>
    /// In the future, the factory may accept hints that indicate
    /// special optimizations which can be performed.
    ///</remarks>
    /// <author>Martin Davis</author>
    public class PreparedGeometryFactory
    {
        ///<summary>
        /// Creates a new <see cref="IPreparedGeometry"/> appropriate for the argument <see cref="IGeometry"/>.
        ///</summary>
        ///<param name="geom">The geometry to prepare</param>
        /// <returns>
        /// the prepared geometry
        /// </returns>
        public static IPreparedGeometry Prepare(IGeometry geom)
        {
            return (new PreparedGeometryFactory()).Create(geom);
        }

        ///<summary>
        /// Creates a new <see cref="IPreparedGeometry"/> appropriate for the argument <see cref="IGeometry"/>.
        ///</summary>
        ///<param name="geom">The geometry to prepare</param>
        /// <returns>
        /// the prepared geometry
        /// </returns>
        public IPreparedGeometry Create(IGeometry geom)
        {
            if (geom is IPolygonal)
                return new PreparedPolygon((IPolygonal)geom);
            if (geom is ILineal)
                return new PreparedLineString((ILineal)geom);
            if (geom is IPuntal)
                return new PreparedPoint((IPuntal)geom);

            /*
             * Default representation.
             */
            return new BasicPreparedGeometry(geom);
        }
    }
}
