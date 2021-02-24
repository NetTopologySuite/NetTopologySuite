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
            : base(GML2.GMLVersion.Three)
        {
        }
    }
}
