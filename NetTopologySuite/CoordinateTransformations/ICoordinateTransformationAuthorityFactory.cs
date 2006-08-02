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
#endregion

namespace GisSharpBlog.NetTopologySuite.CoordinateTransformations
{
	/// <summary>
	/// Creates coordinate transformation objects from codes
	/// </summary>
	/// <remarks>
	/// The codes are maintained by an external authority.
	/// A commonly used authority is EPSG, which is also used in the GeoTIFF
	/// standard.
	/// </remarks>
	public interface ICoordinateTransformationAuthorityFactory 
	{
	
		/// <summary>
		/// The name of the authority.
		/// </summary>
		string Authority{get;}

		/// <summary>
		/// Creates a transformation from a single transformation code. 
		/// </summary>
		/// <remarks>
		/// The 'Authority' and 'AuthorityCode' values of the created object will be set
		/// to the authority of this object, and the code specified by the client,
		/// respectively.  The other metadata values may or may not be set.
		/// </remarks>
		/// <param name="code">param code Coded value for transformation.</param>
		ICoordinateTransformation CreateFromTransformationCode(string code);
		
		/// <summary>
		/// Creates a transformation from coordinate system codes.
		/// </summary>
		/// <param name="sourceCode">Coded value of source coordinate system.</param>
		/// <param name="targetCode">Coded value of target coordinate system.</param>
		ICoordinateTransformation CreateFromCoordinateSystemCodes(string sourceCode,string targetCode);
	}

}
