using System;
using System.Runtime.Serialization;

namespace GisSharpBlog.NetTopologySuite.IO
{
	/// <summary>
	/// The exception that is thrown when a non-fatal application error occurs related to Topology functionality.
	/// </summary>
	internal class ShapefileException : ApplicationException
	{
		/// <summary>
		/// Initializes a new instance of the ShapefileException class.
		/// </summary>
		public ShapefileException() { }

		/// <summary>
		/// Initializes a new instance of the ShapefileException class with a specified error message.
		/// </summary>
		/// <param name="message">A message that describes the error. </param>
		public ShapefileException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the ApplicationException class with serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data. </param>
		/// <param name="context">The contextual information about the source or destination.</param>
		public ShapefileException(SerializationInfo info,StreamingContext context) : base(info, context) { }

		/// <summary>
		/// Initializes a new instance of the ApplicationException class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception</param>
		public ShapefileException(string message, Exception innerException) : base(message, innerException) { }
	}
}
