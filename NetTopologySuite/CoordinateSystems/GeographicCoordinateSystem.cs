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
using GisSharpBlog.NetTopologySuite.Geometries;

using GisSharpBlog.NetTopologySuite.Positioning;
namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems

{
	/// <summary>
	/// A coordinate system based on latitude and longitude. 
	/// </summary>
	/// <remarks>
	/// Some geographic coordinate systems are
	/// Lat/Lon, and some are Lon/Lat. You can find out which this is by examining the axes. You should
	/// also check the angular units, since not all geographic coordinate systems use degrees.
	/// </remarks>
	public class GeographicCoordinateSystem : CoordinateSystem, IGeographicCoordinateSystem
	{

		/*/// <summary>
		/// A geographic coordinate system using WGS84 datum.
		/// This coordinate system use <var>longitude</var>/<var>latitude</var> ordinates
		/// with longitude values increasing north and latitude values increasing east.
		/// Angular units are degrees and prime merid
		/// ian is Greenwich.
		/// </summary>*/
		//TODO:public static GeographicCoordinateSystem WGS84 = null;//new GeographicCoordinateSystem("WGS84", HorizontalDatum.WGS84));

	
		IPrimeMeridian _primeMeridian;
		GisSharpBlog.NetTopologySuite.Positioning.Envelope _defaultEnvelope;
		IHorizontalDatum _horizontalDatum;
		IAngularUnit _angularUnit;
		IAxisInfo[] _axisInfo;


		internal GeographicCoordinateSystem(string name,
			IAngularUnit angularUnit, 
			IHorizontalDatum horizontalDatum, 
			IPrimeMeridian primeMeridian, 
			IAxisInfo axis0, 
			IAxisInfo axis1): 
		this (angularUnit,horizontalDatum,primeMeridian, axis0,  axis1,String.Empty,String.Empty,String.Empty,name,String.Empty,String.Empty)
		{
			
		}

		internal GeographicCoordinateSystem( 
			IAngularUnit angularUnit, 
			IHorizontalDatum horizontalDatum, 
			IPrimeMeridian primeMeridian, 
			IAxisInfo axis0, 
			IAxisInfo axis1,
			string remarks, string authority, string authorityCode, string name, string alias, string abbreviation)
			: base(remarks, authority, authorityCode, name, alias, abbreviation)
		{
			_angularUnit = angularUnit;
			_horizontalDatum = horizontalDatum;
			_primeMeridian = primeMeridian;
			_axisInfo = new IAxisInfo[]{axis0,axis1};

			
			CoordinatePoint minPt = new CoordinatePoint();
			minPt.Ord= new Double[2];
			minPt.Ord.SetValue(-180,0);
			minPt.Ord.SetValue(-90,1);

			CoordinatePoint maxPt = new CoordinatePoint();
			maxPt.Ord= new Double[2];
			maxPt.Ord.SetValue(-180,0);
			maxPt.Ord.SetValue(-90,1);

			// define the envelope.
			_defaultEnvelope = new Positioning.Envelope();
			_defaultEnvelope.MinCP = minPt;
			_defaultEnvelope.MaxCP = maxPt;
		

	
		}

		#region Implementation of IGeographicCoordinateSystem
		/// <summary>
		/// Gets axis information for the specified dimension.
		/// </summary>
		/// <param name="Dimension">The dimension to get the axis information for.</param>
		/// <returns>IAxisInfo containing the axis information.</returns>
		public override IAxisInfo GetAxis(int Dimension)
		{
			return _axisInfo[Dimension];
		}

		/// <summary>
		/// Gets the unit information for the specified dimension.
		/// </summary>
		/// <param name="dimension">The dimentsion to get the units information for.</param>
		/// <returns>IUnit containing infomation about the units.</returns>
		public override IUnit GetUnits(int dimension)
		{
			if (dimension>=0 && dimension<this.Dimension)
			{
				return _angularUnit;
			}
			throw new ArgumentOutOfRangeException(String.Format("Dimension must be between 0 and {0}",this.Dimension));
		}
		/// <summary>
		/// Gets the number of dimensions for a geographic coordinate system (2).
		/// </summary>
		public override int Dimension
		{
			get
			{
				return 2;
			}
		}


		/// <summary>
		/// Gets the WGS 84 conversion information.
		/// </summary>
		/// <param name="index">????</param>
		/// <returns>IWGS84ConversionInfo containing the WGS 84 conversion information.</returns>
		public WGS84ConversionInfo GetWGS84ConversionInfo(int index)
		{
			return new WGS84ConversionInfo();
		}

		/// <summary>
		/// Gets the angular units used in the coordinate system.
		/// </summary>
		public IAngularUnit AngularUnit
		{
			get
			{
				return _angularUnit;
			}
		}

		/// <summary>
		/// Not implemented.
		/// </summary>
		public int NumConversionToWGS84
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		
		/// <summary>
		/// Gets the horizontal datum for the coordinate system.
		/// </summary>
		public IHorizontalDatum HorizontalDatum
		{
			get
			{
				return _horizontalDatum;
			}
		}


		/// <summary>
		/// Gets the prime meridian for this coordinate system.
		/// </summary>
		public IPrimeMeridian PrimeMeridian
		{
			get
			{
				return _primeMeridian;
			}
		}	
		#endregion


		
	}
}
