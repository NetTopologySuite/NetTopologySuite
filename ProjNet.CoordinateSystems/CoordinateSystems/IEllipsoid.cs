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
	/// The IEllipsoid interface defines the standard information stored with ellipsoid objects.
	/// </summary>
	public interface IEllipsoid : IInfo
	{
		/// <summary>
		/// Gets or sets the value of the semi-major axis.
		/// </summary>
		double SemiMajorAxis { get; set; }
		/// <summary>
		/// Gets or sets the value of the semi-minor axis.
		/// </summary>
		double SemiMinorAxis { get; set; }
		/// <summary>
		/// Gets or sets the value of the inverse of the flattening constant of the ellipsoid.
		/// </summary>
		double InverseFlattening { get; set; }
		/// <summary>
		/// Gets or sets the value of the axis unit.
		/// </summary>
		ILinearUnit AxisUnit { get; set; }
		/// <summary>
		/// Is the Inverse Flattening definitive for this ellipsoid? Some ellipsoids use the
		/// IVF as the defining value, and calculate the polar radius whenever asked. Other
		/// ellipsoids use the polar radius to calculate the IVF whenever asked. This
		/// distinction can be important to avoid floating-point rounding errors.
		/// </summary>
		bool IsIvfDefinitive { get; set; }
	}
}
