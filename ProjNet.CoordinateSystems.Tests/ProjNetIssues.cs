using System;
using NUnit.Framework;

namespace ProjNet.UnitTests
{
    [TestFixture]
    public class ProjNetIssues : CoordinateTransformTestsBase
    {
        public ProjNetIssues()
        {
            Verbose = true;
        }

        [Test, Description("WGS_84UTM to WGS_84 is inaccurate")]
        public void TestIssue23773()
        {
            var csUtm18N = CoordinateSystems.ProjectedCoordinateSystem.WGS84_UTM(18, true);
            var csUtm18NWkt = CoordinateSystemFactory.CreateFromWkt(
                "PROJCS[\"WGS 84 / UTM zone 18N\",GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER[\"central_meridian\",-75],PARAMETER[\"scale_factor\",0.9996],PARAMETER[\"false_easting\",500000],PARAMETER[\"false_northing\",0],AUTHORITY[\"EPSG\",\"32618\"],AXIS[\"Easting\",EAST],AXIS[\"Northing\",NORTH]]");
            var csWgs84 = CoordinateSystems.GeographicCoordinateSystem.WGS84;

            var ct = CoordinateTransformationFactory.CreateFromCoordinateSystems(csUtm18N, csWgs84);
            var ct2 = CoordinateTransformationFactory.CreateFromCoordinateSystems(csUtm18NWkt, csWgs84);

            var putm = new[] {307821.867d, 4219306.387d};
            var pgeo = ct.MathTransform.Transform(putm);
            var pgeoWkt = ct2.MathTransform.Transform(putm);
            var pExpected = new[] {-77.191769, 38.101147d};

            Assert.IsTrue(ToleranceLessThan(pgeoWkt, pExpected, 0.00001d),
                TransformationError("UTM18N -> WGS84", pExpected, pgeo));
            Assert.IsTrue(ToleranceLessThan(pgeo, pExpected, 0.00001d),
                TransformationError("UTM18N -> WGS84", pExpected, pgeo));
        }

        [Test, Description("Proj.net reprojection problem, Discussion http://projnet.codeplex.com/discussions/351733")]
        public void TestDiscussion351733()
        {
            var csSource = CoordinateSystemFactory.CreateFromWkt(
                "PROJCS[\"Pulkovo 1942 / Gauss-Kruger zone 14\",GEOGCS[\"Pulkovo 1942\",DATUM[\"Pulkovo_1942\",SPHEROID[\"Krassowsky 1940\",6378245,298.3,AUTHORITY[\"EPSG\",\"7024\"]],TOWGS84[23.92,-141.27,-80.9,-0,0.35,0.82,-0.12],AUTHORITY[\"EPSG\",\"6284\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4284\"]],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER[\"central_meridian\",81],PARAMETER[\"scale_factor\",1],PARAMETER[\"false_easting\",14500000],PARAMETER[\"false_northing\",0],AUTHORITY[\"EPSG\",\"28414\"],AXIS[\"X\",NORTH],AXIS[\"Y\",EAST]]\"");
            var csTarget = CoordinateSystemFactory.CreateFromWkt(
                "GEOGCS[\"Pulkovo 1942\",DATUM[\"Pulkovo_1942\",SPHEROID[\"Krassowsky 1940\",6378245,298.3,AUTHORITY[\"EPSG\",\"7024\"]],TOWGS84[23.92,-141.27,-80.9,-0,0.35,0.82,-0.12],AUTHORITY[\"EPSG\",\"6284\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4284\"]]\"");

            var ct = CoordinateTransformationFactory.CreateFromCoordinateSystems(csSource, csTarget);

            var pp = new[] {14181052.913, 6435927.692};
            var pg = ct.MathTransform.Transform(pp);
            var pExpected = new[] { 75.613911283608331, 57.926509119323505 };
            var pp2 = ct.MathTransform.Inverse().Transform(pg);

            Verbose = true;
            Assert.IsTrue(ToleranceLessThan(pg, pExpected, 1e-6),
                TransformationError("EPSG 28414 -> EPSG 4284", pExpected, pg));
            Assert.IsTrue(ToleranceLessThan(pp, pp2, 1e-3),
                TransformationError("EPSG 28414 -> Pulkovo 1942", pp, pp2, true));
        }

        [Test, Description("Problem converting coordinates, Discussion http://projnet.codeplex.com/discussions/352813")]
        public void TestDiscussion352813()
        {
            var csSource = CoordinateSystems.GeographicCoordinateSystem.WGS84;
            var csTarget = CoordinateSystems.ProjectedCoordinateSystem.WebMercator;
                //           CoordinateSystemFactory.CreateFromWkt(
                //"PROJCS[\"Popular Visualisation CRS / Mercator\"," +
                //         "GEOGCS[\"Popular Visualisation CRS\"," +
                //                  "DATUM[\"Popular Visualisation Datum\"," +
                //                          "SPHEROID[\"Popular Visualisation Sphere\", 6378137, 298.257223563, " +
                //                          "AUTHORITY[\"EPSG\", \"7030\"]]," +
                ///*"TOWGS84[0, 0, 0, 0, 0, 0, 0], */"AUTHORITY[\"EPSG\", \"6055\"]], " +
                //                  "PRIMEM[\"Greenwich\", 0, AUTHORITY[\"EPSG\", \"8901\"]]," +
                //                  "UNIT[\"degree\", 0.0174532925199433, AUTHORITY[\"EPSG\", \"9102\"]]," +
                //                  "AXIS[\"E\", EAST]," +
                //                  "AXIS[\"N\", NORTH]," +
                //                  "AUTHORITY[\"EPSG\", \"4055\"]]," +
                //         "PROJECTION[\"Mercator\"]," +
                //         "PARAMETER[\"semi_major\", 6378137]," +
                //         "PARAMETER[\"semi_minor\", 6378137]," +
                //         "PARAMETER[\"scale_factor\", 1]," +
                //         "PARAMETER[\"False_Easting\", 0]," +
                //         "PARAMETER[\"False_Northing\", 0]," +
                //         "PARAMETER[\"Central_Meridian\", 0]," +
                //         "PARAMETER[\"Latitude_of_origin\", 0]," +
                //         "UNIT[\"metre\", 1, AUTHORITY[\"EPSG\", \"9001\"]]," +
                //         "AXIS[\"East\", EAST]," +
                //"AXIS[\"North\", NORTH]," +
                //"AUTHORITY[\"EPSG\", \"3857\"]]");

            //"PROJCS["WGS 84 / Pseudo-Mercator",GEOGCS["WGS 84",DATUM["WGS_1984",SPHEROID["WGS 84",6378137,298.257223563,AUTHORITY["EPSG","7030"]],AUTHORITY["EPSG","6326"]],PRIMEM["Greenwich",0,AUTHORITY["EPSG","8901"]],UNIT["degree",0.0174532925199433,AUTHORITY["EPSG","9122"]],AUTHORITY["EPSG","4326"]],UNIT["metre",1,AUTHORITY["EPSG","9001"]],PROJECTION["Mercator_1SP"],PARAMETER["central_meridian",0],PARAMETER["scale_factor",1],PARAMETER["false_easting",0],PARAMETER["false_northing",0],EXTENSION["PROJ4","+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +wktext  +no_defs"],AUTHORITY["EPSG","3857"],AXIS["X",EAST],AXIS["Y",NORTH]]"
            var ct = CoordinateTransformationFactory.CreateFromCoordinateSystems(csSource, csTarget);
            //var ct2 = CoordinateTransformationFactory.CreateFromCoordinateSystems(csSource, csTarget2);

            Verbose = true;

            var pg1 = new[] { 23.57892d, 37.94712d };
            //src DotSpatial.Projections
            var pExpected = new[] { 2624793.3678553337, 4571958.333297424 };

            var pp = ct.MathTransform.Transform(pg1);
            Console.WriteLine(TransformationError("EPSG 4326 -> EPSG 3857", pExpected, pp));

            Assert.IsTrue(ToleranceLessThan(pp, pExpected, 1e-9),
                TransformationError("EPSG 4326 -> EPSG 3857", pExpected, pp));

            var pg2 = ct.MathTransform.Inverse().Transform(pp);
            Assert.IsTrue(ToleranceLessThan(pg1, pg2, 1e-13),
                TransformationError("EPSG 4326 -> EPSG 3857", pg1, pg2, true));
        }

    }
}