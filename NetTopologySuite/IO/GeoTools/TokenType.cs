using System;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
	/// <summary>
	/// Represents the type of token created by the StreamTokenizer class.
	/// </summary>
    [Obsolete("This enum is only for GeoTools.NET code compatibility.")]
	public enum TokenType
	{
		/// <summary>
		/// Indicates that the token is a word.
		/// </summary>
		Word,

		/// <summary>
		/// Indicates that the token is a number. 
		/// </summary>
		Number,

		/// <summary>
		/// Indicates that the end of line has been read. The field can only have this value if the eolIsSignificant method has been called with the argument true. 
		/// </summary>
		Eol,

		/// <summary>
		/// Indicates that the end of the input stream has been reached.
		/// </summary>
		Eof,

		/// <summary>
		/// Indictaes that the token is white space (space, tab, newline).
		/// </summary>
		Whitespace,

		/// <summary>
		/// Characters that are not whitespace, numbers, etc...
		/// </summary>
		Symbol
	}
}
