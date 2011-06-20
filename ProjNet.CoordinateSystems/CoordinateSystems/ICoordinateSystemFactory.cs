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
using System.Collections.Generic;
using System.Text;

namespace ProjNet.CoordinateSystems
{
	/// <summary>
	/// Builds up complex objects from simpler objects or values.
	/// </summary>
	/// <remarks>
	/// <para>ICoordinateSystemFactory allows applications to make coordinate systems that 
	/// cannot be created by a <see cref="ICoordinateSystemAuthorityFactory"/>. This factory is very 
	/// flexible, whereas the authority factory is easier to use.</para>
	/// <para>So <see cref="ICoordinateSystemAuthorityFactory"/>can be used to make 'standard' coordinate 
	/// systems, and <see cref="CoordinateSystemFactory"/> can be used to make 'special' 
	/// coordinate systems.</para>
	/// <para>For example, the EPSG authority has codes for USA state plane coordinate systems 
	/// using the NAD83 datum, but these coordinate systems always use meters. EPSG does not 
	/// have codes for NAD83 state plane coordinate systems that use feet units. This factory
	/// lets an application create such a hybrid coordinate system.</para>
	/// </remarks>
	public interface ICoordinateSystemFactory
	{
		/// <summary>
		/// Creates a <see cref="ICompoundCoordinateSystem"/>.
		/// </summary>
		/// <param name="name">Name of compound coordinate system.</param>
		/// <param name="head">Head coordinate system</param>
		/// <param name="tail">Tail coordinate system</param>
		/// <returns>Compound coordinate system</returns>
		ICompoundCoordinateSystem CreateCompoundCoordinateSystem(string name, ICoordinateSystem head, ICoordinateSystem tail);

		/// <summary>
		/// Creates an <see cref="IEllipsoid"/> from radius values.
		/// </summary>
		/// <seealso cref="CreateFlattenedSphere"/>
		/// <param name="name">Name of ellipsoid</param>
		/// <param name="semiMajorAxis"></param>
		/// <param name="semiMinorAxis"></param>
		/// <param name="linearUnit"></param>
		/// <returns>Ellipsoid</returns>
		IEllipsoid CreateEllipsoid(string name, double semiMajorAxis, double semiMinorAxis, ILinearUnit linearUnit);

		/// <summary>
		/// Creates a <see cref="IFittedCoordinateSystem"/>.
		/// </summary>
		/// <remarks>The units of the axes in the fitted coordinate system will be 
		/// inferred from the units of the base coordinate system. If the affine map
		/// performs a rotation, then any mixed axes must have identical units. For
		/// example, a (lat_deg,lon_deg,height_feet) system can be rotated in the 
		/// (lat,lon) plane, since both affected axes are in degrees. But you 
		/// should not rotate this coordinate system in any other plane.</remarks>
		/// <param name="name">Name of coordinate system</param>
		/// <param name="baseCoordinateSystem">Base coordinate system</param>
		/// <param name="toBaseWkt"></param>
		/// <param name="arAxes"></param>
		/// <returns>Fitted coordinate system</returns>
		IFittedCoordinateSystem CreateFittedCoordinateSystem(string name, ICoordinateSystem baseCoordinateSystem, string toBaseWkt, List<AxisInfo> arAxes);

		/// <summary>
		/// Creates an <see cref="IEllipsoid"/> from an major radius, and inverse flattening.
		/// </summary>
		/// <seealso cref="CreateEllipsoid"/>
		/// <param name="name">Name of ellipsoid</param>
		/// <param name="semiMajorAxis">Semi major-axis</param>
		/// <param name="inverseFlattening">Inverse flattening</param>
		/// <param name="linearUnit">Linear unit</param>
		/// <returns>Ellipsoid</returns>
		IEllipsoid CreateFlattenedSphere(string name, double semiMajorAxis, double inverseFlattening, ILinearUnit linearUnit);


		/// <summary>
		/// Creates a coordinate system object from an XML string.
		/// </summary>
		/// <param name="xml">XML representation for the spatial reference</param>
		/// <returns>The resulting spatial reference object</returns>
		ICoordinateSystem CreateFromXml(string xml);

		/// <summary>
		/// Creates a spatial reference object given its Well-known text representation.
		/// The output object may be either a <see cref="IGeographicCoordinateSystem"/> or
		/// a <see cref="IProjectedCoordinateSystem"/>.
		/// </summary>
		/// <param name="WKT">The Well-known text representation for the spatial reference</param>
		/// <returns>The resulting spatial reference object</returns>
		ICoordinateSystem CreateFromWkt(string WKT);

		/// <summary>
		/// Creates a <see cref="IGeographicCoordinateSystem"/>, which could be Lat/Lon or Lon/Lat.
		/// </summary>
		/// <param name="name">Name of geographical coordinate system</param>
		/// <param name="angularUnit">Angular units</param>
		/// <param name="datum">Horizontal datum</param>
		/// <param name="primeMeridian">Prime meridian</param>
		/// <param name="axis0">First axis</param>
		/// <param name="axis1">Second axis</param>
		/// <returns>Geographic coordinate system</returns>
		IGeographicCoordinateSystem CreateGeographicCoordinateSystem(string name, IAngularUnit angularUnit, IHorizontalDatum datum, IPrimeMeridian primeMeridian, AxisInfo axis0, AxisInfo axis1);

		/// <summary>
		/// Creates <see cref="IHorizontalDatum"/> from ellipsoid and Bursa-World parameters.
		/// </summary>
		/// <remarks>
		/// Since this method contains a set of Bursa-Wolf parameters, the created 
		/// datum will always have a relationship to WGS84. If you wish to create a
		/// horizontal datum that has no relationship with WGS84, then you can 
		/// either specify a <see cref="DatumType">horizontalDatumType</see> of <see cref="DatumType.HD_Other"/>, or create it via WKT.
		/// </remarks>
		/// <param name="name">Name of ellipsoid</param>
		/// <param name="datumType">Type of datum</param>
		/// <param name="ellipsoid">Ellipsoid</param>
		/// <param name="toWgs84">Wgs84 conversion parameters</param>
		/// <returns>Horizontal datum</returns>
		IHorizontalDatum CreateHorizontalDatum(string name, DatumType datumType, IEllipsoid ellipsoid, Wgs84ConversionInfo toWgs84);
				
		/// <summary>
		/// Creates a <see cref="ILocalCoordinateSystem">local coordinate system</see>.
		/// </summary>
		/// <remarks>
		///  The dimension of the local coordinate system is determined by the size of 
		/// the axis array. All the axes will have the same units. If you want to make 
		/// a coordinate system with mixed units, then you can make a compound 
		/// coordinate system from different local coordinate systems.
		/// </remarks>
		/// <param name="name">Name of local coordinate system</param>
		/// <param name="datum">Local datum</param>
		/// <param name="unit">Units</param>
		/// <param name="axes">Axis info</param>
		/// <returns>Local coordinate system</returns>
		ILocalCoordinateSystem CreateLocalCoordinateSystem(string name, ILocalDatum datum, IUnit unit, List<AxisInfo> axes);

		/// <summary>
		/// Creates a <see cref="ILocalDatum"/>.
		/// </summary>
		/// <param name="name">Name of datum</param>
		/// <param name="datumType">Datum type</param>
		/// <returns></returns>
		ILocalDatum CreateLocalDatum(string name, DatumType datumType);

		/// <summary>
		/// Creates a <see cref="IPrimeMeridian"/>, relative to Greenwich.
		/// </summary>
		/// <param name="name">Name of prime meridian</param>
		/// <param name="angularUnit">Angular unit</param>
		/// <param name="longitude">Longitude</param>
		/// <returns>Prime meridian</returns>
		IPrimeMeridian CreatePrimeMeridian(string name, IAngularUnit angularUnit, double longitude);

		/// <summary>
		/// Creates a <see cref="IProjectedCoordinateSystem"/> using a projection object.
		/// </summary>
		/// <param name="name">Name of projected coordinate system</param>
		/// <param name="gcs">Geographic coordinate system</param>
		/// <param name="projection">Projection</param>
		/// <param name="linearUnit">Linear unit</param>
		/// <param name="axis0">Primary axis</param>
		/// <param name="axis1">Secondary axis</param>
		/// <returns>Projected coordinate system</returns>
		IProjectedCoordinateSystem CreateProjectedCoordinateSystem(string name, IGeographicCoordinateSystem gcs, IProjection projection, ILinearUnit linearUnit, AxisInfo axis0, AxisInfo axis1);

		/// <summary>
		/// Creates a <see cref="IProjection"/>.
		/// </summary>
		/// <param name="name">Name of projection</param>
		/// <param name="wktProjectionClass">Projection class</param>
		/// <param name="Parameters">Projection parameters</param>
		/// <returns>Projection</returns>
		IProjection CreateProjection(string name, string wktProjectionClass, List<ProjectionParameter> Parameters);

		/// <summary>
		/// Creates a <see cref="IVerticalCoordinateSystem"/> from a <see cref="IVerticalDatum">datum</see> and <see cref="ILinearUnit">linear units</see>.
		/// </summary>
		/// <param name="name">Name of vertical coordinate system</param>
		/// <param name="datum">Vertical datum</param>
		/// <param name="verticalUnit">Unit</param>
		/// <param name="axis">Axis info</param>
		/// <returns>Vertical coordinate system</returns>
		IVerticalCoordinateSystem CreateVerticalCoordinateSystem(string name, IVerticalDatum datum, ILinearUnit verticalUnit, AxisInfo axis);			

		/// <summary>
		/// Creates a <see cref="IVerticalDatum"/> from an enumerated type value.
		/// </summary>
		/// <param name="name">Name of datum</param>
		/// <param name="datumType">Type of datum</param>
		/// <returns>Vertical datum</returns>	
		IVerticalDatum CreateVerticalDatum(string name, DatumType datumType);

		
	}
}
