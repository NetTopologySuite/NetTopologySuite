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
using GisSharpBlog.NetTopologySuite.Positioning;

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// A 2D coordinate system suitable for positions on the Earth's surface.
	/// </summary>
	public class HorizontalCoordinateSystem : CoordinateSystem, IHorizontalCoordinateSystem
	{
		IHorizontalDatum _horizontalDatum;
		IAxisInfo[] _axisInfoArray;

		/// <summary>
		///  Initializes a new instance of the HorizontalCoordinateSystem class with the specified parameters.
		/// </summary>
		/// <param name="horizontalDatum">The horizontal datum to use.</param>
		/// <param name="axisInfoArray">Array of axis information.</param>
		public HorizontalCoordinateSystem(IHorizontalDatum horizontalDatum,IAxisInfo[] axisInfoArray)
			: this(horizontalDatum,axisInfoArray, String.Empty,String.Empty,String.Empty,String.Empty,String.Empty,String.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the HorizontalCoordinateSystem class with the specified parameters.
		/// </summary>
		/// <param name="horizontalDatum">The horizontal datum to use.</param>
		/// <param name="axisInfoArray">Array of axis information.</param>
		/// <param name="remarks">Remarks about this object.</param>
		/// <param name="authority">The name of the authority.</param>
		/// <param name="authorityCode">The code the authority uses to identidy this object.</param>
		/// <param name="name">The name of the object.</param>
		/// <param name="alias">The alias of the object.</param>
		/// <param name="abbreviation">The abbreviated name of this object.</param>

		public HorizontalCoordinateSystem(IHorizontalDatum horizontalDatum,IAxisInfo[] axisInfoArray,
			string remarks, string authority, string authorityCode, string name, string alias, string abbreviation)
			: base(remarks, authority, authorityCode, name, alias, abbreviation)
		{
			if (horizontalDatum==null)
			{
				throw new ArgumentNullException("horizontalDatum");
			}
			if (axisInfoArray==null)
			{
				throw new ArgumentNullException("axisInfoArray");
			}
			_horizontalDatum = horizontalDatum;
			_axisInfoArray = axisInfoArray;
		}


		#region Implementation of IHorizontalCoordinateSystem

		

		/// <summary>
		/// Gets the horizontal datum information.
		/// </summary>
		public IHorizontalDatum HorizontalDatum
		{
			get
			{
				return _horizontalDatum;
			}
		}

		
		#endregion
	}
}
