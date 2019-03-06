using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GeoAPI.Geometries;
using GeoAPI.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.Utilities;
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
        private static readonly ICoordinateSequenceFactory CoordinateSequenceFactoryXYZM = CoordinateArraySequenceFactory.Instance;

        private bool _isAllowOldNtsCoordinateSyntax = true;
        private bool _isAllowOldNtsMultipointSyntax = true;

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
        /// Gets or sets a value indicating whether or not coordinates may have 3 ordinate values
        /// even though no Z or M ordinate indicator is present.  The default value is
        /// <see langword="true"/>.
        /// </summary>
        public bool IsOldNtsCoordinateSyntaxAllowed
        {
            get => _isAllowOldNtsCoordinateSyntax;
            set => _isAllowOldNtsCoordinateSyntax = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not point coordinates in a MultiPoint
        /// geometry must not be enclosed in paren.  The default value is <see langword="true"/>.
        /// </summary>
        public bool IsOldNtsMultiPointSyntaxAllowed
        {
            get => _isAllowOldNtsMultipointSyntax;
            set => _isAllowOldNtsMultipointSyntax = value;
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
                var enumerator = Tokenizer(reader);
                return ReadGeometryTaggedText(enumerator);
            }
            catch (IOException e)
            {
                throw new GeoAPI.IO.ParseException(e.ToString());
            }
        }

        internal TokenStream Tokenizer(TextReader reader)
        {
            var tokenizer = new StreamTokenizer(reader);

            // set tokenizer to NOT parse numbers
            tokenizer.Settings.ResetCharTypeTable();
            tokenizer.Settings.WordChars('a', 'z');
            tokenizer.Settings.WordChars('A', 'Z');
            ////tokenizer.Settings.WordChars(128 + 32, 255);
            tokenizer.Settings.WordChars('0', '9');
            tokenizer.Settings.WordChars('-', '-');
            tokenizer.Settings.WordChars('+', '+');
            tokenizer.Settings.WordChars('.', '.');
            tokenizer.Settings.WhitespaceChars(0, ' ');
            tokenizer.Settings.CommentChar('#');
            return new TokenStream(tokenizer.GetEnumerator());
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
        /// Reads a <c>Coordinate</c> from a stream using the given <see cref="StreamTokenizer"/>.
        /// <para>
        /// All ordinate values are read, but -depending on the <see cref="ICoordinateSequenceFactory"/>
        /// of the underlying <see cref="IGeometryFactory"/>- not necessarily all can be handled.
        /// Those are silently dropped.
        /// </para>
        /// </summary>
        /// <param name="tokens">the tokenizer to use.</param>
        /// <param name="ordinateFlags">a bit-mask defining the ordinates to read.</param>
        /// <param name="tryParen">a value indicating if a starting <code>"("</code> should be probed.</param>
        /// <returns>a <see cref="ICoordinateSequence"/> of length 1 containing the read ordinate values.</returns>
        /// <exception cref="IOException">if an I/O error occurs.</exception>
        /// <exception cref="GeoAPI.IO.ParseException">if an unexpected token was encountered.</exception>
        private ICoordinateSequence GetCoordinate(TokenStream tokens, Ordinates ordinateFlags, bool tryParen)
        {
            bool opened = false;
            if (tryParen && IsOpenerNext(tokens))
            {
                tokens.NextToken(true);
                opened = true;
            }

            // create a sequence for one coordinate
            int offsetM = ordinateFlags.HasFlag(Ordinates.Z) ? 1 : 0;
            var sequence = _coordinateSequencefactory.Create(1, this.ToDimension(ordinateFlags), ordinateFlags.HasFlag(Ordinates.M) ? 1 : 0);
            sequence.SetOrdinate(0, Ordinate.X, _precisionModel.MakePrecise(GetNextNumber(tokens)));
            sequence.SetOrdinate(0, Ordinate.Y, _precisionModel.MakePrecise(GetNextNumber(tokens)));

            // additionally read other vertices
            if (ordinateFlags.HasFlag(Ordinates.Z))
            {
                sequence.SetOrdinate(0, Ordinate.Z, GetNextNumber(tokens));
            }

            if (ordinateFlags.HasFlag(Ordinates.M))
            {
                sequence.SetOrdinate(0, Ordinate.Z + offsetM, GetNextNumber(tokens));
            }

            if (ordinateFlags == Ordinates.XY && _isAllowOldNtsCoordinateSyntax && IsNumberNext(tokens))
            {
                sequence.SetOrdinate(0, Ordinate.Z, GetNextNumber(tokens));
            }

            // read close token if it was opened here
            if (opened)
            {
                GetNextCloser(tokens);
            }

            return sequence;
        }

        /// <summary>
        /// Reads a <c>Coordinate</c> from a stream using the given <see cref="StreamTokenizer"/>.
        /// <para>
        /// All ordinate values are read, but -depending on the <see cref="ICoordinateSequenceFactory"/>
        /// of the underlying <see cref="IGeometryFactory"/>- not necessarily all can be handled.
        /// Those are silently dropped.
        /// </para>
        /// </summary>
        /// <param name="tokens">the tokenizer to use.</param>
        /// <param name="ordinateFlags">a bit-mask defining the ordinates to read.</param>
        /// <returns>a <see cref="ICoordinateSequence"/> of length 1 containing the read ordinate values.</returns>
        /// <exception cref="IOException">if an I/O error occurs.</exception>
        /// <exception cref="GeoAPI.IO.ParseException">if an unexpected token was encountered.</exception>
        private ICoordinateSequence GetCoordinateSequence(TokenStream tokens, Ordinates ordinateFlags)
        {
            return this.GetCoordinateSequence(tokens, ordinateFlags, false);
        }

        /// <summary>
        /// Reads a <c>CoordinateSequence</c> from a stream using the given <see cref="StreamTokenizer"/>.
        /// <para>
        /// All ordinate values are read, but -depending on the <see cref="ICoordinateSequenceFactory"/>
        /// of the underlying <see cref="IGeometryFactory"/>- not necessarily all can be handled.
        /// Those are silently dropped.
        /// </para>
        /// </summary>
        /// <param name="tokens">the tokenizer to use.</param>
        /// <param name="ordinateFlags">a bit-mask defining the ordinates to read.</param>
        /// <param name="tryParen">a value indicating if a starting <code>"("</code> should be probed for each coordinate.</param>
        /// <returns>a <see cref="ICoordinateSequence"/> of length 1 containing the read ordinate values.</returns>
        /// <exception cref="IOException">if an I/O error occurs.</exception>
        /// <exception cref="GeoAPI.IO.ParseException">if an unexpected token was encountered.</exception>
        private ICoordinateSequence GetCoordinateSequence(TokenStream tokens, Ordinates ordinateFlags, bool tryParen)
        {
            if (GetNextEmptyOrOpener(tokens) == "EMPTY")
            {
                return _coordinateSequencefactory.Create(0, this.ToDimension(ordinateFlags));
            }

            var coordinates = new List<ICoordinateSequence>();
            do
            {
                coordinates.Add(this.GetCoordinate(tokens, ordinateFlags, tryParen));
            }
            while (GetNextCloserOrComma(tokens) == ",");

            return this.MergeSequences(coordinates, ordinateFlags);
        }

        /// <summary>
        /// Computes the required dimension based on the given ordinate bit-mask.
        /// It is assumed that <see cref="Ordinates.XY"/> is set.
        /// </summary>
        /// <param name="ordinateFlags">the ordinate bit-mask.</param>
        /// <returns>the number of dimensions required to store ordinates for the given bit-mask.</returns>
        private int ToDimension(Ordinates ordinateFlags)
        {
            int dimension = 2;
            if (ordinateFlags.HasFlag(Ordinates.Z))
            {
                dimension++;
            }

            if (ordinateFlags.HasFlag(Ordinates.M))
            {
                dimension++;
            }

            if (dimension == 2 && _isAllowOldNtsCoordinateSyntax)
            {
                dimension++;
            }

            return dimension;
        }

        /// <summary>
        /// Merges an array of one-coordinate-<see cref="ICoordinateSequence"/>s into one
        /// <see cref="ICoordinateSequence"/>.
        /// </summary>
        /// <param name="sequences">an array of coordinate sequences. Each sequence contains <b>exactly one</b> coordinate.</param>
        /// <param name="ordinateFlags">a bit-mask of required ordinates.</param>
        /// <returns>a coordinate sequence containing all coordinate.</returns>
        private ICoordinateSequence MergeSequences(List<ICoordinateSequence> sequences, Ordinates ordinateFlags)
        {
            // if the sequences array is empty or null create an empty sequence
            if (sequences == null || sequences.Count == 0)
            {
                return _coordinateSequencefactory.Create(0, this.ToDimension(ordinateFlags));
            }

            if (sequences.Count == 1)
            {
                return sequences[0];
            }

            Ordinates mergeOrdinates;
            if (_isAllowOldNtsCoordinateSyntax && ordinateFlags == Ordinates.XY)
            {
                mergeOrdinates = ordinateFlags;
                foreach (var seq in sequences)
                {
                    if (seq.HasZ)
                    {
                        mergeOrdinates |= Ordinates.Z;
                        break;
                    }
                }
            }
            else
            {
                mergeOrdinates = ordinateFlags;
            }

            // create and fill the result sequence
            var sequence = _coordinateSequencefactory.Create(sequences.Count, this.ToDimension(mergeOrdinates), mergeOrdinates.HasFlag(Ordinates.M) ? 1 : 0);

            var offsetM = Ordinate.Z + (mergeOrdinates.HasFlag(Ordinates.Z) ? 1 : 0);
            for (int i = 0; i < sequences.Count; i++)
            {
                var item = sequences[i];
                sequence.SetOrdinate(i, Ordinate.X, item.GetOrdinate(0, Ordinate.X));
                sequence.SetOrdinate(i, Ordinate.Y, item.GetOrdinate(0, Ordinate.Y));
                if (mergeOrdinates.HasFlag(Ordinates.Z))
                {
                    sequence.SetOrdinate(i, Ordinate.Z, item.GetOrdinate(0, Ordinate.Z));
                }

                if (mergeOrdinates.HasFlag(Ordinates.M))
                {
                    sequence.SetOrdinate(i, offsetM, item.GetOrdinate(0, offsetM));
                }
            }

            // return it
            return sequence;
        }

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
        [Obsolete("in favor of functions returning ICoordinateSequences.")]
        private Coordinate[] GetCoordinates(TokenStream tokens, bool skipExtraParenthesis, ref bool hasZ)
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
        /// Returns the next precise <c>Coordinate</c> in the stream.
        /// </summary>
        /// <param name="tokens">
        /// tokenizer over a stream of text in Well-known Text format.
        /// The next element returned by the stream should be a number.
        /// </param>
        /// <returns>the next array of <c>Coordinate</c>s in the stream.</returns>
        /// <exception cref="IOException">if an I/O error occurs.</exception>
        /// <exception cref="GeoAPI.IO.ParseException">if an unexpected token was encountered</exception>
        [Obsolete("in favor of functions returning ICoordinateSequences.")]
        private Coordinate GetPreciseCoordinate(TokenStream tokens, bool skipExtraParenthesis, ref bool hasZ)
        {
            var coord = new Coordinate();
            bool extraParenthesisFound = false;
            if (skipExtraParenthesis)
            {
                extraParenthesisFound = IsStringValueNext(tokens, "(");
                if (extraParenthesisFound)
                {
                    tokens.NextToken(true);
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
                tokens.NextToken(true);
                //_index++;
            }

            _precisionModel.MakePrecise(coord);
            return coord;
        }

        private static bool IsStringValueNext(TokenStream tokens, string stringValue)
        {
            var token = tokens.NextToken(false) /*as Token*/;
            if (token == null)
                throw new InvalidOperationException("current Token is null");
            return token.StringValue == stringValue;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        private static bool IsNumberNext(TokenStream tokens)
        {
            return tokens.NextToken(false) is WordToken;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        private static bool IsOpenerNext(TokenStream tokens)
        {
            return tokens.NextToken(false) is CharToken charToken &&
                   charToken.Object is char c &&
                   c == '(';
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
        private static double GetNextNumber(TokenStream tokens)
        {
            var token = tokens.NextToken(true);
            switch (token)
            {
                case WordToken wordToken:
                    if (wordToken.StringValue.Equals(NaNString, StringComparison.OrdinalIgnoreCase))
                    {
                        return double.NaN;
                    }

                    if (double.TryParse(wordToken.StringValue, NumberStyles.Float | NumberStyles.AllowThousands, InvariantCulture, out double val))
                    {
                        return val;
                    }

                    throw new GeoAPI.IO.ParseException($"Invalid number: {wordToken.StringValue}");

                default:
                    throw new GeoAPI.IO.ParseException($"Expected number but found {token?.ToDebugString() ?? "the end of input"}");
            }
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
        private static string GetNextEmptyOrOpener(TokenStream tokens)
        {
            string nextWord = GetNextWord(tokens);
            if (nextWord.Equals("Z", StringComparison.OrdinalIgnoreCase))
            {
                //z = true;
                nextWord = GetNextWord(tokens);
            }
            else if (nextWord.Equals("M", StringComparison.OrdinalIgnoreCase))
            {
                //m = true;
                nextWord = GetNextWord(tokens);
            }
            else if (nextWord.Equals("ZM", StringComparison.OrdinalIgnoreCase))
            {
                //z = true;
                //m = true;
                nextWord = GetNextWord(tokens);
            }

            if (nextWord.Equals("EMPTY") || nextWord.Equals("("))
                return nextWord;
            throw new GeoAPI.IO.ParseException("Expected 'EMPTY' or '(' but encountered '" + nextWord + "'");
        }

        /// <summary>
        /// Returns the next ordinate flag information in the stream as uppercase text.
        /// This can be Z, M or ZM.
        /// </summary>
        /// <param name="tokens">tokenizer over a stream of text in Well-known Text</param>
        /// <returns>the next EMPTY or L_PAREN in the stream as uppercase text.</returns>
        /// <exception cref="IOException">if an I/O error occurs</exception>
        /// <exception cref="GeoAPI.IO.ParseException">if the next token is not EMPTY or L_PAREN</exception>
        private static Ordinates GetNextOrdinateFlags(TokenStream tokens)
        {
            string nextWord = LookAheadWord(tokens);
            if (nextWord.Equals("Z", StringComparison.OrdinalIgnoreCase))
            {
                tokens.NextToken(true);
                return Ordinates.XYZ;
            }
            else if (nextWord.Equals("M", StringComparison.OrdinalIgnoreCase))
            {
                tokens.NextToken(true);
                return Ordinates.XYZ;
            }
            else if (nextWord.Equals("ZM", StringComparison.OrdinalIgnoreCase))
            {
                tokens.NextToken(true);
                return Ordinates.XYZM;
            }
            return Ordinates.XY;
        }

        /// <summary>
        /// Returns the next word in the stream.
        /// </summary>
        /// <param name="tokens">tokenizer over a stream of text in Well-known Text format. The next token must be a word.</param>
        /// <returns>the next word in the stream as uppercase text</returns>
        /// <exception cref="GeoAPI.IO.ParseException">if the next token is not a word</exception>
        /// <exception cref="IOException">if an I/O error occurs</exception>
        private static string LookAheadWord(TokenStream tokens)
        {
            string nextWord = GetNextWord(tokens, false);
            return nextWord;
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
        private static string GetNextCloserOrComma(TokenStream tokens)
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
        private static string GetNextCloser(TokenStream tokens)
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
        /// <param name="advance">
        /// <see langword="true"/> to advance the stream, <see langword="false"/> to just peek.
        /// </param>
        /// <returns>The next word in the stream as uppercase text.</returns>
        private static string GetNextWord(TokenStream tokens, bool advance = true)
        {
            var token = tokens.NextToken(advance) /*as Token*/;
            switch (token)
            {
                case WordToken wordToken:
                    if (wordToken.StringValue.Equals("EMPTY", StringComparison.OrdinalIgnoreCase))
                    {
                        return "EMPTY";
                    }

                    return wordToken.StringValue;

                case CharToken charToken when charToken.Object is char c && (c == '(' || c == ')' || c == ','):
                    return charToken.StringValue;

                default:
                    throw new GeoAPI.IO.ParseException($"Expected a word but encountered {token?.ToDebugString() ?? "the end of input"}");
            }
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
        internal IGeometry ReadGeometryTaggedText(TokenStream tokens)
        {
            /*
             * A new different implementation by Marc Jacquin:
             * this code manages also SRID values.
             */
            IGeometry returned;

            int srid;
            string type = GetNextWord(tokens);
            if (type.Equals("SRID", StringComparison.OrdinalIgnoreCase))
            {
                srid = Convert.ToInt32(GetNextNumber(tokens));
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

            var ordinateFlags = Ordinates.XY;
            try
            {
                if (type.EndsWith("ZM", StringComparison.OrdinalIgnoreCase))
                {
                    ordinateFlags = Ordinates.XYZM;
                }
                else if (type.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
                {
                    ordinateFlags = Ordinates.XYZ;
                }
                else if (type.EndsWith("M", StringComparison.OrdinalIgnoreCase))
                {
                    ordinateFlags = Ordinates.XYM;
                }
            }
            catch (IOException)
            {
                return null;
            }
            catch (GeoAPI.IO.ParseException)
            {
                return null;
            }

            if (ordinateFlags == Ordinates.XY)
            {
                ordinateFlags = GetNextOrdinateFlags(tokens);
            }

            var csFactory = (_coordinateSequencefactory.Ordinates & ordinateFlags) == ordinateFlags
                ? _coordinateSequencefactory
                : CoordinateSequenceFactoryXYZM;

            // DEVIATION: JTS largely uses the same geometry factory that we were given originally,
            // only swapping out the coordinate sequence factory, BUT then it overwrites the SRID on
            // the created geometry after-the-fact.  Our version of the Geometry.SRID setter does it
            // differently than JTS's, so this is actually how we have to do it to match the output
            // from JTS (which could hypothetically return a collection whose inner elements have
            // different SRIDs than the collection itself if that's how it's specified).
            var factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(_precisionModel, srid,
                csFactory);

            if (type.StartsWith("POINT", StringComparison.OrdinalIgnoreCase))
                returned = ReadPointText(tokens, factory, ordinateFlags);
            else if (type.StartsWith("LINESTRING", StringComparison.OrdinalIgnoreCase))
                returned = ReadLineStringText(tokens, factory, ordinateFlags);
            else if (type.StartsWith("LINEARRING", StringComparison.OrdinalIgnoreCase))
                returned = ReadLinearRingText(tokens, factory, ordinateFlags);
            else if (type.StartsWith("POLYGON", StringComparison.OrdinalIgnoreCase))
                returned = ReadPolygonText(tokens, factory, ordinateFlags);
            else if (type.StartsWith("MULTIPOINT", StringComparison.OrdinalIgnoreCase))
                returned = ReadMultiPointText(tokens, factory, ordinateFlags);
            else if (type.StartsWith("MULTILINESTRING", StringComparison.OrdinalIgnoreCase))
                returned = ReadMultiLineStringText(tokens, factory, ordinateFlags);
            else if (type.StartsWith("MULTIPOLYGON", StringComparison.OrdinalIgnoreCase))
                returned = ReadMultiPolygonText(tokens, factory, ordinateFlags);
            else if (type.StartsWith("GEOMETRYCOLLECTION", StringComparison.OrdinalIgnoreCase))
                returned = ReadGeometryCollectionText(tokens, factory, ordinateFlags);
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
        private IPoint ReadPointText(TokenStream tokens, IGeometryFactory factory, Ordinates ordinateFlags)
        {
            var point = factory.CreatePoint(GetCoordinateSequence(tokens, ordinateFlags));
            return point;
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
        private ILineString ReadLineStringText(TokenStream tokens, IGeometryFactory factory, Ordinates ordinateFlags)
        {
            return factory.CreateLineString(GetCoordinateSequence(tokens, ordinateFlags));
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
        private ILinearRing ReadLinearRingText(TokenStream tokens, IGeometryFactory factory, Ordinates ordinateFlags)
        {
            return factory.CreateLinearRing(GetCoordinateSequence(tokens, ordinateFlags));
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
        private IMultiPoint ReadMultiPointText(TokenStream tokens, IGeometryFactory factory, Ordinates ordinateFlags)
        {
            return factory.CreateMultiPoint(
                GetCoordinateSequence(tokens, ordinateFlags, _isAllowOldNtsMultipointSyntax));
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
        private IPolygon ReadPolygonText(TokenStream tokens, IGeometryFactory factory, Ordinates ordinateFlags)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals("EMPTY"))
                return factory.CreatePolygon();

            var holes = new List<ILinearRing>();
            var shell = ReadLinearRingText(tokens, factory, ordinateFlags);
            nextToken = GetNextCloserOrComma(tokens);
            while (nextToken.Equals(","))
            {
                var hole = ReadLinearRingText(tokens, factory, ordinateFlags);
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
        private IMultiLineString ReadMultiLineStringText(TokenStream tokens, IGeometryFactory factory, Ordinates ordinateFlags)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals("EMPTY"))
                return factory.CreateMultiLineString();

            var lineStrings = new List<ILineString>();
            do
            {
                var lineString = ReadLineStringText(tokens, factory, ordinateFlags);
                lineStrings.Add(lineString);
                nextToken = GetNextCloserOrComma(tokens);
            }
            while (nextToken.Equals(","));

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
        private IMultiPolygon ReadMultiPolygonText(TokenStream tokens, IGeometryFactory factory, Ordinates ordinateFlags)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals("EMPTY"))
                return factory.CreateMultiPolygon();

            var polygons = new List<IPolygon>();
            do
            {
                var polygon = ReadPolygonText(tokens, factory, ordinateFlags);
                polygons.Add(polygon);
                nextToken = GetNextCloserOrComma(tokens);
            }
            while (nextToken.Equals(","));

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
        private IGeometryCollection ReadGeometryCollectionText(TokenStream tokens, IGeometryFactory factory, Ordinates ordinateFlags)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals("EMPTY"))
                return factory.CreateGeometryCollection();

            var geometries = new List<IGeometry>();
            do
            {
                var geometry = ReadGeometryTaggedText(tokens);
                geometries.Add(geometry);
                nextToken = GetNextCloserOrComma(tokens);
            }
            while (nextToken.Equals(","));

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
