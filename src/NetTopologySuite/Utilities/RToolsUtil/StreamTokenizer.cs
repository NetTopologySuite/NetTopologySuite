using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RTools_NTS.Util
{
    // ---------------------------------------------------------------------
    #region Exceptions
    // ---------------------------------------------------------------------

    /// <summary>
    /// Exception class for unterminated tokens.
    /// </summary>
    public class StreamTokenizerUntermException : Exception
    {
        /// <summary>
        /// Construct with a particular message.
        /// </summary>
        /// <param name="msg">The message to store in this object.</param>
        public StreamTokenizerUntermException(string msg) : base(msg) {}
    }

    /// <summary>
    /// Exception class for unterminated quotes.
    /// </summary>
    public class StreamTokenizerUntermQuoteException : StreamTokenizerUntermException
    {
        /// <summary>
        /// Construct with a particular message.
        /// </summary>
        /// <param name="msg">The message to store in this object.</param>
        public StreamTokenizerUntermQuoteException(string msg) : base(msg) {}
    }

    /// <summary>
    /// Exception class for unterminated block comments.
    /// </summary>
    public class StreamTokenizerUntermCommentException : StreamTokenizerUntermException
    {
        /// <summary>
        /// Construct with a particular message.
        /// </summary>
        /// <param name="msg">The message to store in this object.</param>
        public StreamTokenizerUntermCommentException(string msg) : base(msg) {}
    }

    #endregion

    // ---------------------------------------------------------------------
    #region Enumerations
    // ---------------------------------------------------------------------

    /// <summary>
    /// Bitwise enumeration for character types.
    /// </summary>
    [Flags]
    public enum CharTypeBits : byte
    {
        /// <summary>word characters (usually alpha, digits, and domain specific)</summary>
        Word = 1,
        /// <summary># or something for line comments</summary>
        Comment = 2,
        /// <summary>whitespace</summary>
        Whitespace = 4,
        /// <summary>' or " type</summary>
        Quote = 8,
        /// <summary>usually 0 to 9</summary>
        Digit = 16,
        /// <summary>usually 0 to 9, a-f and A-F</summary>
        HexDigit = 32,
        /// <summary>eof char</summary>
        Eof = 64
    }

    #endregion

    /// <summary>
    /// This contains the settings that control the behavior of the tokenizer.
    /// This is separated from the StreamTokenizer so that common settings
    /// are easy to package and keep together.
    /// </summary>
    public class StreamTokenizerSettings
    {
        // ---------------------------------------------------------------------
        #region Properties
        // ---------------------------------------------------------------------

        private byte[] charTypes;
        /// <summary>
        /// This is the character type table.  Each byte is bitwise encoded
        /// with the character attributes, such as whether that character is
        /// word or whitespace.
        /// </summary>
        public byte[] CharTypes => (charTypes);

        bool grabWhitespace;
        /// <summary>
        /// Whether or not to return whitespace tokens.  If not, they're ignored.
        /// </summary>
        public bool GrabWhitespace { get => (grabWhitespace);
            set => grabWhitespace = value;
        }

        bool grabEol;
        /// <summary>
        /// Whether or not to return EolTokens on end of line.  Eol tokens will not
        /// break up other tokens which can be multi-line.  For example block comments
        /// and quotes will not be broken by Eol tokens.  Therefore the number of
        /// Eol tokens does not give you the line count of a stream.
        /// </summary>
        public bool GrabEol { get => (grabEol);
            set => grabEol = value;
        }

        bool slashSlashComments;
        /// <summary>
        /// Whether or not to look for // comments
        /// </summary>
        public bool SlashSlashComments { get => (slashSlashComments);
            set => slashSlashComments = value;
        }

        bool slashStarComments;
        /// <summary>
        /// Whether or not to look for /* */ block comments.
        /// </summary>
        public bool SlashStarComments { get => (slashStarComments);
            set => slashStarComments = value;
        }

        bool grabComments;
        /// <summary>
        /// Whether or not to return comments.
        /// </summary>
        public bool GrabComments { get => (grabComments);
            set => grabComments = value;
        }

        bool doUntermCheck;
        /// <summary>
        /// Whether or not to check for unterminated quotes and block comments.
        /// If true, and one is encoutered, an exception is thrown of the appropriate type.
        /// </summary>
        public bool DoUntermCheck { get => (doUntermCheck);
            set => doUntermCheck = value;
        }

        bool parseNumbers;
        /// <summary>
        /// Whether or not digits are specified as Digit type in the
        /// character table.
        /// This setting is based on the character types table, so this
        /// setting interacts with character type table manipulation.
        /// This setting may become incorrect if you modify the character
        /// types table directly.
        /// </summary>
        public bool ParseNumbers
        {
            get => (parseNumbers);
/* dropped for speed, this means this property isn't accurate if
 * character types table is modified directly.
 *          {
                for (int i = '0'; i <= '9'; i++)
                {
                    if (!IsCharType((char)i, CharTypeBits.Digit))
                    {
                        return(false);
                    }
                }

                return(true);
            }
*/
            set
            {
                if (value)
                {
                    for (int i = '0'; i <= '9'; i++)
                        charTypes[i] |= (byte)CharTypeBits.Digit;
                }
                else
                {
                    byte digit = (byte)CharTypeBits.Digit;

                    for (int i = '0'; i <= '9'; i++)
                    {
                        charTypes[i] &= (byte)(~digit); // not digit
                    }
                }
                parseNumbers = value;
            }
        }

        bool parseHexNumbers;

        /// <summary>
        /// Whether or not to parse Hex (0xABCD...) numbers.
        /// This setting is based on the character types table, so this
        /// setting interacts with character type table manipulation.
        /// </summary>
        public bool ParseHexNumbers
        {
            get => (parseHexNumbers);
            //{
            //    for (int i = 'A'; i <= 'F'; i++)
            //    {
            //        if (!IsCharType((char)i, CharTypeBits.Digit))
            //        {
            //            return(false);
            //        }
            //    }
            //    for (int i = 'a'; i <= 'f'; i++)
            //    {
            //        if (!IsCharType((char)i, CharTypeBits.Digit))
            //        {
            //            return(false);
            //        }
            //    }
            //    if (!IsCharType('x', CharTypeBits.Digit)) return(false);
            //
            //    return(true);
            //}
            set
            {
                parseHexNumbers = value;
                if (parseHexNumbers)
                {
                    for (int i = '0'; i <= '9'; i++)
                        charTypes[i] |= (byte)CharTypeBits.HexDigit;
                    for (int i = 'A'; i <= 'F'; i++)
                        charTypes[i] |= (byte)CharTypeBits.HexDigit;
                    for (int i = 'a'; i <= 'f'; i++)
                        charTypes[i] |= (byte)CharTypeBits.HexDigit;
                    charTypes['x'] |= (byte)CharTypeBits.HexDigit;
                }
                else
                {
                    byte digit = (byte)CharTypeBits.HexDigit;

                    for (int i = 'A'; i <= 'F'; i++)
                    {
                        charTypes[i] &= (byte)(~digit); // not digit
                    }
                    for (int i = 'a'; i <= 'f'; i++)
                    {
                        charTypes[i] &= (byte)(~digit); // not digit
                    }
                    charTypes['x'] &= (byte)(~digit);
                }
            }
        }

        #endregion

        // ---------------------------------------------------------------------
        #region Constructors/Destructor
        // ---------------------------------------------------------------------

        /// <summary>
        /// Default constructor.
        /// </summary>
        public StreamTokenizerSettings()
        {
            charTypes = new byte[StreamTokenizer.NChars + 1];  // plus an EOF entry
            SetDefaults();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public StreamTokenizerSettings(StreamTokenizerSettings other)
        {
            Copy(other);
        }

        /// <summary>
        /// Sets this object to be the same as the specified object.
        /// Note that some settings which are entirely embodied by the character
        /// type table.
        /// </summary>
        public void Copy(StreamTokenizerSettings other)
        {
            charTypes = new byte[StreamTokenizer.NChars + 1];  // plus an EOF entry
            Array.Copy(other.charTypes, 0, charTypes, 0, charTypes.Length);

            grabWhitespace = other.grabWhitespace;
            grabEol = other.grabEol;
            slashSlashComments = other.slashSlashComments;
            slashStarComments = other.slashStarComments;
            grabComments = other.grabComments;
            doUntermCheck = other.doUntermCheck;

            parseHexNumbers = other.parseHexNumbers;
        }

        #endregion

        // ---------------------------------------------------------------------
        #region main Setup
        // ---------------------------------------------------------------------

        /// <summary>
        /// Setup default parse behavior.
        /// This resets to same behavior as on construction.
        /// </summary>
        /// <returns>bool - true for success.</returns>
        public bool SetDefaults()
        {
            slashStarComments = false;
            grabComments = false;
            slashSlashComments = false;
            grabWhitespace = false;
            doUntermCheck = true;
            grabEol = false;

            // setup table
            ResetCharTypeTable();
            ParseNumbers = true;
            ParseHexNumbers = true;
            WordChars('A', 'Z');
            WordChars('a', 'z');
            WhitespaceChars(0, ' ');
            QuoteChar('\'');
            QuoteChar('"');
            WordChars('0', '9');

            return(true);
        }

        /// <summary>
        /// Apply settings which are commonly used for code parsing
        /// C-endCapStyle code, including C++, C#, and Java.
        /// </summary>
        /// <returns></returns>
        public bool SetupForCodeParse()
        {
            GrabWhitespace = true;
            GrabComments = true;
            SlashSlashComments = true;
            DoUntermCheck = true;
            SlashStarComments = true;
            WordChar('_');
            ParseNumbers = true;
            ParseHexNumbers = true;
            return(true);
        }

        #endregion

        // ---------------------------------------------------------------------
        #region Character Table Setup
        // ---------------------------------------------------------------------

        /// <summary>
        /// Clear the character type settings.  This leaves them unset,
        /// as opposed to the default.  Use SetDefaults() for default
        /// settings.
        /// </summary>
        public void ResetCharTypeTable()
        {
            Array.Clear(charTypes, 0, charTypes.Length);
            charTypes[StreamTokenizer.NChars] = (byte)CharTypeBits.Eof; // last entry for Eof
        }

        /// <summary>
        /// Specify that a particular character is a word character.
        /// Character table type manipulation method.
        /// This adds the type to the char(s), rather
        /// than overwriting other types.
        /// </summary>
        /// <param name="c">The character.</param>
        public void WordChar(int c)
        {
            charTypes[c] |= (byte)CharTypeBits.Word;
        }

        /// <summary>
        /// Specify that a range of characters are word characters.
        /// Character table type manipulation method.
        /// This adds the type to the char(s), rather
        /// than overwriting other types.
        /// </summary>
        /// <param name="startChar">First character.</param>
        /// <param name="endChar">Last character.</param>
        public void WordChars(int startChar, int endChar)
        {
            for (int i = startChar; i <= endChar; i++)
            {
                charTypes[i] |= (byte)CharTypeBits.Word;
            }
        }

        /// <summary>
        /// Specify that a string of characters are word characters.
        /// Character table type manipulation method.
        /// This adds the type to the char(s), rather
        /// than overwriting other types.
        /// </summary>
        /// <param name="s"></param>
        public void WordChars(string s)
        {
            for (int i = 0; i < s.Length; i++)
                charTypes[s[i]] |= (byte)CharTypeBits.Word;
        }

        /// <summary>
        /// Specify that a character is a whitespace character.
        /// Character table type manipulation method.
        /// This type is exclusive with other types.
        /// </summary>
        /// <param name="c">The character.</param>
        public void WhitespaceChar(int c)
        {
            charTypes[c] = (byte)CharTypeBits.Whitespace;
        }

        /// <summary>
        /// Specify that a range of characters are whitespace characters.
        /// Character table type manipulation method.
        /// This adds the characteristic to the char(s), rather
        /// than overwriting other characteristics.
        /// </summary>
        /// <param name="startChar">First character.</param>
        /// <param name="endChar">Last character.</param>
        public void WhitespaceChars(int startChar, int endChar)
        {
            for (int i = startChar; i <= endChar; i++)
                charTypes[i] = (byte)CharTypeBits.Whitespace;
        }

        /// <summary>
        /// Remove other type settings from a range of characters.
        /// Character table type manipulation method.
        /// </summary>
        /// <param name="startChar"></param>
        /// <param name="endChar"></param>
        public void OrdinaryChars(int startChar, int endChar)
        {
            for (int i = startChar; i <= endChar; i++)
                charTypes[i] = 0;
        }

        /// <summary>
        /// Remove other type settings from a character.
        /// Character table type manipulation method.
        /// </summary>
        /// <param name="c"></param>
        public void OrdinaryChar(int c)
        {
            charTypes[c] = 0;
        }

        /// <summary>
        /// Specify that a particular character is a comment-starting character.
        /// Character table type manipulation method.
        /// </summary>
        /// <param name="c"></param>
        public void CommentChar(int c)
        {
            charTypes[c] = (byte)CharTypeBits.Comment;
        }

        /// <summary>
        /// Specify that a particular character is a quote character.
        /// Character table type manipulation method.
        /// </summary>
        /// <param name="c"></param>
        public void QuoteChar(int c)
        {
            charTypes[c] = (byte)CharTypeBits.Quote;
        }

        #endregion

        // ---------------------------------------------------------------------
        #region Utility Methods
        // ---------------------------------------------------------------------

        /// <summary>
        /// Return a string representation of a character type setting.
        /// Since the type setting is bitwise encoded, a character
        /// can have more than one type.
        /// </summary>
        /// <param name="ctype">The character type byte.</param>
        /// <returns>The string representation of the type flags.</returns>
        public string CharTypeToString(byte ctype)
        {
            var str = new StringBuilder();

            if (IsCharType(ctype, CharTypeBits.Quote)) str.Append('q');
            if (IsCharType(ctype, CharTypeBits.Comment)) str.Append('m');
            if (IsCharType(ctype, CharTypeBits.Whitespace)) str.Append('w');
            if (IsCharType(ctype, CharTypeBits.Digit)) str.Append('d');
            if (IsCharType(ctype, CharTypeBits.Word)) str.Append('a');
            if (IsCharType(ctype, CharTypeBits.Eof)) str.Append('e');
            if (str.Length == 0)
            {
                str.Append('c');
            }
            return(str.ToString());
        }

        /// <summary>
        /// Check whether the specified char type byte has a
        /// particular type flag set.
        /// </summary>
        /// <param name="ctype">The char type byte.</param>
        /// <param name="type">The CharTypeBits entry to compare to.</param>
        /// <returns>bool - true or false</returns>
        public bool IsCharType(byte ctype, CharTypeBits type)
        {
            return((ctype & (byte)type) != 0);
        }

        /// <summary>
        /// Check whether the specified char has a
        /// particular type flag set.
        /// </summary>
        /// <param name="c">The character.</param>
        /// <param name="type">The CharTypeBits entry to compare to.</param>
        /// <returns>bool - true or false</returns>
        public bool IsCharType(char c, CharTypeBits type)
        {
            return((charTypes[c] & (byte)type) != 0);
        }

        /// <summary>
        /// Check whether the specified char has a
        /// particular type flag set.
        /// </summary>
        /// <param name="c">The character.</param>
        /// <param name="type">The CharTypeBits entry to compare to.</param>
        /// <returns>bool - true or false</returns>
        public bool IsCharType(int c, CharTypeBits type)
        {
            return((charTypes[c] & (byte)type) != 0);
        }

        #endregion

        // ---------------------------------------------------------------------
        #region Standard Methods
        // ---------------------------------------------------------------------

        /// <summary>
        /// Display the state of this object.
        /// </summary>
        public void Display()
        {
            Display(string.Empty);
        }

        /// <summary>
        /// Display the state of this object, with a per-line prefix.
        /// </summary>
        /// <param name="prefix">The pre-line prefix.</param>
        public void Display(string prefix)
        {
        }
        #endregion
    }

    /// <summary>
    /// A StreamTokenizer similar to Java's.  This breaks an input stream
    /// (coming from a TextReader) into Tokens based on various settings.  The settings
    /// are stored in the TokenizerSettings property, which is a
    /// StreamTokenizerSettings instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is configurable in that you can modify TokenizerSettings.CharTypes[] array
    /// to specify which characters are which type, along with other settings
    /// such as whether to look for comments or not.
    /// </para>
    /// <para>
    /// WARNING: This is not internationalized.  This treats all characters beyond
    /// the 7-bit ASCII range (decimal 127) as Word characters.
    /// </para>
    /// <para>
    /// There are two main ways to use this: 1) Parse the entire stream at
    /// once and get an ArrayList of Tokens (see the Tokenize* methods),
    /// and 2) call NextToken() successively.
    /// This reads from a TextReader, which you can set directly, and this
    /// also provides some convenient methods to parse files and strings.
    /// This returns an Eof token if the end of the input is reached.
    /// </para>
    /// <para>
    /// Here's an example of the NextToken() endCapStyle of use:
    /// <code>
    /// StreamTokenizer tokenizer = new StreamTokenizer();
    /// tokenizer.GrabWhitespace = true;
    /// tokenizer.Verbosity = VerbosityLevel.Debug; // just for debugging
    /// tokenizer.TextReader = File.OpenText(fileName);
    /// Token token;
    /// while (tokenizer.NextToken(out token)) log.Info("Token = '{0}'", token);
    /// </code>
    /// </para>
    /// <para>
    /// Here's an example of the Tokenize... endCapStyle of use:
    /// <code>
    /// StreamTokenizer tokenizer = new StreamTokenizer("some string");
    /// ArrayList tokens = new ArrayList();
    /// if (!tokenizer.Tokenize(tokens))
    /// {
    ///     // error handling
    /// }
    /// foreach (Token t in tokens) Console.WriteLine("t = {0}", t);
    /// </code>
    /// </para>
    /// <para>
    /// Comment delimiters are hardcoded (// and /*), not affected by char type table.
    /// </para>
    /// <para>
    /// This sets line numbers in the tokens it produces.  These numbers are normally
    /// the line on which the token starts.
    /// There is one known caveat, and that is that when GrabWhitespace setting
    /// is true, and a whitespace token contains a newline, that token's line number
    /// will be set to the following line rather than the line on which the token
    /// started.
    /// </para>
    /// </remarks>
    public class StreamTokenizer : IEnumerable<Token>
    {
        // ----------------------------------------------------------------
        #region Constants
        // ----------------------------------------------------------------

        /// <summary>
        /// This is the number of characters in the character table.
        /// </summary>
        public static readonly int NChars = 128;
        private static readonly int Eof = NChars;
        #endregion

        // ----------------------------------------------------------------
        #region Private Fields
        // ----------------------------------------------------------------

        // The TextReader we're reading from
        private TextReader textReader;

        // buffered wrap of reader
        //private BufferedTextReader bufferedReader; // was slower

        // keep track of current line number during parse
        private int lineNumber;

        // used to back up in the stream
        private CharBuffer backString;

        // used to collect characters of the current (next to be
        // emitted) token
        private CharBuffer nextTokenSb;

        // for speed, construct these once and re-use
        private CharBuffer tmpSb;
        private CharBuffer expSb;

        #endregion

        // ----------------------------------------------------------------------
        #region Properties
        // ----------------------------------------------------------------------

        /// <summary>
        /// This is the TextReader that this object will read from.
        /// Set this to set the input reader for the parse.
        /// </summary>
        public TextReader TextReader
        {
            get => (textReader);
            set => textReader = value;
        }

        private StreamTokenizerSettings settings;
        /// <summary>
        /// The settings which govern the behavior of the tokenization.
        /// </summary>
        public StreamTokenizerSettings Settings => (settings);

        #endregion

        // ---------------------------------------------------------------------
        #region Constructors/Destructor
        // ---------------------------------------------------------------------

        /// <summary>
        /// Default constructor.
        /// </summary>
        public StreamTokenizer()
        {
            Initialize();
        }

        /// <summary>
        /// Construct and set this object's TextReader to the one specified.
        /// </summary>
        /// <param name="sr">The TextReader to read from.</param>
        public StreamTokenizer(TextReader sr)
        {
            Initialize();
            textReader = sr;
        }

        /// <summary>
        /// Construct and set this object's TextReader to the one specified.
        /// </summary>
        /// <param name="sr">The TextReader to read from.</param>
        /// <param name="tokenizerSettings">Tokenizer settings.</param>
        public StreamTokenizer(TextReader sr, StreamTokenizerSettings tokenizerSettings)
        {
            settings = tokenizerSettings;
            Initialize();
            textReader = sr;
        }

        /// <summary>
        /// Construct and set a string to tokenize.
        /// </summary>
        /// <param name="str">The string to tokenize.</param>
        public StreamTokenizer(string str)
        {
            Initialize();
            textReader = new StringReader(str);
        }

        /// <summary>
        /// Utility function, things common to constructors.
        /// </summary>
        void Initialize()
        {
            backString = new CharBuffer(32);
            nextTokenSb = new CharBuffer(1024);

            InitializeStream();
            if (null == settings)
            {
                settings = new StreamTokenizerSettings();
                settings.SetDefaults();
            }

            expSb = new CharBuffer();
            tmpSb = new CharBuffer();
        }

        /// <summary>
        /// Clear the stream settings.
        /// </summary>
        void InitializeStream()
        {
            lineNumber = 1; // base 1 line numbers
            textReader = null;
        }

        #endregion

        // ---------------------------------------------------------------------
        #region Standard Methods
        // ---------------------------------------------------------------------

        /// <summary>
        /// Display the state of this object.
        /// </summary>
        public void Display()
        {
            Display(string.Empty);
        }

        /// <summary>
        /// Display the state of this object, with a per-line prefix.
        /// </summary>
        /// <param name="prefix">The pre-line prefix.</param>
        public void Display(string prefix)
        {
            if (settings != null) settings.Display(prefix + "    ");
        }

        #endregion

        // ---------------------------------------------------------------------
        #region NextToken (the state machine)
        // ---------------------------------------------------------------------

        /// <summary>
        /// The states of the state machine.
        /// </summary>
        private enum NextTokenState
        {
            Start,
            Whitespace,
            Word,
            Quote,
            EndQuote,
            MaybeNumber, // could be number or word
            MaybeComment, // after first slash, might be comment or not
            MaybeHex, // after 0, may be hex
            HexGot0x, // after 0x, may be hex
            HexNumber,
            LineComment,
            BlockComment,
            EndBlockComment,
            Char,
            Eol,
            Eof,
            Invalid
        }

        /// <summary>
        /// Pick the next state given just a single character.  This is used
        /// at the start of a new token.
        /// </summary>
        /// <param name="ctype">The type of the character.</param>
        /// <param name="c">The character.</param>
        /// <returns>The state.</returns>
        private NextTokenState PickNextState(byte ctype, int c)
        {
            return(PickNextState(ctype, c, NextTokenState.Start));
        }

        /// <summary>
        /// Pick the next state given just a single character.  This is used
        /// at the start of a new token.
        /// </summary>
        /// <param name="ctype">The type of the character.</param>
        /// <param name="c">The character.</param>
        /// <param name="excludeState">Exclude this state from the possible next state.</param>
        /// <returns>The state.</returns>
        private NextTokenState PickNextState(byte ctype, int c, NextTokenState excludeState)
        {
            if (c == '/')
            {
                return(NextTokenState.MaybeComment); // overrides all other cats
            }
            else if ((excludeState != NextTokenState.MaybeHex)
                && settings.ParseHexNumbers && (c == '0'))
            {
                return(NextTokenState.MaybeHex);
            }
            else if ((excludeState != NextTokenState.MaybeNumber) && settings.ParseNumbers
                && (settings.IsCharType(ctype, CharTypeBits.Digit) || (c == '-') || (c == '.')))
            {
                return(NextTokenState.MaybeNumber);
            }
            else if (settings.IsCharType(ctype, CharTypeBits.Word)) return(NextTokenState.Word);
            else if (settings.GrabEol && (c == 10)) return(NextTokenState.Eol);
            else if (settings.IsCharType(ctype, CharTypeBits.Whitespace)) return(NextTokenState.Whitespace);
            else if (settings.IsCharType(ctype, CharTypeBits.Comment)) return(NextTokenState.LineComment);
            else if (settings.IsCharType(ctype, CharTypeBits.Quote)) return(NextTokenState.Quote);
            else if ((c == Eof) || (settings.IsCharType(ctype, CharTypeBits.Eof))) return(NextTokenState.Eof);
            return(NextTokenState.Char);
        }

        /// <summary>
        /// Read the next character from the stream, or from backString
        /// if we backed up.
        /// </summary>
        /// <returns>The next character.</returns>
        private int GetNextChar()
        {
            int c;

            // consume from backString if possible
            if (backString.Length > 0)
            {
                c = backString[0];
                backString.Remove(0, 1);
                return(c);
            }

            if (textReader == null) return(Eof);

            try
            {
                while((c = textReader.Read()) == 13) {} // skip LF (13)
            }
            catch(Exception)
            {
                return(Eof);
            }

            if (c == 10)
            {
                lineNumber++;
            }
            else if (c < 0)
            {
                c = Eof;
            }

            return(c);
        }

        /// <summary>
        /// Get the next token.  The last token will be an EofToken unless
        /// there's an unterminated quote or unterminated block comment
        /// and Settings.DoUntermCheck is true, in which case this throws
        /// an exception of type StreamTokenizerUntermException or sub-class.
        /// </summary>
        /// <param name="token">The output token.</param>
        /// <returns>bool - true for success, false for failure.</returns>
        public bool NextToken(out Token token)
        {
            token = null;
            int thisChar = 0; // current character
            byte ctype; // type of this character

            var state = NextTokenState.Start;
            int prevChar = 0; // previous character
            byte prevCtype = (byte)CharTypeBits.Eof;

            // get previous char from nextTokenSb if there
            // (nextTokenSb is a StringBuilder containing the characters
            //  of the next token to be emitted)
            if (nextTokenSb.Length > 0)
            {
                prevChar = nextTokenSb[nextTokenSb.Length - 1];
                prevCtype = settings.CharTypes[prevChar];
                state = PickNextState(prevCtype, prevChar);
            }

            // extra state for number parse
            int seenDot = 0; // how many .'s in the number
            int seenE = 0; // how many e's or E's have we seen in the number
            bool seenDigit = false; // seen any digits (numbers can start with -)

            // lineNumber can change with each GetNextChar()
            // tokenLineNumber is the line on which the token started
            int tokenLineNumber = lineNumber;

            // State Machine: Produces a single token.
            // Enter a state based on a single character.
            // Generally, being in a state means we're currently collecting chars
            // in that type of token.
            // We do state machine until it builds a token (Eof is a token), then
            // return that token.
            thisChar = prevChar;  // for first iteration, since prevChar is set to this
            bool done = false; // optimization
            while (!done)
            {
                prevChar = thisChar;
                thisChar = GetNextChar();
                if (thisChar >= settings.CharTypes.Length)
                {
                    // greater than 7-bit ascii, treat as word character
                    ctype = (byte)CharTypeBits.Word;
                }
                else ctype = settings.CharTypes[thisChar];

                // see if we need to change states, or emit a token
                switch(state)
                {
                    case NextTokenState.Start:
                        // RESET
                        state = PickNextState(ctype, thisChar);
                        tokenLineNumber = lineNumber;
                        break;

                    case NextTokenState.Char:
                        token = new CharToken((char)prevChar, tokenLineNumber);
                        done = true;
                        nextTokenSb.Length = 0;
                        break;

                    case NextTokenState.Word:
                        if ((!settings.IsCharType(ctype, CharTypeBits.Word))
                            && (!settings.IsCharType(ctype, CharTypeBits.Digit)))
                        {
                            // end of word, emit
                            token = new WordToken(nextTokenSb.ToString(), tokenLineNumber);
                            done = true;
                            nextTokenSb.Length = 0;
                        }
                        break;

                    case NextTokenState.Whitespace:
                        if (!settings.IsCharType(ctype, CharTypeBits.Whitespace)
                            || (settings.GrabEol && (thisChar == 10)))
                        {
                            // end of whitespace, emit
                            if (settings.GrabWhitespace)
                            {
                                token = new WhitespaceToken(nextTokenSb.ToString(), tokenLineNumber);
                                done = true;
                                nextTokenSb.Length = 0;
                            }
                            else
                            {
                                // RESET
                                nextTokenSb.Length = 0;
                                tokenLineNumber = lineNumber;
                                state = PickNextState(ctype, thisChar);
                            }
                        }
                        break;

                    case NextTokenState.EndQuote:
                        // we're now 1 char after end of quote
                        token = new QuoteToken(nextTokenSb.ToString(), tokenLineNumber);
                        done = true;
                        nextTokenSb.Length = 0;
                        break;

                    case NextTokenState.Quote:
                        // looking for end quote matching char that started the quote
                        if (thisChar == nextTokenSb[0])
                        {
                            // handle escaped backslashes: count the immediately prior backslashes
                            // - even (including 0) means it's not escaped
                            // - odd means it is escaped
                            int backSlashCount = 0;
                            for (int i = nextTokenSb.Length - 1; i >= 0; i--)
                            {
                                if (nextTokenSb[ i ] == '\\') backSlashCount++;
                                else break;
                            }

                            if ((backSlashCount % 2) == 0)
                            {
                                state = NextTokenState.EndQuote;
                            }
                        }

                        if ((state != NextTokenState.EndQuote) && (thisChar == Eof))
                        {
                            if (settings.DoUntermCheck)
                            {
                                nextTokenSb.Length = 0;
                                throw new StreamTokenizerUntermQuoteException("Unterminated quote");
                            }

                            token = new QuoteToken(nextTokenSb.ToString(), tokenLineNumber);
                            done = true;
                            nextTokenSb.Length = 0;
                        }
                        break;

                    case NextTokenState.MaybeComment:
                        if (thisChar == Eof)
                        {
                            token = new CharToken(nextTokenSb.ToString(), tokenLineNumber);
                            done = true;
                            nextTokenSb.Length = 0;
                        }
                        else
                        {
                            // if we get the right char, we're in a comment
                            if (settings.SlashSlashComments && (thisChar == '/'))
                                state = NextTokenState.LineComment;
                            else if (settings.SlashStarComments && (thisChar == '*'))
                                state = NextTokenState.BlockComment;
                            else
                            {
                                token = new CharToken(nextTokenSb.ToString(), tokenLineNumber);
                                done = true;
                                nextTokenSb.Length = 0;
                            }
                        }
                        break;

                    case NextTokenState.LineComment:
                        if (thisChar == Eof)
                        {
                            if (settings.GrabComments)
                            {
                                token = new CommentToken(nextTokenSb.ToString(), tokenLineNumber);
                                done = true;
                                nextTokenSb.Length = 0;
                            }
                            else
                            {
                                // RESET
                                nextTokenSb.Length = 0;
                                tokenLineNumber = lineNumber;
                                state = PickNextState(ctype, thisChar);
                            }
                        }
                        else
                        {
                            if (thisChar == '\n')
                            {
                                if (settings.GrabComments)
                                {
                                    token = new CommentToken(nextTokenSb.ToString(), tokenLineNumber);
                                    done = true;
                                    nextTokenSb.Length = 0;
                                }
                                else
                                {
                                    // RESET
                                    nextTokenSb.Length = 0;
                                    tokenLineNumber = lineNumber;
                                    state = PickNextState(ctype, thisChar);
                                }
                            }
                        }
                        break;

                    case NextTokenState.BlockComment:
                        if (thisChar == Eof)
                        {
                            if (settings.DoUntermCheck)
                            {
                                nextTokenSb.Length = 0;
                                throw new StreamTokenizerUntermCommentException("Unterminated comment.");
                            }

                            if (settings.GrabComments)
                            {
                                token = new CommentToken(nextTokenSb.ToString(), tokenLineNumber);
                                done = true;
                                nextTokenSb.Length = 0;
                            }
                            else
                            {
                                // RESET
                                nextTokenSb.Length = 0;
                                tokenLineNumber = lineNumber;
                                state = PickNextState(ctype, thisChar);
                            }
                        }
                        else
                        {
                            if ((thisChar == '/') && (prevChar == '*'))
                            {
                                state = NextTokenState.EndBlockComment;
                            }
                        }
                        break;

                    // special case for 2-character token termination
                    case NextTokenState.EndBlockComment:
                        if (settings.GrabComments)
                        {
                            token = new CommentToken(nextTokenSb.ToString(), tokenLineNumber);
                            done = true;
                            nextTokenSb.Length = 0;
                        }
                        else
                        {
                            // RESET
                            nextTokenSb.Length = 0;
                            tokenLineNumber = lineNumber;
                            state = PickNextState(ctype, thisChar);
                        }
                        break;

                    case NextTokenState.MaybeHex:
                        // previous char was 0
                        if (thisChar != 'x')
                        {
                            // back up and try non-hex
                            // back up to the 0
                            nextTokenSb.Append((char)thisChar);
                            backString.Append(nextTokenSb);
                            nextTokenSb.Length = 0;

                            // reset state and don't choose MaybeNumber state.
                            // pull char from backString
                            thisChar = backString[0];
                            backString.Remove(0, 1);
                            state = PickNextState(settings.CharTypes[thisChar], thisChar,
                                NextTokenState.MaybeHex);
                        }
                        else state = NextTokenState.HexGot0x;
                        break;

                    case NextTokenState.HexGot0x:
                        if (!settings.IsCharType(ctype, CharTypeBits.HexDigit))
                        {
                            // got 0x but now a non-hex char
                            // back up to the 0
                            nextTokenSb.Append((char)thisChar);
                            backString.Append(nextTokenSb);
                            nextTokenSb.Length = 0;

                            // reset state and don't choose MaybeNumber state.
                            // pull char from backString
                            thisChar = backString[0];
                            backString.Remove(0, 1);
                            state = PickNextState(settings.CharTypes[thisChar], thisChar,
                                NextTokenState.MaybeHex);
                        }
                        else state = NextTokenState.HexNumber;
                        break;

                    case NextTokenState.HexNumber:
                        if (!settings.IsCharType(ctype, CharTypeBits.HexDigit))
                        {
                            // emit the hex number we've collected
                            token = IntToken.ParseHex(nextTokenSb.ToString(), tokenLineNumber);
                            done = true;
                            nextTokenSb.Length = 0;
                        }
                        break;

                    case NextTokenState.MaybeNumber:
                        //
                        // Determine whether or not to stop collecting characters for
                        // the number parse.  We terminate when it's clear it's not
                        // a number or no longer a number.
                        //
                        bool term = false;

                        if (settings.IsCharType(ctype, CharTypeBits.Digit)
                            || settings.IsCharType(prevChar, CharTypeBits.Digit)) seenDigit = true;

                        // term conditions
                        if (thisChar == '.')
                        {
                            seenDot++;
                            if (seenDot > 1) term = true;  // more than one dot, it aint a number
                        }
                        else if (((thisChar == 'e') || (thisChar == 'E')))
                        {
                            seenE++;
                            if (!seenDigit) term = true;  // e before any digits is bad
                            else if (seenE > 1) term = true;  // more than 1 e is bad
                            else
                            {
                                term = true; // done regardless

                                // scan the exponent, put its characters into
                                // nextTokenSb, if there are any
                                char c;
                                expSb.Clear();
                                expSb.Append((char)thisChar);
                                if (GrabInt(expSb, true, out c))
                                {
                                    // we got a good exponent, tack it on
                                    nextTokenSb.Append(expSb);
                                    thisChar = c; // and continue after the exponent's characters
                                }
                            }
                        }
                        else if (thisChar == Eof) term = true;
                            // or a char that can't be in a number
                        else if ((!settings.IsCharType(ctype, CharTypeBits.Digit)
                            && (thisChar != 'e') && (thisChar != 'E')
                            && (thisChar != '-') && (thisChar != '.'))
                            || ((thisChar == '+') && (seenE == 0)))
                        {
                            // it's not a normal number character
                            term = true;
                        }
                        // or a dash not after e
                        else if ((thisChar == '-') && (!((prevChar == 'e') || (prevChar == 'E')))) term = true;

                        if (term)
                        {
                            // we are terminating a number, or it wasn't a number
                            if (seenDigit)
                            {
                                if ((nextTokenSb.IndexOf('.') >= 0)
                                    || (nextTokenSb.IndexOf('e') >= 0)
                                    || (nextTokenSb.IndexOf('E') >= 0)
                                    || (nextTokenSb.Length >= 19) // probably too large for Int64, use float
                                    )
                                {
                                    token = new FloatToken(nextTokenSb.ToString(), tokenLineNumber);
                                }
                                else
                                {
                                    token = new IntToken(nextTokenSb.ToString(), tokenLineNumber);
                                }
                                done = true;
                                nextTokenSb.Length = 0;
                            }
                            else
                            {
                                // -whatever or -.whatever
                                // didn't see any digits, must have gotten here by a leading -
                                // and no digits after it
                                // back up to -, pick next state excluding numbers
                                nextTokenSb.Append((char)thisChar);
                                backString.Append(nextTokenSb);
                                nextTokenSb.Length = 0;

                                // restart on the - and don't choose MaybeNumber state
                                // pull char from backString
                                thisChar = backString[0];
                                backString.Remove(0, 1);
                                state = PickNextState(settings.CharTypes[thisChar], thisChar,
                                    NextTokenState.MaybeNumber);
                            }
                        }
                        break;

                    case NextTokenState.Eol:
                        // tokenLineNumber - 1 because the newline char is on the previous line
                        token = new EolToken(tokenLineNumber - 1);
                        done = true;
                        nextTokenSb.Length = 0;
                        break;

                    case NextTokenState.Eof:
                        token = new EofToken(tokenLineNumber);
                        done = true;
                        nextTokenSb.Length = 0;
                        return(false);

                    case NextTokenState.Invalid:
                    default:
                        // not a good sign, some unrepresented state?
                        return(false);
                }

                // use a StringBuilder to accumulate characters which are part of this token
                if (thisChar != Eof) nextTokenSb.Append((char)thisChar);
            }

            return(true);
        }

        /// <summary>
        /// Starting from current stream location, scan forward
        /// over an int.  Determine whether it's an integer or not.  If so,
        /// push the integer characters to the specified CharBuffer.
        /// If not, put them in backString (essentially leave the
        /// stream as it was) and return false.
        /// <para>
        /// If it was an int, the stream is left 1 character after the
        /// end of the int, and that character is output in the thisChar parameter.
        /// </para>
        /// <para>The formats for integers are: 1, +1, and -1</para>
        /// The + and - signs are included in the output buffer.
        /// </summary>
        /// <param name="sb">The CharBuffer to append to.</param>
        /// <param name="allowPlus">Whether or not to consider + to be part
        /// of an integer.</param>
        /// <param name="thisChar">The last character read by this method.</param>
        /// <returns>true for parsed an int, false for not an int</returns>
        private bool GrabInt(CharBuffer sb, bool allowPlus, out char thisChar)
        {
            tmpSb.Clear(); // use tmp CharBuffer

            // first character can be -, maybe can be + depending on arg
            thisChar = (char)GetNextChar();
            if (thisChar == Eof)
            {
                return(false);
            }
            else if (thisChar == '+')
            {
                if (allowPlus)
                {
                    tmpSb.Append(thisChar);
                }
                else
                {
                    backString.Append(thisChar);
                    return(false);
                }
            }
            else if (thisChar == '-')
            {
                tmpSb.Append(thisChar);
            }
            else if (settings.IsCharType(thisChar, CharTypeBits.Digit))
            {
                // a digit, back this out so we can handle it in loop below
                backString.Append(thisChar);
            }
            else
            {
                // not a number starter
                backString.Append(thisChar);
                return(false);
            }

            // rest of chars have to be digits
            bool gotInt = false;
            while(((thisChar = (char)GetNextChar()) != Eof)
                && (settings.IsCharType(thisChar, CharTypeBits.Digit)))
            {
                gotInt = true;
                tmpSb.Append(thisChar);
            }

            if (gotInt)
            {
                sb.Append(tmpSb);
                return(true);
            }
            else
            {
                // didn't get any chars after first
                backString.Append(tmpSb); // put + or - back on
                if (thisChar != Eof) backString.Append(thisChar);
                return(false);
            }
        }

        #endregion

        // ---------------------------------------------------------------------
        #region Tokenize wrapper methods
        // ---------------------------------------------------------------------

        /// <summary>
        /// Parse the rest of the stream and put all the tokens
        /// in the input ArrayList. This resets the line number to 1.
        /// </summary>
        /// <param name="tokens">The ArrayList to append to.</param>
        /// <returns>bool - true for success</returns>
        public bool Tokenize(IList<Token> tokens)
        {
            Token token;
            this.lineNumber = 1;

            while (NextToken(out token))
            {
                if (token == null) throw new NullReferenceException(
                        "StreamTokenizer: Tokenize: Got a null token from NextToken.");
                tokens.Add(token);
            }

            // add the last token returned (EOF)
            tokens.Add(token);
            return(true);
        }

        /// <summary>
        /// Parse all tokens from the specified TextReader, put
        /// them into the input ArrayList.
        /// </summary>
        /// <param name="tr">The TextReader to read from.</param>
        /// <param name="tokens">The ArrayList to append to.</param>
        /// <returns>bool - true for success, false for failure.</returns>
        public bool TokenizeReader(TextReader tr, IList<Token> tokens)
        {
            textReader = tr;
            return(Tokenize(tokens));
        }

        /// <summary>
        /// Parse all tokens from the specified file, put
        /// them into the input ArrayList.
        /// </summary>
        /// <param name="fileName">The file to read.</param>
        /// <param name="tokens">The ArrayList to put tokens in.</param>
        /// <returns>bool - true for success, false for failure.</returns>
        public bool TokenizeFile(string fileName, IList<Token> tokens)
        {
            var fi = new FileInfo(fileName);
            FileStream fr = null;
            try
            {
                fr = fi.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                textReader = new StreamReader(fr);
            }
            catch (DirectoryNotFoundException)
            {
            }
            try
            {
                if (!Tokenize(tokens))
                {
                    textReader.Dispose();
                    if (fr != null) fr.Dispose();
                    return(false);
                }
            }
            catch(StreamTokenizerUntermException e)
            {
                textReader.Dispose();
                if (fr != null) fr.Dispose();
                throw e;
            }

            if (textReader != null) textReader.Dispose();
            if (fr != null) fr.Dispose();
            return(true);
        }

        /// <summary>
        /// Tokenize a file completely and return the tokens in a Token[].
        /// </summary>
        /// <param name="fileName">The file to tokenize.</param>
        /// <returns>A Token[] with all tokens.</returns>
        public Token[] TokenizeFile(string fileName)
        {
            var list = new List<Token>();
            if (!TokenizeFile(fileName, list))
            {
                return(null);
            }
            if (list.Count > 0)
            {
                return list.ToArray();
            }
            return(null);
        }

        /// <summary>
        /// Parse all tokens from the specified string, put
        /// them into the input ArrayList.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="tokens">The ArrayList to put tokens in.</param>
        /// <returns>bool - true for success, false for failure.</returns>
        public bool TokenizeString(string str, IList<Token> tokens)
        {
            textReader = new StringReader(str);
            return(Tokenize(tokens));
        }

        /// <summary>
        /// Parse all tokens from the specified Stream, put
        /// them into the input ArrayList.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="tokens">The ArrayList to put tokens in.</param>
        /// <returns>bool - true for success, false for failure.</returns>
        public bool TokenizeStream(Stream s, IList<Token> tokens)
        {
            textReader = new StreamReader(s);
            return(Tokenize(tokens));
        }

        #endregion

        #region Implementation of IEnumerable

        /// <summary>
        /// Gibt einen Enumerator zurück, der die Auflistung durchläuft.
        /// </summary>
        /// <returns>
        /// Ein <see cref="T:System.Collections.Generic.IEnumerator`1"/>, der zum Durchlaufen der Auflistung verwendet werden kann.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<Token> GetEnumerator()
        {
            Token token;
            lineNumber = 1;

            while (NextToken(out token))
            {
                if (token == null) throw new NullReferenceException(
                        "StreamTokenizer: Tokenize: Got a null token from NextToken.");
                yield return token;
            }

            // add the last token returned (EOF)
            yield return token;
        }

        /// <summary>
        /// Gibt einen Enumerator zurück, der eine Auflistung durchläuft.
        /// </summary>
        /// <returns>
        /// Ein <see cref="T:System.Collections.IEnumerator"/>-Objekt, das zum Durchlaufen der Auflistung verwendet werden kann.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}

