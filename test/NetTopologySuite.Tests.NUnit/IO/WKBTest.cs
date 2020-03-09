#nullable disable
using System;
using System.Collections.Generic;
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

        [Test]
        public void BigEndianTest()
        {
            var g = (Geometry)Rdr.Read("POINT(0 0)");
            RunGeometry(g, 2, ByteOrder.BigEndian, false, 100);
        }

        [Test]
        public void TestFirst()
        {
            RunWKBTest("MULTIPOINT ((0 0), (1 4), (100 200))");
        }
        [Test]
        public void TestPointPcs()
        {
            RunWKBTestPackedCoordinate("POINT (1 2)");
        }

        [Test]
        public void TestPoint()
        {
            RunWKBTest("POINT (1 2)");
        }

        [Test]
        public void TestLineString()
        {
            RunWKBTest("LINESTRING (1 2, 10 20, 100 200)");
        }
        [Test]
        public void TestPolygon()
        {
            RunWKBTest("POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0))");
        }

        [Test]
        public void TestPolygonWithHole()
        {
            RunWKBTest("POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0), (1 1, 1 10, 10 10, 10 1, 1 1) )");
        }

        [Test]
        public void TestMultiPoint()
        {
            RunWKBTest("MULTIPOINT ((0 0), (1 4), (100 200))");
        }

        [Test]
        public void TestMultiLineString()
        {
            RunWKBTest("MULTILINESTRING ((0 0, 1 10), (10 10, 20 30), (123 123, 456 789))");
        }

        [Test]
        public void TestMultiPolygon()
        {
            RunWKBTest("MULTIPOLYGON ( ((0 0, 100 0, 100 100, 0 100, 0 0), (1 1, 1 10, 10 10, 10 1, 1 1) ), ((200 200, 200 250, 250 250, 250 200, 200 200)) )");
        }

        [Test]
        public void TestGeometryCollection()
        {
            RunWKBTest("GEOMETRYCOLLECTION ( POINT ( 1 1), LINESTRING (0 0, 10 10), POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0)) )");
        }

        [Test]
        public void TestNestedGeometryCollection()
        {
            RunWKBTest("GEOMETRYCOLLECTION ( POINT (20 20), GEOMETRYCOLLECTION ( POINT ( 1 1), LINESTRING (0 0, 10 10), POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0)) ) )");
        }

        [Test]
        public void TestLineStringEmpty()
        {
            RunWKBTest("LINESTRING EMPTY");
        }

        [Test]
        public void TestBigPolygon()
        {
            var shapeFactory = new GeometricShapeFactory(GeomFactory);
            shapeFactory.Base = new Coordinate(0, 0);
            shapeFactory.Size = 1000;
            shapeFactory.NumPoints = 1000;
            Geometry geom = shapeFactory.CreateRectangle();
            RunWKBTest(geom, 2, false);
        }

        [Test]
        public void TestPolygonEmpty()
        {
            RunWKBTest("POLYGON EMPTY");
        }

        [Test]
        public void TestMultiPointEmpty()
        {
            RunWKBTest("MULTIPOINT EMPTY");
        }

        [Test]
        public void TestMultiLineStringEmpty()
        {
            RunWKBTest("MULTILINESTRING EMPTY");
        }

        [Test]
        public void TestMultiPolygonEmpty()
        {
            RunWKBTest("MULTIPOLYGON EMPTY");
        }

        [Test]
        public void TestGeometryCollectionEmpty()
        {
            RunWKBTest("GEOMETRYCOLLECTION EMPTY");
        }

        private void RunWKBTest(string wkt)
        {
            RunWKBTestCoordinateArray(wkt);
            RunWKBTestPackedCoordinate(wkt);
        }

        private void RunWKBTestPackedCoordinate(string wkt)
        {
            var factory = new GeometryFactory(
                new PackedCoordinateSequenceFactory(PackedCoordinateSequenceFactory.PackedType.Double));
            var reader = new WKTReader(factory);
            var g = reader.Read(wkt);

            // Since we are using a PCS of dim=2, only check 2-dimensional storage
            RunWKBTest(g, 2, true);
            RunWKBTest(g, 2, false);
        }

        private void RunWKBTestCoordinateArray(string wkt)
        {
            var g = Rdr.Read(wkt);

            // CoordinateArrays support dimension 3, so test both dimensions
            g = SetDimension(g, 2);
            RunWKBTest(g, 2, true);
            RunWKBTest(g, 2, false);
            g = SetDimension(g, 3);
            RunWKBTest(g, 3, true);
            RunWKBTest(g, 3, false);
        }

        private static Geometry SetDimension(Geometry p0, int dimension)
        {
            if (p0 is GeometryCollection)
            {
                var tmp = new List<Geometry>();
                for (int i = 0; i < p0.NumGeometries; i++)
                    tmp.Add(SetDimension(p0.GetGeometryN(i), dimension));
                return p0.Factory.BuildGeometry(tmp);
            }
            var fact = p0.Factory.CoordinateSequenceFactory;
            if (p0 is Point)
            {
                return p0.Factory.CreatePoint(SetDimension(fact, ((Point) p0).CoordinateSequence, dimension));
            }
            if (p0 is LineString)
            {
                return p0.Factory.CreateLineString(SetDimension(fact, ((LineString) p0).CoordinateSequence, dimension));
            }
            if (p0 is Polygon)
            {
                var p = (Polygon) p0;
                var er =
                    p0.Factory.CreateLinearRing(SetDimension(fact, ((LinearRing) p.ExteriorRing).CoordinateSequence,
                        dimension));
                LinearRing[] ir = null;
                if (p.NumInteriorRings > 0)
                {
                    ir = new LinearRing[p.NumInteriorRings];
                    for (int i = 0; i < p.NumInteriorRings; i++)
                        ir[i] =
                            p0.Factory.CreateLinearRing(SetDimension(fact,
                                ((LinearRing) p.GetInteriorRingN(i)).CoordinateSequence,
                                dimension));
                }
                return p.Factory.CreatePolygon(er, ir);
            }
            NetTopologySuite.Utilities.Assert.ShouldNeverReachHere();
            return null;
        }

        private static CoordinateSequence SetDimension(CoordinateSequenceFactory fact, CoordinateSequence seq,
            int dimension)
        {
            if (seq.Dimension == dimension)
                return seq;

            var res = fact.Create(seq.Count, dimension);
            dimension = Math.Min(dimension, seq.Dimension);
            for (int i = 0; i < seq.Count; i++)
            {
                for (int j = 0; j < dimension; j++)
                    res.SetOrdinate(i, j, seq.GetOrdinate(i, j));
            }
            return res;
        }

        private void RunWKBTest(Geometry g, int dimension, bool toHex)
        {
            if (dimension > 2)
            {
                SetOrdinate2(g);
            }

            RunWKBTest(g, dimension, ByteOrder.LittleEndian, toHex);
            RunWKBTest(g, dimension, ByteOrder.BigEndian, toHex);
        }

        private void RunWKBTest(Geometry g, int dimension, ByteOrder byteOrder, bool toHex)
        {
            RunGeometry((Geometry)g, dimension, byteOrder, toHex, 100);
            RunGeometry((Geometry)g, dimension, byteOrder, toHex, 0);
            RunGeometry((Geometry)g, dimension, byteOrder, toHex, 101010);
            RunGeometry((Geometry)g, dimension, byteOrder, toHex, -1);
        }

        private static void SetOrdinate2(Geometry g)
        {
            g.Apply(new AverageOrdinate2Filter());
        }

        //static Comparator comp2D = new Coordinate.DimensionalComparator();
        //static Comparator comp3D = new Coordinate.DimensionalComparator(3);

        static readonly CoordinateSequenceComparator Comp2 = new CoordinateSequenceComparator(2);
        static readonly CoordinateSequenceComparator Comp3 = new CoordinateSequenceComparator(3);
        readonly WKBReader _wkbReader = new WKBReader();

        void RunGeometry(Geometry g, int dimension, ByteOrder byteOrder, bool toHex, int srid)
        {
            bool includeSRID = false;
            if (srid >= 0)
            {
                includeSRID = true;
                g.SRID = srid;
            }

            var wkbWriter = new WKBWriter(byteOrder, includeSRID, dimension==2 ? false : true);
            byte[] wkb = wkbWriter.Write(g);
            string wkbHex = null;
            if (toHex)
                wkbHex = WKBWriter.ToHex(wkb);

            if (toHex)
                wkb = WKBReader.HexToBytes(wkbHex);
            var g2 = (Geometry)_wkbReader.Read(wkb);

            var comp = (dimension == 2) ? Comp2 : Comp3;
            bool isEqual = (g.CompareTo(g2, comp) == 0);
            Assert.IsTrue(isEqual);

            if (includeSRID)
            {
                bool isSRIDEqual = g.SRID == g2.SRID;
                Assert.IsTrue(isSRIDEqual);
            }
        }
    }

    class AverageOrdinate2Filter : ICoordinateFilter
    {
        public void Filter(Coordinate coord)
        {
            coord[2] = (coord.X + coord.Y) / 2;
        }
    }

}
