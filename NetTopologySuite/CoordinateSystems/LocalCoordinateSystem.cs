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
using System;

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// A local coordinate system, with uncertain relationship to the world (Not implemented);
	/// </summary>
	/// <remarks>
	/// In general, a local coordinate system cannot be related to other coordinate systems. However, if two objects supporting this interface have the same dimension, axes, units and datum then client code is permitted to assume that the two coordinate systems are identical. This allows several datasets from a common source (e.g. a CAD system) to be overlaid. In addition, some implementations of the Coordinate Transformation (CT) package may have a mechanism for correlating local datums. (E.g. from a database of transformations, which is created and maintained from real-world measurements.)
	/// </remarks>
	public class LocalCoordinateSystem : CoordinateSystem, ILocalCoordinateSystem
	{
        /// <summary>
        /// 
        /// </summary>
		internal LocalCoordinateSystem() : base(String.Empty,String.Empty,String.Empty,String.Empty,String.Empty,String.Empty)
		{
			throw new NotImplementedException();
		}
		
        /// <summary>
        /// 
        /// </summary>
		public ILocalDatum LocalDatum
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}
