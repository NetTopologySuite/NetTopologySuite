// This code has lifted from ProjNet project code base, and the namespaces 
// updated to fit into NetTopologySuit. This is an interim measure, so that 
// ProjNet can be removed from Sharpmap. This code is to be refactor / written
//  to use the DotSpiatial project library.

// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
//
// This file is part of Proj.Net.
// Proj.Net is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Proj.Net is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Proj.Net; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using NPack.Interfaces;

namespace NetTopologySuite.CoordinateSystems
{
    /// <summary>
    /// Builds up complex objects from simpler objects or values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// ICoordinateSystemFactory allows applications to make coordinate 
    /// systems that cannot be created by a 
    /// <see cref="ICoordinateSystemAuthorityFactory{TCoordinate}"/>. 
    /// This factory is very flexible, whereas the authority factory is easier 
    /// to use.
    /// </para>
    /// <para>
    /// So <see cref="ICoordinateSystemAuthorityFactory{TCoordinate}"/>can be 
    /// used to make 'standard' coordinate systems, and 
    /// <see cref="CoordinateSystemFactory{TCoordinate}"/> can be used to make
    /// 'special' coordinate systems.
    /// </para>
    /// <para>
    /// For example, the EPSG authority has codes for USA state plane 
    /// coordinate systems using the NAD83 datum, but these coordinate 
    /// systems always use meters. EPSG does not have codes for NAD83 state 
    /// plane coordinate systems that use feet units. This factory
    /// lets an application create such a hybrid coordinate system.
    /// </para>
    /// </remarks>
    public class CoordinateSystemFactory<TCoordinate> : ICoordinateSystemFactory<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>,
            IConvertible
    {
        private readonly ICoordinateFactory<TCoordinate> _coordFactory;
        private readonly IGeometryFactory<TCoordinate> _geometryFactory;

        public CoordinateSystemFactory(ICoordinateFactory<TCoordinate> coordinateFactory,
                                       IGeometryFactory<TCoordinate> geometryFactory)
        {
            _coordFactory = coordinateFactory;
            _geometryFactory = geometryFactory;
        }

        /// <summary>
        /// Creates a coordinate system object from an XML String.
        /// </summary>
        /// <param name="xml">
        /// XML representation for the spatial reference.
        /// </param>
        /// <returns>The resulting spatial reference object.</returns>
        public ICoordinateSystem<TCoordinate> CreateFromXml(String xml)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a spatial reference object given its Well-Known Text 
        /// representation. The output object may be either a 
        /// <see cref="IGeographicCoordinateSystem{TCoordinate}"/> or
        /// a <see cref="IProjectedCoordinateSystem{TCoordinate}"/>.
        /// </summary>
        /// <param name="Wkt">
        /// The Well-Known Text representation for the spatial reference.
        /// </param>
        /// <returns>
        /// The resulting spatial reference object.
        /// </returns>
        public ICoordinateSystem<TCoordinate> CreateFromWkt(String Wkt)
        {
            return WktReader<TCoordinate>.ToCoordinateSystemInfo(Wkt, this)
                as ICoordinateSystem<TCoordinate>;
        }

        public IAngularUnit CreateAngularUnit(Double conversionFactor, String name)
        {
            return CreateAngularUnit(conversionFactor, name, "", String.Empty, "", "", "");
        }

        public IAngularUnit CreateAngularUnit(Double conversionFactor,
                                              String name,
                                              String authority,
                                              String authorityCode,
                                              String alias,
                                              String abbreviation,
                                              String remarks)
        {
            return new AngularUnit(conversionFactor, name, authority,
                authorityCode, alias, abbreviation, remarks);
        }

        public IAngularUnit CreateAngularUnit(CommonAngularUnits angularUnitType)
        {
            switch (angularUnitType)
            {
                case CommonAngularUnits.Radian:
                    return AngularUnit.Radian;
                case CommonAngularUnits.Degree:
                    return AngularUnit.Degrees;
                case CommonAngularUnits.Grad:
                    return AngularUnit.Grad;
                case CommonAngularUnits.Gon:
                    return AngularUnit.Gon;
                default:
                    throw new ArgumentException("Unknown angular unit: " + angularUnitType);
            }
        }

        public IAxisInfo CreateAxisInfo(AxisOrientation orientation, String name)
        {
            return CreateAxisInfo(orientation, name, null, String.Empty, null, null, null);
        }

        public IAxisInfo CreateAxisInfo(AxisOrientation orientation, String name,
                                        String authority, String authorityCode,
                                        String alias, String abbreviation,
                                        String remarks)
        {
            return new AxisInfo(orientation, name);
        }

        /// <summary>
        /// Creates a <see cref="ICompoundCoordinateSystem{TCoordinate}"/> [NOT IMPLEMENTED].
        /// </summary>
        /// <param name="name">Name of compound coordinate system.</param>
        /// <param name="head">Head coordinate system.</param>
        /// <param name="tail">Tail coordinate system.</param>
        /// <returns>Compound coordinate system.</returns>
        public ICompoundCoordinateSystem<TCoordinate> CreateCompoundCoordinateSystem(
            ICoordinateSystem<TCoordinate> head,
            ICoordinateSystem<TCoordinate> tail, String name)
        {
            throw new NotImplementedException();
        }

        public ICompoundCoordinateSystem<TCoordinate> CreateCompoundCoordinateSystem(
            ICoordinateSystem<TCoordinate> head, ICoordinateSystem<TCoordinate> tail,
            String name, String authority, String authorityCode, String alias,
            String abbreviation, String remarks)
        {
            throw new NotImplementedException();
        }

        public IAngularUnit CreateDegree()
        {
            return AngularUnit.Degrees;
        }

        /// <summary>
        /// Creates an <see cref="Ellipsoid"/> from radius values.
        /// </summary>
        /// <seealso cref="CreateFlattenedSphere"/>
        /// <param name="name">Name of ellipsoid.</param>
        /// <returns>Ellipsoid.</returns>
        public IEllipsoid CreateEllipsoid(Double semiMajorAxis,
                                          Double semiMinorAxis,
                                          ILinearUnit linearUnit,
                                          String name)
        {
            return new Ellipsoid(semiMajorAxis, semiMinorAxis, 1.0, false,
                                 linearUnit, name, String.Empty, String.Empty,
                                 String.Empty, String.Empty, String.Empty);
        }

        public IEllipsoid CreateEllipsoid(Double semiMajorAxis, Double semiMinorAxis,
                                          ILinearUnit linearUnit, String name,
                                          String authority, String authorityCode,
                                          String alias, String abbreviation,
                                          String remarks)
        {
            return new Ellipsoid(semiMajorAxis, semiMinorAxis, 0, false,
                                 linearUnit, name, authority, authorityCode,
                                 alias, abbreviation, remarks);
        }


        public IEllipsoid CreateEllipsoid(CommonEllipsoids ellipsoidType)
        {
            switch (ellipsoidType)
            {
                case CommonEllipsoids.Wgs84:
                    return Ellipsoid.Wgs84;
                case CommonEllipsoids.Wgs72:
                    return Ellipsoid.Wgs72;
                case CommonEllipsoids.Grs80:
                    return Ellipsoid.Grs80;
                case CommonEllipsoids.International1924:
                    return Ellipsoid.International1924;
                case CommonEllipsoids.Clarke1880:
                    return Ellipsoid.Clarke1880;
                case CommonEllipsoids.Clarke1866:
                    return Ellipsoid.Clarke1866;
                case CommonEllipsoids.Grs80AuthalicSphere:
                    return Ellipsoid.Sphere;
                default:
                    throw new ArgumentException("Unknown ellipsoid: " + ellipsoidType);
            }
        }

        public IEllipsoid CreateEllipsoidFromInverseFlattening(Double semiMajorAxis,
                                                               Double inverseFlattening,
                                                               ILinearUnit linearUnit,
                                                               String name)
        {
            return new Ellipsoid(semiMajorAxis, 0, inverseFlattening, true, linearUnit,
                                 name, null, String.Empty, null, null, null);
        }

        public IEllipsoid CreateEllipsoidFromInverseFlattening(Double semiMajorAxis,
                                                               Double semiMinorAxis,
                                                               ILinearUnit linearUnit,
                                                               String name,
                                                               String authority,
                                                               String authorityCode,
                                                               String alias,
                                                               String abbreviation,
                                                               String remarks)
        {
            return new Ellipsoid(semiMajorAxis, semiMinorAxis, 0, false, linearUnit,
                                 name, authority, authorityCode, alias, abbreviation,
                                 remarks);
        }

        /// <summary>
        /// Creates a <see cref="IFittedCoordinateSystem{TCoordinate}"/>.
        /// </summary>
        /// <remarks>
        /// The units of the axes in the fitted coordinate system will be 
        /// inferred from the units of the base coordinate system. If the affine map
        /// performs a rotation, then any mixed axes must have identical units. For
        /// example, a (lat_deg,lon_deg,height_feet) system can be rotated in the 
        /// (lat,lon) plane, since both affected axes are in degrees. But you 
        /// should not rotate this coordinate system in any other plane.
        /// </remarks>
        /// <param name="name">
        /// Name of coordinate system.
        /// </param>
        /// <param name="baseCoordinateSystem">Base coordinate system.</param>
        /// <returns>Fitted coordinate system.</returns>
        public IFittedCoordinateSystem<TCoordinate> CreateFittedCoordinateSystem(
            ICoordinateSystem<TCoordinate> baseCoordinateSystem,
            String toBaseWkt, IEnumerable<IAxisInfo> axes, String name)
        {
            throw new NotImplementedException();
        }

        public IFittedCoordinateSystem<TCoordinate> CreateFittedCoordinateSystem(
            ICoordinateSystem<TCoordinate> baseCoordinateSystem, String toBaseWkt, IEnumerable<IAxisInfo> axes,
            String name, String authority, String authorityCode, String alias, String abbreviation, String remarks)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates an <see cref="Ellipsoid"/> from an major radius, 
        /// and inverse flattening.
        /// </summary>
        /// <seealso cref="CreateEllipsoid"/>
        /// <param name="name">Name of ellipsoid.</param>
        /// <param name="semiMajorAxis">Semi major-axis.</param>
        /// <param name="inverseFlattening">Inverse flattening.</param>
        /// <param name="linearUnit">Linear unit.</param>
        /// <returns>Ellipsoid.</returns>
        public IEllipsoid CreateFlattenedSphere(Double semiMajorAxis,
                                                Double inverseFlattening,
                                                ILinearUnit linearUnit,
                                                String name)
        {

            return CreateFlattenedSphere(semiMajorAxis, inverseFlattening,
                                         linearUnit, name, String.Empty,
                                         String.Empty, String.Empty, String.Empty,
                                         String.Empty);
        }

        public IEllipsoid CreateFlattenedSphere(Double semiMajorAxis,
                                                Double inverseFlattening,
                                                ILinearUnit linearUnit,
                                                String name,
                                                String authority,
                                                String authorityCode,
                                                String alias,
                                                String abbreviation,
                                                String remarks)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid name");
            }

            return new Ellipsoid(semiMajorAxis, 0, inverseFlattening, true, linearUnit,
                                 name, authority, authorityCode, alias, abbreviation,
                                 remarks);
        }

        /// <summary>
        /// Creates a <see cref="CreateGeocentricCoordinateSystem"/> from a 
        /// <see cref="IHorizontalDatum">datum</see>, 
        /// <see cref="ILinearUnit">linear unit</see> and <see cref="IPrimeMeridian"/>.
        /// </summary>
        /// <param name="name">Name of geocentric coordinate system</param>
        /// <param name="datum">Horizontal datum</param>
        /// <param name="linearUnit">Linear unit</param>
        /// <param name="primeMeridian">Prime meridian</param>
        /// <returns>Geocentric Coordinate System</returns>
        public IGeocentricCoordinateSystem<TCoordinate> CreateGeocentricCoordinateSystem(
            IExtents<TCoordinate> extents, IHorizontalDatum datum,
            ILinearUnit linearUnit, IPrimeMeridian primeMeridian,
            String name)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid name");
            }

            IAxisInfo[] info = new IAxisInfo[]
                {
                    new AxisInfo(AxisOrientation.Other, "X"),
                    new AxisInfo(AxisOrientation.Other, "Y"),
                    new AxisInfo(AxisOrientation.Other, "Z")
                };

            return new GeocentricCoordinateSystem<TCoordinate>(extents, datum,
                                                               linearUnit, primeMeridian,
                                                               info, name, String.Empty,
                                                               String.Empty, String.Empty,
                                                               String.Empty, String.Empty);
        }


        /// <summary>
        /// Creates a <see cref="GeographicCoordinateSystem{TCoordinate}"/>, 
        /// which could be Lat / Lon or Lon / Lat.
        /// </summary>
        /// <param name="name">Name of geographical coordinate system.</param>
        /// <param name="angularUnit">Angular units.</param>
        /// <param name="datum">Horizontal datum.</param>
        /// <param name="primeMeridian">Prime meridian.</param>
        /// <param name="axis0">First axis.</param>
        /// <param name="axis1">Second axis.</param>
        /// <returns>Geographic coordinate system.</returns>
        public IGeographicCoordinateSystem<TCoordinate> CreateGeographicCoordinateSystem(
            IExtents<TCoordinate> extents, IAngularUnit angularUnit, IHorizontalDatum datum,
            IPrimeMeridian primeMeridian, IAxisInfo axis0, IAxisInfo axis1, String name)
        {
            return CreateGeographicCoordinateSystem(extents, angularUnit, datum,
                                                    primeMeridian, axis0, axis1,
                                                    name, "", String.Empty, "", "", "");
        }

        public IGeographicCoordinateSystem<TCoordinate> CreateGeographicCoordinateSystem(
            IExtents<TCoordinate> extents, IAngularUnit angularUnit,
            IHorizontalDatum datum, IPrimeMeridian primeMeridian, IAxisInfo axis0,
            IAxisInfo axis1, String name, String authority, String authorityCode,
            String alias, String abbreviation, String remarks)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid name");
            }

            IAxisInfo[] info = new IAxisInfo[2];
            info[0] = axis0;
            info[1] = axis1;

            if (extents == null)
            {
                extents = _geometryFactory.CreateExtents2D(-180, -90, 180, 90)
                          as IExtents<TCoordinate>;
            }

            return new GeographicCoordinateSystem<TCoordinate>(extents,
                                                               angularUnit, datum,
                                                               primeMeridian, info,
                                                               name, authority,
                                                               authorityCode, alias,
                                                               abbreviation, remarks);
        }

        /// <summary>
        /// Creates <see cref="HorizontalDatum"/> from ellipsoid and Bursa-Wolf 
        /// parameters.
        /// </summary>
        /// <remarks>
        /// Since this method contains a set of Bursa-Wolf parameters, the created 
        /// datum will always have a relationship to WGS84. If you wish to create a
        /// horizontal datum that has no relationship with WGS84, then you can 
        /// either specify a <see cref="DatumType">horizontalDatumType</see> of 
        /// <see cref="DatumType.HorizontalOther"/>, or create it via Wkt.
        /// </remarks>
        /// <param name="name">Name of ellipsoid.</param>
        /// <param name="datumType">Type of datum.</param>
        /// <param name="ellipsoid">Ellipsoid.</param>
        /// <param name="toWgs84">Wgs84 conversion parameters.</param>
        /// <returns>Horizontal datum.</returns>
        public IHorizontalDatum CreateHorizontalDatum(DatumType datumType, IEllipsoid ellipsoid,
                                                      Wgs84ConversionInfo toWgs84, String name)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid name");
            }

            if (ellipsoid == null)
            {
                throw new ArgumentException("Ellipsoid was null");
            }

            return new HorizontalDatum(ellipsoid, toWgs84, datumType, name,
                                       String.Empty, String.Empty, String.Empty,
                                       String.Empty, String.Empty);
        }

        public IHorizontalDatum CreateHorizontalDatum(DatumType datumType,
                                                      IEllipsoid ellipsoid,
                                                      Wgs84ConversionInfo toWgs84,
                                                      String name, String authority,
                                                      String authorityCode, String alias,
                                                      String abbreviation, String remarks)
        {
            return new HorizontalDatum(ellipsoid, toWgs84, datumType,
                                       name, authority, authorityCode,
                                       alias, remarks, abbreviation);
        }


        public IHorizontalDatum CreateHorizontalDatum(CommonHorizontalDatums datumType)
        {
            switch (datumType)
            {
                case CommonHorizontalDatums.Wgs84:
                    return HorizontalDatum.Wgs84;
                case CommonHorizontalDatums.Wgs72:
                    return HorizontalDatum.Wgs72;
                case CommonHorizontalDatums.Etrf89:
                    return HorizontalDatum.Etrf89;
                case CommonHorizontalDatums.ED50:
                    return HorizontalDatum.ED50;
                /*
            case CommonHorizontalDatums.Nad27:
            case CommonHorizontalDatums.Nad83:
            case CommonHorizontalDatums.Harn:
                 */
                default:
                    throw new ArgumentException("Unknown datum: " + datumType);
            }
        }

        public ILinearUnit CreateLinearUnit(Double conversionFactor, String name)
        {
            return new LinearUnit(conversionFactor, name, "", String.Empty, "", "", "");
        }

        public ILinearUnit CreateLinearUnit(Double conversionFactor, String name,
                                            String authority, String authorityCode,
                                            String alias, String abbreviation,
                                            String remarks)
        {
            return new LinearUnit(conversionFactor, name, authority, authorityCode,
                                  alias, abbreviation, remarks);
        }


        public ILinearUnit CreateLinearUnit(CommonLinearUnits linearUnitType)
        {
            switch (linearUnitType)
            {
                case CommonLinearUnits.Meter:
                    return LinearUnit.Meter;
                case CommonLinearUnits.USSurveyFoot:
                    return LinearUnit.USSurveyFoot;
                case CommonLinearUnits.NauticalMile:
                    return LinearUnit.NauticalMile;
                case CommonLinearUnits.ClarkesFoot:
                    return LinearUnit.ClarkesFoot;
                /*
            case CommonLinearUnits.InternationalFoot:
                 */
                default:
                    throw new ArgumentException("Unknown linear unit: " + linearUnitType);
            }
        }

        /// <summary>
        /// Creates a <see cref="ILocalCoordinateSystem{TCoordinate}">local coordinate system</see>.
        /// </summary>
        /// <remarks>
        /// The dimension of the local coordinate system is determined by the size of 
        /// the axis array. All the axes will have the same units. If you want to make 
        /// a coordinate system with mixed units, then you can make a compound 
        /// coordinate system from different local coordinate systems.
        /// </remarks>
        /// <param name="name">Name of local coordinate system.</param>
        /// <param name="datum">Local datum.</param>
        /// <param name="unit">Units.</param>
        /// <param name="axes">Axis info.</param>
        /// <returns>Local coordinate system.</returns>
        public ILocalCoordinateSystem<TCoordinate> CreateLocalCoordinateSystem(
            ILocalDatum datum, IUnit unit, IEnumerable<IAxisInfo> axes, String name)
        {
            throw new NotImplementedException();
        }

        public ILocalCoordinateSystem<TCoordinate> CreateLocalCoordinateSystem(ILocalDatum datum, IUnit unit,
                                                                               IEnumerable<IAxisInfo> axes, String name,
                                                                               String authority, String authorityCode,
                                                                               String alias, String abbreviation,
                                                                               String remarks)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a <see cref="ILocalDatum"/>.
        /// </summary>
        /// <param name="name">Name of datum.</param>
        /// <param name="datumType">Datum type.</param>
        public ILocalDatum CreateLocalDatum(DatumType datumType, String name)
        {
            return CreateLocalDatum(datumType, name, "", String.Empty, "", "", "");
        }

        public ILocalDatum CreateLocalDatum(DatumType datumType, String name,
                                            String authority, String authorityCode,
                                            String alias, String abbreviation,
                                            String remarks)
        {
            throw new NotImplementedException();
        }

        public ILinearUnit CreateMeter()
        {
            return LinearUnit.Meter;
        }

        public IPrimeMeridian CreatePrimeMeridian(IAngularUnit angularUnit,
                                                  Double longitude, String name,
                                                  String authority, String authorityCode,
                                                  String alias, String abbreviation,
                                                  String remarks)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid name");
            }

            return new PrimeMeridian(longitude, angularUnit, name,
                                     authority, authorityCode, alias,
                                     abbreviation, remarks);
        }

        /// <summary>
        /// Creates a <see cref="PrimeMeridian"/>, relative to Greenwich.
        /// </summary>
        /// <param name="name">Name of prime meridian.</param>
        /// <param name="angularUnit">Angular unit.</param>
        /// <param name="longitude">Longitude.</param>
        /// <returns>Prime meridian.</returns>
        public IPrimeMeridian CreatePrimeMeridian(IAngularUnit angularUnit,
                                                  Double longitude,
                                                  String name)
        {
            return CreatePrimeMeridian(angularUnit, longitude, name, "", String.Empty, "", "", "");
        }


        public IPrimeMeridian CreatePrimeMeridian(CommonPrimeMeridians primeMeridian)
        {
            switch (primeMeridian)
            {
                case CommonPrimeMeridians.Greenwich:
                    return PrimeMeridian.Greenwich;
                case CommonPrimeMeridians.Lisbon:
                    return PrimeMeridian.Lisbon;
                case CommonPrimeMeridians.Paris:
                    return PrimeMeridian.Paris;
                case CommonPrimeMeridians.Bogota:
                    return PrimeMeridian.Bogota;
                case CommonPrimeMeridians.Madrid:
                    return PrimeMeridian.Madrid;
                case CommonPrimeMeridians.Rome:
                    return PrimeMeridian.Rome;
                case CommonPrimeMeridians.Bern:
                    return PrimeMeridian.Bern;
                case CommonPrimeMeridians.Jakarta:
                    return PrimeMeridian.Jakarta;
                case CommonPrimeMeridians.Ferro:
                    return PrimeMeridian.Ferro;
                case CommonPrimeMeridians.Brussels:
                    return PrimeMeridian.Brussels;
                case CommonPrimeMeridians.Stockholm:
                    return PrimeMeridian.Stockholm;
                case CommonPrimeMeridians.Athens:
                    return PrimeMeridian.Athens;
                case CommonPrimeMeridians.Oslo:
                    return PrimeMeridian.Oslo;
                /*
            case CommonPrimeMeridians.Antwerp:
                 */
                default:
                    throw new ArgumentException("Unknown prime meridian: " + primeMeridian);
            }
        }

        /// <summary>
        /// Creates a <see cref="ProjectedCoordinateSystem{TCoordinate}"/> using a 
        /// projection object.
        /// </summary>
        /// <param name="name">Name of projected coordinate system.</param>
        /// <param name="gcs">Geographic coordinate system.</param>
        /// <param name="projection">Projection.</param>
        /// <param name="linearUnit">Linear unit.</param>
        /// <param name="axis0">Primary axis.</param>
        /// <param name="axis1">Secondary axis.</param>
        /// <returns>Projected coordinate system.</returns>
        public IProjectedCoordinateSystem<TCoordinate> CreateProjectedCoordinateSystem(
            IGeographicCoordinateSystem<TCoordinate> gcs, IProjection projection,
            ILinearUnit linearUnit, IAxisInfo axis0, IAxisInfo axis1, String name)
        {
            return CreateProjectedCoordinateSystem(gcs, projection,
                                                   linearUnit, axis0, axis1, name,
                                                   String.Empty, String.Empty, String.Empty,
                                                   String.Empty, String.Empty);
        }

        public IProjectedCoordinateSystem<TCoordinate> CreateProjectedCoordinateSystem(
            IGeographicCoordinateSystem<TCoordinate> gcs, IProjection projection,
            ILinearUnit linearUnit, IAxisInfo axis0, IAxisInfo axis1, String name,
            String authority, String authorityCode, String alias, String abbreviation,
            String remarks)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid name.");
            }

            if (gcs == null)
            {
                throw new ArgumentNullException("gcs");
            }

            if (projection == null)
            {
                throw new ArgumentNullException("projection");
            }

            if (linearUnit == null)
            {
                throw new ArgumentNullException("linearUnit");
            }

            IAxisInfo[] info = new IAxisInfo[]
                {
                    axis0,
                    axis1
                };

            return new ProjectedCoordinateSystem<TCoordinate>(gcs, projection,
                                                              linearUnit, info, name, authority, authorityCode, alias,
                                                              remarks, abbreviation);
        }

        /// <summary>
        /// Creates a <see cref="Projection"/>.
        /// </summary>
        /// <param name="name">Name of projection.</param>
        /// <param name="wktProjectionClass">Projection class.</param>
        /// <param name="parameters">Projection parameters.</param>
        /// <returns>Projection.</returns>
        public IProjection CreateProjection(String wktProjectionClass,
                                            IEnumerable<ProjectionParameter> parameters, String name)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid name");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            List<ProjectionParameter> paramList
                = new List<ProjectionParameter>(parameters);

            if (paramList.Count == 0)
            {
                throw new ArgumentException("Invalid projection parameters.");
            }

            return new Projection(wktProjectionClass, paramList, name,
                                  String.Empty, String.Empty, String.Empty,
                                  String.Empty, String.Empty);
        }

        public IProjection CreateProjection(String wktProjectionClass,
                                            IEnumerable<ProjectionParameter> parameters,
                                            String name, String authority,
                                            String authorityCode, String alias,
                                            String abbreviation, String remarks)
        {
            return new Projection(wktProjectionClass, parameters, name, authority,
                                  authorityCode, alias, abbreviation, remarks);
        }

        public IUnit CreateUnit(Double conversionFactor, String name)
        {
            return new Unit(conversionFactor, name);
        }

        public IUnit CreateUnit(Double conversionFactor, String name, String authority,
                                String authorityCode, String alias, String abbreviation,
                                String remarks)
        {
            return new Unit(conversionFactor, name, authority, authorityCode,
                            alias, abbreviation, remarks);
        }

        /// <summary>
        /// Creates a <see cref="IVerticalDatum"/> from an enumerated type value.
        /// </summary>
        /// <param name="name">Name of datum.</param>
        /// <param name="datumType">Type of datum.</param>
        /// <returns>Vertical datum.</returns>	
        public IVerticalDatum CreateVerticalDatum(DatumType datumType, String name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a <see cref="IVerticalCoordinateSystem{TCoordinate}"/> from a 
        /// <see cref="IVerticalDatum">datum</see> and 
        /// <see cref="LinearUnit">linear units</see>.
        /// </summary>
        /// <param name="name">Name of vertical coordinate system.</param>
        /// <param name="datum">Vertical datum.</param>
        /// <param name="verticalUnit">Unit.</param>
        /// <param name="axis">Axis info.</param>
        /// <returns>Vertical coordinate system.</returns>
        public IVerticalCoordinateSystem<TCoordinate> CreateVerticalCoordinateSystem(
            IVerticalDatum datum, ILinearUnit verticalUnit, IAxisInfo axis, String name)
        {
            throw new NotImplementedException();
        }

        public IVerticalCoordinateSystem<TCoordinate> CreateVerticalCoordinateSystem(IVerticalDatum datum,
                                                                                     ILinearUnit verticalUnit,
                                                                                     IAxisInfo axis,
                                                                                     String name,
                                                                                     String authority,
                                                                                     String authorityCode,
                                                                                     String alias,
                                                                                     String abbreviation,
                                                                                     String remarks)
        {
            throw new NotImplementedException();
        }

        public IVerticalDatum CreateVerticalDatum(DatumType datumType, String name,
                                                  String authority, String authorityCode,
                                                  String alias, String abbreviation,
                                                  String remarks)
        {
            throw new NotImplementedException();
        }

        public IGeocentricCoordinateSystem<TCoordinate> CreateWgs84GeocentricCoordinateSystem()
        {
            IEllipsoid wgs84Ellipsoid = HorizontalDatum.Wgs84.Ellipsoid;
            Double semiMajor = wgs84Ellipsoid.SemiMajorAxis;
            Double semiMinor = wgs84Ellipsoid.SemiMinorAxis;
            TCoordinate min = _coordFactory.Create3D(-semiMajor, -semiMajor, -semiMinor);
            TCoordinate max = _coordFactory.Create3D(semiMajor, semiMajor, semiMinor);
            IExtents<TCoordinate> wgs84Extents = _geometryFactory.CreateExtents(min, max);

            return CreateGeocentricCoordinateSystem(wgs84Extents,
                                                    HorizontalDatum.Wgs84,
                                                    LinearUnit.Meter,
                                                    PrimeMeridian.Greenwich,
                                                    "WGS84 Geocentric");
        }

        public IGeographicCoordinateSystem<TCoordinate> CreateWgs84GeographicCoordinateSystem()
        {
            TCoordinate min = _coordFactory.Create(-180, -90);
            TCoordinate max = _coordFactory.Create(180, 90);
            IExtents<TCoordinate> wgs84Extents = _geometryFactory.CreateExtents(min, max);

            return CreateGeographicCoordinateSystem(wgs84Extents,
                                                    AngularUnit.Degrees,
                                                    HorizontalDatum.Wgs84,
                                                    PrimeMeridian.Greenwich,
                                                    new AxisInfo(AxisOrientation.East, "Lon"),
                                                    new AxisInfo(AxisOrientation.North, "Lat"),
                                                    "WGS 84");
        }

        //==================================================================================

        //==================================================================================

        ICompoundCoordinateSystem ICoordinateSystemFactory.CreateCompoundCoordinateSystem(
            ICoordinateSystem head, ICoordinateSystem tail, String name)
        {
            return CreateCompoundCoordinateSystem(convert(head), convert(tail), name, "", String.Empty, "", "", "");
        }

        ICompoundCoordinateSystem ICoordinateSystemFactory.CreateCompoundCoordinateSystem(
            ICoordinateSystem head, ICoordinateSystem tail, String name, String authority,
            String authorityCode, String alias, String abbreviation, String remarks)
        {
            throw new NotImplementedException();
        }

        IFittedCoordinateSystem ICoordinateSystemFactory.CreateFittedCoordinateSystem(ICoordinateSystem baseCoordinateSystem,
                                                                    String toBaseWkt,
                                                                    IEnumerable<IAxisInfo> axes,
                                                                    String name)
        {
            throw new NotImplementedException();
        }

        IFittedCoordinateSystem ICoordinateSystemFactory.CreateFittedCoordinateSystem(
                                                ICoordinateSystem baseCoordinateSystem,
                                                String toBaseWkt,
                                                IEnumerable<IAxisInfo> axes,
                                                String name,
                                                String authority,
                                                String authorityCode,
                                                String alias,
                                                String abbreviation,
                                                String remarks)
        {
            throw new NotImplementedException();
        }

        ICoordinateSystem ICoordinateSystemFactory.CreateFromXml(String xml)
        {
            return CreateFromXml(xml);
        }

        ICoordinateSystem ICoordinateSystemFactory.CreateFromWkt(String wkt)
        {
            return CreateFromWkt(wkt);
        }

        IGeographicCoordinateSystem ICoordinateSystemFactory.CreateGeographicCoordinateSystem(
            CommonGeographicCoordinateSystems coordSystemType)
        {
            switch (coordSystemType)
            {
                case CommonGeographicCoordinateSystems.Wgs84:
                    return GeographicCoordinateSystem<TCoordinate>.GetWgs84(_geometryFactory);
                default:
                    throw new ArgumentException(
                        "Unknown geographic coordinate system: " + coordSystemType);
            }
        }

        IGeographicCoordinateSystem ICoordinateSystemFactory.CreateGeographicCoordinateSystem(
            IExtents extents, IAngularUnit angularUnit, IHorizontalDatum datum,
            IPrimeMeridian primeMeridian, IAxisInfo axis0, IAxisInfo axis1, String name)
        {
            return CreateGeographicCoordinateSystem(convert(extents), angularUnit,
                                                    datum, primeMeridian, axis0, axis1,
                                                    name);
        }

        IGeographicCoordinateSystem ICoordinateSystemFactory.CreateGeographicCoordinateSystem(
            IExtents extents, IAngularUnit angularUnit, IHorizontalDatum datum,
            IPrimeMeridian primeMeridian, IAxisInfo axis0, IAxisInfo axis1,
            String name, String authority, String authorityCode, String alias,
            String abbreviation, String remarks)
        {
            return CreateGeographicCoordinateSystem(convert(extents), angularUnit,
                                                    datum, primeMeridian, axis0, axis1,
                                                    name, authority, authorityCode, alias, abbreviation, remarks);
        }


        ILocalCoordinateSystem ICoordinateSystemFactory.CreateLocalCoordinateSystem(ILocalDatum datum,
                                                                                    IUnit unit,
                                                                                    IEnumerable<IAxisInfo> axes,
                                                                                    String name)
        {
            throw new NotImplementedException();
        }

        ILocalCoordinateSystem ICoordinateSystemFactory.CreateLocalCoordinateSystem(
            ILocalDatum datum, IUnit unit, IEnumerable<IAxisInfo> axes, String name,
            String authority, String authorityCode, String alias, String abbreviation,
            String remarks)
        {
            throw new NotImplementedException();
        }

        IProjectedCoordinateSystem ICoordinateSystemFactory.CreateProjectedCoordinateSystem(
            IGeographicCoordinateSystem gcs, IProjection projection, ILinearUnit linearUnit,
            IAxisInfo axis0, IAxisInfo axis1, String name)
        {
            return (this as ICoordinateSystemFactory).CreateProjectedCoordinateSystem(
                convert(gcs), projection, linearUnit, axis0, axis1, name,
                "", String.Empty, "", "", "");
        }

        IProjectedCoordinateSystem ICoordinateSystemFactory.CreateProjectedCoordinateSystem(
            IGeographicCoordinateSystem gcs, IProjection projection, ILinearUnit linearUnit,
            IAxisInfo axis0, IAxisInfo axis1, String name, String authority,
            String authorityCode, String alias, String abbreviation, String remarks)
        {
            return CreateProjectedCoordinateSystem(convert(gcs), projection, linearUnit,
                                                   axis0, axis1, name, authority,
                                                   authorityCode, alias, abbreviation,
                                                   remarks);
        }

        IVerticalCoordinateSystem ICoordinateSystemFactory.CreateVerticalCoordinateSystem(
            IVerticalDatum datum, ILinearUnit verticalUnit, IAxisInfo axis, String name)
        {
            throw new NotImplementedException();
        }

        IVerticalCoordinateSystem ICoordinateSystemFactory.CreateVerticalCoordinateSystem(
            IVerticalDatum datum, ILinearUnit verticalUnit, IAxisInfo axis, String name,
            String authority, String authorityCode, String alias, String abbreviation,
            String remarks)
        {
            throw new NotImplementedException();
        }

        #region Private helper functions

        private ICoordinateSystem<TCoordinate> convert(ICoordinateSystem coordinateSystem)
        {
            if (coordinateSystem == null)
            {
                return null;
            }

            IGeographicCoordinateSystem gcs = coordinateSystem as IGeographicCoordinateSystem;

            if (gcs != null)
            {
                return convert(gcs);
            }

            throw new NotImplementedException();
        }

        private IGeographicCoordinateSystem<TCoordinate> convert(IGeographicCoordinateSystem coordinateSystem)
        {
            if (coordinateSystem == null)
            {
                return null;
            }

            IGeographicCoordinateSystem<TCoordinate> converted =
                coordinateSystem as IGeographicCoordinateSystem<TCoordinate>;

            if (converted != null)
            {
                return converted;
            }

            converted = CreateGeographicCoordinateSystem(
                convert(coordinateSystem.DefaultEnvelope),
                coordinateSystem.AngularUnit,
                coordinateSystem.HorizontalDatum,
                coordinateSystem.PrimeMeridian,
                coordinateSystem.GetAxis(0),
                coordinateSystem.GetAxis(1),
                coordinateSystem.Name,
                coordinateSystem.Authority,
                coordinateSystem.AuthorityCode,
                coordinateSystem.Alias,
                coordinateSystem.Abbreviation,
                coordinateSystem.Remarks);

            return converted;
        }

        private IExtents<TCoordinate> convert(IExtents extents)
        {
            if (extents == null)
            {
                return null;
            }

            if (extents is IExtents<TCoordinate>)
            {
                return extents as IExtents<TCoordinate>;
            }

            return _geometryFactory.CreateExtents(extents.Min, extents.Max);
        }
        #endregion
    }
}