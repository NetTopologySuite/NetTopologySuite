using System;
using GeoAPI.Geometries;
using GeoAPI.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NetTopologySuite.Utilities;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace NetTopologySuite.Tests.NUnit.IO
{
    /// <summary>
    /// Tests the <see cref="WKBReader"/> and <see cref="WKBWriter"/>.
    /// Tests all geometries with both 2 and 3 dimensions and both byte orderings.
    /// </summary>
    public class WKBTest
    {
        private static readonly GeometryFactory GeomFactory = new GeometryFactory();
        private static readonly WKTReader Rdr = new WKTReader(GeomFactory);

        [TestAttribute]
        public void BigEndianTest()
        {
            var g = (Geometry)Rdr.Read("POINT(0 0)");
            RunGeometry(g, 2, ByteOrder.BigEndian, false, 100);
        }
        
        [TestAttribute]
        public void TestFirst()
        {
            RunWKBTest("MULTIPOINT ((0 0), (1 4), (100 200))");
        }
        [TestAttribute]
        public void TestPointPcs()
        {
            RunWKBTestPackedCoordinate("POINT (1 2)");
        }

        [TestAttribute]
        public void TestPoint()
        {
            RunWKBTest("POINT (1 2)");
        }

        [TestAttribute]
        public void TestLineString()
        {
            RunWKBTest("LINESTRING (1 2, 10 20, 100 200)");
        }
        [TestAttribute]
        public void TestPolygon()
        {
            RunWKBTest("POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0))");
        }

        [TestAttribute]
        public void TestPolygonWithHole()
        {
            RunWKBTest("POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0), (1 1, 1 10, 10 10, 10 1, 1 1) )");
        }

        [TestAttribute]
        public void TestMultiPoint()
        {
            RunWKBTest("MULTIPOINT ((0 0), (1 4), (100 200))");
        }

        [TestAttribute]
        public void TestMultiLineString()
        {
            RunWKBTest("MULTILINESTRING ((0 0, 1 10), (10 10, 20 30), (123 123, 456 789))");
        }

        [TestAttribute]
        public void TestMultiPolygon()
        {
            RunWKBTest("MULTIPOLYGON ( ((0 0, 100 0, 100 100, 0 100, 0 0), (1 1, 1 10, 10 10, 10 1, 1 1) ), ((200 200, 200 250, 250 250, 250 200, 200 200)) )");
        }

        [TestAttribute]
        public void TestGeometryCollection()
        {
            RunWKBTest("GEOMETRYCOLLECTION ( POINT ( 1 1), LINESTRING (0 0, 10 10), POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0)) )");
        }

        [TestAttribute]
        public void TestNestedGeometryCollection()
        {
            RunWKBTest("GEOMETRYCOLLECTION ( POINT (20 20), GEOMETRYCOLLECTION ( POINT ( 1 1), LINESTRING (0 0, 10 10), POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0)) ) )");
        }

        [TestAttribute]
        public void TestLineStringEmpty()
        {
            RunWKBTest("LINESTRING EMPTY");
        }

        [TestAttribute]
        public void TestBigPolygon()
        {
            GeometricShapeFactory shapeFactory = new GeometricShapeFactory(GeomFactory);
            shapeFactory.Base = new Coordinate(0, 0);
            shapeFactory.Size = 1000;
            shapeFactory.NumPoints = 1000;
            IGeometry geom = shapeFactory.CreateRectangle();
            RunWKBTest(geom, 2, false);
        }

        [TestAttribute]
        public void TestPolygonEmpty()
        {
            RunWKBTest("LINESTRING EMPTY");
        }

        [TestAttribute]
        public void TestMultiPointEmpty()
        {
            RunWKBTest("MULTIPOINT EMPTY");
        }

        [TestAttribute]
        public void TestMultiLineStringEmpty()
        {
            RunWKBTest("MULTILINESTRING EMPTY");
        }

        [TestAttribute]
        public void TestMultiPolygonEmpty()
        {
            RunWKBTest("MULTIPOLYGON EMPTY");
        }

        [TestAttribute]
        public void TestGeometryCollectionEmpty()
        {
            RunWKBTest("GEOMETRYCOLLECTION EMPTY");
        }

        private void RunWKBTest(String wkt)
        {
            RunWKBTestCoordinateArray(wkt);
            RunWKBTestPackedCoordinate(wkt);
        }

        private void RunWKBTestPackedCoordinate(String wkt)
        {
            GeometryFactory factory = new GeometryFactory(
                new PackedCoordinateSequenceFactory(PackedCoordinateSequenceFactory.PackedType.Double, 2));
            WKTReader reader = new WKTReader(factory);
            IGeometry g = reader.Read(wkt);

            // Since we are using a PCS of dim=2, only check 2-dimensional storage
            RunWKBTest(g, 2, true);
            RunWKBTest(g, 2, false);
        }

        private void RunWKBTestCoordinateArray(String wkt)
        {
            IGeometry g = Rdr.Read(wkt);

            // CoordinateArrays support dimension 3, so test both dimensions
            RunWKBTest(g, 2, true);
            RunWKBTest(g, 2, false);
            RunWKBTest(g, 3, true);
            RunWKBTest(g, 3, false);
        }

        private void RunWKBTest(IGeometry g, int dimension, bool toHex)
        {
            SetZ(g);
            RunWKBTest(g, dimension, ByteOrder.LittleEndian, toHex);
            RunWKBTest(g, dimension, ByteOrder.BigEndian, toHex);
        }

        private void RunWKBTest(IGeometry g, int dimension, ByteOrder byteOrder, bool toHex)
        {
            RunGeometry((Geometry)g, dimension, byteOrder, toHex, 100);
            RunGeometry((Geometry)g, dimension, byteOrder, toHex, 0);
            RunGeometry((Geometry)g, dimension, byteOrder, toHex, 101010);
            RunGeometry((Geometry)g, dimension, byteOrder, toHex, -1);
        }

        private static void SetZ(IGeometry g)
        {
            g.Apply(new AverageZFilter());
        }

        //static Comparator comp2D = new Coordinate.DimensionalComparator();
        //static Comparator comp3D = new Coordinate.DimensionalComparator(3);

        static readonly CoordinateSequenceComparator Comp2 = new CoordinateSequenceComparator(2);
        static readonly CoordinateSequenceComparator Comp3 = new CoordinateSequenceComparator(3);
        readonly WKBReader _wkbReader = new WKBReader(GeomFactory);

        void RunGeometry(Geometry g, int dimension, ByteOrder byteOrder, bool toHex, int srid)
        {
            bool includeSRID = false;
            if (srid >= 0)
            {
                includeSRID = true;
                g.SRID = srid;
            }

            WKBWriter wkbWriter = new WKBWriter(byteOrder, includeSRID, dimension==2 ? false : true);
            byte[] wkb = wkbWriter.Write(g);
            String wkbHex = null;
            if (toHex)
                wkbHex = WKBWriter.ToHex(wkb);

            if (toHex)
                wkb = WKBReader.HexToBytes(wkbHex);
            Geometry g2 = (Geometry)_wkbReader.Read(wkb);

            CoordinateSequenceComparator comp = (dimension == 2) ? Comp2 : Comp3;
            bool isEqual = (g.CompareTo(g2, comp) == 0);
            Assert.IsTrue(isEqual);

            if (includeSRID)
            {
                bool isSRIDEqual = g.SRID == g2.SRID;
                Assert.IsTrue(isSRIDEqual);
            }
        }
    }

    class AverageZFilter : ICoordinateFilter
    {
        public void Filter(Coordinate coord)
        {
            coord.Z = (coord.X + coord.Y) / 2;
        }
    }

}