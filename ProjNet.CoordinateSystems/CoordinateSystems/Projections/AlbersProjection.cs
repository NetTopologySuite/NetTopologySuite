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
	///		Implements the Albers projection.
	/// </summary>
	/// <remarks>
	/// 	<para>Implements the Albers projection. The Albers projection is most commonly
	/// 	used to project the United States of America. It gives the northern
	/// 	border with Canada a curved appearance.</para>
	/// 	
	///		<para>The <a href="http://www.geog.mcgill.ca/courses/geo201/mapproj/naaeana.gif">Albers Equal Area</a>
	///		projection has the property that the area bounded
	///		by any pair of parallels and meridians is exactly reproduced between the 
	///		image of those parallels and meridians in the projected domain, that is,
	///		the projection preserves the correct area of the earth though distorts
	///		direction, distance and shape somewhat.</para>
	/// </remarks>
	internal class AlbersProjection : MapProjection
	{
		double _falseEasting;
		double _falseNorthing;
		double C;				//constant c 
		double e;				//eccentricity
		double e_sq = 0;
		double ro0;
		double n;
		double lon_center;		//center longitude   
		
		#region Constructors

		/// <summary>
		/// Creates an instance of an Albers projection object.
		/// </summary>
		/// <param name="parameters">List of parameters to initialize the projection.</param>
		/// <remarks>
		/// <para>The parameters this projection expects are listed below.</para>
		/// <list type="table">
		/// <listheader><term>Items</term><description>Descriptions</description></listheader>
		/// <item><term>latitude_of_false_origin</term><description>The latitude of the point which is not the natural origin and at which grid coordinate values false easting and false northing are defined.</description></item>
		/// <item><term>longitude_of_false_origin</term><description>The longitude of the point which is not the natural origin and at which grid coordinate values false easting and false northing are defined.</description></item>
		/// <item><term>latitude_of_1st_standard_parallel</term><description>For a conic projection with two standard parallels, this is the latitude of intersection of the cone with the ellipsoid that is nearest the pole.  Scale is true along this parallel.</description></item>
		/// <item><term>latitude_of_2nd_standard_parallel</term><description>For a conic projection with two standard parallels, this is the latitude of intersection of the cone with the ellipsoid that is furthest from the pole.  Scale is true along this parallel.</description></item>
		/// <item><term>easting_at_false_origin</term><description>The easting value assigned to the false origin.</description></item>
		/// <item><term>northing_at_false_origin</term><description>The northing value assigned to the false origin.</description></item>
		/// </list>
		/// </remarks>
		public AlbersProjection(List<ProjectionParameter> parameters)
			: this(parameters, false)
		{
		}
		
		/// <summary>
		/// Creates an instance of an Albers projection object.
		/// </summary>
		/// <remarks>
		/// <para>The parameters this projection expects are listed below.</para>
		/// <list type="table">
		/// <listheader><term>Items</term><description>Descriptions</description></listheader>
		/// <item><term>latitude_of_center</term><description>The latitude of the point which is not the natural origin and at which grid coordinate values false easting and false northing are defined.</description></item>
		/// <item><term>longitude_of_center</term><description>The longitude of the point which is not the natural origin and at which grid coordinate values false easting and false northing are defined.</description></item>
		/// <item><term>standard_parallel_1</term><description>For a conic projection with two standard parallels, this is the latitude of intersection of the cone with the ellipsoid that is nearest the pole.  Scale is true along this parallel.</description></item>
		/// <item><term>standard_parallel_2</term><description>For a conic projection with two standard parallels, this is the latitude of intersection of the cone with the ellipsoid that is furthest from the pole.  Scale is true along this parallel.</description></item>
		/// <item><term>false_easting</term><description>The easting value assigned to the false origin.</description></item>
		/// <item><term>false_northing</term><description>The northing value assigned to the false origin.</description></item>
		/// </list>
		/// </remarks>
		/// <param name="parameters">List of parameters to initialize the projection.</param>
		/// <param name="isInverse">Indicates whether the projection forward (meters to degrees or degrees to meters).</param>
		public AlbersProjection(List<ProjectionParameter> parameters, bool isInverse)
			: base(parameters, isInverse)
		{
			this.Name = "Albers_Conic_Equal_Area";			

			//Retrieve parameters
			ProjectionParameter longitude_of_center = GetParameter("longitude_of_center");
			ProjectionParameter latitude_of_center = GetParameter("latitude_of_center");
			ProjectionParameter standard_parallel_1 = GetParameter("standard_parallel_1");
			ProjectionParameter standard_parallel_2 = GetParameter("standard_parallel_2");
			ProjectionParameter false_easting = GetParameter("false_easting");
			ProjectionParameter false_northing = GetParameter("false_northing");
			//Check for missing parameters
			if (longitude_of_center == null)
			{
				longitude_of_center = GetParameter("central_meridian"); //Allow for altenative name
				if (longitude_of_center == null)
					throw new ArgumentException("Missing projection parameter 'longitude_of_center'");
			}
			if (latitude_of_center == null)
			{
				latitude_of_center = GetParameter("latitude_of_origin"); //Allow for altenative name
				if (latitude_of_center == null)
					throw new ArgumentException("Missing projection parameter 'latitude_of_center'");
			}
			if (standard_parallel_1 == null)
				throw new ArgumentException("Missing projection parameter 'standard_parallel_1'");
			if (standard_parallel_2 == null)
				throw new ArgumentException("Missing projection parameter 'standard_parallel_2'");
			if (false_easting == null)
				throw new ArgumentException("Missing projection parameter 'false_easting'");
			if (false_northing == null)
				throw new ArgumentException("Missing projection parameter 'false_northing'");

			lon_center = Degrees2Radians(longitude_of_center.Value);
			double lat0 = Degrees2Radians(latitude_of_center.Value);
			double lat1 = Degrees2Radians(standard_parallel_1.Value);
			double lat2 = Degrees2Radians(standard_parallel_2.Value);
			this._falseEasting = false_easting.Value * _metersPerUnit;
			this._falseNorthing = false_northing.Value * _metersPerUnit;

			if (Math.Abs(lat1 + lat2) < double.Epsilon)
				throw new ArgumentException("Equal latitudes for standard parallels on opposite sides of Equator.");

			e_sq = 1.0 - Math.Pow(this._semiMinor / this._semiMajor, 2);
			e = Math.Sqrt(e_sq); //Eccentricity

			double alpha1 = alpha(lat1);
			double alpha2 = alpha(lat2);

			double m1 = Math.Cos(lat1) / Math.Sqrt(1 - e_sq * Math.Pow(Math.Sin(lat1), 2));
			double m2 = Math.Cos(lat2) / Math.Sqrt(1 - e_sq * Math.Pow(Math.Sin(lat2), 2));

			n = (Math.Pow(m1, 2) - Math.Pow(m2, 2)) / (alpha2 - alpha1);
			C = Math.Pow(m1, 2) + (n * alpha1);

			ro0 = Ro(alpha(lat0));
			/*
			double sin_p0 = Math.Sin(lat0);
			double cos_p0 = Math.Cos(lat0);
			double q0 = qsfnz(e, sin_p0, cos_p0);

			double sin_p1 = Math.Sin(lat1);
			double cos_p1 = Math.Cos(lat1);
			double m1 = msfnz(e,sin_p1,cos_p1);
			double q1 = qsfnz(e,sin_p1,cos_p1);


			double sin_p2 = Math.Sin(lat2);
			double cos_p2 = Math.Cos(lat2);
			double m2 = msfnz(e,sin_p2,cos_p2);
			double q2 = qsfnz(e,sin_p2,cos_p2);

			if (Math.Abs(lat1 - lat2) > EPSLN)
				ns0 = (m1 * m1 - m2 * m2)/ (q2 - q1);
			else
				ns0 = sin_p1;
			C = m1 * m1 + ns0 * q1;
			rh = this._semiMajor * Math.Sqrt(C - ns0 * q0)/ns0;
			*/
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Converts coordinates in decimal degrees to projected meters.
		/// </summary>
		/// <param name="lonlat">The point in decimal degrees.</param>
		/// <returns>Point in projected meters</returns>
        public override double[] DegreesToMeters(double[] lonlat)
		{
			double dLongitude = Degrees2Radians(lonlat[0]);
			double dLatitude = Degrees2Radians(lonlat[1]);

			double a = alpha(dLatitude);
			double ro = Ro(a);
			double theta = n * (dLongitude - lon_center);
			dLongitude = _falseEasting + ro * Math.Sin(theta);
			dLatitude = _falseNorthing + ro0 - (ro * Math.Cos(theta));
			if (lonlat.Length == 2)
				return new double[] { dLongitude / _metersPerUnit, dLatitude / _metersPerUnit };
			else
				return new double[] { dLongitude / _metersPerUnit, dLatitude / _metersPerUnit, lonlat[2] };
		}

		/// <summary>
		/// Converts coordinates in projected meters to decimal degrees.
		/// </summary>
		/// <param name="p">Point in meters</param>
		/// <returns>Transformed point in decimal degrees</returns>
        public override double[] MetersToDegrees(double[] p) 
		{
			double theta = Math.Atan((p[0] * _metersPerUnit - _falseEasting) / (ro0 - (p[1] * _metersPerUnit - _falseNorthing)));
			double ro = Math.Sqrt(Math.Pow(p[0] * _metersPerUnit - _falseEasting, 2) + Math.Pow(ro0 - (p[1] * _metersPerUnit - _falseNorthing), 2));
			double q = (C - Math.Pow(ro, 2) * Math.Pow(n, 2) / Math.Pow(this._semiMajor, 2)) / n;
			double b = Math.Sin(q / (1 - ((1 - e_sq) / (2 * e)) * Math.Log((1 - e) / (1 + e))));

			double lat = Math.Asin(q * 0.5);
			double preLat = double.MaxValue;
			int iterationCounter = 0;
			while (Math.Abs(lat - preLat) > 0.000001)
			{
				preLat = lat;
				double sin = Math.Sin(lat);
				double e2sin2 = e_sq * Math.Pow(sin, 2);
				lat += (Math.Pow(1 - e2sin2, 2) / (2 * Math.Cos(lat))) * ((q / (1 - e_sq)) - sin / (1 - e2sin2) + 1 / (2 * e) * Math.Log((1 - e * sin) / (1 + e * sin)));
				iterationCounter++;
				if (iterationCounter > 25)
					throw new ArgumentException("Transformation failed to converge in Albers backwards transformation");
			}
			double lon = lon_center + (theta / n);
			if (p.Length == 2)
				return new double[] { Radians2Degrees(lon), Radians2Degrees(lat) };
			else
				return new double[] { Radians2Degrees(lon), Radians2Degrees(lat), p[2] };
		}

		/// <summary>
		/// Returns the inverse of this projection.
		/// </summary>
		/// <returns>IMathTransform that is the reverse of the current projection.</returns>
		public override IMathTransform Inverse()
		{
			if (_inverse == null)
				_inverse = new AlbersProjection(this._Parameters, !_isInverse);
			return _inverse;
		}
		
		#endregion

		#region Math helper functions

		//private double ToAuthalic(double lat)
		//{
		//    return Math.Atan(Q(lat) / Q(Math.PI * 0.5));
		//}
		//private double Q(double angle)
		//{
		//    double sin = Math.Sin(angle);
		//    double esin = e * sin;
		//    return Math.Abs(sin / (1 - Math.Pow(esin, 2)) - 0.5 * e) * Math.Log((1 - esin) / (1 + esin)));
		//}
		private double alpha(double lat)
		{
			double sin = Math.Sin(lat);
			double sinsq = Math.Pow(sin,2);
			return (1 - e_sq) * (((sin / (1 - e_sq * sinsq)) - 1/(2 * e) * Math.Log((1 - e * sin) / (1 + e * sin))));
		}

		private double Ro(double a)
		{
			return this._semiMajor * Math.Sqrt((C - n * a)) / n;
		}

		#endregion
	}
}
