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
	/// A coordinate system based on latitude and longitude.
	/// </summary>
	/// <remarks>
	///  Some geographic coordinate systems are Lat/Lon, and some are Lon/Lat.
	///  You can find out which this is by examining the axes.  You should also
	///  check the angular units, since not all geographic coordinate systems use
	///  degrees.
	/// </remarks>
	public interface IGeographicCoordinateSystem : IHorizontalCoordinateSystem
	{
		/// <summary>
		/// Returns the AngularUnit.
		/// </summary>
		/// <remarks>
		/// The angular unit must be the same as the CS_CoordinateSystem units.
		/// </remarks>
		IAngularUnit AngularUnit { get; }
	
		/// <summary>
		/// Gets the number of available conversions to WGS84 coordinates.
		/// </summary>
		int NumConversionToWGS84 { get; }

		/// <summary>
		/// Returns the PrimeMeridian.
		/// </summary>
		IPrimeMeridian PrimeMeridian { get; }
		
		/// <summary>
		/// Gets details on a conversion to WGS84.
		/// </summary>
		/// <remarks>
		/// Some geographic coordinate systems provide several transformations
		/// into WGS84, which are designed to provide good accuracy in different
		/// areas of interest.  The first conversion (with index=0) should
		/// provide acceptable accuracy over the largest possible area of
		/// interest.
		/// </remarks>
		/// <param name="Index">Zero based index of conversion to fetch.</param>
		WGS84ConversionInfo GetWGS84ConversionInfo(int Index) ;
	}
}
