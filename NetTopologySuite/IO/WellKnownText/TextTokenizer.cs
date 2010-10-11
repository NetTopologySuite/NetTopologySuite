// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
//
// This file is part of GeoAPI.Net.
// GeoAPI.Net is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// GeoAPI.Net is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with GeoAPI.Net; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

// SOURCECODE IS MODIFIED FROM ANOTHER WORK AND IS ORIGINALLY BASED ON GeoTools.NET:
/*
 *  Copyright (C) 2002 Urban Science Applications, Inc. 
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 */

#region Using

using System;
using System.Globalization;
using System.IO;
using System.Text;

#endregion

// http://java.sun.com/j2se/1.4/docs/api/java/io/StreamTokenizer.html
// a better implementation could be written. Here is a good Java implementation of StreamTokenizer.
//   http://www.flex-compiler.lcs.mit.edu/Harpoon/srcdoc/java/io/StreamTokenizer.html
// a C# StringTokenizer
//  http://sourceforge.net/snippet/detail.php?type=snippet&id=101171

namespace GeoAPI.IO.WellKnownText
{
    /// <summary>
    /// Parses input character data into tokens.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The TextTokenizer class takes an input character stream and parses 
    /// it into "tokens", allowing the tokens to be read one at a time. 
    /// The parsing process is controlled by a table and a number of flags 
    /// that can be set to various states. The stream tokenizer can recognize 
    /// identifiers, numbers, quoted strings, and various comment styles.
    /// </para>
    /// </remarks>
    internal class TextTokenizer
    {
        private static readonly NumberFormatInfo NumberFormat_EnUS = new CultureInfo("en-US", false).NumberFormat;

        private readonly TextReader _reader;
        private readonly Encoding _encoding;
        private TokenType _currentTokenType;
        private TokenType _nextTokenType = TokenType.Bof;
        private String _currentToken;
        private readonly StringBuilder _nextToken = new StringBuilder();
        private readonly Char[] _activeChar = new Char[1];
        private readonly Byte[] _peekCharBytes = new Byte[4];
        private readonly Char[] _peekChar = new Char[1];
        private readonly Boolean _ignoreWhitespace;
        private Int32 _lineNumber = 1;
        private Int32 _colNumber = 1;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TextTokenizer"/> class.
        /// </summary>
        /// <param name="reader">A <see cref="TextReader"/> with text data to tokenize.</param>
        /// <param name="ignoreWhitespace">Flag indicating whether whitespace should be ignored.</param>
        public TextTokenizer(TextReader reader, Boolean ignoreWhitespace)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            _reader = reader;
            _ignoreWhitespace = ignoreWhitespace;

            StreamReader streamReader = reader as StreamReader;

            if (streamReader != null)
            {
                _encoding = streamReader.CurrentEncoding;
            }

            advanceToken();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current line number of the stream being read.
        /// </summary>
        public Int32 LineNumber
        {
            get { return _lineNumber; }
        }

        /// <summary>
        /// Gets the current column number of the stream being read.
        /// </summary>
        public Int32 Column
        {
            get { return _colNumber; }
        }

        public TokenType CurrentTokenType
        {
            get { return _currentTokenType; }
        }

        public TokenType NextTokenType
        {
            get { return _nextTokenType; }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// If the current token is a number, this field contains the value 
        /// of that number. 
        /// </summary>
        /// <remarks>
        /// If the current token is a number, this field contains the value 
        /// of that number. The current token is a number when the value of 
        /// <see cref="CurrentTokenType"/> is <see cref="TokenType.Number"/>.
        /// </remarks>
        /// <exception cref="FormatException">
        /// Current token is not a number in a valid format.
        /// </exception>
        public Double CurrentTokenAsNumber
        {
            get
            {
                String number = CurrentToken;

                if (CurrentTokenType == TokenType.Number)
                {
                    return Double.Parse(number, NumberFormat_EnUS);
                }

                throw new FormatException(String.Format(NumberFormat_EnUS,
                                                  "The token '{0}' is not a number at line {1} column {2}.", number,
                                                  LineNumber, Column));
            }
        }

        /// <summary>
        /// If the current token is a word token, this field contains a 
        /// String giving the characters of the word token. 
        /// </summary>
        public String CurrentToken
        {
            get
            {
                return _currentToken;
            }
        }

        public String NextToken
        {
            get
            {
                return _nextToken.ToString();
            }
        }

        /// <summary>
        /// Returns the <see cref="TokenType"/> of the next token.
        /// </summary>
        /// <param name="ignoreWhitespace">
        /// Determines is whitespace is ignored. True if whitespace is to be ignored.
        /// </param>
        /// <returns>The TokenType of the next token.</returns>
        public TokenType Read(Boolean ignoreWhitespace)
        {
            advanceToken(ignoreWhitespace);

            return _currentTokenType;
        }

        /// <summary>
        /// Returns the next token type.
        /// </summary>
        /// <returns>The TokenType of the next token.</returns>
        public TokenType Read()
        {
            return Read(_ignoreWhitespace);
        }
        #endregion

        #region Private helper methods

        /// <summary>
        /// Returns next token that is not whitespace.
        /// </summary>
        /// <returns></returns>
        private void advanceToken()
        {
            advanceToken(_ignoreWhitespace);
        }

        private void advanceToken(Boolean ignoreWhitespace)
        {
            _currentToken = _nextToken.ToString();
            _currentTokenType = _nextTokenType;

            _nextToken.Length = 0;
            _nextTokenType = TokenType.Eof;

            Int32 finished = _reader.Read(_activeChar, 0, 1);
            Boolean isNumber = false;
            Boolean isWord = false;
            //ASCIIEncoding encoding = new ASCIIEncoding();

            while (finished != 0)
            {
                Char activeCharacter;
                Char? nextCharacter;
                TokenType nextCharacterType;

                activeCharacter = _activeChar[0];
                Int32 nextValue = _reader.Peek();
                nextCharacter = getChar(nextValue);
                _nextTokenType = getType(activeCharacter);
                nextCharacterType = getType(nextCharacter);

                // handling of words with _
                if (isWord && activeCharacter == '_')
                {
                    _nextTokenType = TokenType.Word;
                }

                // handing of words ending in numbers
                if (isWord && _nextTokenType == TokenType.Number)
                {
                    _nextTokenType = TokenType.Word;
                }

                if (_nextTokenType == TokenType.Word && nextCharacter == '_')
                {
                    //enable words with _ inbetween
                    nextCharacterType = TokenType.Word;
                    isWord = true;
                }

                if (_nextTokenType == TokenType.Word && nextCharacterType == TokenType.Number)
                {
                    //enable words ending with numbers
                    nextCharacterType = TokenType.Word;
                    isWord = true;
                }

                // handle negative numbers
                if (activeCharacter == '-' && nextCharacterType == TokenType.Number && isNumber == false)
                {
                    _nextTokenType = TokenType.Number;
                    nextCharacterType = TokenType.Number;
                    //isNumber = true;
                }

                // this handles numbers with a decimal point
                if (isNumber && nextCharacterType == TokenType.Number && activeCharacter == '.')
                {
                    _nextTokenType = TokenType.Number;
                }

                if (_nextTokenType == TokenType.Number && nextCharacter == '.' && isNumber == false)
                {
                    nextCharacterType = TokenType.Number;
                    isNumber = true;
                }

                _colNumber++;

                if (_nextTokenType == TokenType.Eol)
                {
                    _lineNumber++;
                    _colNumber = 1;
                }


                Boolean ignore = ignoreWhitespace &&
                    _nextTokenType == TokenType.Whitespace || _nextTokenType == TokenType.Eol;

                if (!ignore)
                {
                    _nextToken.Append(activeCharacter);
                }

                //if (_nextTokenType==TokenType.Word && nextCharacter=='_')
                //{
                // enable words with _ inbetween
                //	finished = _reader.Read(chars,0,1);
                //}

                if (!ignore && _nextTokenType != nextCharacterType)
                {
                    finished = 0;
                }
                else if (!ignore && _nextTokenType == TokenType.Symbol && activeCharacter != '-')
                {
                    finished = 0;
                }
                else
                {
                    finished = _reader.Read(_activeChar, 0, 1);
                }
            }
        }

        private Char? getChar(Int32 value)
        {
            Char c;

            if (value < 0)
            {
                return null;
            }

            if (_encoding == null)
            {
                c = Convert.ToChar(value);
            }
            else
            {
                _peekCharBytes[0] = (Byte)(0x000000ff & value);
                _peekCharBytes[1] = (Byte)((0x0000ff00 & value) >> 8);
                _peekCharBytes[2] = (Byte)((0x00ff0000 & value) >> 16);
                _peekCharBytes[3] = (Byte)((0xff000000 & value) >> 24);
                _encoding.GetChars(_peekCharBytes, 0, 4, _peekChar, 0);

                c = _peekChar[0];
            }

            return c;
        }

        /// <summary>
        /// Determines a characters type (e.g. number, symbols, character).
        /// </summary>
        /// <param name="character">The character to determine.</param>
        /// <returns>The TokenType the character is.</returns>
        private static TokenType getType(Char? character)
        {
            if(character == null)
            {
                return TokenType.Eof;
            }
            else if (Char.IsDigit((Char)character))
            {
                return TokenType.Number;
            }
            else if (Char.IsLetter((Char)character))
            {
                return TokenType.Word;
            }
            else if (character == '\n')
            {
                return TokenType.Eol;
            }
            else if (Char.IsWhiteSpace((Char)character) || Char.IsControl((Char)character))
            {
                return TokenType.Whitespace;
            }
            else
            {
                return TokenType.Symbol;
            }
        }

        #endregion
    }
}