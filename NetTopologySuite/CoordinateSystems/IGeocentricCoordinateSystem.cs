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
	/// A 3D coordinate system, with its origin at the center of the Earth.
	/// </summary>
	/// <remarks>
	/// The X axis points towards the prime meridian. The Y axis points East
	/// or West. The Z axis points North or South. By default the Z axis will
	/// point North, and the Y axis will point East (e.g. a right handed
	/// system), but you should check the axes for non-default values.
	/// </remarks>
	public interface IGeocentricCoordinateSystem : ICoordinateSystem
	{
		/// <summary>
		/// Returns the HorizontalDatum.
		/// </summary>
		/// <remarks>
		/// The horizontal datum is used to determine where the center of the Earth
		/// is considered to be. All coordinate points will be measured from the
		/// center of the Earth, and not the surface.
		/// </remarks>
		IHorizontalDatum HorizontalDatum { get; }

		/// <summary>
		/// Gets the units used along all the axes.
		/// </summary>
		ILinearUnit LinearUnit { get; }

		/// <summary>
		/// Returns the PrimeMeridian.
		/// </summary>
		IPrimeMeridian PrimeMeridian { get; }
	
	}
}
