// Copyright 2005 - 2009 - Morten Nielsen (www.sharpgis.net)
//
// This file is part of ProjNet.
// ProjNet is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// ProjNet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with ProjNet; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Text;

namespace ProjNet.CoordinateSystems
{
	/// <summary>
	/// The IGeographicCoordinateSystem interface is a subclass of IGeodeticSpatialReference and
	/// defines the standard information stored with geographic coordinate system objects.
	/// </summary>
	public interface IGeographicCoordinateSystem : IHorizontalCoordinateSystem
	{
		/// <summary>
		/// Gets or sets the angular units of the geographic coordinate system.
		/// </summary>
		IAngularUnit AngularUnit { get; set; }
		/// <summary>
		/// Gets or sets the prime meridian of the geographic coordinate system.
		/// </summary>
		IPrimeMeridian PrimeMeridian { get; set; }
		/// <summary>
		/// Gets the number of available conversions to WGS84 coordinates.
		/// </summary>
		int NumConversionToWGS84 { get; }
		/// <summary>
		/// Gets details on a conversion to WGS84.
		/// </summary>
		Wgs84ConversionInfo GetWgs84ConversionInfo(int index);
	}    
}
