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
	/// Procedure used to measure positions on the surface of the Earth.
	/// </summary>
	public class HorizontalDatum : Datum, IHorizontalDatum
	{
		IEllipsoid _ellipsoid;
		WGS84ConversionInfo _wgs84ConversionInfo;
		#region Constructors


		/// <summary>
		///  Initializes a new instance of the HorizontalDatum class with the specififed properties.
		/// </summary>
		/// <param name="name">The name of the datum.</param>
		/// <param name="horizontalDatumType">The datum type.</param>
		/// <param name="ellipsoid">The ellipsoid.</param>
		/// <param name="toWGS84">The WGS conversion parameters.</param>
		internal HorizontalDatum(string name, DatumType horizontalDatumType, IEllipsoid ellipsoid, WGS84ConversionInfo toWGS84)
			:this(name, horizontalDatumType, ellipsoid, toWGS84,String.Empty, String.Empty, String.Empty, String.Empty, String.Empty)
		{
		}

		/// <summary>
		///  Initializes a new instance of the HorizontalDatum class with the specififed properties.
		/// </summary>
		/// <param name="name">The name of the datum.</param>
		/// <param name="horizontalDatumType">The datum type.</param>
		/// <param name="ellipsoid">The ellipsoid.</param>
		/// <param name="toWGS84">The WGS conversion parameters.</param>
		/// <param name="remarks">Remarks about this object.</param>
		/// <param name="authority">The name of the authority.</param>
		/// <param name="authorityCode">The code the authority uses to identidy this object.</param>
		/// <param name="alias">The alias of the object.</param>
		/// <param name="abbreviation">The abbreviated name of this object.</param>
		public HorizontalDatum(string name, DatumType horizontalDatumType, IEllipsoid ellipsoid, WGS84ConversionInfo toWGS84,
			string remarks, string authority, string authorityCode, string alias, string abbreviation)
			: base(horizontalDatumType, remarks, authority, authorityCode, name, alias, abbreviation)
		{
			if (ellipsoid == null)
				throw new ArgumentNullException("ellipsoid");			
			_ellipsoid = ellipsoid;
			_wgs84ConversionInfo = toWGS84;
		}
		#endregion

		#region Static methods
		/// <summary>
		/// The default WGS 1984 datum.
		/// </summary>
		public static IHorizontalDatum WGS84
		{
			get
			{
				Ellipsoid ellipsoid = GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems.Ellipsoid.WGS84Test;
				WGS84ConversionInfo conversionInfo = new WGS84ConversionInfo();
				HorizontalDatum horizontaldatum = new HorizontalDatum("WGS84", DatumType.IHD_Geocentric, ellipsoid, conversionInfo);
				return horizontaldatum;
			}
		}
		#endregion
		#region Implementation of IHorizontalDatum

		/// <summary>
		/// Gets the WGS84 conversion parameters.
		/// </summary>
		public WGS84ConversionInfo WGS84Parameters
		{
			get
			{
				WGS84ConversionInfo wgs = _wgs84ConversionInfo ;
				return wgs;
			}
		}

		/// <summary>
		/// Gets the ellipsoid.
		/// </summary>
		public IEllipsoid Ellipsoid
		{
			get
			{
				return _ellipsoid;
			}
		}

		#endregion
	}
}
