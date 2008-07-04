using System;

namespace GisSharpBlog.NetTopologySuite.IO
{
    /// <summary>  
    /// Thrown by a <c>WKTReader</c> when a parsing problem occurs.
    /// </summary>
    public class ParseException : ApplicationException 
    {
        /// <summary>
        /// Creates a <c>ParseException</c> with the given detail message.
        /// </summary>
        /// <param name="message">A description of this <c>ParseException</c>.</param>
        public ParseException(String message) : base(message) { }

        /// <summary>  
        /// Creates a <c>ParseException</c> with <c>e</c>s detail message.
        /// </summary>
        /// <param name="e">An exception that occurred while a <c>WKTReader</c> was
        /// parsing a Well-known Text string.</param>
        public ParseException(Exception e) : this(e.ToString()) { }
    }
}
