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

using System;

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// A 3D coordinate system, with its origin at the centre of the Earth. 
	/// </summary>
	/// <remarks>
	/// The X axis points towards the
	/// prime meridian. The Y axis points East or West. The Z axis points North or South. By default the
	/// Z axis will point North, and the Y axis will point East (e.g. a right handed system), but you should
	/// check the axes for non-default values.
	/// </remarks>
	public class GeocentricCoordinateSystem: CoordinateSystem, IGeocentricCoordinateSystem
	{
        /// <summary>
        /// 
        /// </summary>
        internal GeocentricCoordinateSystem(): base(String.Empty,String.Empty,String.Empty,String.Empty,String.Empty,String.Empty)
		{
			throw new NotImplementedException();
		}
		
        /// <summary>
        /// 
        /// </summary>
		public ILinearUnit LinearUnit
		{
			get
			{
				throw new NotImplementedException();
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public IHorizontalDatum HorizontalDatum
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		
        /// <summary>
        /// 
        /// </summary>
		public IPrimeMeridian PrimeMeridian
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}
