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

using System;

namespace ProjNet.CoordinateSystems
{ 
	/// <summary>
	/// Parameters for a geographic transformation into WGS84. The Bursa Wolf parameters should be applied 
	/// to geocentric coordinates, where the X axis points towards the Greenwich Prime Meridian, the Y axis
	/// points East, and the Z axis points North.
	/// </summary>
	/// <remarks>
	/// <para>These parameters can be used to approximate a transformation from the horizontal datum to the
	/// WGS84 datum using a Bursa Wolf transformation. However, it must be remembered that this transformation
	/// is only an approximation. For a given horizontal datum, different Bursa Wolf transformations can be
	/// used to minimize the errors over different regions.</para>
	/// <para>If the DATUM clause contains a TOWGS84 clause, then this should be its “preferred” transformation,
	/// which will often be the transformation which gives a broad approximation over the whole area of interest
	/// (e.g. the area of interest in the containing geographic coordinate system).</para>
	/// <para>Sometimes, only the first three or six parameters are defined. In this case the remaining
	/// parameters must be zero. If only three parameters are defined, then they can still be plugged into the
	/// Bursa Wolf formulas, or you can take a short cut. The Bursa Wolf transformation works on geocentric
	/// coordinates, so you cannot apply it onto geographic coordinates directly. If there are only three
	/// parameters then you can use the Molodenski or abridged Molodenski formulas.</para>
	/// <para>If a datums ToWgs84Parameters parameter values are zero, then the receiving
	/// application can assume that the writing application believed that the datum is approximately equal to
	/// WGS84.</para>
	/// </remarks>
	public class Wgs84ConversionInfo : IEquatable<Wgs84ConversionInfo>
	{
		private const double SEC_TO_RAD = 4.84813681109535993589914102357e-6;
		/// <summary>
		/// Initializes an instance of Wgs84ConversionInfo with default parameters (all values = 0)
		/// </summary>
		public Wgs84ConversionInfo() : this(0,0,0,0,0,0,0,String.Empty)
		{
		}
		/// <summary>
		/// Initializes an instance of Wgs84ConversionInfo
		/// </summary>
		/// <param name="dx">Bursa Wolf shift in meters.</param>
		/// <param name="dy">Bursa Wolf shift in meters.</param>
		/// <param name="dz">Bursa Wolf shift in meters.</param>
		/// <param name="ex">Bursa Wolf rotation in arc seconds.</param>
		/// <param name="ey">Bursa Wolf rotation in arc seconds.</param>
		/// <param name="ez">Bursa Wolf rotation in arc seconds.</param>
		/// <param name="ppm">Bursa Wolf scaling in parts per million.</param>
		public Wgs84ConversionInfo(double dx, double dy, double dz, double ex, double ey, double ez, double ppm)
			:
			this(dx, dy, dz, ex, ey, ez, ppm, String.Empty)
		{
		}
		/// <summary>
		/// Initializes an instance of Wgs84ConversionInfo
		/// </summary>
		/// <param name="dx">Bursa Wolf shift in meters.</param>
		/// <param name="dy">Bursa Wolf shift in meters.</param>
		/// <param name="dz">Bursa Wolf shift in meters.</param>
		/// <param name="ex">Bursa Wolf rotation in arc seconds.</param>
		/// <param name="ey">Bursa Wolf rotation in arc seconds.</param>
		/// <param name="ez">Bursa Wolf rotation in arc seconds.</param>
		/// <param name="ppm">Bursa Wolf scaling in parts per million.</param>
		/// <param name="areaOfUse">Area of use for this transformation</param>
		public Wgs84ConversionInfo(double dx, double dy, double dz, double ex, double ey, double ez, double ppm, string areaOfUse)
		{
			Dx = dx; Dy = dy; Dz = dz;
			Ex = ex; Ey = ey; Ez = ez;
			Ppm = ppm;
			AreaOfUse = areaOfUse;
		}

		/// <summary>
		/// Bursa Wolf shift in meters.
		/// </summary>
		public double Dx;
		/// <summary>
		/// Bursa Wolf shift in meters.
		/// </summary>
		public double Dy;
		/// <summary>
		/// Bursa Wolf shift in meters.
		/// </summary>
		public double Dz;
		/// <summary>
		/// Bursa Wolf rotation in arc seconds.
		/// </summary>
		public double Ex;
		/// <summary>
		/// Bursa Wolf rotation in arc seconds.
		/// </summary>
		public double Ey;
		/// <summary>
		/// Bursa Wolf rotation in arc seconds.
		/// </summary>
		public double Ez;
		/// <summary>
		/// Bursa Wolf scaling in parts per million.
		/// </summary>
		public double Ppm;
		/// <summary>
		/// Human readable text describing intended region of transformation.
		/// </summary>
		public string AreaOfUse;

		/// <summary>
		/// Affine Bursa-Wolf matrix transformation
		/// </summary>
		/// <remarks>
		/// <para>Transformation of coordinates from one geographic coordinate system into another 
		/// (also colloquially known as a "datum transformation") is usually carried out as an 
		/// implicit concatenation of three transformations:</para>
		/// <para>[geographical to geocentric >> geocentric to geocentric >> geocentric to geographic</para>
		/// <para>
		/// The middle part of the concatenated transformation, from geocentric to geocentric, is usually 
		/// described as a simplified 7-parameter Helmert transformation, expressed in matrix form with 7 
		/// parameters, in what is known as the "Bursa-Wolf" formula:<br/>
		/// <code>
		///  S = 1 + Ppm/1000000
		///  [ Xt ]    [     S   -Ez*S   +Ey*S   Dx ]  [ Xs ]
		///  [ Yt ]  = [ +Ez*S       S   -Ex*S   Dy ]  [ Ys ]
		///  [ Zt ]    [ -Ey*S   +Ex*S       S   Dz ]  [ Zs ]
		///  [ 1  ]    [     0       0       0    1 ]  [ 1  ]
		/// </code><br/>
		/// The parameters are commonly referred to defining the transformation "from source coordinate system 
		/// to target coordinate system", whereby (XS, YS, ZS) are the coordinates of the point in the source 
		/// geocentric coordinate system and (XT, YT, ZT) are the coordinates of the point in the target 
		/// geocentric coordinate system. But that does not define the parameters uniquely; neither is the
		/// definition of the parameters implied in the formula, as is often believed. However, the 
		/// following definition, which is consistent with the "Position Vector Transformation" convention, 
		/// is common E&amp;P survey practice: 
		/// </para>	
		/// <para>(dX, dY, dZ): Translation vector, to be added to the point's position vector in the source 
		/// coordinate system in order to transform from source system to target system; also: the coordinates 
		/// of the origin of source coordinate system in the target coordinate system </para>
		/// <para>(RX, RY, RZ): Rotations to be applied to the point's vector. The sign convention is such that 
		/// a positive rotation about an axis is defined as a clockwise rotation of the position vector when 
		/// viewed from the origin of the Cartesian coordinate system in the positive direction of that axis;
		/// e.g. a positive rotation about the Z-axis only from source system to target system will result in a
		/// larger longitude value for the point in the target system. Although rotation angles may be quoted in
		/// any angular unit of measure, the formula as given here requires the angles to be provided in radians.</para>
		/// <para>: The scale correction to be made to the position vector in the source coordinate system in order 
		/// to obtain the correct scale in the target coordinate system. M = (1 + dS*10-6), whereby dS is the scale
		/// correction expressed in parts per million.</para>
		/// <para><see href="http://www.posc.org/Epicentre.2_2/DataModel/ExamplesofUsage/eu_cs35.html"/> for an explanation of the Bursa-Wolf transformation</para>
		/// </remarks>
		/// <returns></returns>
		internal double[] GetAffineTransform()
		{
			double RS = 1 + Ppm * 0.000001;
			return new double[7] { RS, Ex * SEC_TO_RAD * RS, Ey * SEC_TO_RAD * RS, Ez * SEC_TO_RAD * RS, Dx, Dy, Dz };
			/*return new double[3,4] {
				{ RS,				-Ez*SEC_TO_RAD*RS,	+Ey*SEC_TO_RAD*RS,	Dx} ,
				{ Ez*SEC_TO_RAD*RS,	RS,					-Ex*SEC_TO_RAD*RS,	Dy} ,
				{ -Ey*SEC_TO_RAD*RS,Ex*SEC_TO_RAD*RS,	RS,					Dz}
			};*/
		}

		

		/// <summary>
		/// Returns the Well Known Text (WKT) for this object.
		/// </summary>
		/// <remarks>The WKT format of this object is: <code>TOWGS84[dx, dy, dz, ex, ey, ez, ppm]</code></remarks>
		/// <returns>WKT representaion</returns>
		public string WKT
		{
			get
			{
				return String.Format(System.Globalization.CultureInfo.InvariantCulture.NumberFormat, "TOWGS84[{0}, {1}, {2}, {3}, {4}, {5}, {6}]", Dx, Dy, Dz, Ex, Ey, Ez, Ppm);
			}
		}

		/// <summary>
		/// Gets an XML representation of this object
		/// </summary>
		public string XML
		{
			get
			{
                return String.Format(System.Globalization.CultureInfo.InvariantCulture.NumberFormat, "<CS_WGS84ConversionInfo Dx=\"{0}\" Dy=\"{1}\" Dz=\"{2}\" Ex=\"{3}\" Ey=\"{4}\" Ez=\"{5}\" Ppm=\"{6}\" />", Dx, Dy, Dz, Ex, Ey, Ez, Ppm);
			}
		}

		/// <summary>
		/// Returns the Well Known Text (WKT) for this object.
		/// </summary>
		/// <remarks>The WKT format of this object is: <code>TOWGS84[dx, dy, dz, ex, ey, ez, ppm]</code></remarks>
		/// <returns>WKT representaion</returns>
		public override string ToString()
		{
			return WKT;
		}

		/// <summary>
		/// Returns true of all 7 parameter values are 0.0
		/// </summary>
		/// <returns></returns>
		public bool HasZeroValuesOnly
		{
			get {
				return !(Dx != 0 || Dy != 0 || Dz != 0 || Ex != 0 || Ey != 0 || Ez != 0 || Ppm != 0);
			}
		}

		#region IEquatable<Wgs84ConversionInfo> Members

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as Wgs84ConversionInfo);
		}

		/// <summary>
		/// Returns a hash code for the specified object
		/// </summary>
		/// <returns>A hash code for the specified object</returns>
		public override int GetHashCode()
		{
			return Dx.GetHashCode() ^ Dy.GetHashCode() ^ Dz.GetHashCode() ^
				Ex.GetHashCode() ^ Ey.GetHashCode() ^ Ez.GetHashCode() ^
				Ppm.GetHashCode();
		}

		/// <summary>
		/// Checks whether the values of this instance is equal to the values of another instance.
		/// Only parameters used for coordinate system are used for comparison.
		/// Name, abbreviation, authority, alias and remarks are ignored in the comparison.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns>True if equal</returns>
		public bool Equals(Wgs84ConversionInfo obj)
		{
			if (obj == null)
				return false;
			return obj.Dx == this.Dx && obj.Dy == this.Dy && obj.Dz == this.Dz &&
				obj.Ex == this.Ex && obj.Ey == this.Ey && obj.Ez == this.Ez && obj.Ppm == this.Ppm;
		}

		#endregion
	}
}

