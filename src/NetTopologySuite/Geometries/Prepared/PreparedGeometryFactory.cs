namespace NetTopologySuite.Geometries.Prepared
{
    /// <summary>
    /// A factory for creating <see cref="IPreparedGeometry"/>s. It chooses an appropriate implementation of PreparedGeometry
    /// based on the geometric type of the input geometry.
    /// <para/>
    /// In the future, the factory may accept hints that indicate
    /// special optimizations which can be performed.
    /// <para/>
    /// Instances of this class are thread-safe.
    /// </summary>
    /// <author>Martin Davis</author>
    public class PreparedGeometryFactory
    {
        /// <summary>
        /// Creates a new <see cref="IPreparedGeometry"/> appropriate for the argument <see cref="Geometry"/>.
        /// </summary>
        /// <param name="geom">The geometry to prepare</param>
        /// <returns>
        /// the prepared geometry
        /// </returns>
        public static IPreparedGeometry Prepare(Geometry geom)
        {
            return (new PreparedGeometryFactory()).Create(geom);
        }

        /// <summary>
        /// Creates a new <see cref="IPreparedGeometry"/> appropriate for the argument <see cref="Geometry"/>.
        /// </summary>
        /// <param name="geom">The geometry to prepare</param>
        /// <returns>
        /// the prepared geometry
        /// </returns>
        public IPreparedGeometry Create(Geometry geom)
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
