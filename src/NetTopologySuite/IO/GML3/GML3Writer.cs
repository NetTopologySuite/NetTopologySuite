namespace NetTopologySuite.IO.GML3
{
    /// <summary>
    /// Writes the GML representation of the features of NetTopologySuite model.
    /// Uses GML 3.2.2 <c>gml.xsd</c> schema for base for features.
    /// </summary>
    public class GML3Writer : GML2.GMLWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GML3Writer"/> class.
        /// </summary>
        public GML3Writer()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GML3Writer"/> class with
        /// the flag <paramref name="writeSrsNameAttribute"/> indicating if information
        /// about the spatial reference system should be written to a geometry's
        /// <c>srsName</c> attribute-
        /// </summary>
        /// <param name="writeSrsNameAttribute">Flag indicating to write <c>srsName</c></param>
        public GML3Writer(bool writeSrsNameAttribute)
            : base(GML2.GMLVersion.Three, writeSrsNameAttribute)
        {
        }

        /// <summary>
        /// Provides the srsName expressed by a <see cref="Geometries.Geometry.SRID"/> value.
        /// </summary>
        /// <param name="srid">An SRID value</param>
        /// <returns>An URL to the definition of the spatial reference system defined by <paramref name="srid"/></returns>
        protected override string GetSrsName(int srid)
        {
            return $"https://www.opengis.net/def/crs/EPSG/0/{srid}";
        }
    }
}
