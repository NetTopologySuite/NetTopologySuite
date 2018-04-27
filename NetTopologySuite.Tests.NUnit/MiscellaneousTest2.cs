﻿using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Planargraph;
using NetTopologySuite.Utilities;
using Assert = NUnit.Framework.Assert;

namespace NetTopologySuite.Tests.NUnit
{
    public class MiscellaneousTest2 
    {

        private static readonly IPrecisionModel precisionModel = new PrecisionModel(1);
        private static readonly IGeometryFactory geometryFactory = new GeometryFactory(precisionModel, 0);
        private static readonly WKTReader reader = new WKTReader(geometryFactory);

        [TestAttribute]
        public void testCoordinateHash() {
            doTestCoordinateHash(true, new Coordinate(1, 2), new Coordinate(1, 2));
            doTestCoordinateHash(false, new Coordinate(1, 2), new Coordinate(3, 4));
            doTestCoordinateHash(false, new Coordinate(1, 2), new Coordinate(1, 4));
            doTestCoordinateHash(false, new Coordinate(1, 2), new Coordinate(3, 2));
            doTestCoordinateHash(false, new Coordinate(1, 2), new Coordinate(2, 1));
        }

        private void doTestCoordinateHash(bool equal, Coordinate a, Coordinate b) {
            Assert.AreEqual(equal, a.Equals(b));
            Assert.AreEqual(equal, a.GetHashCode() == b.GetHashCode());
        }

        [TestAttribute]
        public void testZeroAreaPolygon()
        {
            var g = reader.Read(
                "POLYGON((0 0, 0 0, 0 0, 0 0, 0 0))");
            var valid = g.IsValid;
            Assert.IsTrue(true); //No exception thrown [Jon Aquino]
        }

        [TestAttribute]
        public void testLineStringIsValid()
        {
            var g = reader.Read(
                "LINESTRING(0 0, 0 0)");
            var valid = g.IsValid;
            Assert.IsTrue(true); //No exception thrown [Jon Aquino]
        }

        [TestAttribute]
        public void testUniqueCoordinateArrayFilter()
        {
            var g = reader.Read(
                "MULTIPOINT(10 10, 20 20, 30 30, 20 20, 10 10)");
            var f = new UniqueCoordinateArrayFilter();
            g.Apply(f);
            Assert.AreEqual(3, f.Coordinates.Length);
            Assert.AreEqual(new Coordinate(10, 10), f.Coordinates[0]);
            Assert.AreEqual(new Coordinate(20, 20), f.Coordinates[1]);
            Assert.AreEqual(new Coordinate(30, 30), f.Coordinates[2]);
        }

        [TestAttribute]
        public void testPointLocatorLinearRingLineString()
        {
            var pointLocator = new PointLocator();
            var gc = reader.Read("GEOMETRYCOLLECTION( LINESTRING(0 0, 10 10), LINEARRING(10 10, 10 20, 20 10, 10 10))");
            Assert.AreEqual(Location.Boundary, pointLocator.Locate(new Coordinate(10, 10), gc));
        }
        [TestAttribute]
        public void testPointLocator()  {
            var pointLocator = new PointLocator();
            var polygon = reader.Read("POLYGON ((70 340, 430 50, 70 50, 70 340))");
            Assert.AreEqual(Location.Exterior, pointLocator.Locate(new Coordinate(420, 340), polygon));
            Assert.AreEqual(Location.Boundary, pointLocator.Locate(new Coordinate(350, 50), polygon));
            Assert.AreEqual(Location.Boundary, pointLocator.Locate(new Coordinate(410, 50), polygon));
            Assert.AreEqual(Location.Interior, pointLocator.Locate(new Coordinate(190, 150), polygon));
        }
        [TestAttribute]
        public void test1()  {
            Assert.IsTrue(PointLocation.IsOnLine(new Coordinate(10, 10),
                                                new Coordinate[] {new Coordinate(0,10), new Coordinate(20,10)}));
            Assert.IsTrue(!PointLocation.IsOnLine(new Coordinate(30, 10),
                                                 new Coordinate[] {new Coordinate(0,10), new Coordinate(20,10)}));
        }

        [TestAttribute]
        public void testDirectedEdgeComparator() {
            var d1 = new DirectedEdge(new Node(new Coordinate(0, 0)),
                                      new Node(new Coordinate(10, 10)), new Coordinate(10, 10), true);
            var d2 = new DirectedEdge(new Node(new Coordinate(0, 0)),
                                      new Node(new Coordinate(20, 20)), new Coordinate(20, 20), false);
            Assert.AreEqual(0, d2.CompareTo(d1));
        }

        [TestAttribute]
        public void testDirectedEdgeToEdges() {
            var d1 = new DirectedEdge(new Node(new Coordinate(0, 0)),
                                      new Node(new Coordinate(10, 10)), new Coordinate(10, 10), true);
            var d2 = new DirectedEdge(new Node(new Coordinate(20, 0)),
                                      new Node(new Coordinate(20, 10)), new Coordinate(20, 10), false);
            var edges = DirectedEdge.ToEdges(new List<DirectedEdge> (new []{d1, d2}));
            Assert.AreEqual(2, edges.Count);
            Assert.IsNull(edges[0]);
            Assert.IsNull(edges[1]);
        }
        [TestAttribute]
        public void testNoding()  {
            var a = (ILineString)reader.Read("LINESTRING(0 0, 100 100)");
            var b = (ILineString)reader.Read("LINESTRING(0 100, 100 0)");
            var lineStrings = new List<ILineString>(new[] {a, b});
            var nodedLineStrings = (IGeometry)lineStrings[0];
            for (int i = 1; i < lineStrings.Count; i++) {
                nodedLineStrings = nodedLineStrings.Union(lineStrings[i]);
            }
            var expected =
                reader.Read("MULTILINESTRING ((0 0, 50 50), (50 50, 100 100), (0 100, 50 50), (50 50, 100 0))")
                    .Normalized();
            Assert.IsTrue(expected.Equals(nodedLineStrings.Normalized()));
        }
        [TestAttribute]
        public void testQuickPolygonUnion()  {
            var a = reader.Read("POLYGON((0 0, 100 0, 100 100, 0 100, 0 0))");
            var b = reader.Read("POLYGON((50 50, 150 50, 150 150, 50 150, 50 50))");
            var polygons = new [] {a, b};
            var polygonCollection = new GeometryFactory().CreateGeometryCollection(polygons);
            var union = polygonCollection.Buffer(0);
            Console.WriteLine(union.AsText());
            Assert.AreEqual("POLYGON ((0 0, 0 100, 50 100, 50 150, 150 150, 150 50, 100 50, 100 0, 0 0))", union.AsText());
        }
        [TestAttribute]
        public void testNoOutgoingDirEdgeFoundException()  {
            var a = new WKTReader().Read("MULTIPOLYGON (((1668033.7441322226 575074.5372325261, 1668043.6526485088 575601.4901441064, 1668049.5076808596 575876.2262774946, 1668054.4619390026 576155.4662819218, 1668057.61464873 576428.4008668943, 1668059.8665842495 576711.2439681528, 1668063.9200681846 576991.3847467878, 1668071.576648951 577269.7239770072, 1668075.630132886 577547.1624330188, 1668077.8820684056 577825.5016632382, 1668081.935552341 578102.9401192497, 1668087.7905846918 578380.3785752613, 1668094.5463912506 578650.6108376103, 1668097.699100978 578919.9423257514, 1668103.5541333288 579191.0753623082, 1668111.2107140953 579455.9029794101, 1668112.5230371233 579490.6388405386, 1668120.62746972 579490.4954984378, 1668113.4626496148 579183.8691686456, 1668108.5083914716 578916.3392289202, 1668104.4549075365 578642.50386974, 1668100.401423601 578368.6685105597, 1668094.54639125 578095.7339255873, 1668088.6913588992 577822.7993406148, 1668085.5386491718 577548.9639814346, 1668082.3859394444 577275.1286222544, 1668076.5309070935 577002.1940372819, 1668072.4774231582 576729.2594523095, 1668066.6223908074 576456.324867337, 1668063.46968108 576183.3902823646, 1668059.416197145 575910.4556973921, 1668055.3627132094 575637.5211124197, 1668052.210003482 575366.3880758629, 1668046.354971131 575097.9573619296, 1668046.805358235 575068.2318130712, 1668033.7441322226 575074.5372325261)))");
            var b = new WKTReader().Read("MULTIPOLYGON (((1665830.62 580116.54, 1665859.44 580115.84, 1666157.24 580108.56, 1666223.3 580107.1, 1666313 580105.12, 1666371.1 580103.62, 1666402 580102.78, 1666452.1 580101.42, 1666491.02 580100.36, 1666613.94 580097.02, 1666614.26 580097.02, 1666624 580096.74, 1666635.14 580096.42, 1666676.16 580095.28, 1666722.42 580093.94, 1666808.26 580091.44, 1666813.42 580091.3, 1666895.02 580088.78, 1666982.06 580086.1, 1667067.9 580083.46, 1667151.34 580080.88, 1667176.8 580080.1, 1667273.72 580077.14, 1667354.54 580074.68, 1667392.4 580073.88, 1667534.24 580070.9, 1667632.7 580068.82, 1667733.94 580066.68, 1667833.62 580064.58, 1667933.24 580062.5, 1667985 580061.4, 1668033.12 580060.14, 1668143.7 580057.24, 1668140.64 579872.78, 1668134.7548600042 579519.7278276943, 1668104.737250423 579518.9428425882, 1668110.64 579873.68, 1668113.18 580025.46, 1668032.4 580027.46, 1667932.66 580030.08, 1667832.8 580032.58, 1667632.28 580037.78, 1667392.14 580043.78, 1667273.4 580046.72, 1667150.62 580049.46, 1667067.14 580051.78, 1666981.14 580053.84, 1666807.4 580057.96, 1666613.64 580062.58, 1666490.14 580065.78, 1666400.9 580067.78, 1666312.18 580070.36, 1666222.1 580072.6, 1665859.28 580079.52, 1665830.28 580080.14, 1665830.62 580116.54)), ((1668134.2639058917 579490.2543124713, 1668130.62 579270.86, 1668125.86 578984.78, 1668117.3 578470.2, 1668104.02 577672.06, 1668096.78 577237.18, 1668093.4 577033.64, 1668087.28 576666.92, 1668085.24 576543.96, 1668083.32 576428.36, 1668081.28 576305.86, 1668075.38 575950.9, 1668061.12 575018.44, 1666745.6 575072.62, 1665835.48 575109.72, 1665429.26 575126.26, 1664940.66 575148.86, 1664365.4 575170.64, 1664116.02 575181.78, 1662804.22 575230.32, 1662804.780409841 575260.319992344, 1664086.52 575208.92, 1664150.3090003466 579072.2660557877, 1664180.345101783 579073.7529915024, 1664174.46 578717.2, 1664204.44 578716.82, 1664173.3 576830.12, 1664146.48 575206.52, 1665410.98 575155.82, 1665439.18 576784.24, 1665441.16 576899.44, 1665441.88 576940.4, 1665478.5547472103 579058.5389785315, 1665518.6155320513 579061.3502616781, 1665450.98 575156.2, 1668030.38 575050.3, 1668104.2687072477 579490.7848338542, 1668134.2639058917 579490.2543124713)), ((1664150.7710040906 579100.2470608585, 1664160.68 579700.38, 1664165.68 579987.66, 1664195.2 579986.98, 1664190.68 579699.9, 1664180.7918241904 579100.8179797827, 1664150.7710040906 579100.2470608585)), ((1665478.9532824333 579081.5562602862, 1665483.38 579337.22, 1665503.38 579336.64, 1665505.06 579443.26, 1665525.22 579442.68, 1665522.9750161383 579313.0587927903, 1665513.4612495075 579308.8304520656, 1665510.9439672586 579258.4848070825, 1665510.9439672586 579114.9997188805, 1665503.392120511 579082.2750496415, 1665478.9532824333 579081.5562602862)))");
            a.Difference(b);
            b.Difference(a);
        }

    }
}