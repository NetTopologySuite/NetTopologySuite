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
	/// The IGeographicTransform interface is implemented on geographic transformation
	/// objects and implements datum transformations between geographic coordinate systems.
	/// </summary>
	public interface IGeographicTransform : IInfo
	{
		/// <summary>
		/// Gets or sets source geographic coordinate system for the transformation.
		/// </summary>
		IGeographicCoordinateSystem SourceGCS { get; set; }

		/// <summary>
		/// Gets or sets the target geographic coordinate system for the transformation.
		/// </summary>
		IGeographicCoordinateSystem TargetGCS { get; set; }

		/// <summary>
		/// Returns an accessor interface to the parameters for this geographic transformation.
		/// </summary>
		IParameterInfo ParameterInfo { get; }

		/// <summary>
		/// Transforms an array of points from the source geographic coordinate system
		/// to the target geographic coordinate system.
		/// </summary>
		/// <param name="points">Points in the source geographic coordinate system</param>
		/// <returns>Points in the target geographic coordinate system</returns>
        List<double[]> Forward(List<double[]> points);

		/// <summary>
		/// Transforms an array of points from the target geographic coordinate system
		/// to the source geographic coordinate system.
		/// </summary>
		/// <param name="points">Points in the target geographic coordinate system</param>
		/// <returns>Points in the source geographic coordinate system</returns>
        List<double[]> Inverse(List<double[]> points);

	}
}
