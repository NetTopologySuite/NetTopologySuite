using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.CoordinateSystems.Transformations;
using GisSharpBlog.NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;
using NUnit.Framework;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class SRSConversionTest : SimpleTests.BaseSamples
    {
        [Test]
        public void TestAlbersProjection()
        {
            CoordinateSystemFactory<BufferedCoordinate2D> cFac
                = new CoordinateSystemFactory<BufferedCoordinate2D>(CoordFactory, GeoFactory);

            IEllipsoid ellipsoid = cFac.CreateFlattenedSphere(
                6378206.4,
                294.9786982138982,
                LinearUnit.USSurveyFoot,
                "Clarke 1866");

            IHorizontalDatum datum = cFac.CreateHorizontalDatum(
                DatumType.HorizontalGeocentric,
                ellipsoid,
                null,
                "Clarke 1866");

            IGeographicCoordinateSystem<BufferedCoordinate2D> gcs
                = cFac.CreateGeographicCoordinateSystem(
                    GeoFactory.CreateExtents(CoordFactory.Create(-180, -90), 
                                             CoordFactory.Create(180, 90)),
                    AngularUnit.Degrees,
                    datum,
                    PrimeMeridian.Greenwich,
                    new AxisInfo(AxisOrientation.East, "Lon"),
                    new AxisInfo(AxisOrientation.North, "Lat"),
                    "Clarke 1866");

            List<ProjectionParameter> parameters = new List<ProjectionParameter>(5);
            parameters.Add(new ProjectionParameter("central_meridian", -96));
            parameters.Add(new ProjectionParameter("latitude_of_center", 23));
            parameters.Add(new ProjectionParameter("standard_parallel_1", 29.5));
            parameters.Add(new ProjectionParameter("standard_parallel_2", 45.5));
            parameters.Add(new ProjectionParameter("false_easting", 0));
            parameters.Add(new ProjectionParameter("false_northing", 0));

            IProjection projection = cFac.CreateProjection(
                "albers",
                parameters,
                "Albers Conical Equal Area");

            IProjectedCoordinateSystem<BufferedCoordinate2D> coordsys
                = cFac.CreateProjectedCoordinateSystem(
                    gcs,
                    projection,
                    LinearUnit.Meter,
                    new AxisInfo(AxisOrientation.East, "East"),
                    new AxisInfo(AxisOrientation.North, "North"),
                    "Albers Conical Equal Area");

            throw new NotImplementedException("Find a way to inject a matrix factory here...");
            
            //ICoordinateTransformation<BufferedCoordinate2D> trans
            //    = new CoordinateTransformationFactory<BufferedCoordinate2D>(CoordFactory, GeoFactory)
            //        .CreateFromCoordinateSystems(gcs, coordsys);

            //IPoint<BufferedCoordinate2D> pGeo 
            //    = GeoFactory.CreatePoint(CoordFactory.Create(-75, 35));

            //IPoint<BufferedCoordinate2D> pUtm 
            //    = GeometryTransform<BufferedCoordinate2D>.TransformPoint(pGeo,
            //                                                             trans.MathTransform,
            //                                                             GeoFactory);

            //IPoint<BufferedCoordinate2D> pGeo2 
            //    = GeometryTransform<BufferedCoordinate2D>.TransformPoint(pUtm, 
            //                                                             trans.MathTransform.Inverse(),
            //                                                             GeoFactory);

            //IPoint<BufferedCoordinate2D> expected 
            //    = GeoFactory.CreatePoint(CoordFactory.Create(1885472.7, 1535925));

            //Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.05),
            //              String.Format("Albers forward transformation outside " +
            //                            "tolerance, Expected [{0},{1}], got [{2},{3}]",
            //                             expected[Ordinates.X], expected[Ordinates.Y],
            //                             pUtm[Ordinates.X], pUtm[Ordinates.Y]));

            //Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001),
            //              String.Format("Albers reverse transformation outside "+
            //                            "tolerance, Expected [{0},{1}], got [{2},{3}]",
            //                            pGeo[Ordinates.X], pGeo[Ordinates.Y],
            //                            pGeo2[Ordinates.X], pGeo2[Ordinates.Y]));
        }

        private bool ToleranceLessThan(IPoint<BufferedCoordinate2D> p1,
                                       IPoint<BufferedCoordinate2D> p2,
                                       double tolerance)
        {
            if (!Double.IsNaN(p1[Ordinates.Z]) && !Double.IsNaN(p2[Ordinates.Z]))
            {
                return Math.Abs(p1[Ordinates.X] - p2[Ordinates.X]) < tolerance &&
                       Math.Abs(p1[Ordinates.Y] - p2[Ordinates.Y]) < tolerance &&
                       Math.Abs(p1[Ordinates.Z] - p2[Ordinates.Z]) < tolerance;
            }
            else
            {
                return Math.Abs(p1[Ordinates.X] - p2[Ordinates.X]) < tolerance &&
                       Math.Abs(p1[Ordinates.Y] - p2[Ordinates.Y]) < tolerance;
            }
        }
    }
}