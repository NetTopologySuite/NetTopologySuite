#if tuPrologStreamTokenizer
using System;
using System.IO;

namespace NetTopologySuite.Utilities
{

    /**
    * <summary>
    * The <code>StreamTokenizer</code> class takes an input stream and
    * parses it into "tokens", allowing the tokens to be
    * read one at a time. The parsing process is controlled by a table
    * and a number of flags that can be set to various states. The
    * stream tokenizer can recognize identifiers, numbers, quoted
    * strings, and various comment styles.
    * <p>
    * Each byte read from the input stream is regarded as a character
    * in the range <code>'&#92;u0000'</code> through <code>'&#92;u00FF'</code>.
    * The character value is used to look up five possible attributes of
    * the character: <i>white space</i>, <i>alphabetic</i>,
    * <i>numeric</i>, <i>string quote</i>, and <i>comment character</i>.
    * Each character can have zero or more of these attributes. </p>
    * <p>
    * In addition, an instance has four flags. These flags indicate:
    * <ul>
    * <li>Whether line terminators are to be returned as tokens or treated
    *     as white space that merely separates tokens.</li>
    * <li>Whether C-style comments are to be recognized and skipped.</li>
    * <li>Whether C++-style comments are to be recognized and skipped.</li>
    * <li>Whether the characters of identifiers are converted to lowercase.</li>
    * </ul> </p>
    * <p>
    * A typical application first constructs an instance of this class,
    * sets up the syntax tables, and then repeatedly loops calling the
    * <code>nextToken</code> method in each iteration of the loop until
    * it returns the value <code>TT_EOF</code>. </p>
    * </summary>
    */
    public class StreamTokenizer
    {
        /** <summary> Legge da uno stream di caratteri </summary> */
        private TextReader _reader = null;

        private char[] _buf = new char[20];

        /**<summary>
         * The next character to be considered by the nextToken method.  May also
         * be NEED_CHAR to indicate that a new character should be read, or SKIP_LF
         * to indicate that a new character should be read and, if it is a '\n'
         * character, it should be discarded and a second new character should be
         * read.
         * </summary>
         */
        private int _peekc = NEED_CHAR;

        private const int NEED_CHAR = Int32.MaxValue;
        private const int SKIP_LF = Int32.MaxValue - 1;

        private bool _pushedBack;
        private bool _forceLower;
        /** <summary>The line number of the last token read </summary>*/
        private int LINENO = 1;

        private bool _eolIsSignificantP = false;
        private bool _slashSlashCommentsP = false;
        private bool _slashStarCommentsP = false;

        private byte[] _ctype = new byte[256];
        private const byte CT_WHITESPACE = 1;
        private const byte CT_DIGIT = 2;
        private const byte CT_ALPHA = 4;
        private const byte CT_QUOTE = 8;
        private const byte CT_COMMENT = 16;

        /**
         * <summary>
         * After a call to the <code>nextToken</code> method, this field
         * contains the type of the token just read. For a single character
         * token, its value is the single character, converted to an integer.
         * For a quoted string token (see , its value is the quote character.
         * Otherwise, its value is one of the following:
         * <ul>
         * <li><code>TT_WORD</code> indicates that the token is a word.</li>
         * <li><code>TT_NUMBER</code> indicates that the token is a number.</li>
         * <li><code>TT_EOL</code> indicates that the end of line has been read.
         *     The field can only have this value if the
         *     <code>eolIsSignificant</code> method has been called with the
         *     argument <code>true</code>.</li>
         * <li><code>TT_EOF</code> indicates that the end of the input stream
         *     has been reached.</li>
         * </ul>
         * </summary>
         */
        public int _ttype = TT_NOTHING;

        /**<summary>
         * A constant indicating that the end of the stream has been read.
         * </summary>
         */
        public const int TT_EOF = -1;

        /**<summary>
         * A constant indicating that the end of the line has been read.
         * </summary>
         */
        public const int TT_EOL = '\n';

        /**<summary>
         * A constant indicating that a number token has been read.
         * </summary>
         */
        public const int TT_NUMBER = -2;

        /**<summary>
         * A constant indicating that a word token has been read.
         * </summary>
         */
        public const int TT_WORD = -3;

        /** <summary>
         * A constant indicating that no token has been read, used for
         * initializing ttype.  FIXME This could be made public and
         * made available as the part of the API in a future release.
         * </summary>
         */
        private const int TT_NOTHING = -4;

        /**<summary>
         * If the current token is a word token, this field contains a
         * string giving the characters of the word token. When the current
         * token is a quoted string token, this field contains the body of
         * the string.
         * <p>
         * The current token is a word when the value of the
         * <code>ttype</code> field is <code>TT_WORD</code>. The current token is
         * a quoted string token when the value of the <code>ttype</code> field is
         * a quote character.
         * </p>
         * The initial value of this field is null.
         * </summary>
         */
        public string _sval;

        /**<summary>
         * If the current token is a number, this field contains the value
         * of that number. The current token is a number when the value of
         * the <code>ttype</code> field is <code>TT_NUMBER</code>.
         *
         * The initial value of this field is 0.0.
         *</summary>
         */
        public double _nval;

        /**<summary> Private constructor that initializes everything except the streams. </summary>*/
        protected StreamTokenizer()
        {
            WordChars('a', 'z');
            WordChars('A', 'Z');
            WordChars(128 + 32, 255);
            WhitespaceChars(0, ' ');
            CommentChar('/');
            QuoteChar('"');
            QuoteChar('\'');
            ParseNumbers();
        }

        /**<summary>
         * Create a tokenizer that parses the given character stream.
         *
         * <param name="r">  a Reader object providing the input stream.</param>
         * </summary>
         */
        public StreamTokenizer(TextReader r)
            : this()
        {
            if (r == null)
            {
                throw new ArgumentNullException();
            }
            _reader = r;
        }

        /**<summary>
         * Resets this tokenizer's syntax table so that all characters are
         * "ordinary." See the <code>ordinaryChar</code> method
         * for more information on a character being ordinary.
         *</summary>
         */
        public void ResetSyntax()
        {
            for (int i = _ctype.Length; --i >= 0; )
                _ctype[i] = 0;
        }

        /**<summary>
         * Specifies that all characters <i>c</i> in the range
         * <code>low&nbsp;&lt;=&nbsp;<i>c</i>&nbsp;&lt;=&nbsp;high</code>
         * are word constituents. A word token consists of a word constituent
         * followed by zero or more word constituents or number constituents.
         *
         * <param name="low">   the low end of the range.</param>
         * <param name="hi">    the high end of the range.</param>
         * </summary>
         */
        public void WordChars(int low, int hi)
        {
            if (low < 0)
                low = 0;
            if (hi >= _ctype.Length)
                hi = _ctype.Length - 1;
            while (low <= hi)
                _ctype[low++] |= CT_ALPHA;
        }

        /**<summary>
         * Specifies that all characters <i>c</i> in the range
         * <code>low&nbsp;&lt;=&nbsp;<i>c</i>&nbsp;&lt;=&nbsp;high</code>
         * are white space characters. White space characters serve only to
         * separate tokens in the input stream.
         *
         * <p>Any other attribute settings for the characters in the specified
         * range are cleared.</p>
         *
         * <param name="low">   the low end of the range.</param>
         * <param name="hi">    the high end of the range.</param>
         * </summary>
         */
        public void WhitespaceChars(int low, int hi)
        {
            if (low < 0)
                low = 0;
            if (hi >= _ctype.Length)
                hi = _ctype.Length - 1;
            while (low <= hi)
                _ctype[low++] = CT_WHITESPACE;
        }

        /**<summary>
         * Specifies that all characters <i>c</i> in the range
         * <code>low&nbsp;&lt;=&nbsp;<i>c</i>&nbsp;&lt;=&nbsp;high</code>
         * are "ordinary" in this tokenizer. See the
         * <code>ordinaryChar</code> method for more information on a
         * character being ordinary.
         *
         * <param name="low">   the low end of the range.</param>
         * <param name="hi">    the high end of the range.</param>
         * </summary>
         */
        public void OrdinaryChars(int low, int hi)
        {
            if (low < 0)
                low = 0;
            if (hi >= _ctype.Length)
                hi = _ctype.Length - 1;
            while (low <= hi)
                _ctype[low++] = 0;
        }

        /**<summary>
         * Specifies that the character argument is "ordinary"
         * in this tokenizer. It removes any special significance the
         * character has as a comment character, word component, string
         * delimiter, white space, or number character. When such a character
         * is encountered by the parser, the parser treats it as a
         * single-character token and sets <code>ttype</code> field to the
         * character value.
         *
         * <p>Making a line terminator character "ordinary" may interfere
         * with the ability of a <code>StreamTokenizer</code> to count
         * lines. The <code>lineno</code> method may no longer reflect
         * the presence of such terminator characters in its line count.
         *</p>
         * <param name="ch"> the Character </param>
         * </summary>
         */
        public void OrdinaryChar(int ch)
        {
            if (ch >= 0 && ch < _ctype.Length)
                _ctype[ch] = 0;
        }

        /**<summary>
         * Specified that the character argument starts a single-line
         * comment. All characters from the comment character to the end of
         * the line are ignored by this stream tokenizer.
         *
         * <p>Any other attribute settings for the specified character are cleared.
         * </p>
         * <param name="ch">   the character. </param>
         * </summary>
         */
        public void CommentChar(int ch)
        {
            if (ch >= 0 && ch < _ctype.Length)
                _ctype[ch] = CT_COMMENT;
        }

        /**
         * <summary>
         * Specifies that matching pairs of this character delimit string
         * constants in this tokenizer.
         * <p>
         * When the <code>nextToken</code> method encounters a string
         * constant, the <code>ttype</code> field is set to the string
         * delimiter and the <code>sval</code> field is set to the body of
         * the string.</p>
         * <p>
         * If a string quote character is encountered, then a string is
         * recognized, consisting of all characters after (but not including)
         * the string quote character, up to (but not including) the next
         * occurrence of that same string quote character, or a line
         * terminator, or end of file. The usual escape sequences such as
         * <code>"&#92;n"</code> and <code>"&#92;t"</code> are recognized and
         * converted to single characters as the string is parsed.</p>
         *
         * <p>Any other attribute settings for the specified character are cleared.
         * </p>
         * <param name="ch"> the character </param>
         * </summary>
         */
        public void QuoteChar(int ch)
        {
            if (ch >= 0 && ch < _ctype.Length)
                _ctype[ch] = CT_QUOTE;
        }

        /**<summary>
         * Specifies that numbers should be parsed by this tokenizer. The
         * syntax table of this tokenizer is modified so that each of the twelve
         * characters:
         * <blockquote><pre>
         *      0 1 2 3 4 5 6 7 8 9 . -
         * </pre></blockquote>
         * <p>
         * has the "numeric" attribute.</p>
         * <p>
         * When the parser encounters a word token that has the format of a
         * double precision floating-point number, it treats the token as a
         * number rather than a word, by setting the <code>ttype</code>
         * field to the value <code>TT_NUMBER</code> and putting the numeric
         * value of the token into the <code>nval</code> field.</p>
         *</summary>
         */
        public void ParseNumbers()
        {
            for (int i = '0'; i <= '9'; i++)
                _ctype[i] |= CT_DIGIT;
            _ctype['.'] |= CT_DIGIT;
            _ctype['-'] |= CT_DIGIT;
        }

        /**<summary>
         * Determines whether or not ends of line are treated as tokens.
         * If the flag argument is true, this tokenizer treats end of lines
         * as tokens; the <code>nextToken</code> method returns
         * <code>TT_EOL</code> and also sets the <code>ttype</code> field to
         * this value when an end of line is read.
         * <p>
         * A line is a sequence of characters ending with either a
         * carriage-return character (<code>'&#92;r'</code>) or a newline
         * character (<code>'&#92;n'</code>). In addition, a carriage-return
         * character followed immediately by a newline character is treated
         * as a single end-of-line token.</p>
         * <p>
         * If the <code>flag</code> is false, end-of-line characters are
         * treated as white space and serve only to separate tokens.
         * </p>
         * <param name="flag">   <code>true</code> indicates that end-of-line characters
         *                 are separate tokens; <code>false</code> indicates that
         *                 end-of-line characters are white space.</param
         * </summary>
         */
        public void EolIsSignificant(bool flag)
        {
            _eolIsSignificantP = flag;
        }

        /**<summary>
         * Determines whether or not the tokenizer recognizes C-style comments.
         * If the flag argument is <code>true</code>, this stream tokenizer
         * recognizes C-style comments. All text between successive
         * occurrences of <code>/*</code> and <code>*&#47;</code> are discarded.
         * <p>
         * If the flag argument is <code>false</code>, then C-style comments
         * are not treated specially.</p>
         *
         * <param name="flag">  <code>true</code> indicates to recognize and ignore
         *                 C-style comments. </param>
         * </summary>
         */
        public void SlashStarComments(bool flag)
        {
            _slashStarCommentsP = flag;
        }

        /**<summary>
         * Determines whether or not the tokenizer recognizes C++-style comments.
         * If the flag argument is <code>true</code>, this stream tokenizer
         * recognizes C++-style comments. Any occurrence of two consecutive
         * slash characters (<code>'/'</code>) is treated as the beginning of
         * a comment that extends to the end of the line.
         * <p>
         * If the flag argument is <code>false</code>, then C++-style
         * comments are not treated specially.</p>
         *
         * <param name="flag">   <code>true</code> indicates to recognize and ignore
         *                 C++-style comments. </param>
         * </summary>
         */
        public void SlashSlashComments(bool flag)
        {
            _slashSlashCommentsP = flag;
        }

        /**<summary>
         * Determines whether or not word token are automatically lowercased.
         * If the flag argument is <code>true</code>, then the value in the
         * <code>sval</code> field is lowercased whenever a word token is
         * returned (the <code>ttype</code> field has the
         * value <code>TT_WORD</code> by the <code>nextToken</code> method
         * of this tokenizer.
         * <p>
         * If the flag argument is <code>false</code>, then the
         * <code>sval</code> field is not modified.
         * </p>
         * <param name="fl">   <code>true</code> indicates that all word tokens should
         *               be lowercased. </param>
         * </summary>
         */
        public void LowerCaseMode(bool fl)
        {
            _forceLower = fl;
        }

        /**<summary> Read the next character </summary>*/
        private int Read()
        {
            if (_reader != null)
                return _reader.Read();
            else
                throw new NotSupportedException();
        }

        /**<summary>
         * Parses the next token from the input stream of this tokenizer.
         * The type of the next token is returned in the <code>ttype</code>
         * field. Additional information about the token may be in the
         * <code>nval</code> field or the <code>sval</code> field of this
         * tokenizer.
         * <p>
         * Typical clients of this
         * class first set up the syntax tables and then sit in a loop
         * calling nextToken to parse successive tokens until TT_EOF
         * is returned.
         * </p>
         * <returns>    the value of the <code>ttype</code> field. </returns>
         * Exception:  IOException  if an I/O error occurs.
         * </summary>
         */
        public int NextToken()
        {
            if (_pushedBack)
            {
                _pushedBack = false;
                return _ttype;
            }
            byte[] ct = _ctype;
            _sval = null;

            int c = _peekc;
            if (c < 0)
                c = NEED_CHAR;
            if (c == SKIP_LF)
            {
                c = Read();
                if (c < 0)
                    return _ttype = TT_EOF;
                if (c == '\n')
                    c = NEED_CHAR;
            }
            if (c == NEED_CHAR)
            {
                c = Read();
                if (c < 0)
                    return _ttype = TT_EOF;
            }
            _ttype = c; /* Just to be safe */

            /* Set peekc so that the next invocation of nextToken will read
             * another character unless peekc is reset in this invocation
             */
            _peekc = NEED_CHAR;

            int ctype = c < 256 ? ct[c] : CT_ALPHA;
            while ((ctype & CT_WHITESPACE) != 0)
            {
                if (c == '\r')
                {
                    LINENO++;
                    if (_eolIsSignificantP)
                    {
                        _peekc = SKIP_LF;
                        return _ttype = TT_EOL;
                    }
                    c = Read();
                    if (c == '\n')
                        c = Read();
                }
                else
                {
                    if (c == '\n')
                    {
                        LINENO++;
                        if (_eolIsSignificantP)
                        {
                            return _ttype = TT_EOL;
                        }
                    }
                    c = Read();
                }
                if (c < 0)
                    return _ttype = TT_EOF;
                ctype = c < 256 ? ct[c] : CT_ALPHA;
            }

            if ((ctype & CT_DIGIT) != 0)
            {
                bool neg = false;
                if (c == '-')
                {
                    c = Read();
                    if (c != '.' && (c < '0' || c > '9'))
                    {
                        _peekc = c;
                        return _ttype = '-';
                    }
                    neg = true;
                }
                double v = 0;
                int decexp = 0;
                int seendot = 0;
                while (true)
                {
                    if (c == '.' && seendot == 0)
                        seendot = 1;
                    else if ('0' <= c && c <= '9')
                    {
                        v = v * 10 + (c - '0');
                        decexp += seendot;
                    }
                    else
                        break;
                    c = Read();
                }
                _peekc = c;
                if (decexp != 0)
                {
                    double denom = 10;
                    decexp--;
                    while (decexp > 0)
                    {
                        denom *= 10;
                        decexp--;
                    }
                    /* Do one division of a likely-to-be-more-accurate number */
                    v = v / denom;
                }
                _nval = neg ? -v : v;
                return _ttype = TT_NUMBER;
            }

            if ((ctype & CT_ALPHA) != 0)
            {
                int i = 0;
                do
                {
                    if (i >= _buf.Length)
                    {
                        char[] nb = new char[_buf.Length * 2];
                        Array.Copy(_buf, nb, _buf.Length);
                        _buf = nb;
                    }
                    _buf[i++] = (char)c;
                    c = Read();
                    ctype = c < 0 ? CT_WHITESPACE : c < 256 ? ct[c] : CT_ALPHA;
                } while ((ctype & (CT_ALPHA | CT_DIGIT)) != 0);
                _peekc = c;
                _sval = CopyValueOf(_buf, 0, i);
                if (_forceLower)
                    _sval = _sval.ToLower();
                return _ttype = TT_WORD;
            }

            if ((ctype & CT_QUOTE) != 0)
            {
                _ttype = c;
                int i = 0;
                /* Invariants (because \Octal needs a lookahead):
                 *   (i)  c contains char value
                 *   (ii) d contains the lookahead
                 */
                int d = Read();
                while (d >= 0 && d != _ttype && d != '\n' && d != '\r')
                {
                    if (d == '\\')
                    {
                        c = Read();
                        int first = c;   /* To allow \377, but not \477 */
                        if (c >= '0' && c <= '7')
                        {
                            c = c - '0';
                            int c2 = Read();
                            if ('0' <= c2 && c2 <= '7')
                            {
                                c = (c << 3) + (c2 - '0');
                                c2 = Read();
                                if ('0' <= c2 && c2 <= '7' && first <= '3')
                                {
                                    c = (c << 3) + (c2 - '0');
                                    d = Read();
                                }
                                else
                                    d = c2;
                            }
                            else
                                d = c2;
                        }
                        else
                        {
                            switch (c)
                            {
                                case 'a':
                                    c = 0x7;
                                    break;
                                case 'b':
                                    c = '\b';
                                    break;
                                case 'f':
                                    c = 0xC;
                                    break;
                                case 'n':
                                    c = '\n';
                                    break;
                                case 'r':
                                    c = '\r';
                                    break;
                                case 't':
                                    c = '\t';
                                    break;
                                case 'v':
                                    c = 0xB;
                                    break;
                            }
                            d = Read();
                        }
                    }
                    else
                    {
                        c = d;
                        d = Read();
                    }
                    if (i >= _buf.Length)
                    {
                        char[] nb = new char[_buf.Length * 2];
                        Array.Copy(_buf, nb, _buf.Length);
                        _buf = nb;
                    }
                    _buf[i++] = (char)c;
                }

                /* If we broke out of the loop because we found a matching quote
                 * character then arrange to read a new character next time
                 * around; otherwise, save the character.
                 */
                _peekc = (d == _ttype) ? NEED_CHAR : d;

                _sval = CopyValueOf(_buf, 0, i);
                return _ttype;
            }

            if (c == '/' && (_slashSlashCommentsP || _slashStarCommentsP))
            {
                c = Read();
                if (c == '*' && _slashStarCommentsP)
                {
                    int prevc = 0;
                    while ((c = Read()) != '/' || prevc != '*')
                    {
                        if (c == '\r')
                        {
                            LINENO++;
                            c = Read();
                            if (c == '\n')
                            {
                                c = Read();
                            }
                        }
                        else
                        {
                            if (c == '\n')
                            {
                                LINENO++;
                                c = Read();
                            }
                        }
                        if (c < 0)
                            return _ttype = TT_EOF;
                        prevc = c;
                    }
                    return NextToken();
                }
                else if (c == '/' && _slashSlashCommentsP)
                {
                    while ((c = Read()) != '\n' && c != '\r' && c >= 0) ;
                    _peekc = c;
                    return NextToken();
                }
                else
                {
                    /* Now see if it is still a single line comment */
                    if ((ct['/'] & CT_COMMENT) != 0)
                    {
                        while ((c = Read()) != '\n' && c != '\r' && c >= 0) ;
                        _peekc = c;
                        return NextToken();
                    }
                    else
                    {
                        _peekc = c;
                        return _ttype = '/';
                    }
                }
            }

            if ((ctype & CT_COMMENT) != 0)
            {
                while ((c = Read()) != '\n' && c != '\r' && c >= 0) ;
                _peekc = c;
                return NextToken();
            }

            return _ttype = c;
        }

        /**<summary>
         * Causes the next call to the <code>nextToken</code> method of this
         * tokenizer to return the current value in the <code>ttype</code>
         * field, and not to modify the value in the <code>nval</code> or
         * <code>sval</code> field.
         *</summary>
         */
        public void PushBack()
        {
            if (_ttype != TT_NOTHING)   /* No-op if nextToken() not called */
                _pushedBack = true;
        }

        /**<summary>
         * Return the current line number.
         *
         * <returns>  the current line number of this stream tokenizer.</returns>
         * </summary>
         */
        public int Lineno
        {
            get { return LINENO; }
        }

        /**<summary>
         * Returns the string representation of the current stream token and
         * the line number it occurs on.
         *
         * <p>The precise string returned is unspecified, although the following
         * example can be considered typical:
         * </p>
         * <blockquote><pre>Token['a'], line 10</pre></blockquote>
         *
         * <returns>  a string representation of the token </returns>
         * </summary>
         */
        public override string ToString()
        {
            string ret;
            switch (_ttype)
            {
                case TT_EOF:
                    ret = "EOF";
                    break;
                case TT_EOL:
                    ret = "EOL";
                    break;
                case TT_WORD:
                    ret = _sval;
                    break;
                case TT_NUMBER:
                    ret = "n=" + _nval;
                    break;
                case TT_NOTHING:
                    ret = "NOTHING";
                    break;
                default:
                    {
                        /*
                         * ttype is the first character of either a quoted string or
                         * is an ordinary character. ttype can definitely not be less
                         * than 0, since those are reserved values used in the previous
                         * case statements
                         */
                        if (_ttype < 256 && ((_ctype[_ttype] & CT_QUOTE) != 0))
                        {
                            ret = _sval;
                            break;
                        }

                        char[] s = new char[3];
                        s[0] = s[2] = '\'';
                        s[1] = (char)_ttype;
                        ret = new string(s);
                        break;
                    }
            }
            return "Token[" + ret + "], line " + LINENO;
        }

        /**<summary>
         * Returns a String that represents the character sequence in the
         * array specified.
         *
         * <param name="data">     the character array.</param>
         * <param name="offset">   initial offset of the subarray.</param>
         * <param name="count">    length of the subarray.</param>
         * <returns>  a <code>String</code> that contains the characters of the
         *          specified subarray of the character array.</returns>
         * </summary>
         */
        public static string CopyValueOf(char[] data, int offset, int count)
        {
            // All public String constructors now copy the data.
            return new String(data, offset, count);
        }
    }
}
#endif
