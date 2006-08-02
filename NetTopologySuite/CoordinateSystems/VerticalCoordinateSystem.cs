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
	/// A one-dimensional coordinate system suitable for vertical measurements.
	/// </summary>
	public class VerticalCoordinateSystem : CoordinateSystem, IVerticalCoordinateSystem
	{
		IVerticalDatum _verticaldatum;
		IAxisInfo[] _axisinfo;
		ILinearUnit _units;

		/// <summary>
		/// Initializes a new instance of the Projection class.
		/// </summary>
		/// <param name="name">The name of the coordinate system.</param>
		/// <param name="verticaldatum">The vertical datum the coordiniate system is to use.</param>
		/// <param name="axisinfo">Axis information.</param>
		/// <param name="units">The units this coordinae system uses.</param>
		internal VerticalCoordinateSystem(string name, IVerticalDatum verticaldatum, IAxisInfo axisinfo, ILinearUnit units) 
			: base(name,String.Empty,String.Empty,String.Empty,String.Empty,String.Empty) 
		{
			if (verticaldatum==null)
			{
				throw new ArgumentNullException("verticaldatum");
			}
			if (units==null)
			{
				throw new ArgumentNullException("units");
			}
		
			_name = name;
			_verticaldatum = verticaldatum;
			_axisinfo= new IAxisInfo[1]{axisinfo};
			_units = units;
		}
		/// <summary>
		/// Initializes a new instance of the Projection class. The units are set to meters and the axis is set to represent altitude.
		/// </summary>
		/// <param name="verticaldatum"></param>
		/// <param name="remarks">Remarks about this object.</param>
		/// <param name="authority">The name of the authority.</param>
		/// <param name="authorityCode">The code the authority uses to identidy this object.</param>
		/// <param name="name">The name of the object.</param>
		/// <param name="alias">The alias of the object.</param>
		/// <param name="abbreviation">The abbreviated name of this object.</param>
		internal VerticalCoordinateSystem(
			string name,
			IVerticalDatum verticaldatum,
			string remarks, string authority, string authorityCode, string alias, string abbreviation)
			:base(remarks, authority,  authorityCode,name, alias, abbreviation)
		{
			if (verticaldatum==null)
			{
				throw new ArgumentNullException("verticaldatum");
			}
			_verticaldatum = verticaldatum;
			_units = LinearUnit.Meters;
			_axisinfo= new IAxisInfo[1]{AxisInfo.Altitude};
		}
		/// <summary>
		/// Initializes a new instance of the Projection class.
		/// </summary>
		/// <param name="verticaldatum">The vertical datum the coordiniate system is to use.</param>
		/// <param name="axisinfo">Axis information.</param>
		/// <param name="linearUnit">The units this coordinate system uses.</param>
		/// <param name="remarks">Remarks about this object.</param>
		/// <param name="authority">The name of the authority.</param>
		/// <param name="authorityCode">The code the authority uses to identidy this object.</param>
		/// <param name="name">The name of the object.</param>
		/// <param name="alias">The alias of the object.</param>
		/// <param name="abbreviation">The abbreviated name of this object.</param>
		public VerticalCoordinateSystem(	string name,
			IVerticalDatum verticaldatum,
			IAxisInfo axisinfo,
			ILinearUnit linearUnit,
			string remarks, string authority, string authorityCode, string alias, string abbreviation)
			:base(remarks, authority,  authorityCode,name, alias, abbreviation)
		{
			if (verticaldatum==null)
			{
				throw new ArgumentNullException("verticaldatum");
			}
		
			_verticaldatum = verticaldatum;
			_axisinfo = new IAxisInfo[1]{axisinfo};
			_units = linearUnit;
		}

		#region Static
		/// <summary>
		/// Default vertical coordinate system using ellipsoidal datum.
		/// Ellipsoidal heights are measured along the normal to the
		/// ellipsoid used in the definition of horizontal datum.
		/// </summary>
		public static IVerticalCoordinateSystem Ellipsoidal
		{
			get
			{
				VerticalDatum datum = GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems.VerticalDatum.Ellipsoidal;
				return new VerticalCoordinateSystem("Ellipsoidal",datum,AxisInfo.Altitude,LinearUnit.Meters);
			}
		}

		#endregion

		#region Implementation of IVerticalCoordinateSystem
	
		/// <summary>
		/// Gets the dimension of this coordinate system (1).
		/// </summary>
		public override int Dimension
		{
			get
			{
				return 1;
			}
		}



		/// <summary>
		/// Gets the vertical datum for this coordinate system.
		/// </summary>
		public IVerticalDatum VerticalDatum
		{
			get
			{
				return _verticaldatum;
			}
		}
	
		/// <summary>
		/// Gets the linear units for this coordinate system.
		/// </summary>
		public ILinearUnit VerticalUnit
		{
			get
			{
				return _units;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns></returns>
		public override IAxisInfo GetAxis(int dimension)
		{
			return _axisinfo[dimension];
		}
		#endregion
	}
}
