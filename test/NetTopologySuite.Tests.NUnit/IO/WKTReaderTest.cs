using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    [TestFixture]
    public class WKTReaderTest : GeometryTestCase
    {
        // WKT readers used throughout this test
        private readonly WKTReader readerXY;
        private readonly WKTReader readerXYOld;
        private readonly WKTReader readerXYZ;
        private readonly WKTReader readerXYM;
        private readonly WKTReader readerXYZM;

        public WKTReaderTest()
        {
            this.readerXY = GetWKTReader(Ordinates.XY, 1);
            this.readerXY.IsOldNtsCoordinateSyntaxAllowed = false;

            this.readerXYOld = GetWKTReader(Ordinates.XY, 1);
            this.readerXYOld.IsOldNtsCoordinateSyntaxAllowed = true;

            this.readerXYZ = GetWKTReader(Ordinates.XYZ, 1);

            this.readerXYM = GetWKTReader(Ordinates.XYM, 1);

            this.readerXYZM = GetWKTReader(Ordinates.XYZM, 1);
        }

        [Test]
        public void TestReadSpecialValues()
        {
            Geometry geom = null;
            Assert.That(() => geom = readerXYZ.Read("POINT Z(NaN -Inf Inf)"), Throws.Nothing);
            Assert.That(geom, Is.Not.Null);
        }

        [Test]
        public void TestReadPoint()
        {
            // arrange
            double[] coordinates = { 10, 10 };
            var seqPt2D = CreateSequence(Ordinates.XY, coordinates);
            var seqPt2DE = CreateSequence(Ordinates.XY, Array.Empty<double>());
            var seqPt3D = CreateSequence(Ordinates.XYZ, coordinates);
            var seqPt2DM = CreateSequence(Ordinates.XYM, coordinates);
            var seqPt3DM = CreateSequence(Ordinates.XYZM, coordinates);

            // act
            var pt2D = (Point)this.readerXY.Read("POINT (10 10)");
            var pt2DE = (Point)this.readerXY.Read("Point EMPTY");
            var pt3D = (Point)this.readerXYZ.Read("POINT Z(10 10 10)");
            var pt2DM = (Point)this.readerXYM.Read("POINT M(10 10 11)");
            var pt3DM = (Point)this.readerXYZM.Read("POINT ZM(10 10 10 11)");

            // assert
            Assert.That(IsEqual(seqPt2D, pt2D.CoordinateSequence));
            Assert.That(IsEqual(seqPt2DE, pt2DE.CoordinateSequence));
            Assert.That(IsEqual(seqPt3D, pt3D.CoordinateSequence));
            Assert.That(IsEqual(seqPt2DM, pt2DM.CoordinateSequence));
            Assert.That(IsEqual(seqPt3DM, pt3DM.CoordinateSequence));
        }

        [Test]
        public void TestReadLineString()
        {
            // arrange
            double[] coordinates = { 10, 10, 20, 20, 30, 40 };
            var seqLs2D = CreateSequence(Ordinates.XY, coordinates);
            var seqLs2DE = CreateSequence(Ordinates.XY, Array.Empty<double>());
            var seqLs3D = CreateSequence(Ordinates.XYZ, coordinates);
            var seqLs2DM = CreateSequence(Ordinates.XYM, coordinates);
            var seqLs3DM = CreateSequence(Ordinates.XYZM, coordinates);

            // act
            var ls2D = (LineString)this.readerXY.Read("LINESTRING (10 10, 20 20, 30 40)");
            var ls2DE = (LineString)this.readerXY.Read("LineString EMPTY");
            var ls3D = (LineString)this.readerXYZ.Read("LINESTRING Z(10 10 10, 20 20 10, 30 40 10)");
            var ls2DM = (LineString)this.readerXYM.Read("LINESTRING M(10 10 11, 20 20 11, 30 40 11)");
            var ls3DM = (LineString)this.readerXYZM.Read("LINESTRING ZM(10 10 10 11, 20 20 10 11, 30 40 10 11)");

            // assert
            Assert.That(IsEqual(seqLs2D, ls2D.CoordinateSequence));
            Assert.That(IsEqual(seqLs2DE, ls2DE.CoordinateSequence));
            Assert.That(IsEqual(seqLs3D, ls3D.CoordinateSequence));
            Assert.That(IsEqual(seqLs2DM, ls2DM.CoordinateSequence));
            Assert.That(IsEqual(seqLs3DM, ls3DM.CoordinateSequence));
        }

        [Test]
        public void TestReadLinearRing()
        {
            // arrange
            double[] coordinates = { 10, 10, 20, 20, 30, 40, 10, 10 };
            var seqLs2D = CreateSequence(Ordinates.XY, coordinates);
            var seqLs2DE = CreateSequence(Ordinates.XY, Array.Empty<double>());
            var seqLs3D = CreateSequence(Ordinates.XYZ, coordinates);
            var seqLs2DM = CreateSequence(Ordinates.XYM, coordinates);
            var seqLs3DM = CreateSequence(Ordinates.XYZM, coordinates);

            // act
            var ls2D = (LineString)this.readerXY.Read("LINEARRING (10 10, 20 20, 30 40, 10 10)");
            var ls2DE = (LineString)this.readerXY.Read("LinearRing EMPTY");
            var ls3D = (LineString)this.readerXYZ.Read("LINEARRING Z(10 10 10, 20 20 10, 30 40 10, 10 10 10)");
            var ls2DM = (LineString)this.readerXYM.Read("LINEARRING M(10 10 11, 20 20 11, 30 40 11, 10 10 11)");
            var ls3DM = (LineString)this.readerXYZM.Read(
                "LINEARRING ZM(10 10 10 11, 20 20 10 11, 30 40 10 11, 10 10 10 11)");

            // assert
            Assert.That(IsEqual(seqLs2D, ls2D.CoordinateSequence));
            Assert.That(IsEqual(seqLs2DE, ls2DE.CoordinateSequence));
            Assert.That(IsEqual(seqLs3D, ls3D.CoordinateSequence));
            Assert.That(IsEqual(seqLs2DM, ls2DM.CoordinateSequence));
            Assert.That(IsEqual(seqLs3DM, ls3DM.CoordinateSequence));
        }

        [Test]
        public void TestReadLinearRingNotClosed()
        {
            Assert.That(() => this.readerXY.Read("LINEARRING (10 10, 20 20, 30 40, 10 99)"),
                Throws.ArgumentException.With.Message.Contains("must form a closed linestring"));
        }

        [Test]
        public void TestReadPolygon()
        {
            // arrange
            double[] shell = { 10, 10, 10, 20, 20, 20, 20, 15, 10, 10 };
            double[] ring1 = { 11, 11, 12, 11, 12, 12, 12, 11, 11, 11 };
            double[] ring2 = { 11, 19, 11, 18, 12, 18, 12, 19, 11, 19 };

            CoordinateSequence[] csPoly2D =
            {
                CreateSequence(Ordinates.XY, shell),
                CreateSequence(Ordinates.XY, ring1),
                CreateSequence(Ordinates.XY, ring2),
            };
            var csPoly2DE = CreateSequence(Ordinates.XY, Array.Empty<double>());
            CoordinateSequence[] csPoly3D =
            {
                CreateSequence(Ordinates.XYZ, shell),
                CreateSequence(Ordinates.XYZ, ring1),
                CreateSequence(Ordinates.XYZ, ring2),
            };
            CoordinateSequence[] csPoly2DM =
            {
                CreateSequence(Ordinates.XYM, shell),
                CreateSequence(Ordinates.XYM, ring1),
                CreateSequence(Ordinates.XYM, ring2),
            };
            CoordinateSequence[] csPoly3DM =
            {
                CreateSequence(Ordinates.XYZM, shell),
                CreateSequence(Ordinates.XYZM, ring1),
                CreateSequence(Ordinates.XYZM, ring2),
            };

            // act
            var rdr = this.readerXY;
            Polygon[] poly2D =
            {
                (Polygon) rdr.Read("POLYGON ((10 10, 10 20, 20 20, 20 15, 10 10))"),
                (Polygon) rdr.Read(
                    "POLYGON ((10 10, 10 20, 20 20, 20 15, 10 10), (11 11, 12 11, 12 12, 12 11, 11 11))"),
                (Polygon) rdr.Read(
                    "POLYGON ((10 10, 10 20, 20 20, 20 15, 10 10), (11 11, 12 11, 12 12, 12 11, 11 11), (11 19, 11 18, 12 18, 12 19, 11 19))"),
            };
            var poly2DE = (Polygon)rdr.Read("Polygon EMPTY");
            rdr = this.readerXYZ;
            Polygon[] poly3D =
            {
                (Polygon) rdr.Read("POLYGON Z((10 10 10, 10 20 10, 20 20 10, 20 15 10, 10 10 10))"),
                (Polygon) rdr.Read(
                    "POLYGON Z((10 10 10, 10 20 10, 20 20 10, 20 15 10, 10 10 10), (11 11 10, 12 11 10, 12 12 10, 12 11 10, 11 11 10))"),
                (Polygon) rdr.Read(
                    "POLYGON Z((10 10 10, 10 20 10, 20 20 10, 20 15 10, 10 10 10), (11 11 10, 12 11 10, 12 12 10, 12 11 10, 11 11 10), (11 19 10, 11 18 10, 12 18 10, 12 19 10, 11 19 10))"),
            };
            rdr = this.readerXYM;
            Polygon[] poly2DM =
            {
                (Polygon) rdr.Read("POLYGON M((10 10 11, 10 20 11, 20 20 11, 20 15 11, 10 10 11))"),
                (Polygon) rdr.Read(
                    "POLYGON M((10 10 11, 10 20 11, 20 20 11, 20 15 11, 10 10 11), (11 11 11, 12 11 11, 12 12 11, 12 11 11, 11 11 11))"),
                (Polygon) rdr.Read(
                    "POLYGON M((10 10 11, 10 20 11, 20 20 11, 20 15 11, 10 10 11), (11 11 11, 12 11 11, 12 12 11, 12 11 11, 11 11 11), (11 19 11, 11 18 11, 12 18 11, 12 19 11, 11 19 11))"),
            };
            rdr = this.readerXYZM;
            Polygon[] poly3DM =
            {
                (Polygon) rdr.Read("POLYGON ZM((10 10 10 11, 10 20 10 11, 20 20 10 11, 20 15 10 11, 10 10 10 11))"),
                (Polygon) rdr.Read(
                    "POLYGON ZM((10 10 10 11, 10 20 10 11, 20 20 10 11, 20 15 10 11, 10 10 10 11), (11 11 10 11, 12 11 10 11, 12 12 10 11, 12 11 10 11, 11 11 10 11))"),
                (Polygon) rdr.Read(
                    "POLYGON ZM((10 10 10 11, 10 20 10 11, 20 20 10 11, 20 15 10 11, 10 10 10 11), (11 11 10 11, 12 11 10 11, 12 12 10 11, 12 11 10 11, 11 11 10 11), (11 19 10 11, 11 18 10 11, 12 18 10 11, 12 19 10 11, 11 19 10 11))"),
            };

            // assert
            Assert.That(IsEqual(csPoly2D[0], poly2D[2].ExteriorRing.CoordinateSequence));
            Assert.That(IsEqual(csPoly2D[1], poly2D[2].GetInteriorRingN(0).CoordinateSequence));
            Assert.That(IsEqual(csPoly2D[2], poly2D[2].GetInteriorRingN(1).CoordinateSequence));
            Assert.That(IsEqualDim(csPoly2DE, poly2DE.ExteriorRing.CoordinateSequence, 2));

            Assert.That(IsEqual(csPoly3D[0], poly3D[2].ExteriorRing.CoordinateSequence));
            Assert.That(IsEqual(csPoly3D[1], poly3D[2].GetInteriorRingN(0).CoordinateSequence));
            Assert.That(IsEqual(csPoly3D[2], poly3D[2].GetInteriorRingN(1).CoordinateSequence));
            Assert.That(IsEqual(csPoly2DM[0], poly2DM[2].ExteriorRing.CoordinateSequence));
            Assert.That(IsEqual(csPoly2DM[1], poly2DM[2].GetInteriorRingN(0).CoordinateSequence));
            Assert.That(IsEqual(csPoly2DM[2], poly2DM[2].GetInteriorRingN(1).CoordinateSequence));
            Assert.That(IsEqual(csPoly3DM[0], poly3DM[2].ExteriorRing.CoordinateSequence));
            Assert.That(IsEqual(csPoly3DM[1], poly3DM[2].GetInteriorRingN(0).CoordinateSequence));
            Assert.That(IsEqual(csPoly3DM[2], poly3DM[2].GetInteriorRingN(1).CoordinateSequence));
        }

        private static readonly double[][] mpCoords = { new double[] { 10, 10 }, new double[] { 20, 20 } };

        [Test]
        public void TestMultiPointXY()
        {
            var mp = (MultiPoint)readerXY.Read("MULTIPOINT ((10 10), (20 20))");
            var cs = CreateSequences(Ordinates.XY, mpCoords);
            CheckCS(cs[0], mp.GetGeometryN(0));
            CheckCS(cs[1], mp.GetGeometryN(1));
        }

        [Test]
        public void TestMultiPointXYOldSyntax()
        {
            var mp = (MultiPoint)readerXY.Read("MULTIPOINT (10 10, 20 20)");
            var cs = CreateSequences(Ordinates.XY, mpCoords);
            CheckCS(cs[0], mp.GetGeometryN(0));
            CheckCS(cs[1], mp.GetGeometryN(1));
        }

        [Test]
        public void TestMultiPointXY_Empty()
        {
            var mp = (MultiPoint)readerXY.Read("MULTIPOINT EMPTY");
            CheckEmpty(mp);
        }

        [Test]
        public void TestMultiPointXY_WithEmpty()
        {
            var mp = (MultiPoint)readerXY.Read("MULTIPOINT ((10 10), EMPTY, (20 20))");
            var cs = CreateSequences(Ordinates.XY, mpCoords);
            CheckCS(cs[0], mp.GetGeometryN(0));
            CheckEmpty(mp.GetGeometryN(1));
            CheckCS(cs[1], mp.GetGeometryN(2));
        }

        [Test]
        public void TestMultiPointXYM()
        {
            var mp = (MultiPoint)readerXYM.Read("MULTIPOINT M((10 10 11), (20 20 11))");
            var cs = CreateSequences(Ordinates.XYM, mpCoords);
            CheckCS(cs[0], mp.GetGeometryN(0));
            CheckCS(cs[1], mp.GetGeometryN(1));
        }

        [Test]
        public void TestMultiPointXYZ()
        {
            var mp = (MultiPoint)readerXYZ.Read("MULTIPOINT Z((10 10 10), (20 20 10))");
            var cs = CreateSequences(Ordinates.XYZ, mpCoords);
            CheckCS(cs[0], mp.GetGeometryN(0));
            CheckCS(cs[1], mp.GetGeometryN(1));
        }

        [Test]
        public void TestMultiPointXYZM()
        {
            var mp = (MultiPoint)readerXYZM.Read("MULTIPOINT ZM((10 10 10 11), (20 20 10 11))");
            var cs = CreateSequences(Ordinates.XYZM, mpCoords);
            CheckCS(cs[0], mp.GetGeometryN(0));
            CheckCS(cs[1], mp.GetGeometryN(1));
        }

        double[][] mLcoords = new double[][]
        {
            new double[] {10, 10, 20, 20},
            new double[] {15, 15, 30, 15}
        };

        [Test]
        public void TestMultiLineStringXY()
        {
            var mp = (MultiLineString)readerXY.Read("MULTILINESTRING ((10 10, 20 20), (15 15, 30 15))");
            var cs = CreateSequences(Ordinates.XY, mLcoords);
            CheckCS(cs[0], mp.GetGeometryN(0));
            CheckCS(cs[1], mp.GetGeometryN(1));
        }

        [Test]
        public void TestMultiLineStringXY_Empty()
        {
            var mp = (MultiLineString)readerXY.Read("MULTILINESTRING EMPTY");
            CheckEmpty(mp);
        }

        [Test]
        public void TestMultiLineStringXY_WithEmpty()
        {
            var mp = (MultiLineString)readerXY.Read("MULTILINESTRING ((10 10, 20 20), EMPTY, (15 15, 30 15))");
            var cs = CreateSequences(Ordinates.XY, mLcoords);
            CheckCS(cs[0], mp.GetGeometryN(0));
            CheckEmpty(mp.GetGeometryN(1));
            CheckCS(cs[1], mp.GetGeometryN(2));
        }

        [Test]
        public void TestMultiLineStringXYM()
        {
            var mp = (MultiLineString)readerXYM.Read("MULTILINESTRING M((10 10 11, 20 20 11), (15 15 11, 30 15 11))");
            var cs = CreateSequences(Ordinates.XYM, mLcoords);
            CheckCS(cs[0], mp.GetGeometryN(0));
            CheckCS(cs[1], mp.GetGeometryN(1));
        }

        [Test]
        public void TestMultiLineStringXYZ()
        {
            var mp = (MultiLineString)readerXYZ.Read("MULTILINESTRING Z((10 10 10, 20 20 10), (15 15 10, 30 15 10))");
            var cs = CreateSequences(Ordinates.XYZ, mLcoords);
            CheckCS(cs[0], mp.GetGeometryN(0));
            CheckCS(cs[1], mp.GetGeometryN(1));
        }

        [Test]
        public void TestMultiLineStringYZM()
        {
            var mp = (MultiLineString)readerXYZM.Read(
                "MULTILINESTRING ZM((10 10 10 11, 20 20 10 11), (15 15 10 11, 30 15 10 11))");
            var cs = CreateSequences(Ordinates.XYZM, mLcoords);
            CheckCS(cs[0], mp.GetGeometryN(0));
            CheckCS(cs[1], mp.GetGeometryN(1));
        }

        double[][] mAcoords =
        {
            new double[] {10, 10, 10, 20, 20, 20, 20, 15, 10, 10},
            new double[] {11, 11, 12, 11, 12, 12, 12, 11, 11, 11},
            new double[] {60, 60, 70, 70, 80, 60, 60, 60}
        };

        [Test]
        public void TestMultiPolygonXY()
        {
            var mp = (MultiPolygon)readerXY.Read(
                "MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10), (11 11, 12 11, 12 12, 12 11, 11 11)), ((60 60, 70 70, 80 60, 60 60)))");
            var cs = CreateSequences(Ordinates.XY, mAcoords);
            CheckCS(cs[0], ((Polygon)mp.GetGeometryN(0)).ExteriorRing);
            CheckCS(cs[1], ((Polygon)mp.GetGeometryN(0)).GetInteriorRingN(0));
            CheckCS(cs[2], ((Polygon)mp.GetGeometryN(1)).ExteriorRing);
        }

        [Test]
        public void TestMultiPolygonXY_Empty()
        {
            var mp = (MultiPolygon)readerXY.Read("MULTIPOLYGON EMPTY");
            CheckEmpty(mp);
        }

        [Test]
        public void TestMultiPolygonXY_WithEmpty()
        {
            var mp = (MultiPolygon)readerXY.Read(
                "MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10), (11 11, 12 11, 12 12, 12 11, 11 11)), EMPTY, ((60 60, 70 70, 80 60, 60 60)))");
            var cs = CreateSequences(Ordinates.XY, mAcoords);
            CheckCS(cs[0], ((Polygon)mp.GetGeometryN(0)).ExteriorRing);
            CheckCS(cs[1], ((Polygon)mp.GetGeometryN(0)).GetInteriorRingN(0));
            CheckEmpty(((Polygon)mp.GetGeometryN(1)));
            CheckCS(cs[2], ((Polygon)mp.GetGeometryN(2)).ExteriorRing);
        }

        [Test]
        public void TestMultiPolygonXYM()
        {
            var mp = (MultiPolygon)readerXYM.Read(
                "MULTIPOLYGON M(((10 10 11, 10 20 11, 20 20 11, 20 15 11, 10 10 11), (11 11 11, 12 11 11, 12 12 11, 12 11 11, 11 11 11)), ((60 60 11, 70 70 11, 80 60 11, 60 60 11)))");
            var cs = CreateSequences(Ordinates.XYM, mAcoords);
            CheckCS(cs[0], ((Polygon)mp.GetGeometryN(0)).ExteriorRing);
            CheckCS(cs[1], ((Polygon)mp.GetGeometryN(0)).GetInteriorRingN(0));
            CheckCS(cs[2], ((Polygon)mp.GetGeometryN(1)).ExteriorRing);
        }

        [Test]
        public void TestMultiPolygonXYZ()
        {
            var mp = (MultiPolygon)readerXYZ.Read(
                "MULTIPOLYGON Z(((10 10 10, 10 20 10, 20 20 10, 20 15 10, 10 10 10), (11 11 10, 12 11 10, 12 12 10, 12 11 10, 11 11 10)), ((60 60 10, 70 70 10, 80 60 10, 60 60 10)))");
            var cs = CreateSequences(Ordinates.XYZ, mAcoords);
            CheckCS(cs[0], ((Polygon)mp.GetGeometryN(0)).ExteriorRing);
            CheckCS(cs[1], ((Polygon)mp.GetGeometryN(0)).GetInteriorRingN(0));
            CheckCS(cs[2], ((Polygon)mp.GetGeometryN(1)).ExteriorRing);
        }

        [Test]
        public void TestMultiPolygonYZM()
        {
            var mp = (MultiPolygon)readerXYZM.Read(
                "MULTIPOLYGON ZM(((10 10 10 11, 10 20 10 11, 20 20 10 11, 20 15 10 11, 10 10 10 11), (11 11 10 11, 12 11 10 11, 12 12 10 11, 12 11 10 11, 11 11 10 11)), ((60 60 10 11, 70 70 10 11, 80 60 10 11, 60 60 10 11)))");
            var cs = CreateSequences(Ordinates.XYZM, mAcoords);
            CheckCS(cs[0], ((Polygon)mp.GetGeometryN(0)).ExteriorRing);
            CheckCS(cs[1], ((Polygon)mp.GetGeometryN(0)).GetInteriorRingN(0));
            CheckCS(cs[2], ((Polygon)mp.GetGeometryN(1)).ExteriorRing);
        }


        [Test]
        public void TestReadGeometryCollection()
        {
            // arrange
            double[][] coordinates =
            {
                new double[] {10, 10},
                new double[] {30, 30},
                new double[] {15, 15, 20, 20},
                Array.Empty<double>(),
                new double[] {10, 10, 20, 20, 30, 40, 10, 10},
            };

            CoordinateSequence[] css =
            {
                CreateSequence(Ordinates.XY, coordinates[0]),
                CreateSequence(Ordinates.XY, coordinates[1]),
                CreateSequence(Ordinates.XY, coordinates[2]),
                CreateSequence(Ordinates.XY, coordinates[3]),
                CreateSequence(Ordinates.XY, coordinates[4]),
            };

            // act
            var rdr = GetWKTReader(Ordinates.XY, 1);
            var gc0 = (GeometryCollection)rdr.Read(
                "GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))");
            var gc1 = (GeometryCollection)rdr.Read(
                "GEOMETRYCOLLECTION (POINT (10 10), LINEARRING EMPTY, LINESTRING (15 15, 20 20))");
            var gc2 = (GeometryCollection)rdr.Read(
                "GEOMETRYCOLLECTION (POINT (10 10), LINEARRING (10 10, 20 20, 30 40, 10 10), LINESTRING (15 15, 20 20))");
            var gc3 = (GeometryCollection)rdr.Read("GeometryCollection EMPTY");

            // assert
            Assert.That(IsEqual(css[0], ((Point)gc0.GetGeometryN(0)).CoordinateSequence));
            Assert.That(IsEqual(css[1], ((Point)gc0.GetGeometryN(1)).CoordinateSequence));
            Assert.That(IsEqual(css[2], ((LineString)gc0.GetGeometryN(2)).CoordinateSequence));
            Assert.That(IsEqual(css[0], ((Point)gc1.GetGeometryN(0)).CoordinateSequence));
            Assert.That(IsEqual(css[3], ((LinearRing)gc1.GetGeometryN(1)).CoordinateSequence));
            Assert.That(IsEqual(css[2], ((LineString)gc1.GetGeometryN(2)).CoordinateSequence));
            Assert.That(IsEqual(css[0], ((Point)gc2.GetGeometryN(0)).CoordinateSequence));
            Assert.That(IsEqual(css[4], ((LinearRing)gc2.GetGeometryN(1)).CoordinateSequence));
            Assert.That(IsEqual(css[2], ((LineString)gc2.GetGeometryN(2)).CoordinateSequence));
            Assert.That(gc3.IsEmpty);
        }

        [Test]
        public void TestEmptyLineDimOldSyntax()
        {
            var wktReader = new WKTReader();
            var geom = (LineString)wktReader.Read("LINESTRING EMPTY");
            int dim = geom.CoordinateSequence.Dimension;
            CheckCSDim(geom.CoordinateSequence, 3);
        }

        [Test]
        public void TestEmptyLineDim()
        {
            var wktReader = new WKTReader();
            wktReader.IsOldNtsCoordinateSyntaxAllowed = false;
            var geom = (LineString)wktReader.Read("LINESTRING EMPTY");
            CheckCSDim(geom.CoordinateSequence, 2);
        }

        [Test]
        public void TestEmptyPolygonDim()
        {
            var wktReader = new WKTReader();
            wktReader.IsOldNtsCoordinateSyntaxAllowed = false;
            var geom = (Polygon)wktReader.Read("POLYGON EMPTY");
            CheckCSDim(geom.ExteriorRing.CoordinateSequence, 2);
        }


        [Test]
        public void TestReadNaN()
        {
            // arrange
            var seq = CreateSequence(Ordinates.XYZ, new double[] { 10, 10 });
            seq.SetOrdinate(0, Ordinate.Z, double.NaN);
            var seq2 = CreateSequence(Ordinates.XY, new double[] { 10, 10 });

            // act
            var pt1 = (Point)this.readerXYOld.Read("POINT (10 10 NaN)");
            var pt2 = (Point)this.readerXYOld.Read("POINT (10 10 nan)");
            var pt3 = (Point)this.readerXYOld.Read("POINT (10 10 NAN)");
            var pt4 = (Point)this.readerXYOld.Read("POINT (10 10)");

            // assert
            Assert.That(IsEqual(seq, pt1.CoordinateSequence));
            Assert.That(IsEqual(seq, pt2.CoordinateSequence));
            Assert.That(IsEqual(seq, pt3.CoordinateSequence));
            Assert.That(pt4.CoordinateSequence.Dimension, Is.EqualTo(2));
            Assert.That(IsEqual(seq2, pt4.CoordinateSequence));
        }

        [Test]
        public void TestIsOldNtsSyntaxAllowed()
        {
            // arrange
            var seqPt1 = CreateSequence(Ordinates.XY, new double[] { 10, 10 });
            var seqPt2 = CreateSequence(Ordinates.XYZ, new double[] { 10, 10 });
            seqPt2.SetZ(0, double.NaN);
            var seqLs = CreateSequence(Ordinates.XYZ, new double[] { 10, 10, 20, 10, 30, 15 });
            seqLs.SetZ(0, double.NaN);
            seqLs.SetZ(1, 5);
            seqLs.SetZ(2, double.NaN);

            // act
            var pt1 = (Point)this.readerXYOld.Read("POINT (10 10)");
            var pt2 = (Point)this.readerXYOld.Read("POINT (10 10 NaN)");
            var ls = (LineString)this.readerXYOld.Read("LINESTRING (10 10 NaN, 20 10 5, 30 15 NaN)");

            // assert
            Assert.That(IsEqual(seqPt1, pt1.CoordinateSequence));
            Assert.That(pt1.AsText(), Is.EqualTo("POINT (10 10)"));
            Assert.That(IsEqual(seqPt2, pt2.CoordinateSequence));
            // This is inconsistent!
            //Assert.That(pt2.AsText(), Is.EqualTo("POINT (10 10 NaN)"));
            Assert.That(IsEqual(seqLs, ls.CoordinateSequence));
            Assert.That(ls.AsText(), Is.EqualTo("LINESTRING Z(10 10 NaN, 20 10 5, 30 15 NaN)"));
        }

        [Test]
        public void TestReadLargeNumbers()
        {
            var precisionModel = new PrecisionModel(1E9);
            var gs = new NtsGeometryServices(precisionModel, 0);
            var geometryFactory = gs.CreateGeometryFactory();
            var reader = new WKTReader(gs);
            var point1 = ((Point)reader.Read("POINT (123456789.01234567890 10)")).CoordinateSequence;
            var point2 = geometryFactory.CreatePoint(new Coordinate(123456789.01234567890, 10)).CoordinateSequence;
            Assert.That(point1.GetOrdinate(0, Ordinate.X), Is.EqualTo(point2.GetOrdinate(0, Ordinate.X)).Within(1E-7));
            Assert.That(point1.GetOrdinate(0, Ordinate.Y), Is.EqualTo(point2.GetOrdinate(0, Ordinate.Y)).Within(1E-7));
        }

        [Test]
        public void TestReadSRID()
        {
            // arrange
            int srid = new Random().Next();

            // act
            var pt = readerXY.Read($"SRID={srid};POINT EMPTY");

            // assert
            Assert.That(pt.SRID, Is.EqualTo(srid));
        }

        [TestCase("tr")]
        public void TestLocale(string cultureName)
        {
            var culture = CultureInfo.CreateSpecificCulture("tr");
            var current = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = culture;
                var point = (Point)readerXY.Read("point (10 20)");
                Assert.That(point.X, Is.EqualTo(10.0).Within(1E-7));
                Assert.That(point.Y, Is.EqualTo(20.0).Within(1E-7));
            }
            finally
            {
                CultureInfo.CurrentCulture = current;
            }
        }

        private void CheckCS(CoordinateSequence cs, Geometry geom)
        {
            Assert.That(IsEqual(cs, ExtractCS(geom)), Is.True);
        }

        private CoordinateSequence ExtractCS(Geometry geom)
        {
            if (geom is Point p) return p.CoordinateSequence;
            if (geom is LineString l) return l.CoordinateSequence;
            throw new ArgumentException("Can't extract coordinate sequence from geometry of type " + geom.GeometryType);
        }

        private void CheckEmpty(Geometry geom)
        {
            Assert.That(geom.IsEmpty, Is.True);
            if (geom is GeometryCollection gc)
            {
                Assert.That(geom.NumGeometries == 0);
            }
        }
        private void CheckCSDim(CoordinateSequence cs, int expectedCoordDim)
        {
            int dim = cs.Dimension;
            Assert.That(dim, Is.EqualTo(expectedCoordDim));
        }

        private static CoordinateSequence[] CreateSequences(Ordinates ordinateFlags, double[][] xyarray)
        {
            var csarray = new CoordinateSequence[xyarray.Length];
            for (int i = 0; i < xyarray.Length; i++)
            {
                csarray[i] = CreateSequence(ordinateFlags, xyarray[i]);
            }

            return csarray;
        }


        private static CoordinateSequence CreateSequence(Ordinates ordinateFlags, double[] xy)
        {
            // get the number of dimension to verify size of provided ordinate values array
            int dimension = RequiredDimension(ordinateFlags);

            // inject additional values
            double[] ordinateValues = InjectZM(ordinateFlags, xy);

            // get the required size of the sequence
            int size = Math.DivRem(ordinateValues.Length, dimension, out int remainder);
            if (remainder != 0)
            {
                throw new ArgumentException("ordinateFlags and number of provided ordinate values don't match");
            }

            // create a sequence capable of storing all ordinate values.
            var res = GetCSFactory(ordinateFlags)
                .Create(size, RequiredDimension(ordinateFlags), OrdinatesUtility.OrdinatesToMeasures(ordinateFlags));

            // fill in values
            int k = 0;
            for (int i = 0; i < ordinateValues.Length; i += dimension)
            {
                for (int j = 0; j < dimension; j++)
                {
                    res.SetOrdinate(k, j, ordinateValues[i + j]);
                }

                k++;
            }

            return res;
        }

        private static int RequiredDimension(Ordinates ordinateFlags)
        {
            switch (ordinateFlags)
            {
                case Ordinates.XY:
                    return 2;

                case Ordinates.XYZ:
                case Ordinates.XYM:
                    return 3;

                default:
                    return 4;
            }
        }

        private static double[] InjectZM(Ordinates ordinateFlags, double[] xy)
        {
            int size = xy.Length / 2;
            int dimension = RequiredDimension(ordinateFlags);
            double[] res = new double[size * dimension];
            int k = 0;
            for (int i = 0; i < xy.Length; i += 2)
            {
                res[k++] = xy[i];
                res[k++] = xy[i + 1];
                if (ordinateFlags.HasFlag(Ordinates.Z))
                {
                    res[k++] = 10;
                }

                if (ordinateFlags.HasFlag(Ordinates.M))
                {
                    res[k++] = 11;
                }
            }

            return res;
        }
    }
}
