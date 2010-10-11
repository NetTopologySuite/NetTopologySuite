using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GeoAPI.Operations.Buffer;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;
using NetTopologySuite.Simplify;
using NetTopologySuite.Coordinates;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.NTS
{
    [TestFixture]
    public class Issues
    {
        [Test]
        public void Issue62()
        {
            IGeometryFactory<Coordinate> geomFactory
                = new GeometryFactory<Coordinate>(new CoordinateSequenceFactory());
            IWktGeometryReader<Coordinate> wktReader = new WktReader<Coordinate>(geomFactory, null);

            // Examples of failing combinations: (s1,*), (s2,*), (s3,s4)
            string[] s = new string[]
                             {
                                 "POINT (140 280)",
                                 "MULTIPOINT ((-500 -500), (-500 500), (500 500), (500 -500))",
                                 "LINESTRING (150 100, 200 100, 200 200, 100 200)",
                                 "POLYGON ((-80 210, -10 250, 60 160, -10 110, -100 120, -80 210))"
                             };

            try
            {
                for (int i = 1; i < s.Length; i += 1)
                {
                    // Select the 2 input geometries that we want to Test
                    IGeometry<Coordinate> g1 = wktReader.Read(s[i-1]);
                    IGeometry<Coordinate> g2 = wktReader.Read(s[i]);
                    Console.WriteLine("Geometry 1: " + g1);
                    Console.WriteLine("Geometry 2: " + g2);

                    //g1.Distance(g2);
                    DistanceOp<Coordinate> distOp = new DistanceOp<Coordinate>(g1, g2);

                    // Get the 2 closest points between g1 and g2
                    //var neares = distOp.ClosestLocations();
                    Pair<Coordinate>? closestPt = distOp.ClosestPoints();
                    if (closestPt == null)
                        Console.WriteLine("==> Computation of closestPt returned null.");
                    else
                    {
                        ILineString line = geomFactory.CreateLineString(closestPt);
                        Console.WriteLine("==> Line: " + line);
                    }

                    // Display distance between the 2 input geometries
                    double distance = distOp.Distance;
                    Console.WriteLine(string.Format("Distance between closest points: {0}\n", distance));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error:\n" + e.Message);
            }
        }
        
        
        [Test]
        public void Issue60()
        {
            ICoordinateFactory<Coordinate> cf = GeometryUtils.CoordFac;
            ICoordinateSequenceFactory<Coordinate> csf = GeometryUtils.CoordSeqFac;

            List<Coordinate> list = new List<Coordinate>(
                new Coordinate[] { cf.Create(10, 10), cf.Create(10, 20), cf.Create(20, 20),
                                    cf.Create(20, 10), cf.Create(30, 10)});
            ICoordinateSequence<Coordinate> coordSeq = csf.Create(list);
            Console.WriteLine(string.Format("Input: {0}", coordSeq));
            try
            {
                ICoordinateSequence outputCoords = DouglasPeuckerLineSimplifier<Coordinate>.Simplify(coordSeq, 5);
                Console.WriteLine(string.Format("Output: {0}", outputCoords));
            }
            catch (Exception e)
            {
                throw;
            }
        }
        [Test]
        public void Issue61()
        {
            ICoordinateFactory<Coordinate> coordFactory = GeometryUtils.CoordFac;
            IGeometryFactory<Coordinate> geomFactory
                = new GeometryFactory<Coordinate>(
                    new CoordinateSequenceFactory(coordFactory as CoordinateFactory));
            double tolerance = 63.0;

            IGeometry < Coordinate > inputGeometry = geomFactory.WktReader.Read("LINESTRING (2783 2949, 2788 -1237, 764 -2410, -1589 -2274, -2724 2451, 2783 2949)");
            IGeometry<Coordinate> buffer = inputGeometry.Buffer(tolerance);
            Console.WriteLine(buffer);

            inputGeometry = geomFactory.WktReader.Read(@"LINESTRING (3155 91, 2975 -3041, 979 -2893, -1497 -4490, -2664 -2422, -1486 -490,
-4033 1558, -2373 3682, 368 3548, 3155 91)");
            buffer = inputGeometry.Buffer(tolerance);
            Console.WriteLine(buffer);
            inputGeometry = geomFactory.WktReader.Read(@"LINESTRING (3282 89, 3062 -2335, -104 -1722, -3146 -2745, -2676 990, -1014 1831, -508
4821, 664 987, 3282 89)
");
            buffer = inputGeometry.Buffer(tolerance);
            Console.WriteLine(buffer);
            inputGeometry = geomFactory.WktReader.Read(@"LINESTRING (6757 5207, 6602 3743, 3575 4282, 3294 4897, 3623 6276, 4925 7132, 5947
6943, 6757 5207)
");
            buffer = inputGeometry.Buffer(tolerance);
            Console.WriteLine(buffer);
        }

        [Test]
        public void Issue54()
        {
            var geom = GeometryUtils.Reader.Read(@"POLYGON((906.4827 217.8143,927.6762 
0.0099999999999909051,36.486899999999991 
0.0099999999999909051,0.0099999999999909051 374.8819,906.4827 217.8143))
");
            var res = geom.Buffer(2d, 4, BufferStyle.Round);
            Console.WriteLine(res);
     

        }

        [Test]
        public void Issue36_1()
        {
            var geom1 = GeometryUtils.Reader.Read(
                @"POLYGON((719068.76798974432 6178827.370335687 31.0995,
719070.73569863627 6178830.5852228012 31.0995,
719076.87100000086 6178826.8299 31.0995,
719078.2722488807 6178825.9722172953 31.0995,
719076.30480000074 6178822.7577000009 31.0995,
719068.76798974432 6178827.370335687 31.0995))");
            Assert.IsNotNull(geom1);
            Assert.IsTrue(geom1.IsValid);
            Console.WriteLine(string.Format("Input:\n{0}", geom1));

            var actual = geom1.Buffer(0.01); // Throws exception
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.IsValid);
            var expected = GeometryUtils.Reader.Read(
                @"POLYGON ((719068.7627696363 6178827.361806299, 
719068.7612059064 6178827.36298861, 
719068.759902887 6178827.364453278, 
719068.7589106546 6178827.366144013, 
719068.7582673416 6178827.36799584, 
719068.7579976716 6178827.369937588, 
719068.758112008 6178827.371894637, 
719068.7586059568 6178827.373791773, 
719068.759460535 6178827.375556088, 
719070.727169427 6178830.590443202, 
719070.7283517772 6178830.592006875, 
719070.7298164691 6178830.593309835, 
719070.7315072143 6178830.594302007, 
719070.7333590367 6178830.594945263, 
719070.7353007706 6178830.595214883, 
719070.7372577946 6178830.595100504, 
719070.7391548998 6178830.594606523, 
719070.7409191799 6178830.593751923, 
719076.8762205443 6178826.838429122, 
719078.2774694242 6178825.980746417, 
719078.2790330398 6178825.979564076, 
719078.2803359521 6178825.97809941, 
719078.2813280922 6178825.976408705, 
719078.2819713341 6178825.974556932, 
719078.2822409591 6178825.97261525, 
719078.2821266061 6178825.970658276, 
719078.2816326693 6178825.968761212, 
719078.2807781297 6178825.966996959, 
719076.3133292497 6178822.752479665, 
719076.3121469484 6178822.750916023, 
719076.3106823174 6178822.749613076, 
719076.3089916399 6178822.748620895, 
719076.3071398855 6178822.747977607, 
719076.3051982138 6178822.747707932, 
719076.3032412394 6178822.747822234, 
719076.3013441653 6178822.748316121, 
719076.2995798928 6178822.749170613, 
719068.7627696363 6178827.361806299))");
            Assert.IsTrue( expected.Equals( actual, new Tolerance(1e-8)));
            Console.WriteLine(string.Format("\nResult:\n{0}", actual));
        }

        [Test]
        public void Issue36_2()
        {
            var geom1 = GeometryUtils.Reader.Read(
                @"LINESTRING(1250.7665 446.9385,
1137.8786 170.4488,
1136.3666106287267 166.74557327980631,
1139.485009866369 125.36515638486206,
1137.8786 121.7019)");
            Assert.IsNotNull(geom1);
            Assert.IsTrue(geom1.IsValid);
            Console.WriteLine(string.Format("Input:\n{0}", geom1));

            var result = geom1.Buffer(5);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
            result.Normalize();
            Console.WriteLine(string.Format("\nResult:\n{0}", result));

            var expected = GeometryUtils.Reader.Read(
                @"POLYGON ((1142.5076334002422 168.55881328590834, 1141.4410130256356 165.94640267866984, 1144.4708724926961 125.74088750067949, 1144.413756618301 124.52405267228002, 1144.0640803474464 123.35714405190089, 1142.4576704810775 119.69388766703882, 1141.9779411531201 118.83913872627556, 1141.3406764435852 118.09440409580651, 1140.5703660778781 117.4883035098286, 1139.696612651358 117.04412907395584, 1138.7529940181994 116.77895016266294, 1137.7757729132873 116.70295745279682, 1136.8025033956515 116.81907130158044, 1135.8705876670388 117.12282951892244, 1135.0158387262757 117.60255884687994, 1134.2711040958065 118.23982355641485, 1133.6650035098287 119.01013392212188, 1133.2208290739559 119.88388734864198, 1132.955650162663 120.82750598180051, 1132.879657452797 121.80472708671265, 1132.9957713015804 122.7779966043485, 1133.2995295189226 123.70991233296117, 1134.4055086776207 126.23198674214223, 1131.3807480023995 166.36984216398886, 1131.427488670157 167.52343848559283, 1131.7375772284845 168.63555999389797, 1133.2495665997578 172.33878671409167, 1246.1374665997578 448.82848671409164, 1246.595130295206 449.6952507660154, 1247.2130973890783 450.4560744320048, 1247.967619752517 451.0817196852508, 1248.8297015142364 451.5481433295369, 1249.7662133555946 451.8374209657867, 1250.7411656517302 451.9384358166678, 1251.7170915326622 451.8473059381107, 1252.6564867140917 451.5675334002421, 1253.5232507660153 451.109869704794, 1254.284074432005 450.4919026109218, 1254.9097196852508 449.737380247483, 1255.376143329537 448.8752984857635, 1255.6654209657866 447.93878664440524, 1255.7664358166678 446.9638343482697, 1255.6753059381106 445.98790846733783, 1255.3955334002421 445.0485132859083, 1142.5076334002422 168.55881328590834))");
            expected.Normalize();
            Console.WriteLine(string.Format("\nExpected:\n{0}",expected));
            Assert.IsTrue(expected.Equals(result, new Tolerance(1e-8)));
        }

        [Test]
        public void Issue23()
        {
            const string wkt = "LINESTRING (-3 2, 0 -2, 3 2)";

            var csf =
                (ICoordinateSequenceFactory<BufferedCoordinate>)
                new BufferedCoordinateSequenceFactory(new BufferedCoordinateFactory(PrecisionModelType.DoubleFloating));
            var gf = (IGeometryFactory<BufferedCoordinate>) new GeometryFactory<BufferedCoordinate>(csf);

            var linestring = gf.WktReader.Read(wkt);

            var centroid = linestring.Centroid;
            var expected = gf.CreatePoint2D(0, 0);
            Assert.AreEqual(expected, centroid);
        }
    }
}
