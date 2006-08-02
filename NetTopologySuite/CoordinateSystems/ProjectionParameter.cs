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
	/// A named projection parameter value.
	/// </summary>
	/// <remarks>
	/// The linear units of parameters' values match the linear units of the
	/// containing projected coordinate system.  The angular units of parameter
	/// values match the angular units of the geographic coordinate system that
	/// the projected coordinate system is based on.  (Notice that this is
	/// different from CT_Parameter, where the units are always meters and
	/// degrees.)
	/// </remarks>
	public struct ProjectionParameter
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="v"></param>
		public ProjectionParameter(string name, double v)
		{
			Name=name;
			Value=v;
		}

		/// <summary>
		/// The parameter name.
		/// </summary>
		public string Name;
		
        /// <summary>
		/// The parameter value.
		/// </summary>
		public double Value;
	}
}
