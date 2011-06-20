// Copyright 2005 - 2009 - Morten Nielsen (www.sharpgis.net)
//
// This file is part of ProjNet.
// ProjNet is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// ProjNet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with ProjNet; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

// SOURCECODE IS MODIFIED FROM ANOTHER WORK AND IS ORIGINALLY BASED ON GeoTools.NET:
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
using System.Collections.Generic;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace ProjNet.CoordinateSystems.Projections
{
	/// <summary>
	/// Implements the Mercator projection.
	/// </summary>
	/// <remarks>
	/// <para>This map projection introduced in 1569 by Gerardus Mercator. It is often described as a cylindrical projection,
	/// but it must be derived mathematically. The meridians are equally spaced, parallel vertical lines, and the
	/// parallels of latitude are parallel, horizontal straight lines, spaced farther and farther apart as their distance
	/// from the Equator increases. This projection is widely used for navigation charts, because any straight line
	/// on a Mercator-projection map is a line of constant true bearing that enables a navigator to plot a straight-line
	/// course. It is less practical for world maps because the scale is distorted; areas farther away from the equator
	/// appear disproportionately large. On a Mercator projection, for example, the landmass of Greenland appears to be
	/// greater than that of the continent of South America; in actual area, Greenland is smaller than the Arabian Peninsula.
	/// </para>
	/// </remarks>
	internal class Mercator : MapProjection
	{
		double _falseEasting;
		double _falseNorthing;
		double lon_center;		//Center longitude (projection center)
		double lat_origin;		//center latitude
		double e,e2;			//eccentricity constants
		double k0;				//small value m

		/// <summary>
		/// Initializes the MercatorProjection object with the specified parameters to project points. 
		/// </summary>
		/// <param name="parameters">ParameterList with the required parameters.</param>
		/// <remarks>
		/// </remarks>
		public Mercator(List<ProjectionParameter> parameters)
			: this(parameters, false)
		{
		}

		/// <summary>
		/// Initializes the MercatorProjection object with the specified parameters.
		/// </summary>
		/// <param name="parameters">List of parameters to initialize the projection.</param>
		/// <param name="isInverse">Indicates whether the projection forward (meters to degrees or degrees to meters).</param>
		/// <remarks>
		/// <para>The parameters this projection expects are listed below.</para>
		/// <list type="table">
		/// <listheader><term>Items</term><description>Descriptions</description></listheader>
		/// <item><term>central_meridian</term><description>The longitude of the point from which the values of both the geographical coordinates on the ellipsoid and the grid coordinates on the projection are deemed to increment or decrement for computational purposes. Alternatively it may be considered as the longitude of the point which in the absence of application of false coordinates has grid coordinates of (0,0).</description></item>
		/// <item><term>latitude_of_origin</term><description>The latitude of the point from which the values of both the geographical coordinates on the ellipsoid and the grid coordinates on the projection are deemed to increment or decrement for computational purposes. Alternatively it may be considered as the latitude of the point which in the absence of application of false coordinates has grid coordinates of (0,0).</description></item>
		/// <item><term>scale_factor</term><description>The factor by which the map grid is reduced or enlarged during the projection process, defined by its value at the natural origin.</description></item>
		/// <item><term>false_easting</term><description>Since the natural origin may be at or near the centre of the projection and under normal coordinate circumstances would thus give rise to negative coordinates over parts of the mapped area, this origin is usually given false coordinates which are large enough to avoid this inconvenience. The False Easting, FE, is the easting value assigned to the abscissa (east).</description></item>
		/// <item><term>false_northing</term><description>Since the natural origin may be at or near the centre of the projection and under normal coordinate circumstances would thus give rise to negative coordinates over parts of the mapped area, this origin is usually given false coordinates which are large enough to avoid this inconvenience. The False Northing, FN, is the northing value assigned to the ordinate.</description></item>
		/// </list>
		/// </remarks>
		public Mercator(List<ProjectionParameter> parameters, bool isInverse)
			: base(parameters, isInverse)
		{
			this.Authority = "EPSG";
			ProjectionParameter central_meridian = GetParameter("central_meridian");
			ProjectionParameter latitude_of_origin = GetParameter("latitude_of_origin");
			ProjectionParameter scale_factor = GetParameter("scale_factor");
			ProjectionParameter false_easting = GetParameter("false_easting");
			ProjectionParameter false_northing = GetParameter("false_northing");			
			//Check for missing parameters
			if (central_meridian == null)
				throw new ArgumentException("Missing projection parameter 'central_meridian'");
			if (latitude_of_origin == null)
				throw new ArgumentException("Missing projection parameter 'latitude_of_origin'");
			if (false_easting == null)
				throw new ArgumentException("Missing projection parameter 'false_easting'");
			if (false_northing == null)
				throw new ArgumentException("Missing projection parameter 'false_northing'");

			lon_center = Degrees2Radians(central_meridian.Value);
			lat_origin = Degrees2Radians(latitude_of_origin.Value);
			_falseEasting = false_easting.Value * _metersPerUnit;
			_falseNorthing = false_northing.Value * _metersPerUnit;
			
			double temp = this._semiMinor / this._semiMajor;
			e2 = 1 - temp * temp;
			e = Math.Sqrt(e2);
			if (scale_factor == null) //This is a two standard parallel Mercator projection (2SP)
			{
				k0 = Math.Cos(lat_origin) / Math.Sqrt(1.0 - e2 * Math.Sin(lat_origin) * Math.Sin(lat_origin));
				this.AuthorityCode = 9805;
				this.Name = "Mercator_2SP";
			}
			else //This is a one standard parallel Mercator projection (1SP)
			{
				k0 = scale_factor.Value;
				this.Name = "Mercator_1SP";
			}
			this.Authority = "EPSG";
		}

		/// <summary>
		/// Converts coordinates in decimal degrees to projected meters.
		/// </summary>
		/// <remarks>
		/// <para>The parameters this projection expects are listed below.</para>
		/// <list type="table">
		/// <listheader><term>Items</term><description>Descriptions</description></listheader>
		/// <item><term>longitude_of_natural_origin</term><description>The longitude of the point from which the values of both the geographical coordinates on the ellipsoid and the grid coordinates on the projection are deemed to increment or decrement for computational purposes. Alternatively it may be considered as the longitude of the point which in the absence of application of false coordinates has grid coordinates of (0,0).  Sometimes known as ""central meridian""."</description></item>
		/// <item><term>latitude_of_natural_origin</term><description>The latitude of the point from which the values of both the geographical coordinates on the ellipsoid and the grid coordinates on the projection are deemed to increment or decrement for computational purposes. Alternatively it may be considered as the latitude of the point which in the absence of application of false coordinates has grid coordinates of (0,0).</description></item>
		/// <item><term>scale_factor_at_natural_origin</term><description>The factor by which the map grid is reduced or enlarged during the projection process, defined by its value at the natural origin.</description></item>
		/// <item><term>false_easting</term><description>Since the natural origin may be at or near the centre of the projection and under normal coordinate circumstances would thus give rise to negative coordinates over parts of the mapped area, this origin is usually given false coordinates which are large enough to avoid this inconvenience. The False Easting, FE, is the easting value assigned to the abscissa (east).</description></item>
		/// <item><term>false_northing</term><description>Since the natural origin may be at or near the centre of the projection and under normal coordinate circumstances would thus give rise to negative coordinates over parts of the mapped area, this origin is usually given false coordinates which are large enough to avoid this inconvenience. The False Northing, FN, is the northing value assigned to the ordinate .</description></item>
		/// </list>
		/// </remarks>
		/// <param name="lonlat">The point in decimal degrees.</param>
		/// <returns>Point in projected meters</returns>
        public override double[] DegreesToMeters(double[] lonlat)
		{
			if (double.IsNaN(lonlat[0]) || double.IsNaN(lonlat[1]))
                return new double[] { Double.NaN, Double.NaN, };
			double dLongitude = Degrees2Radians(lonlat[0]);
			double dLatitude = Degrees2Radians(lonlat[1]);

			/* Forward equations */
			if (Math.Abs(Math.Abs(dLatitude) - HALF_PI)  <= EPSLN)
				throw new ArgumentException("Transformation cannot be computed at the poles.");
			else
			{
				double esinphi = e*Math.Sin(dLatitude);
				double x = _falseEasting + this._semiMajor * k0 * (dLongitude - lon_center);
				double y = _falseNorthing + this._semiMajor * k0 * Math.Log(Math.Tan(PI * 0.25 + dLatitude * 0.5) * Math.Pow((1 - esinphi) / (1 + esinphi), e * 0.5));
				if (lonlat.Length < 3)
					return new double[] { x / _metersPerUnit, y / _metersPerUnit };
				else
					return new double[] { x / _metersPerUnit, y / _metersPerUnit, lonlat[2] };
			}
		}

		/// <summary>
		/// Converts coordinates in projected meters to decimal degrees.
		/// </summary>
		/// <param name="p">Point in meters</param>
		/// <returns>Transformed point in decimal degrees</returns>
        public override double[] MetersToDegrees(double[] p)
		{	
			double dLongitude = Double.NaN ; 
			double dLatitude = Double.NaN ;
	
			/* Inverse equations
			  -----------------*/
			double dX = p[0] * _metersPerUnit - this._falseEasting;
			double dY = p[1] * _metersPerUnit - this._falseNorthing;
			double ts = Math.Exp(-dY / (this._semiMajor * k0)); //t
			
			double chi = HALF_PI - 2 * Math.Atan(ts);
			double e4 = Math.Pow(e, 4);
			double e6 = Math.Pow(e, 6);
			double e8 = Math.Pow(e, 8);

			dLatitude = chi + (e2 * 0.5 + 5 * e4 / 24 + e6 / 12 + 13 * e8 / 360) * Math.Sin(2 * chi)
			    + (7 * e4 / 48 + 29 * e6 / 240 + 811 * e8 / 11520) * Math.Sin(4 * chi) +
			    +(7 * e6 / 120 + 81 * e8 / 1120) * Math.Sin(6 * chi) +
			    +(4279 * e8 / 161280) * Math.Sin(8 * chi);

            dLongitude = dX / (this._semiMajor * k0) + lon_center;
			if (p.Length < 3)
				return new double[] { Radians2Degrees(dLongitude), Radians2Degrees(dLatitude) };
			else
				return new double[] { Radians2Degrees(dLongitude), Radians2Degrees(dLatitude), p[2] };
		}
		
		/// <summary>
		/// Returns the inverse of this projection.
		/// </summary>
		/// <returns>IMathTransform that is the reverse of the current projection.</returns>
		public override IMathTransform Inverse()
		{
			if (_inverse == null)
				_inverse = new Mercator(this._Parameters, ! _isInverse);			
			return _inverse;
		}
	}
}
