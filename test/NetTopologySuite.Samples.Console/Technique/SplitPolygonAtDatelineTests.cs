using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NetTopologySuite.Samples.Technique
{
    [TestFixture]
    public class SplitPolygonAtDatelineTests
    {
        private const string wktPrj = "GEOGCS['WGS 84',DATUM['WGS_1984',SPHEROID['WGS 84',6378137,298.257223563,AUTHORITY['EPSG','7030']],AUTHORITY['EPSG','6326']]," +
            "PRIMEM['Greenwich',0,AUTHORITY['EPSG','8901']],UNIT['degree',0.0174532925199433,AUTHORITY['EPSG','9122']],AUTHORITY['EPSG','4326']]";

        private const string wktGeom = "MULTIPOINT ((170 3.5), (180 3.5), (180 -5), (188.927777777778 -5), (187.061666666667 -9.46444444444444), " +
            "(185.718888888889 -12.4869444444444), (185.281944444444 -13.4555555555556), (184.472777777778 -15.225), (184.3275 -15.5397222222222), " +
            "(184.171944444444 -15.9), (183.188888888889 -18.1488888888889), (183.019444444444 -18.5302777777778), (181.530555555556 -21.8), " +
            "(180 -25), (177.333333333333 -25), (171.416666666667 -25), (168 -28), (163 -30), (163 -24), (163 -17.6666666666667), (161.25 -14), " +
            "(163 -14), (166.875 -11.8), (170 -10), (170 3.5))";

        private static CoordinateSequence GetCoords()
        {
            var rdr = new WKTReader();
            var geom = rdr.Read(wktGeom);
            var mps = (MultiPoint)geom;

            var geomFactory = new GeometryFactory();
            var coordSeqFactory = geomFactory.CoordinateSequenceFactory;
            var coordSeq = coordSeqFactory.Create(mps.Coordinates);

            return coordSeq;
        }

        private static void DebugPrintCoords(IList<Coordinate> coords, string hdr)
        {
            Debug.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Debug.Print(hdr + "={0}", coords.Count);
            Debug.Print("kIdx,x,y");
            Coordinate coordK;
            for (int k = 0; k <= coords.Count - 1; k++)
            {
                coordK = coords[k];
                Debug.Print("{0},{1},{2}", k, coordK.X, coordK.Y);
            }
            Debug.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        }

        [Test]
        public void TestWithDefaults()
        {
            // ========================================================================================
            // function with defaults
            // ========================================================================================
            var coordSeq = GetCoords();
            var wtr = new WKTWriter();
            var resGeoms = SplitPolygonAtDateline.ToPolyExOp(coordSeq, wktPrj);
            Assert.That(resGeoms, Is.Not.Null);
            Assert.That(resGeoms, Is.InstanceOf<MultiPolygon>());
            Assert.That(resGeoms.NumGeometries, Is.EqualTo(2));
            for (int i = 0; i < resGeoms.NumGeometries; i++)
            {
                var resGeom = resGeoms.GetGeometryN(i);
                Assert.That(resGeom, Is.Not.Null);
                Assert.That(resGeom, Is.InstanceOf<Polygon>());
                string resWkt = wtr.Write(resGeom);
                Debug.Print("geometry {0} = {1}", i, resWkt);
                DebugPrintCoords(resGeom.Coordinates, $"resGeom geom [{i}] count");
            }
        }

        [Test]
        public void TestWithSpecificInputs1()
        {
            // ========================================================================================
            // function with specific parameter inputs. trim 0.75 on parts in the Eastern hemisphere and
            // densify every 0.5 and attempt to fix invalid polygons via the buffer method.
            // ========================================================================================
            var coordSeq = GetCoords();
            var wtr = new WKTWriter();

            var outParams = new SplitDatelineOuput();
            outParams.OutType = OgcGeometryType.Polygon;
            outParams.OutTrimGap = 0.75;
            outParams.OutDensifyResolution = 0.5;
            outParams.InvGeomFixMethod = InvalidGeomFixMethod.FixViaBuffer;

            var resGeoms = SplitPolygonAtDateline.ToPolyExOp(coordSeq, wktPrj, outParams);
            Assert.That(resGeoms, Is.Not.Null);
            Assert.That(resGeoms, Is.InstanceOf<MultiPolygon>());
            Assert.That(resGeoms.NumGeometries, Is.EqualTo(2));
            for (int i = 0; i < resGeoms.NumGeometries; i++)
            {
                var resGeom = resGeoms.GetGeometryN(i);
                Assert.That(resGeom, Is.Not.Null);
                Assert.That(resGeom, Is.InstanceOf<Polygon>());
                string resWkt = wtr.Write(resGeom);
                Debug.Print("geometry {0} = {1}", i, resWkt);
                DebugPrintCoords(resGeom.Coordinates, $"resGeom geom [{i}] count");
            }
        }

        [Test]
        public void TestWithSpecificInputs2()
        {
            // ========================================================================================
            // function with specific parameter inputs. trim 0.75 on parts in the Western hemisphere and
            // densify every 0.25 and do not attempt to fix invalid polygons.
            // ========================================================================================
            var coordSeq = GetCoords();
            var wtr = new WKTWriter();

            var outParams = new SplitDatelineOuput();
            outParams.OutType = OgcGeometryType.Polygon;
            outParams.OutTrimGap = -0.75;
            outParams.OutDensifyResolution = 0.25;
            outParams.InvGeomFixMethod = InvalidGeomFixMethod.FixNone;

            var resGeoms = SplitPolygonAtDateline.ToPolyExOp(coordSeq, wktPrj, outParams);
            Assert.That(resGeoms, Is.Not.Null);
            Assert.That(resGeoms, Is.InstanceOf<MultiPolygon>());
            Assert.That(resGeoms.NumGeometries, Is.EqualTo(2));
            for (int i = 0; i < resGeoms.NumGeometries; i++)
            {
                var resGeom = resGeoms.GetGeometryN(i);
                Assert.That(resGeom, Is.Not.Null);
                Assert.That(resGeom, Is.InstanceOf<Polygon>());
                string resWkt = wtr.Write(resGeom);
                Debug.Print("geometry {0} = {1}", i, resWkt);
                DebugPrintCoords(resGeom.Coordinates, $"resGeom geom [{i}] count");
            }
        }
    }
}
