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
using GisSharpBlog.NetTopologySuite.Positioning;
namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// A 2D cartographic coordinate system
	/// </summary>
	public class ProjectedCoordinateSystem : AbstractInformation, IProjectedCoordinateSystem
	{
		IHorizontalDatum _horizontalDatum;
		IAxisInfo[] _axisInfoArray;
		IGeographicCoordinateSystem _geographicCoordSystem;
		IProjection _projection;
		ILinearUnit _linearUnit;

		/// <summary>
		/// Initializes a new instance of the ProjectedCoordinateSystem class.
		/// </summary>
		/// <param name="horizontalDatum">The horizontal datum to use.</param>
		/// <param name="axisInfoArray">An array of IAxisInfo representing the axis information.</param>
		/// <param name="geographicCoordSystem">The geographic coordinate system.</param>
		/// <param name="linearUnit">The linear units to use.</param>
		/// <param name="projection">The projection to use.</param>
		internal ProjectedCoordinateSystem(
			IHorizontalDatum horizontalDatum,
			IAxisInfo[] axisInfoArray,
			IGeographicCoordinateSystem geographicCoordSystem,
			ILinearUnit linearUnit,
			IProjection projection): this(horizontalDatum, axisInfoArray, geographicCoordSystem, linearUnit, projection, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty)
		{
		}
		/// <summary>
		/// Initializes a new instance of the ProjectedCoordinateSystem class.
		/// </summary>
		/// <param name="horizontalDatum">The horizontal datum to use.</param>
		/// <param name="axisInfoArray">An array of IAxisInfo representing the axis information.</param>
		/// <param name="geographicCoordSystem">The geographic coordinate system.</param>
		/// <param name="linearUnit">The linear units to use.</param>
		/// <param name="projection">The projection to use.</param>
		/// <param name="remarks">Remarks about this object.</param>
		/// <param name="authority">The name of the authority.</param>
		/// <param name="authorityCode">The code the authority uses to identidy this object.</param>
		/// <param name="name">The name of the object.</param>
		/// <param name="alias">The alias of the object.</param>
		/// <param name="abbreviation">The abbreviated name of this object.</param>
		internal ProjectedCoordinateSystem(
			IHorizontalDatum horizontalDatum,
			IAxisInfo[] axisInfoArray,
			IGeographicCoordinateSystem geographicCoordSystem,
			ILinearUnit linearUnit,
			IProjection projection,
			string remarks, string authority, string authorityCode, string name, string alias, string abbreviation)
			: base(remarks, authority, authorityCode, name, alias, abbreviation)
		{
			
			if (axisInfoArray==null)
			{
				throw new ArgumentNullException("axisInfoArray");
			}
			if (geographicCoordSystem==null)
			{
				throw new ArgumentNullException("geographicCoordSystem");
			}
			if (projection==null)
			{
				throw new ArgumentNullException("projection");
			}
			if (linearUnit==null)
			{
				throw new ArgumentNullException("linearUnit");
			}
			_horizontalDatum=horizontalDatum;
			_axisInfoArray=  axisInfoArray;
			_geographicCoordSystem = geographicCoordSystem;
			_projection=	 projection;
			_linearUnit = linearUnit;
		}


		#region Implementation of IProjectedCoordinateSystem
		/// <summary>
		/// Gets information for the specified axis.
		/// </summary>
		/// <param name="dimension">The dimension to get axis information for.</param>
		/// <returns>IAxisInfo.</returns>
		public IAxisInfo GetAxis(int dimension)
		{
			return _axisInfoArray[dimension];
		}

		/// <summary>
		/// Not implemented.
		/// </summary>
		/// <param name="dimension"></param>
		/// <returns></returns>
		public IUnit GetUnits(int dimension)
		{
			throw new NotImplementedException();
		}

		
		/// <summary>
		/// Gets the number of dimensions this coordinate system represents.
		/// </summary>
		public int Dimension
		{
			get
			{
				return _axisInfoArray.Length;
			}
		}

		/// <summary>
		/// Gets the linear units for this coordinate system.
		/// </summary>
		public ILinearUnit LinearUnit
		{
			get
			{
				return _linearUnit;
			}
		}

		/// <summary>
		/// Gets the horizontal datum for this coordinate system.
		/// </summary>
		public IHorizontalDatum HorizontalDatum
		{
			get
			{
				return _horizontalDatum;
			}
		}

		/// <summary>
		/// Not implemented.
		/// </summary>
		public Envelope DefaultEnvelope
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the geographic coordinate system.
		/// </summary>
		public IGeographicCoordinateSystem GeographicCoordinateSystem
		{
			get
			{
				return _geographicCoordSystem;
			}
		}

		/// <summary>
		/// Gets the projection for this coordinate system.
		/// </summary>
		public IProjection Projection
		{
			get
			{
				return _projection;
			}
		}
		#endregion
	}
}