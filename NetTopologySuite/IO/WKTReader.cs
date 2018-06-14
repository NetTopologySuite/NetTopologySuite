using System;
using System.Collections.Generic;
using System.IO;
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
            using (var reader = new StringReader(wellKnownText))
            {
                return Read(reader);
            }
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
                return Read(reader);
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
            /*
            var tokens = Tokenize(reader);
            StreamTokenizer tokenizer = new StreamTokenizer(reader);
            IList<Token> tokens = new List<Token>();
            tokenizer.Tokenize(tokens);     // Read directly all tokens
             */
            //_index = 0;                      // Reset pointer to start of tokens
            try
            {
                var enumerator = new StreamTokenizer(reader).GetEnumerator();
                enumerator.MoveNext();
                return ReadGeometryTaggedText(enumerator);
            }
            catch (IOException e)
            {
                throw new GeoAPI.IO.ParseException(e.ToString());
            }
        }

        internal IEnumerator<Token> Tokenizer(TextReader reader)
        {
            return new StreamTokenizer(reader).GetEnumerator();
        }

        internal IList<Token> Tokenize(TextReader reader)
        {
            var tokenizer = new StreamTokenizer(reader);
            var tokens = new List<Token>();
            tokenizer.Tokenize(tokens);     // Read directly all tokens
            return tokens;
        }

        //internal int Index { get { return _index; } set { _index = value; } }

        /// <summary>
        /// Returns the next array of <c>Coordinate</c>s in the stream.
        /// </summary>
        /// <param name="tokens">
        /// Tokenizer over a stream of text in Well-known Text
        /// format. The next element returned by the stream should be "(" (the
        /// beginning of "(x1 y1, x2 y2, ..., xn yn)") or "EMPTY".
        /// </param>
        /// <param name="skipExtraParenthesis">
        /// if set to <c>true</c> skip extra parenthesis around coordinates.
        /// </param>
        /// <returns>
        /// The next array of <c>Coordinate</c>s in the
        /// stream, or an empty array if "EMPTY" is the next element returned by
        /// the stream.
        /// </returns>
        private Coordinate[] GetCoordinates(IEnumerator<Token> tokens, bool skipExtraParenthesis, ref bool hasZ)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals("EMPTY"))
                return new Coordinate[]{};
            var coordinates = new List<Coordinate>();
            coordinates.Add(GetPreciseCoordinate(tokens, skipExtraParenthesis, ref hasZ));
            nextToken = GetNextCloserOrComma(tokens);
            while (nextToken.Equals(","))
            {
                coordinates.Add(GetPreciseCoordinate(tokens, skipExtraParenthesis, ref hasZ));
                nextToken = GetNextCloserOrComma(tokens);
            }
            return coordinates.ToArray();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="skipExtraParenthesis"></param>
        /// <returns></returns>
        private Coordinate GetPreciseCoordinate(IEnumerator<Token> tokens, bool skipExtraParenthesis, ref bool hasZ)
        {
            var coord = new Coordinate();
            bool extraParenthesisFound = false;
            if (skipExtraParenthesis)
            {
                extraParenthesisFound = IsStringValueNext(tokens, "(");
                if (extraParenthesisFound)
                {
                    tokens.MoveNext();
                    //_index++;
                }
            }
            coord.X = GetNextNumber(tokens);
            coord.Y = GetNextNumber(tokens);
            if (IsNumberNext(tokens))
            {
                coord.Z = GetNextNumber(tokens);
                if (!double.IsNaN(coord.Z)) hasZ = true;
            }

            if (skipExtraParenthesis &&
                extraParenthesisFound &&
                IsStringValueNext(tokens, ")"))
            {
                tokens.MoveNext();
                //_index++;
            }

            _precisionModel.MakePrecise(coord);
            return coord;
        }

        private static bool IsStringValueNext(IEnumerator<Token> tokens, string stringValue)
        {
            var token = tokens.Current /*as Token*/;
            if (token == null)
                throw new InvalidOperationException("current Token is null");
            return token.StringValue == stringValue;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        private static bool IsNumberNext(IEnumerator<Token> tokens)
        {
            var token = tokens.Current /*as Token*/;
            return token is FloatToken ||
                   token is IntToken ||
                   (token is WordToken && string.Compare(token.Object.ToString(), NaNString, StringComparison.OrdinalIgnoreCase) == 0);
        }

        /// <summary>
        /// Returns the next number in the stream.
        /// </summary>
        /// <param name="tokens">
        /// Tokenizer over a stream of text in Well-known Text
        /// format. The next token must be a number.
        /// </param>
        /// <returns>The next number in the stream.</returns>
        /// <exception cref="GeoAPI.IO.ParseException">if the next token is not a valid number</exception>
        private static double GetNextNumber(IEnumerator<Token> tokens)
        {
            var token = tokens.Current /*as Token*/;
            if (!tokens.MoveNext())
                throw new InvalidOperationException("premature end of enumerator");

            if (token == null)
                throw new ArgumentNullException("tokens", "Token list contains a null value");
            if (token is EofToken)
                throw new GeoAPI.IO.ParseException("Expected number but encountered end of stream");
            if (token is EolToken)
                throw new GeoAPI.IO.ParseException("Expected number but encountered end of line");
            if (token is FloatToken || token is IntToken)
                return (double) token.ConvertToType(typeof(double));
            if (token is WordToken)
            {
                if (string.Compare(token.Object.ToString(), NaNString, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return double.NaN;
                }
                throw new GeoAPI.IO.ParseException("Expected number but encountered word: " + token.StringValue);
            }
            if (token.StringValue == "(")
                throw new GeoAPI.IO.ParseException("Expected number but encountered '('");
            if (token.StringValue == ")")
                throw new GeoAPI.IO.ParseException("Expected number but encountered ')'");
            if (token.StringValue == ",")
                throw new GeoAPI.IO.ParseException("Expected number but encountered ','");

            throw new GeoAPI.IO.ParseException("Expected number but encountered '" + token.StringValue + "'");
        }

        /// <summary>
        /// Returns the next "EMPTY" or "(" in the stream as uppercase text.
        /// </summary>
        /// <param name="tokens">
        /// Tokenizer over a stream of text in Well-known Text
        /// format. The next token must be "EMPTY" or "(".
        /// </param>
        /// <returns>
        /// The next "EMPTY" or "(" in the stream as uppercase text.</returns>
        private static string GetNextEmptyOrOpener(IEnumerator<Token> tokens)
        {
            string nextWord = GetNextWord(tokens);
            if (nextWord.Equals("EMPTY") || nextWord.Equals("("))
                return nextWord;
            throw new GeoAPI.IO.ParseException("Expected 'EMPTY' or '(' but encountered '" + nextWord + "'");
        }

        /// <summary>
        /// Returns the next ")" or "," in the stream.
        /// </summary>
        /// <param name="tokens">
        /// Tokenizer over a stream of text in Well-known Text
        /// format. The next token must be ")" or ",".
        /// </param>
        /// <returns>
        /// The next ")" or "," in the stream.</returns>
        private static string GetNextCloserOrComma(IEnumerator<Token> tokens)
        {
            string nextWord = GetNextWord(tokens);
            if (nextWord.Equals(",") || nextWord.Equals(")"))
                return nextWord;

            throw new GeoAPI.IO.ParseException("Expected ')' or ',' but encountered '" + nextWord
                + "'");
        }

        /// <summary>
        /// Returns the next ")" in the stream.
        /// </summary>
        /// <param name="tokens">
        /// Tokenizer over a stream of text in Well-known Text
        /// format. The next token must be ")".
        /// </param>
        /// <returns>
        /// The next ")" in the stream.</returns>
        private static string GetNextCloser(IEnumerator<Token> tokens)
        {
            string nextWord = GetNextWord(tokens);
            if (nextWord.Equals(")"))
                return nextWord;
            throw new GeoAPI.IO.ParseException("Expected ')' but encountered '" + nextWord + "'");
        }

        /// <summary>
        /// Returns the next word in the stream as uppercase text.
        /// </summary>
        /// <param name="tokens">
        /// Tokenizer over a stream of text in Well-known Text
        /// format. The next token must be a word.
        /// </param>
        /// <returns>The next word in the stream as uppercase text.</returns>
        private static string GetNextWord(IEnumerator<Token> tokens)
        {
            var token = tokens.Current /*as Token*/;
            if (token == null)
                throw new InvalidOperationException("current token is null");

            if (!tokens.MoveNext())
                throw new InvalidOperationException("premature end of enumerator");

            if (token is EofToken)
                throw new GeoAPI.IO.ParseException("Expected number but encountered end of stream");
            if (token is EolToken)
                throw new GeoAPI.IO.ParseException("Expected number but encountered end of line");
            if (token is FloatToken || token is IntToken)
                throw new GeoAPI.IO.ParseException("Expected word but encountered number: " + token.StringValue);
            if (token is WordToken)
                return token.StringValue.ToUpper();
            if (token.StringValue == "(")
                return "(";
            if (token.StringValue == ")")
                return ")";
            if (token.StringValue == ",")
                return ",";

            throw new InvalidOperationException("Should never reach here!");
            //Assert.ShouldNeverReachHere();
            //return null;
        }

        /// <summary>
        /// Creates a <c>Geometry</c> using the next token in the stream.
        /// </summary>
        /// <param name="tokens">
        /// Tokenizer over a stream of text in Well-known Text
        /// format. The next tokens must form a &lt;Geometry Tagged Text.
        /// </param>
        /// <returns>A <c>Geometry</c> specified by the next token
        /// in the stream.</returns>
        internal IGeometry ReadGeometryTaggedText(IEnumerator<Token> tokens)
        {
            /*
             * A new different implementation by Marc Jacquin:
             * this code manages also SRID values.
             */
            IGeometry returned;

            int srid;
            string type = GetNextWord(tokens);
            if (type == "SRID")
            {
                tokens.MoveNext(); // =
                srid = Convert.ToInt32(GetNextNumber(tokens));
                tokens.MoveNext(); // ;
                type = GetNextWord(tokens);

                //sridValue = tokens[2].ToString();
                //// tokens.RemoveRange(0, 4);
                //tokens.RemoveAt(0);
                //tokens.RemoveAt(0);
                //tokens.RemoveAt(0);
                //tokens.RemoveAt(0);
            }
            else
                srid = DefaultSRID;

            /*Test of Z, M or ZM suffix*/
            var suffix = tokens.Current;

            if (suffix is WordToken)
            {
                if (suffix == "Z")
                {
                    tokens.MoveNext();
                }
                else if (suffix == "ZM")
                {
                    tokens.MoveNext();
                }
                else if (suffix == "M")
                {
                    tokens.MoveNext();
                }
            }

            var factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(_precisionModel, srid,
                _coordinateSequencefactory);

            if (type.Equals("POINT"))
                returned = ReadPointText(tokens, factory);
            else if (type.Equals("LINESTRING"))
                returned = ReadLineStringText(tokens, factory);
            else if (type.Equals("LINEARRING"))
                returned = ReadLinearRingText(tokens, factory);
            else if (type.Equals("POLYGON"))
                returned = ReadPolygonText(tokens, factory);
            else if (type.Equals("MULTIPOINT"))
                returned = ReadMultiPointText(tokens, factory);
            else if (type.Equals("MULTILINESTRING"))
                returned = ReadMultiLineStringText(tokens, factory);
            else if (type.Equals("MULTIPOLYGON"))
                returned = ReadMultiPolygonText(tokens, factory);
            else if (type.Equals("GEOMETRYCOLLECTION"))
                returned = ReadGeometryCollectionText(tokens, factory);
            else throw new GeoAPI.IO.ParseException("Unknown type: " + type);

            if (returned == null)
                throw new NullReferenceException("Error reading geometry");

            return returned;
        }

        /// <summary>
        /// Creates a <c>Point</c> using the next token in the stream.
        /// </summary>
        /// <param name="tokens">
        ///   Tokenizer over a stream of text in Well-known Text
        ///   format. The next tokens must form a &lt;Point Text.
        /// </param>
        /// <param name="factory"> </param>
        /// <returns>A <c>Point</c> specified by the next token in
        /// the stream.</returns>
        private IPoint ReadPointText(IEnumerator<Token> tokens, IGeometryFactory factory)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals("EMPTY"))
                return factory.CreatePoint();
            bool hasZ = false;
            var coord = GetPreciseCoordinate(tokens, false, ref hasZ);
            var point = factory.CreatePoint(ToSequence(hasZ, coord));
            /*var closer = */GetNextCloser(tokens);
            return point;
        }

        private ICoordinateSequence ToSequence(bool hasZ, params Coordinate[] coords)
        {
            int dimensions = hasZ ? 3 : 2;
            var seq = _coordinateSequencefactory.Create(coords.Length, dimensions);
            for (int i = 0; i < coords.Length; i++)
            {
                seq.SetOrdinate(i, Ordinate.X, coords[i].X);
                seq.SetOrdinate(i, Ordinate.Y, coords[i].Y);
            }
            if (dimensions == 3)
            {
                for (int i = 0; i < coords.Length; i++)
                    seq.SetOrdinate(i, Ordinate.Z, coords[i].Z);
            }
            return seq;
        }

        /// <summary>
        /// Creates a <c>LineString</c> using the next token in the stream.
        /// </summary>
        /// <param name="tokens">
        ///   Tokenizer over a stream of text in Well-known Text
        ///   format. The next tokens must form a &lt;LineString Text.
        /// </param>
        /// <param name="factory"> </param>
        /// <returns>
        /// A <c>LineString</c> specified by the next
        /// token in the stream.</returns>
        private ILineString ReadLineStringText(IEnumerator<Token> tokens, IGeometryFactory factory)
        {
            bool hasZ = false;
            var coords = GetCoordinates(tokens, false, ref hasZ);
            return factory.CreateLineString(ToSequence(hasZ, coords));
        }

        /// <summary>
        /// Creates a <c>LinearRing</c> using the next token in the stream.
        /// </summary>
        /// <param name="tokens">
        ///   Tokenizer over a stream of text in Well-known Text
        ///   format. The next tokens must form a &lt;LineString Text.
        /// </param>
        /// <param name="factory"> </param>
        /// <returns>A <c>LinearRing</c> specified by the next
        /// token in the stream.</returns>
        private ILinearRing ReadLinearRingText(IEnumerator<Token> tokens, IGeometryFactory factory)
        {
            bool hasZ = false;
            var coords = GetCoordinates(tokens, false, ref hasZ);
            return factory.CreateLinearRing(ToSequence(hasZ, coords));
        }

        /// <summary>
        /// Creates a <c>MultiPoint</c> using the next token in the stream.
        /// </summary>
        /// <param name="tokens">
        ///   Tokenizer over a stream of text in Well-known Text
        ///   format. The next tokens must form a &lt;MultiPoint Text.
        /// </param>
        /// <param name="factory"> </param>
        /// <returns>
        /// A <c>MultiPoint</c> specified by the next
        /// token in the stream.</returns>
        private IMultiPoint ReadMultiPointText(IEnumerator<Token> tokens, IGeometryFactory factory)
        {
            bool hasZ = false;
            var coords = GetCoordinates(tokens, true, ref hasZ);
            return factory.CreateMultiPoint(ToPoints(ToSequence(hasZ, coords), factory));
        }

        /// <summary>
        /// Creates an array of <c>Point</c>s having the given <c>Coordinate</c>s.
        /// </summary>
        /// <param name="coordinates">
        /// The <c>Coordinate</c>s with which to create the <c>Point</c>s
        /// </param>
        /// <param name="factory">The factory to create the points</param>
        /// <returns>
        /// <c>Point</c>s created using this <c>WKTReader</c>
        /// s <c>GeometryFactory</c>.
        /// </returns>
        private IPoint[] ToPoints(ICoordinateSequence coordinates, IGeometryFactory factory)
        {
            var points = new IPoint[coordinates.Count];
            for (int i = 0; i < coordinates.Count; i++)
            {
                var cs = _coordinateSequencefactory.Create(1, coordinates.Ordinates);
                CoordinateSequences.Copy(coordinates, i, cs, 0, 1);
                points[i] = factory.CreatePoint(cs);
            }
            return points;
        }

        /// <summary>
        /// Creates a <c>Polygon</c> using the next token in the stream.
        /// </summary>
        /// <param name="tokens">
        ///   Tokenizer over a stream of text in Well-known Text
        ///   format. The next tokens must form a Polygon Text.
        /// </param>
        /// <param name="factory"> </param>
        /// <returns>
        /// A <c>Polygon</c> specified by the next token
        /// in the stream.
        /// </returns>
        private IPolygon ReadPolygonText(IEnumerator<Token> tokens, IGeometryFactory factory)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals("EMPTY"))
                return factory.CreatePolygon();

            var holes = new List<ILinearRing>();
            var shell = ReadLinearRingText(tokens, factory);
            nextToken = GetNextCloserOrComma(tokens);
            while (nextToken.Equals(","))
            {
                var hole = ReadLinearRingText(tokens, factory);
                holes.Add(hole);
                nextToken = GetNextCloserOrComma(tokens);
            }
            return factory.CreatePolygon(shell, holes.ToArray());
        }

        /// <summary>
        /// Creates a <c>MultiLineString</c> using the next token in the stream.
        /// </summary>
        /// <param name="tokens">
        ///   Tokenizer over a stream of text in Well-known Text
        ///   format. The next tokens must form a MultiLineString Text.
        /// </param>
        /// <param name="factory"> </param>
        /// <returns>
        /// A <c>MultiLineString</c> specified by the
        /// next token in the stream.</returns>
        private IMultiLineString ReadMultiLineStringText(IEnumerator<Token> tokens, IGeometryFactory factory)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals("EMPTY"))
                return factory.CreateMultiLineString();

            var lineStrings = new List<ILineString>();
            var lineString = ReadLineStringText(tokens, factory);
            lineStrings.Add(lineString);
            nextToken = GetNextCloserOrComma(tokens);
            while (nextToken.Equals(",")) {

                lineString = ReadLineStringText(tokens, factory);
                lineStrings.Add(lineString);
                nextToken = GetNextCloserOrComma(tokens);
            }
            return factory.CreateMultiLineString(lineStrings.ToArray());
        }

        /// <summary>
        /// Creates a <c>MultiPolygon</c> using the next token in the stream.
        /// </summary>
        /// <param name="tokens">Tokenizer over a stream of text in Well-known Text
        ///   format. The next tokens must form a MultiPolygon Text.
        /// </param>
        /// <param name="factory"> </param>
        /// <returns>
        /// A <c>MultiPolygon</c> specified by the next
        /// token in the stream, or if if the coordinates used to create the
        /// <c>Polygon</c> shells and holes do not form closed linestrings.</returns>
        private IMultiPolygon ReadMultiPolygonText(IEnumerator<Token> tokens, IGeometryFactory factory)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals("EMPTY"))
                return factory.CreateMultiPolygon();

            var polygons = new List<IPolygon>();
            var polygon = ReadPolygonText(tokens, factory);
            polygons.Add(polygon);
            nextToken = GetNextCloserOrComma(tokens);
            while (nextToken.Equals(","))
            {
                polygon = ReadPolygonText(tokens, factory);
                polygons.Add(polygon);
                nextToken = GetNextCloserOrComma(tokens);
            }
            return factory.CreateMultiPolygon(polygons.ToArray());
        }

        /// <summary>
        /// Creates a <c>GeometryCollection</c> using the next token in the
        /// stream.
        /// </summary>
        /// <param name="tokens">
        ///   Tokenizer over a stream of text in Well-known Text
        ///   format. The next tokens must form a &lt;GeometryCollection Text.
        /// </param>
        /// <param name="factory"> </param>
        /// <returns>
        /// A <c>GeometryCollection</c> specified by the
        /// next token in the stream.</returns>
        private IGeometryCollection ReadGeometryCollectionText(IEnumerator<Token> tokens, IGeometryFactory factory)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals("EMPTY"))
                return factory.CreateGeometryCollection();

            var geometries = new List<IGeometry>();
            var geometry = ReadGeometryTaggedText(tokens);
            geometries.Add(geometry);
            nextToken = GetNextCloserOrComma(tokens);
            while (nextToken.Equals(","))
            {
                geometry = ReadGeometryTaggedText(tokens);
                geometries.Add(geometry);
                nextToken = GetNextCloserOrComma(tokens);
            }
            return factory.CreateGeometryCollection(geometries.ToArray());
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
