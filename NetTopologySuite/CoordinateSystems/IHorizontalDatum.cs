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
	/// Procedure used to measure positions on the surface of the Earth.
	/// </summary>
	public interface IHorizontalDatum : IDatum
	{
		/// <summary>
		/// Returns the Ellipsoid.
		/// </summary>
		IEllipsoid Ellipsoid { get; }

		/// <summary>
		/// Gets preferred parameters for a Bursa Wolf transformation into WGS84. 
		/// </summary>
		/// <remarks>
		/// The 7 returned values correspond to (dx,dy,dz) in meters, (ex,ey,ez)
		/// in arc-seconds, and scaling in parts-per-million.
		/// This method will always fail for horizontal datums with type CS_HD_Other.
		/// This method may also fail if no suitable transformation is available.
		/// Failures are indicated using the normal failing behavior of the DCP
		/// (e.g. throwing an exception).
		/// </remarks>
		WGS84ConversionInfo WGS84Parameters { get; }
		
	}
}
