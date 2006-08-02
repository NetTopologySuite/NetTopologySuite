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
	/// An object that represents a prime meridian.
	/// </summary>
	public class PrimeMeridian : AbstractInformation, IPrimeMeridian
	{

		IAngularUnit _angularUnit;
		double _longitude;
		
		/// <summary>
		/// Initializes a new instance of the PrimeMeridian class.
		/// </summary>
		/// <param name="name">The name of the prime meridian.</param>
		/// <param name="angularUnit">The angular unit the longitude is measured in.</param>
		/// <param name="longitude">The longitude of the prime meridian.</param>
		/// <param name="remarks">Remarks about this object.</param>
		/// <param name="authority">The name of the authority.</param>
		/// <param name="authorityCode">The code the authority uses to identidy this object.</param>
		/// <param name="alias">The alias of the object.</param>
		/// <param name="abbreviation">The abbreviated name of this object.</param>
		internal PrimeMeridian(string name, IAngularUnit angularUnit, double longitude, string remarks, string authority, string authorityCode, string alias, string abbreviation)
			: base(remarks, authority, authorityCode, name, alias, abbreviation)
		{
			if (angularUnit==null)
			{
				throw new ArgumentNullException("angularUnit");
			}
			_name = name;
			_angularUnit = angularUnit;
			_longitude = longitude;
		}

		/// <summary>
		/// Initializes a new instance of the PrimeMeridian class.
		/// </summary>
		/// <param name="name">The name of the prime meridian.</param>
		/// <param name="angularUnit">The angular unit the longitude is measured in.</param>
		/// <param name="longitude">The longitude of the prime meridian.</param>
		internal PrimeMeridian(string name, IAngularUnit angularUnit, double longitude) 
			:this(name,angularUnit, longitude, String.Empty,String.Empty,String.Empty,String.Empty,String.Empty)
		{
		}

		#region Static methods
		/// <summary>
		/// Returns the Greenwich prime meridian.
		/// </summary>
		public static PrimeMeridian Greenwich
		{
			get
			{
				PrimeMeridian greenwich = new PrimeMeridian("Greenwich(default)", new AngularUnit(1),0.0);
				return greenwich;
			}
		}
		#endregion

		#region Implementation of IPrimeMeridian
		/// <summary>
		/// The angular units used for the prime meridian.
		/// </summary>
		public IAngularUnit AngularUnit
		{
			get
			{
				return _angularUnit;
			}
		}

		/// <summary>
		/// The central longitude of the prime meridian in the units specified by the AngularUnits property.
		/// </summary>
		public double Longitude
		{
			get
			{
				return _longitude * _angularUnit.RadiansPerUnit;
			}
		}
		#endregion
	}
}
