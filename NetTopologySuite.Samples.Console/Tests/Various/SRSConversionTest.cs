using System;
using System.Collections.Generic;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.CoordinateSystems.Transformations;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;
using SharpMap.CoordinateSystems;
using SharpMap.CoordinateSystems.Transformations;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class SRSConversionTest :  BaseSamples
    {
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestAlbersProjection()
        {
            CoordinateSystemFactory cFac = new CoordinateSystemFactory();
            IEllipsoid ellipsoid = cFac.CreateFlattenedSphere(
                "Clarke 1866", 
                6378206.4, 
                294.9786982138982, 
                LinearUnit.USSurveyFoot);

            IHorizontalDatum datum = cFac.CreateHorizontalDatum(
                "Clarke 1866",
                DatumType.HD_Geocentric,
                ellipsoid, 
                null);

            IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem(
                "Clarke 1866", 
                AngularUnit.Degrees, 
                datum, 
                PrimeMeridian.Greenwich, 
                new AxisInfo("Lon", AxisOrientationEnum.East),
                new AxisInfo("Lat", AxisOrientationEnum.North));

            List<ProjectionParameter> parameters = new List<ProjectionParameter>(5);
            parameters.Add(new ProjectionParameter("central_meridian", -96));
            parameters.Add(new ProjectionParameter("latitude_of_center", 23));
            parameters.Add(new ProjectionParameter("standard_parallel_1", 29.5));
            parameters.Add(new ProjectionParameter("standard_parallel_2", 45.5));
            parameters.Add(new ProjectionParameter("false_easting", 0));
            parameters.Add(new ProjectionParameter("false_northing", 0));

            IProjection projection = cFac.CreateProjection(
                "Albers Conical Equal Area", 
                "albers", 
                parameters);

            IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem(
                "Albers Conical Equal Area", 
                gcs, 
                projection, 
                LinearUnit.Metre,
                new AxisInfo("East", AxisOrientationEnum.East),
                new AxisInfo("North", AxisOrientationEnum.North));

            ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

            IGeometryFactory f = GeometryFactory.Default;
            IPoint pGeo = f.CreatePoint(new Coordinate(-75, 35));
            IPoint pUtm = GeometryTransform.TransformPoint(f, pGeo, trans.MathTransform);
            IPoint pGeo2 = GeometryTransform.TransformPoint(f, pUtm, trans.MathTransform.Inverse());
            IPoint expected = f.CreatePoint(new Coordinate(1885472.7, 1535925));

            Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.05), String.Format("Albers forward transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", expected.X, expected.Y, pUtm.X, pUtm.Y));
            Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), String.Format("Albers reverse transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", pGeo.X, pGeo.Y, pGeo2.X, pGeo2.Y));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        private bool ToleranceLessThan(IPoint p1, IPoint p2, double tolerance)
        {
            if (!p1.Z.Equals(Double.NaN) && !p2.Z.Equals(Double.NaN))
                 return Math.Abs(p1.X - p2.X) < tolerance && Math.Abs(p1.Y - p2.Y) < tolerance && Math.Abs(p1.Z - p2.Z) < tolerance;
            else return Math.Abs(p1.X - p2.X) < tolerance && Math.Abs(p1.Y - p2.Y) < tolerance;
        }
    }
}
