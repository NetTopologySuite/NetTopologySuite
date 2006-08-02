/*
 *  Copyright (C) 2002 Urban Science Applications, Inc. (translated from Java Topology Suite, 
 *  Copyright 2001 Vivid Solutions)
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
	/// An aggregate of two coordinate systems (CRS).
	/// One of these is usually a CRS based on a two dimensional coordinate system
	/// such as a geographic or a projected coordinate system with a horizontal
	/// datum.  The other is a vertical CRS which is a one-dimensional coordinate
	/// system with a vertical datum.
	/// </summary>
	public interface ICompoundCoordinateSystem : ICoordinateSystem
	{
		/// <summary>
		/// Gets first sub-coordinate system. 
		/// </summary>
		ICoordinateSystem HeadCS { get; }
		/// <summary>
		/// Gets second sub-coordinate system.
		/// </summary>
		ICoordinateSystem TailCS { get; }	
	}
}
