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

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// Gets number of parameters of the projection.
	/// </summary>
	public interface IProjection : IInfo
	{
		/// <summary>
		/// Gets the projection classification name (e.g. 'Transverse_Mercator').
		/// </summary>
		string ClassName { get; }

		/// <summary>
		/// Gets number of parameters of the projection. 
		/// </summary>
		int NumParameters { get; }

		/// <summary>
		/// Gets an indexed parameter of the projection.
		/// </summary>
		/// <param name="Index">Zero based index of parameter to fetch.</param>
		ProjectionParameter GetParameter(int Index);
	}
}
