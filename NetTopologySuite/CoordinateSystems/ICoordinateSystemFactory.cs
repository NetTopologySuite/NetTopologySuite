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


#region Using
using System;
#endregion

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// Builds up complex objects from simpler objects or values.
	/// </summary>
	/// <remarks>
	/// <para>
	/// CS_CoordinateSystemFactory allows applications to make coordinate
	/// systems that cannot be created by a ICoordinateSystemAuthorityFactory.
	/// This factory is very flexible, whereas the authority factory is easier to use.</para>
	/// 
	/// <para>So ICoordinateSystemAuthorityFactory can be used to make 'standard'
	/// coordinate systems, and ICoordinateSystemAuthorityFactory can be used to make
	/// 'special' coordinate systems.</para>
	/// 
	/// <para>For example, the EPSG authority has codes for USA state plane coordinate systems
	/// using the NAD83 datum, but these coordinate systems always use meters.  EPSG does
	/// not have codes for NAD83 state plane coordinate systems that use feet units.  This
	/// factory lets an application create such a hybrid coordinate system.
	/// </para>
	/// </remarks>
	public interface ICoordinateSystemFactory
	{
		/// <summary>
		/// Creates a fitted coordinate system. 
		/// </summary>
		/// <remarks>
		/// The units of the axes in the fitted coordinate system will be inferred
		/// from the units of the base coordinate system.  If the affine map
		/// performs a rotation, then any mixed axes must have identical units.
		/// For example, a (lat_deg,lon_deg,height_feet) system can be rotated in
		/// the (lat,lon) plane, since both affected axes are in degrees.  But you
		/// should not rotate this coordinate system in any other plane.
		/// </remarks>
		/// <param name="name">Name to give new object.</param>
		/// <param name="head">Head Coordinate system to use for earlier ordinates.</param>
		/// <param name="tail">Tail Coordinate system to use for later ordinates.</param>
		ICompoundCoordinateSystem CreateCompoundCoordinateSystem(string name, ICoordinateSystem head, ICoordinateSystem tail);

		/// <summary>
		/// Creates an ellipsoid from radius values.
		/// </summary>
		/// <param name="name">Name to give new object.</param>
		/// <param name="semiMajorAxis">Equatorial radius in supplied linear units.</param>
		/// <param name="semiMinorAxis">Polar radius in supplied linear units.</param>
		/// <param name="linearUnit">Linear units of ellipsoid axes.</param>
		IEllipsoid CreateEllipsoid(string name, double semiMajorAxis, double semiMinorAxis, ILinearUnit linearUnit);

		/// <summary>
		/// Creates a fitted coordinate system.
		/// </summary>
		/// <remarks>
		/// The units of the axes in the fitted coordinate system will be inferred
		/// from the units of the base coordinate system.  If the affine map
		/// performs a rotation, then any mixed axes must have identical units.
		/// For example, a (lat_deg,lon_deg,height_feet) system can be rotated in
		/// the (lat,lon) plane, since both affected axes are in degrees.  But you
		/// should not rotate this coordinate system in any other plane.
		/// </remarks>
		/// <param name="name">Name to give new object.</param>
		/// <param name="baseCoordinateSystem">Coordinate system to base the fitted CS on.</param>
		/// <param name="toBaseWKT">Well-Known Text of transform from returned CS to base CS.</param>
		/// <param name="arAxes">Axes for fitted coordinate system.  The number of axes must match the source dimension of the transform "toBaseWKT".</param>
		IFittedCoordinateSystem CreateFittedCoordinateSystem(string name, ICoordinateSystem baseCoordinateSystem, string toBaseWKT, IAxisInfo[] arAxes);
		
		/// <summary>
		/// Creates an ellipsoid from an major radius, and inverse flattening.
		/// </summary>
		/// <param name="name">Name to give new object.</param>
		/// <param name="semiMajorAxis">Equatorial radius in supplied linear units.</param>
		/// <param name="inverseFlattening">Eccentricity of ellipsoid.</param>
		/// <param name="linearUnit">Linear units of major axis.</param>
		IEllipsoid CreateFlattenedSphere(string name, double semiMajorAxis, double inverseFlattening, ILinearUnit linearUnit);

		/// <summary>
		/// Creates a coordinate system object from a Well-Known Text string.
		/// </summary>
		/// <param name="wellKnownText">Coordinate system encoded in Well-Known Text format.</param>
		ICoordinateSystem CreateFromWKT(string wellKnownText);

		/// <summary>
		/// Creates a coordinate system object from an XML string.
		/// </summary>
		/// <param name="xml">Coordinate system encoded in XML format.</param>
		ICoordinateSystem CreateFromXML(string xml);

		/// <summary>
		/// Creates a GCS, which could be Lat/Lon or Lon/Lat. 
		/// </summary>
		/// <param name="name">Name to give new object.</param>
		/// <param name="angularUnit">Angular units for created GCS.</param>
		/// <param name="horizontalDatum">Horizontal datum for created GCS.</param>
		/// <param name="primeMeridian">Prime Meridian for created GCS.</param>
		/// <param name="axis0">Details of 0th ordinates in returned GCS coordinates.</param>
		/// <param name="axis1">Details of 1st ordinates in returned GCS coordinates.</param>
		IGeographicCoordinateSystem CreateGeographicCoordinateSystem(string name, IAngularUnit angularUnit, IHorizontalDatum horizontalDatum, IPrimeMeridian primeMeridian, IAxisInfo axis0, IAxisInfo axis1);

		/// <summary>
		/// Creates horizontal datum from ellipsoid and Bursa-Wolf parameters. 
		/// </summary>
		/// <remarks>
		/// Since this method contains a set of Bursa-Wolf parameters, the created datum
		/// will always have a relationship to WGS84.  If you wish to create a horizontal datum
		/// that has no relationship with WGS84, then you can either specify CS_HD_Other as the
		/// horizontalDatumType, or create it via WKT.
		/// </remarks>
		///<param name="name">Name to give new object.</param>
		///<param name="horizontalDatumType">Type of horizontal datum to create.</param>
		///<param name="ellipsoid">Ellipsoid to use in new horizontal datum.</param>
		///<param name="toWGS84">Suggested approximate conversion from new datum to WGS84.</param>
		IHorizontalDatum CreateHorizontalDatum(string name, DatumType horizontalDatumType, IEllipsoid ellipsoid,  WGS84ConversionInfo toWGS84);
		/// <summary>
		/// Creates a local coordinate system.
		/// </summary>
		/// <remarks>
		/// The dimension of the local coordinate system is determined by the size
		/// of the axis array.  All the axes will have the same units.  If you want
		/// to make a coordinate system with mixed units, then you can make a
		/// compound coordinate system from different local coordinate systems.
		/// </remarks>
		/// <param name="name">Name to give new object.</param>
		/// <param name="datum">Local datum to use in created CS.</param>
		/// <param name="unit">Units to use for all axes in created CS.</param>
		/// <param name="arAxes">Axes to use in created CS.</param>
		ILocalCoordinateSystem CreateLocalCoordinateSystem(string name, ILocalDatum datum, IUnit unit, IAxisInfo[] arAxes);

		/// <summary>
		/// Creates a local datum.
		/// </summary>
		///<param name="name">Name to give new object.</param>
		///<param name="localDatumType">Type of local datum to create.</param>
		ILocalDatum CreateLocalDatum(string name, DatumType localDatumType);

		/// <summary>
		/// Creates a prime meridian, relative to Greenwich.
		/// </summary>
		/// <param name="name">Name to give new object.</param>
		/// <param name="angularUnit">Angular units of longitude.</param>
		/// <param name="longitude">Longitude of prime meridian in supplied angular units East of Greenwich.</param>
		IPrimeMeridian CreatePrimeMeridian(string name, IAngularUnit angularUnit, double longitude);

		/// <summary>
		/// Creates a projected coordinate system using a projection object. 
		/// </summary>
		/// <param name="name">Name to give new object.</param>
		/// <param name="gcs">Geographic coordinate system to base projection on.</param>
		/// <param name="projection">Projection from GCS to PCS.</param>
		/// <param name="linearUnit">Linear units of returned PCS.</param>
		/// <param name="axis0">Details of 0th ordinates in returned PCS coordinates.</param>
		/// <param name="axis1">Details of 1st ordinates in returned PCS coordinates.</param>
		IProjectedCoordinateSystem CreateProjectedCoordinateSystem(string name, IGeographicCoordinateSystem gcs, IProjection projection, ILinearUnit linearUnit,  IAxisInfo axis0,  IAxisInfo axis1);

		/// <summary>
		///  Creates a projection. 
		/// </summary>
		/// <param name="name">Name to give new object.</param>
		/// <param name="wktProjectionClass">wktProjectionClass Classification string for projection (e.g. "Transverse_Mercator").</param>
		/// <param name="Parameters">Parameters to use for projection, in units of intended PCS.</param>
		IProjection CreateProjection(string name, string wktProjectionClass, ProjectionParameter[] Parameters);

		/// <summary>
		/// Creates a vertical coordinate system from a datum and linear units.
		/// </summary>
		/// <param name="name">Name to give new object.</param>
		/// <param name="verticalUnit">Datum to use for new coordinate system.</param>
		/// <param name="verticalDatum">Units to use for new coordinate system.</param>
		/// <param name="axis">Axis to use for new coordinate system.</param>
		IVerticalCoordinateSystem CreateVerticalCoordinateSystem(string name, IVerticalDatum verticalDatum, ILinearUnit verticalUnit,  IAxisInfo axis);

		/// <summary>
		/// Creates a vertical datum from an enumerated type value. 
		/// </summary>
		/// <param name="name">Name to give new object.</param>
		/// <param name="verticalDatumType">Type of vertical datum to create.</param>
		IVerticalDatum CreateVerticalDatum(string name, DatumType verticalDatumType);
 
	}
}
