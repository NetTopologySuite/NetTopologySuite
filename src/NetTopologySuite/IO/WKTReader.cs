using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

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
    /// <para/>
    /// As of version 2.0, NTS can read WKT syntax
    /// which specifies coordinate dimension Z, M or ZM as modifiers(e.g.POINT Z)
    /// or in the name of the geometry type(e.g.LINESTRINGZM).
    /// If the coordinate dimension is specified it will be set in the created geometry.
    /// If the coordinate dimension is not specified, the default behaviour is to
    /// create XYZ geometry(this is backwards compatible with older JTS versions).
    /// This can be altered to create XY geometry by
    /// setting <see cref="IsOldNtsCoordinateSyntaxAllowed"/> to <c>false</c>.
    /// <para/>
    /// A reader can be set to ensure the input is structurally valid
    /// by setting <see cref="FixStructure"/> to <c>true</c>.
    /// This ensures that geometry can be constructed without errors due to missing coordinates.
    /// The created geometry may still be topologically invalid.
    /// <para/>
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
    /// <see cref="WKTReader" /> reads also non-standard <see cref="WKTConstants.LINEARRING"/> tags.
    /// </remarks>
    /// </summary>
    public class WKTReader
    {
        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        private static readonly CoordinateSequenceFactory CoordinateSequenceFactoryXYZM = CoordinateArraySequenceFactory.Instance;

        private static readonly StreamTokenizerSettings TokenizerSettings = CreateDefaultStreamTokenizerSettings();

        private NtsGeometryServices _ntsGeometryServices;

        private int? _overriddenDefaultSRID;

        private bool _isAllowOldNtsCoordinateSyntax = true;
        private bool _isAllowOldNtsMultipointSyntax = true;

        /*
         * true if structurally invalid input should be reported rather than repaired.
         */
        private bool _isStrict = true;

        private static StreamTokenizerSettings CreateDefaultStreamTokenizerSettings()
        {
            var tokenizerSettings = new StreamTokenizerSettings();
            tokenizerSettings.SetDefaults();
            // set tokenizer to NOT parse numbers
            tokenizerSettings.ResetCharTypeTable();
            tokenizerSettings.WordChars('a', 'z');
            tokenizerSettings.WordChars('A', 'Z');
            ////tokenizer.Settings.WordChars(128 + 32, 255);
            tokenizerSettings.WordChars('0', '9');
            tokenizerSettings.WordChars('-', '-');
            tokenizerSettings.WordChars('+', '+');
            tokenizerSettings.WordChars('.', '.');
            tokenizerSettings.WhitespaceChars(0, ' ');
            tokenizerSettings.CommentChar('#');
            return tokenizerSettings;
        }

        /// <summary>
        /// Creates a <c>WKTReader</c> that creates objects using a basic GeometryFactory.
        /// </summary>
        public WKTReader() : this(NtsGeometryServices.Instance) { }

        /// <summary>
        /// Creates a <c>WKTReader</c> that creates objects using a basic GeometryFactory.
        /// </summary>
        public WKTReader(NtsGeometryServices ntsGeometryServices)
        {
            _ntsGeometryServices = ntsGeometryServices ?? throw new ArgumentNullException(nameof(ntsGeometryServices));
        }

        /// <summary>
        /// Creates a <c>WKTReader</c> that creates objects using the given
        /// <c>GeometryFactory</c>.
        /// </summary>
        /// <param name="factory">The factory used to create <c>Geometry</c>s.</param>
        [Obsolete("Use a constructor with a configured NtsGeometryServices instance.")]
        public WKTReader(GeometryFactory factory)
            : this(factory.GeometryServices)
        {
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
        /// Gets or sets a flag indicating that the structure of input geometry should be fixed
        /// so that the geometry can be constructed without error.<br/>
        /// This involves adding coordinates if the input coordinate sequence is shorter than required.
        /// </summary>
        /// <seealso cref="LineString.MinimumValidSize"/>
        public bool FixStructure { get; set; }

        /// <summary>
        /// Gets or sets the factory to create geometries
        /// </summary>
        [Obsolete("Supply an appropriate NtsGeometryServices instance to the constructor instead.  The ability to set this value after an instance is created may be removed in a future release.")]
        public GeometryFactory Factory
        {
            get => _ntsGeometryServices.CreateGeometryFactory(DefaultSRID);
            set
            {
                if (value != null)
                {
                    _ntsGeometryServices = value.GeometryServices;
                    // Not sure about this:
                    // DefaultSRID = value.SRID;
                }
            }
        }

        /// <summary>
        /// Gets or sets the default SRID
        /// </summary>
        [Obsolete("Supply an appropriate NtsGeometryServices instance to the constructor instead.  The ability to set this value after an instance is created may be removed in a future release.")]
        public int DefaultSRID
        {
            // set the CompilerGenerated attribute so that ApiCompat stops complaining.
            [CompilerGenerated] get => _overriddenDefaultSRID ?? _ntsGeometryServices.DefaultSRID;
            [CompilerGenerated] set => _overriddenDefaultSRID = value;
        }

        /// <summary>
        /// Gets or sets a value indicating if the reader should attempt to repair malformed input.
        /// </summary>
        /// <remarks>
        /// <i>Malformed</i> in this case means the ring has too few points (4),
        /// or is not closed.
        /// </remarks>
        public bool IsStrict
        {
            get => _isStrict;
            set => _isStrict = value;
        }

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
        public Geometry Read(string wellKnownText)
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
        public Geometry Read(Stream stream)
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
        public Geometry Read(TextReader reader)
        {
            try
            {
                var enumerator = Tokenizer(reader);
                return ReadGeometryTaggedText(enumerator);
            }
            catch (IOException e)
            {
                throw new ParseException(e.ToString());
            }
        }

        internal TokenStream Tokenizer(TextReader reader)
        {
            var tokenizer = new StreamTokenizer(reader, TokenizerSettings);
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
        /// All ordinate values are read, but -depending on the <see cref="CoordinateSequenceFactory"/>
        /// of the underlying <see cref="GeometryFactory"/>- not necessarily all can be handled.
        /// Those are silently dropped.
        /// </para>
        /// </summary>
        /// <param name="factory">A geometry factory</param>
        /// <param name="tokens">the tokenizer to use.</param>
        /// <param name="ordinateFlags">a bit-mask defining the ordinates to read.</param>
        /// <param name="tryParen">a value indicating if a starting "<c>(</c>" should be probed.</param>
        /// <returns>a <see cref="Coordinate"/> of appropriate dimension containing the read ordinate values.</returns>
        /// <exception cref="IOException">if an I/O error occurs.</exception>
        /// <exception cref="ParseException">if an unexpected token was encountered.</exception>
        private Coordinate GetCoordinate(GeometryFactory factory, TokenStream tokens, Ordinates ordinateFlags, bool tryParen)
        {
            bool opened = false;
            if (tryParen && IsOpenerNext(tokens))
            {
                tokens.NextToken(true);
                opened = true;
            }

            // Read x- and y- ordinates
            double x = factory.PrecisionModel.MakePrecise(GetNextNumber(tokens));
            double y = factory.PrecisionModel.MakePrecise(GetNextNumber(tokens));

            // Coordinate type
            var readOrdinates = Ordinates.XY;

            // additionally read other vertices
            double z = Coordinate.NullOrdinate, m = 0d;
            if (ordinateFlags.HasFlag(Ordinates.Z))
            {
                z = GetNextNumber(tokens);
                readOrdinates |= Ordinates.Z;
            }

            if (ordinateFlags.HasFlag(Ordinates.M))
            {
                m = GetNextNumber(tokens);
                readOrdinates |= Ordinates.M;
            }

            if (ordinateFlags == Ordinates.XY && _isAllowOldNtsCoordinateSyntax && IsNumberNext(tokens))
            {
                z = GetNextNumber(tokens);
                readOrdinates |= Ordinates.Z;
            }

            Coordinate res;
            switch(readOrdinates)
            {
                case Ordinates.XY:
                    res = new Coordinate(x, y); break;
                case Ordinates.XYZ:
                    res = new CoordinateZ(x, y, z); ; break;
                case Ordinates.XYM:
                    res = new CoordinateM(x, y, m); break;
                case Ordinates.XYZM:
                    res = new CoordinateZM(x, y, z, m); break;
                default:
                    throw new InvalidOperationException($"Unknown ordinate read: {readOrdinates}");
            }

            // read close token if it was opened here
            if (opened)
            {
                GetNextCloser(tokens);
            }

            return res;
        }

        /// <summary>
        /// Reads a <c>Coordinate</c> from a stream using the given <see cref="StreamTokenizer"/>.
        /// <para>
        /// All ordinate values are read, but -depending on the <see cref="CoordinateSequenceFactory"/>
        /// of the underlying <see cref="GeometryFactory"/>- not necessarily all can be handled.
        /// Those are silently dropped.
        /// </para>
        /// </summary>
        /// <param name="factory">A geometry factory</param>
        /// <param name="tokens">the tokenizer to use.</param>
        /// <param name="ordinateFlags">a bit-mask defining the ordinates to read.</param>
        /// <param name="minSize">The minumum number of coordinates that have to be in the sequence</param>
        /// <param name="isRing">A flag indicating if the sequence should form a ring</param>
        /// <returns>a <see cref="CoordinateSequence"/> of length 1 containing the read ordinate values.</returns>
        /// <exception cref="IOException">if an I/O error occurs.</exception>
        /// <exception cref="ParseException">if an unexpected token was encountered.</exception>
        private CoordinateSequence GetCoordinateSequence(GeometryFactory factory, TokenStream tokens, Ordinates ordinateFlags, int minSize, bool isRing)
        {
            if (GetNextEmptyOrOpener(tokens).Equals(WKTConstants.EMPTY))
                return CreateCoordinateSequenceEmpty(factory.CoordinateSequenceFactory, ordinateFlags);

            var coordinates = new List<Coordinate>();
            do
            {
                coordinates.Add(GetCoordinate(factory, tokens, ordinateFlags, false));
            } while (GetNextCloserOrComma(tokens).Equals(","));

            if (FixStructure)
            {
                DoFixStructure(coordinates, minSize, isRing);
            }
            var coordArray = coordinates.ToArray();
            return factory.CoordinateSequenceFactory.Create(coordArray);
        }

        // This function is called FixStructure in JTS
        private static void DoFixStructure(List<Coordinate> coords, int minSize, bool isRing)
        {
            if (coords.Count == 0)
                return;
            if (isRing && !IsClosed(coords))
            {
                coords.Add(coords[0].Copy());
            }
            while (coords.Count < minSize)
            {
                coords.Add(coords[coords.Count - 1].Copy());
            }
        }

        private static bool IsClosed(List<Coordinate> coords)
        {
            if (coords.Count == 0) return true;
            if (coords.Count == 1
                || !coords[0].Equals2D(coords[coords.Count - 1]))
            {
                return false;
            }
            return true;
        }

        private CoordinateSequence CreateCoordinateSequenceEmpty(CoordinateSequenceFactory csFactory, Ordinates ordinateFlags)
        { 
            return csFactory.Create(0, ToDimension(ordinateFlags), ordinateFlags.HasFlag(Ordinates.M)? 1 : 0);
        }

        /// <summary>
        /// Reads a <c>CoordinateSequence</c> from a stream using the given <see cref="StreamTokenizer"/>
        /// for an old-style JTS MultiPoint (Point coordinates not enclosed in parentheses).
        /// <para>
        /// All ordinate values are read, but -depending on the <see cref="CoordinateSequenceFactory"/>
        /// of the underlying <see cref="GeometryFactory"/>- not necessarily all can be handled.
        /// Those are silently dropped.
        /// </para>
        /// </summary>
        /// <param name="factory">A geometry factory</param>
        /// <param name="tokens">the tokenizer to use.</param>
        /// <param name="ordinateFlags">a bit-mask defining the ordinates to read.</param>
        /// <returns>a <see cref="CoordinateSequence"/> of length 1 containing the read ordinate values.</returns>
        /// <exception cref="IOException">if an I/O error occurs.</exception>
        /// <exception cref="ParseException">if an unexpected token was encountered.</exception>
        private CoordinateSequence GetCoordinateSequenceOldMultiPoint(GeometryFactory factory, TokenStream tokens, Ordinates ordinateFlags)
        {
            var coordinates = new List<Coordinate>();
            do
            {
                coordinates.Add(GetCoordinate(factory, tokens, ordinateFlags, true));
            }
            while (GetNextCloserOrComma(tokens) == ",");

            var coordArray = coordinates.ToArray();
            return factory.CoordinateSequenceFactory.Create(coordArray);
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
        /// <exception cref="ParseException">if the next token is not a valid number</exception>
        private static double GetNextNumber(TokenStream tokens)
        {
            var token = tokens.NextToken(true);
            switch (token)
            {
                case WordToken wordToken:
                    if (wordToken.StringValue.Equals(OrdinateFormat.REP_NAN, StringComparison.OrdinalIgnoreCase))
                    {
                        return double.NaN;
                    }
                    if (wordToken.StringValue.Equals(OrdinateFormat.REP_NEG_INF, StringComparison.OrdinalIgnoreCase))
                    {
                        return double.NegativeInfinity;
                    }
                    if (wordToken.StringValue.Equals(OrdinateFormat.REP_POS_INF, StringComparison.OrdinalIgnoreCase))
                    {
                        return double.PositiveInfinity;
                    }
                    if (double.TryParse(wordToken.StringValue, NumberStyles.Float | NumberStyles.AllowThousands, InvariantCulture, out double val))
                    {
                        return val;
                    }

                    throw new ParseException($"Invalid number: {wordToken.StringValue}");

                default:
                    throw new ParseException($"Expected number but found {token?.ToDebugString() ?? "the end of input"}");
            }
        }

        /// <summary>
        /// Returns the next WKTConstants.EMPTY or "(" in the stream as uppercase text.
        /// </summary>
        /// <param name="tokens">
        /// Tokenizer over a stream of text in Well-known Text
        /// format. The next token must be <see cref="WKTConstants.EMPTY"/> or "(".
        /// </param>
        /// <returns>
        /// The next WKTConstants.EMPTY or "(" in the stream as uppercase text.</returns>
        private static string GetNextEmptyOrOpener(TokenStream tokens)
        {
            string nextWord = GetNextWord(tokens);
            if (nextWord.Equals(WKTConstants.Z, StringComparison.OrdinalIgnoreCase))
            {
                //z = true;
                nextWord = GetNextWord(tokens);
            }
            else if (nextWord.Equals(WKTConstants.M, StringComparison.OrdinalIgnoreCase))
            {
                //m = true;
                nextWord = GetNextWord(tokens);
            }
            else if (nextWord.Equals(WKTConstants.ZM, StringComparison.OrdinalIgnoreCase))
            {
                //z = true;
                //m = true;
                nextWord = GetNextWord(tokens);
            }

            if (nextWord.Equals(WKTConstants.EMPTY) || nextWord.Equals("("))
                return nextWord;
            throw new ParseException($"Expected '{WKTConstants.EMPTY}' or '(' but encountered '" + nextWord + "'");
        }

        /// <summary>
        /// Returns the next ordinate flag information in the stream as uppercase text.
        /// This can be Z, M or ZM.
        /// </summary>
        /// <param name="tokens">tokenizer over a stream of text in Well-known Text</param>
        /// <returns>the next ordinate flags.</returns>
        /// <exception cref="IOException">if an I/O error occurs</exception>
        /// <exception cref="ParseException">if the next token is not EMPTY or L_PAREN</exception>
        private static Ordinates GetNextOrdinateFlags(TokenStream tokens)
        {
            string nextWord = LookAheadWord(tokens);
            if (nextWord.Equals(WKTConstants.Z, StringComparison.OrdinalIgnoreCase))
            {
                tokens.NextToken(true);
                return Ordinates.XYZ;
            }
            else if (nextWord.Equals(WKTConstants.M, StringComparison.OrdinalIgnoreCase))
            {
                tokens.NextToken(true);
                return Ordinates.XYM;
            }
            else if (nextWord.Equals(WKTConstants.ZM, StringComparison.OrdinalIgnoreCase))
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
        /// <exception cref="ParseException">if the next token is not a word</exception>
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

            throw new ParseException("Expected ')' or ',' but encountered '" + nextWord
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
            throw new ParseException("Expected ')' but encountered '" + nextWord + "'");
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
                    if (wordToken.StringValue.Equals(WKTConstants.EMPTY, StringComparison.OrdinalIgnoreCase))
                    {
                        return WKTConstants.EMPTY;
                    }

                    return wordToken.StringValue;

                case CharToken charToken when charToken.Object is char c && (c == '(' || c == ')' || c == ','):
                    return charToken.StringValue;

                default:
                    throw new ParseException($"Expected a word but encountered {token?.ToDebugString() ?? "the end of input"}");
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
        internal Geometry ReadGeometryTaggedText(TokenStream tokens)
        {
            /*
             * A new different implementation by Marc Jacquin:
             * this code manages also SRID values.
             */
            Geometry returned;

            int srid;
            string type = GetNextWord(tokens);
            if (type.Equals("SRID", StringComparison.OrdinalIgnoreCase))
            {
                var tok = tokens.NextToken(true);
                if (!(tok is CharToken eq && ((char)eq.Object) == '='))
                {
                    return null;
                }

                srid = Convert.ToInt32(GetNextNumber(tokens));
                tok = tokens.NextToken(true);
                if (!(tok is CharToken semi && ((char)semi.Object) == ';'))
                {
                    return null;
                }

                type = GetNextWord(tokens);

                //sridValue = tokens[2].ToString();
                //// tokens.RemoveRange(0, 4);
                //tokens.RemoveAt(0);
                //tokens.RemoveAt(0);
                //tokens.RemoveAt(0);
                //tokens.RemoveAt(0);
            }
            else
                srid = _overriddenDefaultSRID ?? _ntsGeometryServices.DefaultSRID;

            var ordinateFlags = Ordinates.XY;
            try
            {
                if (type.EndsWith(WKTConstants.ZM, StringComparison.OrdinalIgnoreCase))
                {
                    ordinateFlags = Ordinates.XYZM;
                }
                else if (type.EndsWith(WKTConstants.Z, StringComparison.OrdinalIgnoreCase))
                {
                    ordinateFlags = Ordinates.XYZ;
                }
                else if (type.EndsWith(WKTConstants.M, StringComparison.OrdinalIgnoreCase))
                {
                    ordinateFlags = Ordinates.XYM;
                }
            }
            catch (IOException)
            {
                return null;
            }
            catch (ParseException)
            {
                return null;
            }

            if (ordinateFlags == Ordinates.XY)
            {
                ordinateFlags = GetNextOrdinateFlags(tokens);
            }

            var csFactory = (_ntsGeometryServices.DefaultCoordinateSequenceFactory.Ordinates & ordinateFlags) == ordinateFlags
                ? _ntsGeometryServices.DefaultCoordinateSequenceFactory
                : CoordinateSequenceFactoryXYZM;

            // DEVIATION: JTS largely uses the same geometry factory that we were given originally,
            // only swapping out the coordinate sequence factory, BUT then it overwrites the SRID on
            // the created geometry after-the-fact.  Our version of the Geometry.SRID setter does it
            // differently than JTS's, so this is actually how we have to do it to match the output
            // from JTS (which could hypothetically return a collection whose inner elements have
            // different SRIDs than the collection itself if that's how it's specified).
            var factory = _ntsGeometryServices.CreateGeometryFactory(_ntsGeometryServices.DefaultPrecisionModel, srid, csFactory);

            if (IsTypeName(tokens, type, WKTConstants.POINT))
                returned = ReadPointText(tokens, factory, ordinateFlags);
            else if (IsTypeName(tokens, type, WKTConstants.LINESTRING))
                returned = ReadLineStringText(tokens, factory, ordinateFlags);
            else if (IsTypeName(tokens, type, WKTConstants.LINEARRING))
                returned = ReadLinearRingText(tokens, factory, ordinateFlags);
            else if (IsTypeName(tokens, type, WKTConstants.POLYGON))
                returned = ReadPolygonText(tokens, factory, ordinateFlags);
            else if (IsTypeName(tokens, type, WKTConstants.MULTIPOINT))
                returned = ReadMultiPointText(tokens, factory, ordinateFlags);
            else if (IsTypeName(tokens, type, WKTConstants.MULTILINESTRING))
                returned = ReadMultiLineStringText(tokens, factory, ordinateFlags);
            else if (IsTypeName(tokens, type, WKTConstants.MULTIPOLYGON))
                returned = ReadMultiPolygonText(tokens, factory, ordinateFlags);
            else if (IsTypeName(tokens, type, WKTConstants.GEOMETRYCOLLECTION))
                returned = ReadGeometryCollectionText(tokens, factory, ordinateFlags);
            else throw new ParseException("Unknown type: " + type);

            if (returned == null)
                throw new NullReferenceException("Error reading geometry");

            return returned;
        }

        private static bool IsTypeName(TokenStream tokens, string type, string typeName)
        {
            if (!type.StartsWith(typeName, StringComparison.OrdinalIgnoreCase))
                return false;
    
            string modifiers = type.Substring(typeName.Length);
            bool isValidMod = modifiers.Length <= 2 &&
                 (modifiers.Length == 0
                 || modifiers.Equals(WKTConstants.Z)
                 || modifiers.Equals(WKTConstants.M)
                 || modifiers.Equals(WKTConstants.ZM));

            if (! isValidMod) {
                throw new ParseException($"Invalid dimension modifiers: '{type}'");
            }
    
            return true;
        }

/// <summary>
/// Creates a <c>Point</c> using the next token in the stream.
/// </summary>
/// <param name="tokens">
///   Tokenizer over a stream of text in Well-known Text
///   format. The next tokens must form a &lt;Point Text.
/// </param>
/// <param name="factory">The factory to create the geometry</param>
/// <param name="ordinateFlags">A flag indicating the ordinates to expect.</param>
/// <returns>A <c>Point</c> specified by the next token in
/// the stream.</returns>
private Point ReadPointText(TokenStream tokens, GeometryFactory factory, Ordinates ordinateFlags)
        {
            var point = factory.CreatePoint(GetCoordinateSequence(factory, tokens, ordinateFlags, 1, false));
            return point;
        }

        /// <summary>
        /// Creates a <c>LineString</c> using the next token in the stream.
        /// </summary>
        /// <param name="tokens">
        ///   Tokenizer over a stream of text in Well-known Text
        ///   format. The next tokens must form a &lt;LineString Text.
        /// </param>
        /// <param name="factory">The factory to create the geometry</param>
        /// <param name="ordinateFlags">A flag indicating the ordinates to expect.</param>
        /// <returns>
        /// A <c>LineString</c> specified by the next
        /// token in the stream.</returns>
        private LineString ReadLineStringText(TokenStream tokens, GeometryFactory factory, Ordinates ordinateFlags)
        {
            var sequence = GetCoordinateSequence(factory, tokens, ordinateFlags, LineString.MinimumValidSize, false);
            if (!IsStrict && sequence.Count == 1)
                sequence = CoordinateSequences.Extend(factory.CoordinateSequenceFactory, sequence, 2);
            return factory.CreateLineString(sequence);
        }

        /// <summary>
        /// Creates a <c>LinearRing</c> using the next token in the stream.
        /// </summary>
        /// <param name="tokens">
        ///   Tokenizer over a stream of text in Well-known Text
        ///   format. The next tokens must form a &lt;LineString Text.
        /// </param>
        /// <param name="factory">The factory to create the geometry</param>
        /// <param name="ordinateFlags">A flag indicating the ordinates to expect.</param>
        /// <returns>A <c>LinearRing</c> specified by the next
        /// token in the stream.</returns>
        private LinearRing ReadLinearRingText(TokenStream tokens, GeometryFactory factory, Ordinates ordinateFlags)
        {
            var sequence = GetCoordinateSequence(factory, tokens, ordinateFlags, LinearRing.MinimumValidSize, true);
            if (!IsStrict && !CoordinateSequences.IsRing(sequence))
                sequence = CoordinateSequences.EnsureValidRing(factory.CoordinateSequenceFactory, sequence);
            return factory.CreateLinearRing(sequence);
        }

        /// <summary>
        /// Creates a <c>MultiPoint</c> using the next token in the stream.
        /// </summary>
        /// <param name="tokens">
        ///   Tokenizer over a stream of text in Well-known Text
        ///   format. The next tokens must form a &lt;MultiPoint Text.
        /// </param>
        /// <param name="factory">The factory to create the geometry</param>
        /// <param name="ordinateFlags">A flag indicating the ordinates to expect.</param>
        /// <returns>
        /// A <c>MultiPoint</c> specified by the next
        /// token in the stream.</returns>
        private MultiPoint ReadMultiPointText(TokenStream tokens, GeometryFactory factory, Ordinates ordinateFlags)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals(WKTConstants.EMPTY))
            {
                return factory.CreateMultiPoint(new Point[0]);
            }

            // check for old-style JTS syntax (no parentheses surrounding Point coordinates) and parse it if present
            // MD 2009-02-21 - this is only provided for backwards compatibility for a few versions
            if (IsOldNtsMultiPointSyntaxAllowed)
            {
                string nextWord = LookAheadWord(tokens);
                if (nextWord != "(" && nextWord != WKTConstants.EMPTY)
                {
                    return factory.CreateMultiPoint(
                        GetCoordinateSequenceOldMultiPoint(factory, tokens, ordinateFlags));
                }
            }

            var points = new List<Point>();
            var point = ReadPointText(tokens, factory, ordinateFlags);
            points.Add(point);
            nextToken = GetNextCloserOrComma(tokens);
            while (nextToken.Equals(","))
            {
                point = ReadPointText(tokens, factory, ordinateFlags);
                points.Add(point);
                nextToken = GetNextCloserOrComma(tokens);
            }
            return factory.CreateMultiPoint(points.ToArray());
        }

        /// <summary>
        /// Creates a <c>Polygon</c> using the next token in the stream.
        /// </summary>
        /// <param name="tokens">
        ///   Tokenizer over a stream of text in Well-known Text
        ///   format. The next tokens must form a Polygon Text.
        /// </param>
        /// <param name="factory">The factory to create the geometry</param>
        /// <param name="ordinateFlags">A flag indicating the ordinates to expect.</param>
        /// <returns>
        /// A <c>Polygon</c> specified by the next token
        /// in the stream.
        /// </returns>
        private Polygon ReadPolygonText(TokenStream tokens, GeometryFactory factory, Ordinates ordinateFlags)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals(WKTConstants.EMPTY))
                return factory.CreatePolygon(CreateCoordinateSequenceEmpty(factory.CoordinateSequenceFactory, ordinateFlags));

            var holes = new List<LinearRing>();
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
        /// <param name="factory">The factory to create the geometry</param>
        /// <param name="ordinateFlags">A flag indicating the ordinates to expect.</param>
        /// <returns>
        /// A <c>MultiLineString</c> specified by the
        /// next token in the stream.</returns>
        private MultiLineString ReadMultiLineStringText(TokenStream tokens, GeometryFactory factory, Ordinates ordinateFlags)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals(WKTConstants.EMPTY))
                return factory.CreateMultiLineString();

            var lineStrings = new List<LineString>();
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
        /// <param name="factory">The factory to create the geometry</param>
        /// <param name="ordinateFlags">A flag indicating the ordinates to expect.</param>
        /// <returns>
        /// A <c>MultiPolygon</c> specified by the next
        /// token in the stream, or if if the coordinates used to create the
        /// <c>Polygon</c> shells and holes do not form closed linestrings.</returns>
        private MultiPolygon ReadMultiPolygonText(TokenStream tokens, GeometryFactory factory, Ordinates ordinateFlags)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals(WKTConstants.EMPTY))
                return factory.CreateMultiPolygon();

            var polygons = new List<Polygon>();
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
        /// <param name="factory">The factory to create the geometry</param>
        /// <param name="ordinateFlags">A flag indicating the ordinates to expect.</param>
        /// <returns>
        /// A <c>GeometryCollection</c> specified by the
        /// next token in the stream.</returns>
        private GeometryCollection ReadGeometryCollectionText(TokenStream tokens, GeometryFactory factory, Ordinates ordinateFlags)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals(WKTConstants.EMPTY))
                return factory.CreateGeometryCollection();

            var geometries = new List<Geometry>();
            do
            {
                var geometry = ReadGeometryTaggedText(tokens);
                geometries.Add(geometry);
                nextToken = GetNextCloserOrComma(tokens);
            }
            while (nextToken.Equals(","));

            return factory.CreateGeometryCollection(geometries.ToArray());
        }
    }
}
