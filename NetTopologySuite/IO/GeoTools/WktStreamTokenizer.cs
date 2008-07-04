using System;
using System.IO;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.IO
{
	/// <summary>
	/// Reads a stream of Well Known Text (wkt) string and returns a stream of tokens.
	/// </summary>
    [Obsolete("This class is only for GeoTools.NET code compatibility: use WKT Reader for read WKT Streams.")]
	public class WktStreamTokenizer : GeoToolsStreamTokenizer
	{
		/// <summary>
		/// Initializes a new instance of the WktStreamTokenizer class.
		/// </summary>
		/// <remarks>The WktStreamTokenizer class ais in reading WKT streams.</remarks>
		/// <param name="reader">A TextReader that contains WKT.</param>
		public WktStreamTokenizer(TextReader reader) : base(reader, true)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");
		}

		/// <summary>
		/// Reads a token and checks it is what is expected.
		/// </summary>
		/// <param name="expectedToken">The expected token.</param>
		/// <exception cref="ParseException">If the token is not what is expected.</exception>
		public void ReadToken(string expectedToken)
		{
			this.NextToken();
			if (this.GetStringValue() != expectedToken)
				throw new ParseException(String.Format("Expecting comma ('{3}') but got a '{0}' at line {1} column {2}.",
                    this.GetStringValue(), this.LineNumber, this.Column, expectedToken));
		}
		
		/// <summary>
		/// Reads a string inside double quotes.
		/// </summary>
		/// <remarks>
		/// White space inside quotes is preserved.
		/// </remarks>
		/// <returns>The string inside the double quotes.</returns>
		public string ReadDoubleQuotedWord()
		{
			string word = String.Empty;
			ReadToken("\"");	
			NextToken(false);
			// while (GetStringValue() != String.Empty)
            while (GetStringValue() != String.Empty && GetStringValue() != "\"") // monoGIS-paul42 fix
			{
				word = word + GetStringValue();
				NextToken(false);
			} 
			return word;
		}

		/// <summary>
		/// Reads the authority and authority code.
		/// </summary>
		/// <param name="authority">String to place the authority in.</param>
		/// <param name="authorityCode">String to place the authority code in.</param>
		public void ReadAuthority(ref string authority,ref string authorityCode)
		{
			//AUTHORITY["EPSG","9102"]]
			ReadToken("AUTHORITY");
			ReadToken("[");
			authority = this.ReadDoubleQuotedWord();
			ReadToken(",");
			authorityCode = this.ReadDoubleQuotedWord();
			ReadToken("]");
		}
	}
}
