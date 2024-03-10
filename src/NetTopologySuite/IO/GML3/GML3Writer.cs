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

        public GML3Writer(bool writeSrsNameAttribute)
            : base(GML2.GMLVersion.Three, writeSrsNameAttribute)
        {
        }

        /// <summary>
        /// Provides the srsName exposing the SRID of the geometry
        /// </summary>
        /// <param name="srid">The SRID of the geometry</param>
        /// <returns></returns>
        protected override string GetSrsName(int srid)
        {
            return $"https://www.opengis.net/def/crs/EPSG/0/{srid}";
        }
    }
}
