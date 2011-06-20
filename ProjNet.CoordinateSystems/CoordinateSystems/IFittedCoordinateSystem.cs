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
	/// A coordinate system which sits inside another coordinate system. The fitted 
	/// coordinate system can be rotated and shifted, or use any other math transform
	/// to inject itself into the base coordinate system.
	/// </summary>
	public interface IFittedCoordinateSystem : ICoordinateSystem
	{
		/// <summary>
		/// Gets underlying coordinate system.
		/// </summary>
		ICoordinateSystem BaseCoordinateSystem { get; }
		/// <summary>
		/// Gets Well-Known Text of a math transform to the base coordinate system. 
		/// The dimension of this fitted coordinate system is determined by the source 
		/// dimension of the math transform. The transform should be one-to-one within 
		/// this coordinate system's domain, and the base coordinate system dimension 
		/// must be at least as big as the dimension of this coordinate system.
		/// </summary>
		/// <returns></returns>
		string ToBase();
	}
}
