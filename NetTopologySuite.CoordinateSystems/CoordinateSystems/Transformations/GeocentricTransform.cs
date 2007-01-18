// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Text;

using GisSharpBlog.NetTopologySuite.CoordinateSystems;
using GisSharpBlog.NetTopologySuite.CoordinateSystems.Transformations;
using GisSharpBlog.NetTopologySuite.Geometries.LightStructs;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems.Transformations
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// <para>Latitude, Longitude and ellipsoidal height in terms of a 3-dimensional geographic system
	/// may by expressed in terms of a geocentric (earth centered) Cartesian coordinate reference system
	/// X, Y, Z with the Z axis corresponding to the earth's rotation axis positive northwards, the X
	/// axis through the intersection of the prime meridian and equator, and the Y axis through
	/// the intersection of the equator with longitude 90 degrees east. The geographic and geocentric
	/// systems are based on the same geodetic datum.</para>
	/// <para>Geocentric coordinate reference systems are conventionally taken to be defined with the X
	/// axis through the intersection of the Greenwich meridian and equator. This requires that the equivalent
	/// geographic coordinate reference systems based on a non-Greenwich prime meridian should first be
	/// transformed to their Greenwich equivalent. Geocentric coordinates X, Y and Z take their units from
	/// the units of the ellipsoid axes (a and b). As it is conventional for X, Y and Z to be in metres,
	/// if the ellipsoid axis dimensions are given in another linear unit they should first be converted
	/// to metres.</para>
	/// </remarks>
	internal class GeocentricTransform : MathTransform
	{
		private const double COS_67P5 = 0.38268343236508977;  /* cosine of 67.5 degrees */
		private const double AD_C = 1.0026000;            /* Toms region 1 constant */

 
		protected bool _isInverse = false;
		/// <summary>
		/// Eccentricity squared : (a^2 - b^2)/a^2
		/// </summary>
		private double es;
		private double semiMajor;		// major axis
		private double semiMinor;		// minor axis
		private double ab;				// Semi_major / semi_minor
		private double ba;				// Semi_minor / semi_major
		/// <summary>
		/// Second eccentricity squared : (a^2 - b^2)/b^2    
		/// </summary>
		private double ses;
		protected List<ProjectionParameter> _Parameters;
		protected MathTransform _inverse;

		/// <summary>
		/// Initializes a geocentric projection object
		/// </summary>
		/// <param name="parameters">List of parameters to initialize the projection.</param>
		/// <param name="isInverse">Indicates whether the projection forward (meters to degrees or degrees to meters).</param>
		public GeocentricTransform(List<ProjectionParameter> parameters, bool isInverse)
			: this(parameters)
		{
			_isInverse = isInverse;
		}

		/// <summary>
		/// Initializes a geocentric projection object
		/// </summary>
		/// <param name="parameters">List of parameters to initialize the projection.</param>
		internal GeocentricTransform(List<ProjectionParameter> parameters)
		{
			_Parameters = parameters;
			semiMajor = _Parameters.Find(delegate(ProjectionParameter par)
								{ return par.Name.Equals("semi_major", StringComparison.OrdinalIgnoreCase); }).Value;
			semiMinor = _Parameters.Find(delegate(ProjectionParameter par)
								{ return par.Name.Equals("semi_minor", StringComparison.OrdinalIgnoreCase); }).Value;

			es = 1.0 - (semiMinor * semiMinor) / (semiMajor * semiMajor); //e^2
			ses = (Math.Pow(semiMajor, 2) - Math.Pow(semiMinor, 2)) / Math.Pow(semiMinor, 2);
			ba = semiMinor / semiMajor;
			ab = semiMajor / semiMinor;
		}


		/// <summary>
		/// Returns the inverse of this conversion.
		/// </summary>
		/// <returns>IMathTransform that is the reverse of the current conversion.</returns>
		public override IMathTransform Inverse()
		{
			if (_inverse == null)
				_inverse = new GeocentricTransform(this._Parameters, !_isInverse);
			return _inverse;
		}

		/// <summary>
		/// Converts coordinates in decimal degrees to projected meters.
		/// </summary>
		/// <param name="lonlat">The point in decimal degrees.</param>
		/// <returns>Point in projected meters</returns>
		private Point DegreesToMeters(Point lonlat)
		{
			double lon = Degrees2Radians(lonlat.X);
			double lat = Degrees2Radians(lonlat.Y);            
            double h = lonlat.Z.Equals(Double.NaN) ? 0 : lonlat.Z;  // HACK: lonlat.Z == Double.NaN not works
			double v = semiMajor / Math.Sqrt(1 - es * Math.Pow(Math.Sin(lat), 2));
			double x = (v + h) * Math.Cos(lat) * Math.Cos(lon);
			double y = (v + h) * Math.Cos(lat) * Math.Sin(lon);
			double z = ((1 - es) * v + h) * Math.Sin(lat);
			return new Point(x, y, z);
		}

		/// <summary>
		/// Converts coordinates in projected meters to decimal degrees.
		/// </summary>
		/// <param name="pnt">Point in meters</param>
		/// <returns>Transformed point in decimal degrees</returns>		
		private Point MetersToDegrees(Point pnt)
		{
			bool At_Pole = false; // indicates whether location is in polar region */
            double Z = pnt.Z.Equals(Double.NaN) ? 0 : pnt.Z; // HACK: lonlat.Z == Double.NaN not works

			double lon = 0;
			double lat = 0;
			double Height = 0;
			if (pnt.X != 0.0)
				lon = Math.Atan2(pnt.Y, pnt.X);
			else
			{
				if (pnt.Y > 0)
					lon = Math.PI/2;
				else if (pnt.Y < 0)
					lon = -Math.PI * 0.5;
				else
				{
					At_Pole = true;
					lon = 0.0;
					if (Z > 0.0)
					{   /* north pole */
						lat = Math.PI * 0.5;
					}
					else if (Z < 0.0)
					{   /* south pole */
						lat = -Math.PI * 0.5;
					}
					else
					{   /* center of earth */
						return new Point(Radians2Degrees(lon), Radians2Degrees(Math.PI * 0.5), -semiMinor);
					}
				}
			}
			double W2 = pnt.X * pnt.X + pnt.Y * pnt.Y; // Square of distance from Z axis
			double W = Math.Sqrt(W2); // distance from Z axis
			double T0 = Z * AD_C; // initial estimate of vertical component
			double S0 = Math.Sqrt(T0 * T0 + W2); //initial estimate of horizontal component
			double Sin_B0 = T0 / S0; //sin(B0), B0 is estimate of Bowring aux variable
			double Cos_B0 = W / S0; //cos(B0)
			double Sin3_B0 = Math.Pow(Sin_B0, 3);
			double T1 = Z + semiMinor * ses * Sin3_B0; //corrected estimate of vertical component
			double Sum = W - semiMajor * es * Cos_B0 * Cos_B0 * Cos_B0; //numerator of cos(phi1)
			double S1 = Math.Sqrt(T1 * T1 + Sum * Sum); //corrected estimate of horizontal component
			double Sin_p1 = T1 / S1; //sin(phi1), phi1 is estimated latitude
			double Cos_p1 = Sum / S1; //cos(phi1)
			double Rn = semiMajor / Math.Sqrt(1.0 - es * Sin_p1 * Sin_p1); //Earth radius at location
			if (Cos_p1 >= COS_67P5)
				Height = W / Cos_p1 - Rn;
			else if (Cos_p1 <= -COS_67P5)
				 Height = W / -Cos_p1 - Rn;
			else Height = Z / Sin_p1 + Rn * (es - 1.0);
			if(!At_Pole)
				lat = Math.Atan(Sin_p1 / Cos_p1);
			return new Point(Radians2Degrees(lon), Radians2Degrees(lat), Height);
		}

		public override Point Transform(Point point)
		{
			if (!_isInverse)
				return this.DegreesToMeters(point);
			else
				return this.MetersToDegrees(point);
		}

		public override List<Point> TransformList(List<Point> points)
		{
			List<Point> result = new List<Point>(points.Count);
			for (int i = 0; i < points.Count; i++)
			{
				Point point = points[i];
				result.Add(Transform(point));
			}
			return result;
		}

		/// <summary>
		/// Reverses the transformation
		/// </summary>
		public override void Invert()
		{
			_isInverse = !_isInverse;
		}

		public override string WKT
		{
			get { throw new NotImplementedException("The method or operation is not implemented."); }
		}
		public override string XML
		{
			get { throw new NotImplementedException("The method or operation is not implemented."); }
		}
	}
}
