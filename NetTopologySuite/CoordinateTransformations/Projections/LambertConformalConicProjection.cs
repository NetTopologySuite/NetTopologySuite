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
namespace GisSharpBlog.NetTopologySuite.CoordinateTransformations
{

	/// <summary>
	/// Implemetns the Lambert Conformal Conic 2SP Projection.
	/// </summary>
	/// <remarks>
	/// <para>The Lambert Conformal Conic projection is a standard projection for presenting maps
	/// of land areas whose East-West extent is large compared with their North-South extent.
	/// This projection is "conformal" in the sense that lines of latitude and longitude, 
	/// which are perpendicular to one another on the earth's surface, are also perpendicular
	/// to one another in the projected domain.</para>
	/// </remarks>
	/// <exception cref="TransformException"></exception>
	internal class LambertConformalConic2SPProjection : MapProjection
	{
	
		double _falseEasting;
		double _falseNorthing;
	
		private double es=0;              /* eccentricity squared         */
		private double e=0;               /* eccentricity                 */
		private double center_lon=0;      /* center longituted            */
		private double center_lat=0;      /* cetner latitude              */
		private double ns=0;              /* ratio of angle between meridian*/
		private double f0=0;              /* flattening of ellipsoid      */
		private double rh=0;              /* height above ellipsoid       */

		#region Constructors
		/// <summary>
		/// Creates an instance of an LambertConformalConic2SPProjection projection object.
		/// </summary>
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
		/// <param name="parameters">List of parameters to initialize the projection.</param>
		public LambertConformalConic2SPProjection(ParameterList parameters) : this(parameters,false)
		{
		}
	
		/// <summary>
		/// Creates an instance of an Albers projection object.
		/// </summary>
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
		/// <param name="parameters">List of parameters to initialize the projection.</param>
		/// <param name="isInverse">Indicates whether the projection forward (meters to degrees or degrees to meters).</param>
		public LambertConformalConic2SPProjection(ParameterList parameters, bool isInverse) : base(parameters,isInverse)
		{

			double c_lat = Degrees2Radians(_parameters.GetDouble("latitude_of_false_origin"));
			double c_lon = Degrees2Radians(_parameters.GetDouble("longitude_of_false_origin"));
			double lat1 = Degrees2Radians(_parameters.GetDouble("latitude_of_1st_standard_parallel"));
			double lat2 = Degrees2Radians(_parameters.GetDouble("latitude_of_2nd_standard_parallel"));
			this._falseEasting = _parameters.GetDouble("easting_at_false_origin");
			this._falseNorthing = _parameters.GetDouble("northing_at_false_origin");


			double sin_po;                  /* sin value                            */
			double cos_po;                  /* cos value                            */
			double con;                     /* temporary variable                   */
			double ms1;                     /* small m 1                            */
			double ms2;                     /* small m 2                            */
			double temp;                    /* temporary variable                   */
			double ts0;                     /* small t 0                            */
			double ts1;                     /* small t 1                            */
			double ts2;                     /* small t 2                            */



			/* Standard Parallels cannot be equal and on opposite sides of the equator
			------------------------------------------------------------------------*/
			if (Math.Abs(lat1+lat2) < EPSLN)
			{
				//Debug.Assert(true,"LambertConformalConic:LambertConformalConic() - Equal Latitiudes for St. Parallels on opposite sides of equator");
				throw new ArgumentException("Equal Latitiudes for St. Parallels on opposite sides of equator.");
			}

			temp = this._semiMinor / this._semiMajor;
			es = 1.0 - SQUARE(temp);
			e = Math.Sqrt(es);

			
			center_lon = c_lon;
			center_lat = c_lat;
			sincos(lat1,out sin_po,out cos_po);
			con = sin_po;
			ms1 = msfnz(e,sin_po,cos_po);
			ts1 = tsfnz(e,lat1,sin_po);
			sincos(lat2,out sin_po,out cos_po);
			ms2 = msfnz(e,sin_po,cos_po);
			ts2 = tsfnz(e,lat2,sin_po);
			sin_po = Math.Sin(center_lat);
			ts0 = tsfnz(e,center_lat,sin_po);

			if (Math.Abs(lat1 - lat2) > EPSLN)
				ns = Math.Log(ms1/ms2)/ Math.Log (ts1/ts2);
			else
				ns = con;
			f0 = ms1 / (ns * Math.Pow(ts1,ns));
			rh = this._semiMajor * f0 * Math.Pow(ts0,ns);
		}
		#endregion


		/// <summary>
		/// Converts coordinates in decimal degrees to projected meters.
		/// </summary>
		/// <param name="dLongitude">The longitude in decimal degrees.</param>
		/// <param name="dLatitude">The latitude in decimal degrees.</param>
		/// <param name="dX">The resulting x coordinate in projected meters.</param>
		/// <param name="dY">The resutting y coordinate in projected meters.</param>
		public override void DegreesToMeters(double dLongitude, double dLatitude,out double dX, out double dY)
		{
			dLongitude = Degrees2Radians(dLongitude);
			dLatitude = Degrees2Radians(dLatitude);

			dX=Double.NaN;
			dY=Double.NaN;

			double con;                     /* temporary angle variable             */
			double rh1;                     /* height above ellipsoid               */
			double sinphi;                  /* sin value                            */
			double theta;                   /* angle                                */
			double ts;                      /* small value t                        */


			con  = Math.Abs( Math.Abs(dLatitude) - HALF_PI);
			if (con > EPSLN)
			{
				sinphi = Math.Sin(dLatitude);
				ts = tsfnz(e,dLatitude,sinphi);
				rh1 = this._semiMajor * f0 * Math.Pow(ts,ns);
			}
			else
			{
				con = dLatitude * ns;
				if (con <= 0)
				{
					throw new TransformException();
				}
				rh1 = 0;
			}
			theta = ns * adjust_lon(dLongitude - center_lon);
			dX = rh1 * Math.Sin(theta) + this._falseEasting;
			dY = rh - rh1 * Math.Cos(theta) + this._falseNorthing;
	
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
			dLongitude =Double.NaN;
			dLatitude =Double.NaN;

			double rh1;			/* height above ellipsoid	*/
			double con;			/* sign variable		*/
			double ts;			/* small t			*/
			double theta;			/* angle			*/
			long   flag;			/* error flag			*/

			flag = 0;
			dX -= this._falseEasting;
			dY = rh - dY + this._falseNorthing;
			if (ns > 0)
			{
				rh1 = Math.Sqrt(dX * dX + dY * dY);
				con = 1.0;
			}
			else
			{
				rh1 = -Math.Sqrt(dX * dX + dY * dY);
				con = -1.0;
			}
			theta = 0.0;
			if (rh1 != 0)
				theta = Math.Atan2((con * dX),(con * dY));
			if ((rh1 != 0) || (ns > 0.0))
			{
				con = 1.0/ns;
				ts = Math.Pow((rh1/(this._semiMajor * f0)),con);
				dLatitude = phi2z(e,ts,out flag);
				if (flag != 0)
				{
					throw new TransformException();
				}
			}
			else
			{
				dLatitude = -HALF_PI;
			}
			dLongitude = adjust_lon(theta/ns + center_lon);

			dLongitude = Radians2Degrees(dLongitude);
			dLatitude = Radians2Degrees(dLatitude);
		}

		/// <summary>
		/// Returns the inverse of this projection.
		/// </summary>
		/// <returns>IMathTransform that is the reverse of the current projection.</returns>
		public override IMathTransform GetInverse()
		{
			if (_inverse==null)
			{
				_inverse = new LambertConformalConic2SPProjection(this._parameters, ! _isInverse);
			}
			return _inverse;
		}
	}
}
