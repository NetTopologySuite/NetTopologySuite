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
using GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems;
#endregion

namespace GisSharpBlog.NetTopologySuite.CoordinateTransformations
{
	
	/// <summary>
	/// Creates coordinate transformations.
	/// </summary>
	public interface ICoordinateTransformationFactory 
	{
		/// <summary>
		/// Creates a transformation between two coordinate systems.
		/// </summary>
		/// <remarks>
		/// This method will examine the coordinate systems in order to
		/// construct a transformation between them. This method may fail if no
		/// path between the coordinate systems is found, using the normal failing
		/// behavior of the DCP (e.g. throwing an exception).
		/// </remarks>
		/// <param name="sourceCS">Input coordinate system.</param>
		/// <param name="targetCS">Output coordinate system.</param>
		ICoordinateTransformation CreateFromCoordinateSystems(ICoordinateSystem sourceCS, ICoordinateSystem targetCS);
	}

}
