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
using System.Globalization;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NPack;
using NPack.Interfaces;
using NetTopologySuite.CoordinateSystems.Projections;

namespace NetTopologySuite.CoordinateSystems.Transformations
{
    /// <summary>
    /// Creates coordinate transformations.
    /// </summary>
    public class CoordinateTransformationFactory<TCoordinate> : ICoordinateTransformationFactory<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly ICoordinateFactory<TCoordinate> _coordinateFactory;
        private readonly IGeometryFactory<TCoordinate> _geometryFactory;
        private readonly IMatrixFactory<DoubleComponent> _matrixFactory;
        private readonly IGeocentricCoordinateSystem<TCoordinate> _wgs84;

        public CoordinateTransformationFactory(ICoordinateFactory<TCoordinate> coordinateFactory,
                                               IGeometryFactory<TCoordinate> geometryFactory,
                                               IMatrixFactory<DoubleComponent> matrixFactory)
        {
            _coordinateFactory = coordinateFactory;
            _geometryFactory = geometryFactory;
            _matrixFactory = matrixFactory;

            CoordinateSystemFactory<TCoordinate> coordSystemFactory = 
                new CoordinateSystemFactory<TCoordinate>(_coordinateFactory, _geometryFactory);

            _wgs84 = coordSystemFactory.CreateWgs84GeocentricCoordinateSystem();
        }

        #region ICoordinateTransformationFactory Members

        ICoordinateTransformation ICoordinateTransformationFactory.CreateFromCoordinateSystems(
                                                                                ICoordinateSystem source,
                                                                                ICoordinateSystem target)
        {
            ICoordinateSystem<TCoordinate> sourceTyped = source as ICoordinateSystem<TCoordinate>;
            ICoordinateSystem<TCoordinate> targetTyped = target as ICoordinateSystem<TCoordinate>;

            if (sourceTyped == null)
            {
                throw new ArgumentException("Parameter must be a non-null " +
                                            "ICoordinateSystem<TCoordinate>", "source");
            }

            if (targetTyped == null)
            {
                throw new ArgumentException("Parameter must be a non-null " +
                                            "ICoordinateSystem<TCoordinate>", "target");
            }

            return CreateFromCoordinateSystems(sourceTyped, targetTyped);
        }

        #endregion

        #region ICoordinateTransformationFactory<TCoordinate> Members
        public ICoordinateTransformation<TCoordinate> CreateFromCoordinateSystems(
                                                                    ICoordinateSystem<TCoordinate> source,
                                                                    ICoordinateSystem<TCoordinate> target)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");

            // ================================================
            // Source is a geographic coordinate system
            IGeographicCoordinateSystem<TCoordinate> sourceGeographic =
                source as IGeographicCoordinateSystem<TCoordinate>;

            if (sourceGeographic != null)
            {
                // ================================================
                // Target is projected
                IProjectedCoordinateSystem<TCoordinate> targetProjected =
                    target as IProjectedCoordinateSystem<TCoordinate>;

                // Geographic -> Projected
                if (targetProjected != null)
                {
                    return geographicToProjected(sourceGeographic, targetProjected);
                }

                // ================================================
                // Target is geographic
                IGeographicCoordinateSystem<TCoordinate> targetGeographic =
                    target as IGeographicCoordinateSystem<TCoordinate>;

                // Geographic -> Geographic
                if (targetGeographic != null)
                {
                    return targetGeographic.EqualParams(sourceGeographic)
                        ?
                            null
                        :
                            createGeographicToGeographic(sourceGeographic, targetGeographic);

                }

                // ================================================
                // Target is geocentric
                IGeocentricCoordinateSystem<TCoordinate> targetGeocentric =
                    target as IGeocentricCoordinateSystem<TCoordinate>;

                // Geographic -> Geocentric
                if (targetGeocentric != null)
                {
                    return geographicToGeocentric(sourceGeographic, targetGeocentric);
                }
            }

            // ================================================
            // Source is a geocentric coordinate system
            IGeocentricCoordinateSystem<TCoordinate> sourceGeocentric =
                source as IGeocentricCoordinateSystem<TCoordinate>;

            if (sourceGeocentric != null)
            {
                // ================================================
                // Target is geocentric
                IGeocentricCoordinateSystem<TCoordinate> targetGeocentric =
                    target as IGeocentricCoordinateSystem<TCoordinate>;

                // Geocentric -> Geocentric
                if (targetGeocentric != null)
                {
                    return createGeocentricToGeocentric(sourceGeocentric, targetGeocentric);
                }

                // ================================================
                // Target is geographic
                IGeographicCoordinateSystem<TCoordinate> targetGeographic =
                    target as IGeographicCoordinateSystem<TCoordinate>;

                // Geocentric -> Geographic
                if (targetGeographic != null)
                {
                    return geocentricToGeographic(sourceGeocentric, targetGeographic);
                }
            }

            // ================================================
            // Source is a projected coordinate system
            IProjectedCoordinateSystem<TCoordinate> sourceProjected =
                source as IProjectedCoordinateSystem<TCoordinate>;

            if (sourceProjected != null)
            {
                // ================================================
                // Target is projected
                IProjectedCoordinateSystem<TCoordinate> targetProjected =
                    target as IProjectedCoordinateSystem<TCoordinate>;

                // Projected -> Projected
                if (targetProjected != null)
                {
                    return projectedToProjected(sourceProjected, targetProjected);
                }

                // ================================================
                // Target is geographic
                IGeographicCoordinateSystem<TCoordinate> targetGeographic =
                    target as IGeographicCoordinateSystem<TCoordinate>;

                // Projected -> Geographic
                if (targetGeographic != null)
                {
                    return projectedToGeographic(sourceProjected, targetGeographic);
                }
            }

            throw new NotSupportedException("No support for transforming between " +
                                            "the two specified coordinate systems");
        }

        ICoordinateTransformation<TCoordinate> ICoordinateTransformationFactory<TCoordinate>.CreateFromCoordinateSystems(ICoordinateSystem<TCoordinate> source, ICoordinateSystem<TCoordinate> target)
        {
            return CreateFromCoordinateSystems(source, target);
        }

        #endregion

        #region Methods for converting between specific systems

        private ICoordinateTransformation<TCoordinate> geographicToGeocentric(
                                                            IGeographicCoordinateSystem<TCoordinate> source,
                                                            IGeocentricCoordinateSystem<TCoordinate> target)
        {
            IMathTransform<TCoordinate> geocMathTransform = createCoordinateOperation(target);
            return new CoordinateTransformation<TCoordinate>(source,
                                                             target,
                                                             TransformType.Conversion,
                                                             geocMathTransform,
                                                             String.Empty,
                                                             String.Empty,
                                                             null,
                                                             String.Empty,
                                                             String.Empty);
        }

        private ICoordinateTransformation<TCoordinate> geocentricToGeographic(
                                                            IGeocentricCoordinateSystem<TCoordinate> source,
                                                            IGeographicCoordinateSystem<TCoordinate> target)
        {
            IMathTransform<TCoordinate> geocMathTransform
                = createCoordinateOperation(source).Inverse;

            return new CoordinateTransformation<TCoordinate>(source,
                                                             target,
                                                             TransformType.Conversion,
                                                             geocMathTransform,
                                                             String.Empty,
                                                             String.Empty,
                                                             null,
                                                             String.Empty,
                                                             String.Empty);
        }

        private ICoordinateTransformation<TCoordinate> projectedToProjected(
                                                            IProjectedCoordinateSystem<TCoordinate> source,
                                                            IProjectedCoordinateSystem<TCoordinate> target)
        {
            CoordinateTransformationFactory<TCoordinate> ctFac
                = new CoordinateTransformationFactory<TCoordinate>(_coordinateFactory,
                                                                   _geometryFactory,
                                                                   _matrixFactory);

            ICoordinateTransformation<TCoordinate>[] transforms
                = new ICoordinateTransformation<TCoordinate>[]
                    {
                        //First transform from projection to geographic
                        ctFac.CreateFromCoordinateSystems(source, source.GeographicCoordinateSystem),
                        //Transform geographic to geographic:
                        ctFac.CreateFromCoordinateSystems(source.GeographicCoordinateSystem,
                                                          target.GeographicCoordinateSystem),
                        //Transform to new projection
                        ctFac.CreateFromCoordinateSystems(target.GeographicCoordinateSystem, target)
                    };

            ConcatenatedTransform<TCoordinate> ct =
                new ConcatenatedTransform<TCoordinate>(transforms, _coordinateFactory);

            return new CoordinateTransformation<TCoordinate>(source,
                                                             target,
                                                             TransformType.Transformation,
                                                             ct,
                                                             String.Empty,
                                                             String.Empty,
                                                             null,
                                                             String.Empty,
                                                             String.Empty);
        }

        private ICoordinateTransformation<TCoordinate> geographicToProjected(
                                                            IGeographicCoordinateSystem<TCoordinate> source,
                                                            IProjectedCoordinateSystem<TCoordinate> target)
        {
            if (source.EqualParams(target.GeographicCoordinateSystem))
            {
                IProjection projection = target.Projection;
                IEllipsoid ellipsoid = target.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid;
                ILinearUnit linearUnit = target.LinearUnit;

                IMathTransform<TCoordinate> mathTransform = createCoordinateOperation(projection,
                                                                                      ellipsoid,
                                                                                      linearUnit);

                return new CoordinateTransformation<TCoordinate>(source,
                                                                 target,
                                                                 TransformType.Transformation,
                                                                 mathTransform,
                                                                 String.Empty,
                                                                 String.Empty,
                                                                 null,
                                                                 String.Empty,
                                                                 String.Empty);
            }
            else
            {
                // Geographic coordinate systems differ - create concatenated transform
                CoordinateTransformationFactory<TCoordinate> ctFac
                    = new CoordinateTransformationFactory<TCoordinate>(_coordinateFactory,
                                                                       _geometryFactory,
                                                                       _matrixFactory);

                ICoordinateTransformation<TCoordinate>[] transforms = new ICoordinateTransformation<TCoordinate>[]
                    {
                        ctFac.CreateFromCoordinateSystems(source, target.GeographicCoordinateSystem),
                        ctFac.CreateFromCoordinateSystems(target.GeographicCoordinateSystem, target)
                    };

                ConcatenatedTransform<TCoordinate> ct =
                    new ConcatenatedTransform<TCoordinate>(transforms, _coordinateFactory);

                return new CoordinateTransformation<TCoordinate>(source,
                                                                 target,
                                                                 TransformType.Transformation,
                                                                 ct,
                                                                 String.Empty,
                                                                 String.Empty,
                                                                 null,
                                                                 String.Empty,
                                                                 String.Empty);
            }
        }

        private ICoordinateTransformation<TCoordinate> projectedToGeographic(
                                                        IProjectedCoordinateSystem<TCoordinate> source,
                                                        IGeographicCoordinateSystem<TCoordinate> target)
        {
            CoordinateTransformation<TCoordinate> transformation;

            if (source.GeographicCoordinateSystem.EqualParams(target))
            {
                IProjection projection = source.Projection;
                IEllipsoid ellipsoid = source.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid;
                ILinearUnit linearUnit = source.LinearUnit;
                IMathTransform<TCoordinate> mathTransform = createCoordinateOperation(projection,
                                                                                      ellipsoid,
                                                                                      linearUnit);
                IMathTransform<TCoordinate> inverse = mathTransform.Inverse;

                transformation = new CoordinateTransformation<TCoordinate>(source,
                                                                           target,
                                                                           TransformType.Transformation,
                                                                           inverse,
                                                                           String.Empty,
                                                                           String.Empty,
                                                                           null,
                                                                           String.Empty,
                                                                           String.Empty);
            }
            else
            {
                // Geographic coordinate systems differ - create concatenated transform
                CoordinateTransformationFactory<TCoordinate> ctFac
                    = new CoordinateTransformationFactory<TCoordinate>(_coordinateFactory,
                                                                       _geometryFactory,
                                                                       _matrixFactory);

                ICoordinateTransformation<TCoordinate>[] transforms = new ICoordinateTransformation<TCoordinate>[]
                    {
                        ctFac.CreateFromCoordinateSystems(source, source.GeographicCoordinateSystem),
                        ctFac.CreateFromCoordinateSystems(source.GeographicCoordinateSystem, target)
                    };

                ConcatenatedTransform<TCoordinate> ct =
                    new ConcatenatedTransform<TCoordinate>(transforms, _coordinateFactory);

                transformation = new CoordinateTransformation<TCoordinate>(source,
                                                                           target,
                                                                           TransformType.Transformation,
                                                                           ct,
                                                                           String.Empty,
                                                                           String.Empty,
                                                                           null,
                                                                           String.Empty,
                                                                           String.Empty);
            }

            return transformation;
        }

        private ICoordinateTransformation<TCoordinate> createGeographicToGeographic(
                                                            IGeographicCoordinateSystem<TCoordinate> source,
                                                            IGeographicCoordinateSystem<TCoordinate> target)
        {
            if (source.HorizontalDatum.EqualParams(target.HorizontalDatum))
            {
                GeographicTransform<TCoordinate> transform = new GeographicTransform<TCoordinate>(source,
                                                                                                  target,
                                                                                                  _coordinateFactory);
                // No datum shift needed
                return new CoordinateTransformation<TCoordinate>(source,
                                                                 target,
                                                                 TransformType.Conversion,
                                                                 transform,
                                                                 String.Empty,
                                                                 String.Empty,
                                                                 null,
                                                                 String.Empty,
                                                                 String.Empty);
            }
            else
            {
                // Create datum shift
                // Convert to geocentric, perform shift and return to geographic
                CoordinateTransformationFactory<TCoordinate> ctFac
                    = new CoordinateTransformationFactory<TCoordinate>(_coordinateFactory,
                                                                       _geometryFactory,
                                                                       _matrixFactory);

                CoordinateSystemFactory<TCoordinate> cFac
                    = new CoordinateSystemFactory<TCoordinate>(_coordinateFactory, _geometryFactory);

                // TODO: The DefaultEnvelope extents shouldn't be null...
                IGeocentricCoordinateSystem<TCoordinate> sourceCentric = cFac.CreateGeocentricCoordinateSystem(
                    null, source.HorizontalDatum, LinearUnit.Meter,
                    source.PrimeMeridian, source.HorizontalDatum.Name + " Geocentric");

                IGeocentricCoordinateSystem<TCoordinate> targetCentric = cFac.CreateGeocentricCoordinateSystem(
                    null, target.HorizontalDatum, LinearUnit.Meter,
                    source.PrimeMeridian, target.HorizontalDatum.Name + " Geocentric");

                ICoordinateTransformation<TCoordinate>[] transforms = new ICoordinateTransformation<TCoordinate>[]
                    {
                        ctFac.CreateFromCoordinateSystems(source, sourceCentric),
                        ctFac.CreateFromCoordinateSystems(sourceCentric, targetCentric),
                        ctFac.CreateFromCoordinateSystems(targetCentric, target)
                    };

                ConcatenatedTransform<TCoordinate> ct =
                    new ConcatenatedTransform<TCoordinate>(transforms, _coordinateFactory);

                return new CoordinateTransformation<TCoordinate>(source,
                                                                 target,
                                                                 TransformType.Transformation,
                                                                 ct,
                                                                 String.Empty,
                                                                 String.Empty,
                                                                 null,
                                                                 String.Empty,
                                                                 String.Empty);
            }
        }

        /// <summary>
        /// Geocentric to Geocentric transformation
        /// </summary>
        private ICoordinateTransformation<TCoordinate> createGeocentricToGeocentric(
            IGeocentricCoordinateSystem<TCoordinate> source, IGeocentricCoordinateSystem<TCoordinate> target)
        {
            List<ICoordinateTransformation<TCoordinate>> transforms = new List<ICoordinateTransformation<TCoordinate>>();

            //Does source has a datum different from WGS84 and is there a shift specified?
            if (source.HorizontalDatum.Wgs84Parameters != null &&
                !source.HorizontalDatum.Wgs84Parameters.HasZeroValuesOnly)
            {
                IMathTransform<TCoordinate> dataumTransform =
                    new DatumTransform<TCoordinate>(source.HorizontalDatum.Wgs84Parameters,
                                                    _coordinateFactory,
                                                    _matrixFactory);

                target = target.HorizontalDatum.Wgs84Parameters == null ||
                         target.HorizontalDatum.Wgs84Parameters.HasZeroValuesOnly
                             ? target
                             : _wgs84;

                transforms.Add(new CoordinateTransformation<TCoordinate>(target,
                                                                         source,
                                                                         TransformType.Transformation,
                                                                         dataumTransform,
                                                                         "",
                                                                         "",
                                                                         null,
                                                                         "",
                                                                         ""));
            }

            //Does target has a datum different from WGS84 and is there a shift specified?
            if (target.HorizontalDatum.Wgs84Parameters != null &&
                !target.HorizontalDatum.Wgs84Parameters.HasZeroValuesOnly)
            {
                IMathTransform<TCoordinate> datumTransform =
                    new DatumTransform<TCoordinate>(target.HorizontalDatum.Wgs84Parameters,
                                                    _coordinateFactory,
                                                    _matrixFactory);

                IMathTransform<TCoordinate> inverseDatumTransform = datumTransform.Inverse;

                source = source.HorizontalDatum.Wgs84Parameters == null ||
                         source.HorizontalDatum.Wgs84Parameters.HasZeroValuesOnly
                             ? source
                             : _wgs84;

                transforms.Add(new CoordinateTransformation<TCoordinate>(source,
                                                                         target,
                                                                         TransformType.Transformation,
                                                                         inverseDatumTransform,
                                                                         "",
                                                                         "",
                                                                         null,
                                                                         "",
                                                                         ""));
            }

            if (transforms.Count == 1) // Since we only have one shift, lets just return the datumshift from/to wgs84
            {
                return new CoordinateTransformation<TCoordinate>(source,
                                                                 target,
                                                                 TransformType.ConversionAndTransformation,
                                                                 transforms[0].MathTransform,
                                                                 "",
                                                                 "",
                                                                 null,
                                                                 "",
                                                                 "");
            }
            else
            {
                ConcatenatedTransform<TCoordinate> ct =
                    new ConcatenatedTransform<TCoordinate>(transforms, _coordinateFactory);

                return new CoordinateTransformation<TCoordinate>(source,
                                                                 target,
                                                                 TransformType.ConversionAndTransformation,
                                                                 ct,
                                                                 "",
                                                                 "",
                                                                 null,
                                                                 "",
                                                                 "");
            }
        }

        #endregion

        #region Private helper methods

        private IMathTransform<TCoordinate> createCoordinateOperation(
            IGeocentricCoordinateSystem<TCoordinate> geo)
        {
            ProjectionParameter[] parameters = new ProjectionParameter[]
                {
                    new ProjectionParameter("semi_major", geo.HorizontalDatum.Ellipsoid.SemiMajorAxis),
                    new ProjectionParameter("semi_minor", geo.HorizontalDatum.Ellipsoid.SemiMinorAxis)
                };

            return new GeocentricTransform<TCoordinate>(parameters, _coordinateFactory);
        }

        private IMathTransform<TCoordinate> createCoordinateOperation(IProjection projection,
                                                                      IEllipsoid ellipsoid, 
                                                                      ILinearUnit unit)
        {
            List<ProjectionParameter> parameterList = new List<ProjectionParameter>(projection);

            parameterList.Add(new ProjectionParameter("semi_major", ellipsoid.SemiMajorAxis));
            parameterList.Add(new ProjectionParameter("semi_minor", ellipsoid.SemiMinorAxis));
            parameterList.Add(new ProjectionParameter("unit", unit.MetersPerUnit));

            IMathTransform<TCoordinate> transform;

            String className = projection.ProjectionClassName.ToLower(CultureInfo.InvariantCulture);
            className = className.Replace(" ", "");
            className = className.Replace("_", "");

            switch (className)
            {
                case "mercator":
                case "mercator1sp":
                case "mercator2sp":
                    transform = new Mercator<TCoordinate>(parameterList, _coordinateFactory);
                    break;
                case "transversemercator":
                    transform = new TransverseMercator<TCoordinate>(parameterList, _coordinateFactory);
                    break;
                case "albers":
                case "albersconicequalarea":
                    transform = new AlbersProjection<TCoordinate>(parameterList, _coordinateFactory);
                    break;
                case "lambertconformalconic":
                case "lambertconformalconic2sp":
                case "lambertconicconformal(2sp)":
                    transform = new LambertConformalConic2SP<TCoordinate>(parameterList, _coordinateFactory);
                    break;
                case "krovak":
                    transform = new KrovakProjection<TCoordinate>(parameterList, _coordinateFactory);
                    break;
                case "cassinisoldner":
                    transform = new CassiniSoldnerProjection<TCoordinate>(parameterList, _coordinateFactory);
                    break;
				case "obliquemercator":
                    transform = new HotineObliqueMercator<TCoordinate>(parameterList, _coordinateFactory, false);
					break;
				case "hotineoblique_mercator":
                    transform = new HotineObliqueMercator<TCoordinate>(parameterList, _coordinateFactory, true);
					break;
                case "affine":
                case "abridgedmolodenski":
                case "geocentrictoellipsoid":
                case "ellipsoidtogeocentric":
                case "longituderotation":
                default:
                    String message = 
                        String.Format("Projection {0} is not supported.", projection.ProjectionClassName);
                    throw new NotSupportedException(message);
            }

            return transform;
        }

        #endregion
    }
}