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
using GisSharpBlog.NetTopologySuite.Utilities;
namespace GisSharpBlog.NetTopologySuite.CoordinateTransformations
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
	internal class MercatorProjection: MapProjection
	{
		double _falseEasting;
		double _falseNorthing;
		double lon_center;		//Center longitude (projection center)
		double lat_origin;		//center latitude
		double e,es;			//eccentricity constants
		double m1;				//small value m
		double _scaleFactor;

		/// <summary>
		/// Initializes the MercatorProjection object with the specified parameters to project points. 
		/// </summary>
		/// <param name="parameters">ParaemterList with the required parameters.</param>
		/// <remarks>
		/// </remarks>
		public MercatorProjection(ParameterList parameters) : this(parameters,false)
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
		/// <item><term>longitude_of_natural_origin</term><description>The longitude of the point from which the values of both the geographical coordinates on the ellipsoid and the grid coordinates on the projection are deemed to increment or decrement for computational purposes. Alternatively it may be considered as the longitude of the point which in the absence of application of false coordinates has grid coordinates of (0,0).  Sometimes known as String.Emptycentral meridianString.Empty."</description></item>
		/// <item><term>latitude_of_natural_origin</term><description>The latitude of the point from which the values of both the geographical coordinates on the ellipsoid and the grid coordinates on the projection are deemed to increment or decrement for computational purposes. Alternatively it may be considered as the latitude of the point which in the absence of application of false coordinates has grid coordinates of (0,0).</description></item>
		/// <item><term>scale_factor_at_natural_origin</term><description>The factor by which the map grid is reduced or enlarged during the projection process, defined by its value at the natural origin.</description></item>
		/// <item><term>false_easting</term><description>Since the natural origin may be at or near the centre of the projection and under normal coordinate circumstances would thus give rise to negative coordinates over parts of the mapped area, this origin is usually given false coordinates which are large enough to avoid this inconvenience. The False Easting, FE, is the easting value assigned to the abscissa (east).</description></item>
		/// <item><term>false_northing</term><description>Since the natural origin may be at or near the centre of the projection and under normal coordinate circumstances would thus give rise to negative coordinates over parts of the mapped area, this origin is usually given false coordinates which are large enough to avoid this inconvenience. The False Northing, FN, is the northing value assigned to the ordinate .</description></item>
		/// </list>
		/// </remarks>
		public MercatorProjection(ParameterList parameters, bool isInverse) : base(parameters,isInverse)
		{
			lon_center = Degrees2Radians( _parameters.GetDouble("longitude_of_natural_origin") );
			lat_origin = Degrees2Radians( _parameters.GetDouble("latitude_of_natural_origin") );
			_scaleFactor =  _parameters.GetDouble("scale_factor_at_natural_origin");
			_falseEasting		= _parameters.GetDouble("false_easting");
			_falseNorthing		= _parameters.GetDouble("false_northing");
			
			//double temp = r_minor / r_major;
			double temp = this._semiMinor / this._semiMajor;
			es = 1.0 - temp*temp;
			e = Math.Sqrt(es);
			m1 = Math.Cos(lat_origin)/(Math.Sqrt(1.0 - es * Math.Sin(lat_origin) * Math.Sin(lat_origin)));
		}

		/// <summary>
		/// Converts coordinates in decimal degrees to projected meters.
		/// </summary>
		/// <remarks>
		/// <para>The parameters this projection expects are listed below.</para>
		/// <list type="table">
		/// <listheader><term>Items</term><description>Descriptions</description></listheader>
		/// <item><term>longitude_of_natural_origin</term><description>The longitude of the point from which the values of both the geographical coordinates on the ellipsoid and the grid coordinates on the projection are deemed to increment or decrement for computational purposes. Alternatively it may be considered as the longitude of the point which in the absence of application of false coordinates has grid coordinates of (0,0).  Sometimes known as String.Emptycentral meridianString.Empty."</description></item>
		/// <item><term>latitude_of_natural_origin</term><description>The latitude of the point from which the values of both the geographical coordinates on the ellipsoid and the grid coordinates on the projection are deemed to increment or decrement for computational purposes. Alternatively it may be considered as the latitude of the point which in the absence of application of false coordinates has grid coordinates of (0,0).</description></item>
		/// <item><term>scale_factor_at_natural_origin</term><description>The factor by which the map grid is reduced or enlarged during the projection process, defined by its value at the natural origin.</description></item>
		/// <item><term>false_easting</term><description>Since the natural origin may be at or near the centre of the projection and under normal coordinate circumstances would thus give rise to negative coordinates over parts of the mapped area, this origin is usually given false coordinates which are large enough to avoid this inconvenience. The False Easting, FE, is the easting value assigned to the abscissa (east).</description></item>
		/// <item><term>false_northing</term><description>Since the natural origin may be at or near the centre of the projection and under normal coordinate circumstances would thus give rise to negative coordinates over parts of the mapped area, this origin is usually given false coordinates which are large enough to avoid this inconvenience. The False Northing, FN, is the northing value assigned to the ordinate .</description></item>
		/// </list>
		/// </remarks>
		/// <param name="dLongitude">The longitude in decimal degrees.</param>
		/// <param name="dLatitude">The latitude in decimal degrees.</param>
		/// <param name="dX">The resulting x coordinate in projected meters.</param>
		/// <param name="dY">The resutting y coordinate in projected meters.</param>
		public override void DegreesToMeters(double dLongitude, double dLatitude,out double dX, out double dY)
		{
			dLongitude = Degrees2Radians(dLongitude);
			dLatitude = Degrees2Radians(dLatitude);

			/* Forward equations
			  -----------------*/
			if (Math.Abs(Math.Abs(dLatitude) - HALF_PI)  <= EPSLN)
			{
				throw new TransformException("Transformation cannot be computed at the poles.");
			}
			else
			{
				double sinphi = Math.Sin(dLatitude);
				double ts = tsfnz(e,dLatitude,sinphi);
				dX = this._falseEasting + this._semiMajor * m1 * adjust_lon(dLongitude - lon_center);
				dY = this._falseNorthing - this._semiMajor * m1 * Math.Log(ts);
			}
		}

		/// <summary>
		/// Converts coordinates in projected meters to decimal degrees.
		/// </summary>
		/// <param name="dX">The x coordinate in projected meters.</param>
		/// <param name="dY">The y coordinate in projected meters.</param>
		/// <param name="dLongitude">The resulting longitude in decimal degrees.</param>
		/// <param name="dLatitude">The resulitng latitude in decimal degrees.</param>
		public override void MetersToDegrees(double dX, double dY,out double dLongitude, out double dLatitude)
		{	
			dLongitude =Double.NaN ; 
			dLatitude =Double.NaN ;
	
			/* Inverse equations
			  -----------------*/
			long flag = 0;
			dX -= this._falseEasting;
			dY -= this._falseNorthing;
			double ts = Math.Exp(-dY/(this._semiMajor * m1));
			dLatitude = phi2z(e,ts,out flag);
			if (flag != 0)
			{
				throw new TransformException();
			}
			dLongitude = adjust_lon(lon_center + dX/(this._semiMajor * m1));

			dLongitude = Radians2Degrees(dLongitude);
			dLatitude = Radians2Degrees(dLatitude);
		}
		
		


		/// <summary>
		/// Returns the inverse of this projection.
		/// </summary>
		/// <returns>IMathTransform that is the reverse of the current projection.</returns>
		public IMathTransform Inverse()
		{
			if (_inverse==null)
			{
				_inverse = new MercatorProjection(this._parameters, ! _isInverse);
			}
			return _inverse;
		}
	}
}
