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
	/// Orientation of axis. Some coordinate systems use non-standard orientations. 
	/// For example, the first axis in South African grids usually points West, 
	/// instead of East. This information is obviously relevant for algorithms
	/// converting South African grid coordinates into Lat/Long.
	/// </summary>
	public enum AxisOrientationEnum : short
	{
		/// <summary>
		/// Unknown or unspecified axis orientation. This can be used for local or fitted coordinate systems.
		/// </summary>
		Other = 0,
		/// <summary>
		/// Increasing ordinates values go North. This is usually used for Grid Y coordinates and Latitude.
		/// </summary>
		North = 1,
		/// <summary>
		/// Increasing ordinates values go South. This is rarely used.
		/// </summary>
		South = 2,
		/// <summary>
		/// Increasing ordinates values go East. This is rarely used.
		/// </summary>
		East = 3,
		/// <summary>
		/// Increasing ordinates values go West. This is usually used for Grid X coordinates and Longitude.
		/// </summary>
		West = 4,
		/// <summary>
		/// Increasing ordinates values go up. This is used for vertical coordinate systems.
		/// </summary>
		Up = 5,
		/// <summary>
		/// Increasing ordinates values go down. This is used for vertical coordinate systems.
		/// </summary>
		Down = 6
	}
}
