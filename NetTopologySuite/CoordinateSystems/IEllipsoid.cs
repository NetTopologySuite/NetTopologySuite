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
	/// An approximation of the Earth's surface as a squashed sphere.
	/// </summary>
	public interface IEllipsoid : IInfo
	{
		/// <summary>
		/// Is the Inverse Flattening definitive for this ellipsoid?
		/// </summary>
		/// <remarks>
		/// Some ellipsoids use the IVF as the defining value, and calculate the
		/// polar radius whenever asked. Other ellipsoids use the polar radius to
		/// calculate the IVF whenever asked. This distinction can be important to
		/// avoid floating-point rounding errors.
		/// </remarks>
		/// <returns></returns>
		bool IsIvfDefinitive();

		/// <summary>
		///  The units of the semi-major and semi-smaller axis values.
		/// </summary>
		ILinearUnit AxisUnit { get; }

		/// <summary>
		/// Returns the value of the inverse of the flattening constant.
		/// </summary>
		/// <remarks>
		/// The inverse flattening is related to the equatorial/polar radius
		/// by the formula ivf=re/(re-rp). For perfect spheres, this formula
		/// breaks down, and a special IVF value of zero is used.
		/// </remarks>
		double InverseFlattening { get; }

		/// <summary>
		///  The returned length is expressed in this object's axis units.
		/// </summary>
		double SemiMajorAxis { get; }

		/// <summary>
		/// The returned length is expressed in this object's axis units.
		/// </summary>
		double SemiMinorAxis { get; }
		
	}
}
