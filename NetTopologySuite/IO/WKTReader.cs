using System;
using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;
using GeoAPI.Geometries;
using GeoAPI.IO;
using NetTopologySuite.Geometries;
using RTools_NTS.Util;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Converts a Well-Known Text string to a <c>Geometry</c>.
    ///
    /// The <c>WKTReader</c> allows
    /// extracting <c>Geometry</c> objects from either input streams or
    /// internal strings. This allows it to function as a parser to read <c>Geometry</c>
    /// objects from text blocks embedded in other data formats (e.g. XML).
    ///
    /// The Well-known
    /// Text format is defined in the <A HREF="http://www.opengis.org/techno/specs.htm">
    /// OpenGIS Simple Features Specification for SQL</A> .
    ///
    /// NOTE:  There is an inconsistency in the SFS.
    /// The WKT grammar states that <c>MultiPoints</c> are represented by
    /// <c>MULTIPOINT ( ( x y), (x y) )</c>,
    /// but the examples show <c>MultiPoint</c>s as <c>MULTIPOINT ( x y, x y )</c>.
    /// Other implementations follow the latter syntax, so NTS will adopt it as well.
    /// A <c>WKTReader</c> is parameterized by a <c>GeometryFactory</c>,
    /// to allow it to create <c>Geometry</c> objects of the appropriate
    /// implementation. In particular, the <c>GeometryFactory</c> will
    /// determine the <c>PrecisionModel</c> and <c>SRID</c> that is used.
    /// The <c>WKTReader</c> will convert the input numbers to the precise
    /// internal representation.
    /// <remarks>
    /// <see cref="WKTReader" /> reads also non-standard "LINEARRING" tags.
    /// </remarks>
    /// </summary>
    public class WKTReader : ITextGeometryReader
    {
        private ICoordinateSequenceFactory _coordinateSequencefactory;
        private IPrecisionModel _precisionModel;

        private static readonly System.Globalization.CultureInfo InvariantCulture =
            System.Globalization.CultureInfo.InvariantCulture;
        private static readonly string NaNString = double.NaN.ToString(InvariantCulture); /*"NaN"*/

        /// <summary>
        /// Creates a <c>WKTReader</c> that creates objects using a basic GeometryFactory.
        /// </summary>
        public WKTReader() : this(GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory()) { }

        /// <summary>
        /// Creates a <c>WKTReader</c> that creates objects using the given
        /// <c>GeometryFactory</c>.
        /// </summary>
        /// <param name="geometryFactory">The factory used to create <c>Geometry</c>s.</param>
        public WKTReader(IGeometryFactory geometryFactory)
        {
            _coordinateSequencefactory = geometryFactory.CoordinateSequenceFactory;
            _precisionModel = geometryFactory.PrecisionModel;
            DefaultSRID = geometryFactory.SRID;
        }

        /// <summary>
        /// Gets or sets the factory to create geometries
        /// </summary>
        public IGeometryFactory Factory
        {
            get => GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(_precisionModel, DefaultSRID, _coordinateSequencefactory);
            set
            {
                if (value != null)
                {
                    _coordinateSequencefactory = value.CoordinateSequenceFactory;
                    _precisionModel = value.PrecisionModel;
                    DefaultSRID = value.SRID;
                }
            }
        }

        /// <summary>
        /// Gets or sets the default SRID
        /// </summary>
        public int DefaultSRID { get; set; }

        /// <summary>
        /// Converts a Well-known Text representation to a <c>Geometry</c>.
        /// </summary>
        /// <param name="wellKnownText">
        /// one or more Geometry Tagged Text strings (see the OpenGIS
        /// Simple Features Specification) separated by whitespace.
        /// </param>
        /// <returns>
        /// A <c>Geometry</c> specified by <c>wellKnownText</c>
        /// </returns>
        public IGeometry Read(string wellKnownText)
        {
            var input = new AntlrInputStream(wellKnownText);
            var lexer = new WKTLexer(input);
            var tokenStream = new CommonTokenStream(lexer);
            return this.Read(tokenStream);
        }

        /// <summary>
        /// Converts a Well-known Text representation to a <c>Geometry</c>.
        /// </summary>
        /// <param name="stream">
        /// one or more Geometry Tagged Text strings (see the OpenGIS
        /// Simple Features Specification) separated by whitespace.
        /// </param>
        /// <returns>
        /// A <c>Geometry</c> specified by <c>wellKnownText</c>
        /// </returns>
        public IGeometry Read(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return this.Read(reader);
            }
        }

        /// <summary>
        /// Converts a Well-known Text representation to a <c>Geometry</c>.
        /// </summary>
        /// <param name="reader">
        /// A Reader which will return a "Geometry Tagged Text"
        /// string (see the OpenGIS Simple Features Specification).
        /// </param>
        /// <returns>A <c>Geometry</c> read from <c>reader</c>.
        /// </returns>
        public IGeometry Read(TextReader reader)
        {
            var input = new UnbufferedCharStream(reader);
            var lexer = new WKTLexer(input);
            var tokenStream = new UnbufferedTokenStream(lexer);
            return this.Read(tokenStream);
        }

        private IGeometry Read(ITokenStream tokenStream)
        {
            var parser = new WKTParser(tokenStream);
            var geom = parser.geom();

            // TODO
            return null;
        }

        #region Implementation of IGeometryIOSettings

        public bool HandleSRID
        {
            get => true;
            set { }
        }

        public Ordinates AllowedOrdinates => Ordinates.XYZ;

        public Ordinates HandleOrdinates
        {
            get => AllowedOrdinates;
            set { }
        }

        /// <summary>
        /// Gets or sets whether invalid linear rings should be fixed
        /// </summary>
        public bool RepairRings { get; set; }

        #endregion
    }
}
