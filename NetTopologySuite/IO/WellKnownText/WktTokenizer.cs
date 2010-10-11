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

#region Usings

using System;
using System.Globalization;
using System.IO;

#endregion

namespace GeoAPI.IO.WellKnownText
{
    /// <summary>
    /// Reads a stream of Well Known Text (WKT) and returns a 
    /// stream of tokens.
    /// </summary>
    internal class WktTokenizer : TextTokenizer
    {
        internal static readonly NumberFormatInfo NumberFormat_enUS = new CultureInfo("en-US", false).NumberFormat;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the WktStreamTokenizer class.
        /// </summary>
        /// <remarks>The WktStreamTokenizer class ais in reading WKT streams.</remarks>
        /// <param name="reader">A TextReader that contains </param>
        public WktTokenizer(TextReader reader)
            : base(reader, true) { }

        #endregion

        #region Methods

        /// <summary>
        /// Reads a token and checks it is what is expected.
        /// </summary>
        /// <param name="expectedToken">The expected token.</param>
        internal void ReadToken(String expectedToken)
        {
            Read();

            if (CurrentToken != expectedToken)
            {
                throw new ParseException(String.Format(NumberFormat_enUS,
                                                       "Expecting ('{3}') but got a '{0}' at line {1} column {2}.",
                                                       CurrentToken, LineNumber, Column, expectedToken));
            }
        }

        /// <summary>
        /// Reads a string inside double quotes.
        /// </summary>
        /// <remarks>
        /// White space inside quotes is preserved.
        /// </remarks>
        /// <returns>The string inside the double quotes.</returns>
        public String ReadDoubleQuotedWord()
        {
            String word = String.Empty;
            ReadToken("\"");
            Read(false);

            while (CurrentToken != "\"")
            {
                word += CurrentToken;
                Read(false);
            }

            return word;
        }

        /// <summary>
        /// Reads the authority and authority code.
        /// </summary>
        /// <param name="authority">String to place the authority in.</param>
        /// <param name="authorityCode">Int64 to place the authority code in.</param>
        public void ReadAuthority(ref String authority, ref String authorityCode)
        {
            // AUTHORITY["EPGS","9102"]]
            if (CurrentToken != "AUTHORITY")
            {
                ReadToken("AUTHORITY");
            }

            ReadToken("[");
            authority = ReadDoubleQuotedWord();
            ReadToken(",");
            authorityCode = ReadDoubleQuotedWord();
            ReadToken("]");
        }

        #endregion
    }
}