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

using System;
using System.Runtime.Serialization;

namespace GisSharpBlog.NetTopologySuite.CoordinateTransformations
{
	/// <summary>
	/// The exception that is thrown when a non-fatal application error occurs related to projection functionality.
	/// </summary>
	public	 class TransformException : System.ApplicationException
	{
		/// <summary>
		/// Initializes a new instance of the TransformException class.
		/// </summary>
		public TransformException() : base()
		{
		}

		/// <summary>
		/// Initializes a new instance of the TransformException class with a specified error message.
		/// </summary>
		/// <param name="message">A message that describes the error. </param>
		public TransformException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the ApplicationException class with serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data. </param>
		/// <param name="context">The contextual information about the source or destination.</param>
		public TransformException(SerializationInfo info,StreamingContext context) : base(info, context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the ApplicationException class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception</param>
		public TransformException(string message,	Exception innerException) : base(message, innerException)
		{
		}
	}
}
