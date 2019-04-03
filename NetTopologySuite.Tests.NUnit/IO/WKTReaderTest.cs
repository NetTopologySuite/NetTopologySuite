using System;

using GeoAPI.Geometries;

using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    [TestFixture]
    public class WKTReaderTest : GeometryTestCase
    {
        // WKT readers used throughout this test
        private readonly WKTReader reader2D;
        private readonly WKTReader reader2DOld;
        private readonly WKTReader reader3D;
        private readonly WKTReader reader2DM;
        private readonly WKTReader reader3DM;

        public WKTReaderTest()
        {
            this.reader2D = GetWKTReader(Ordinates.XY, 1);
            this.reader2D.IsOldNtsCoordinateSyntaxAllowed = false;

            this.reader2DOld = GetWKTReader(Ordinates.XY, 1);
            this.reader2DOld.IsOldNtsCoordinateSyntaxAllowed = true;

            this.reader3D = GetWKTReader(Ordinates.XYZ, 1);

            this.reader2DM = GetWKTReader(Ordinates.XYM, 1);

            this.reader3DM = GetWKTReader(Ordinates.XYZM, 1);
        }

        [Test]
        public void TestReadNaN()
        {
            // arrange
            var seq = CreateSequence(Ordinates.XYZ, new double[] { 10, 10 });
            seq.SetOrdinate(0, Ordinate.Z, double.NaN);

            // act
            var pt1 = (IPoint)this.reader2DOld.Read("POINT (10 10 NaN)");
            var pt2 = (IPoint)this.reader2DOld.Read("POINT (10 10 nan)");
            var pt3 = (IPoint)this.reader2DOld.Read("POINT (10 10 NAN)");

            // assert
            Assert.That(CheckEqual(seq, pt1.CoordinateSequence));
            Assert.That(CheckEqual(seq, pt2.CoordinateSequence));
            Assert.That(CheckEqual(seq, pt3.CoordinateSequence));
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
            var pt2D = (IPoint)this.reader2D.Read("POINT (10 10)");
            var pt2DE = (IPoint)this.reader2D.Read("Point EMPTY");
            var pt3D = (IPoint)this.reader3D.Read("POINT Z(10 10 10)");
            var pt2DM = (IPoint)this.reader2DM.Read("POINT M(10 10 11)");
            var pt3DM = (IPoint)this.reader3DM.Read("POINT ZM(10 10 10 11)");

            // assert
            Assert.That(CheckEqual(seqPt2D, pt2D.CoordinateSequence));
            Assert.That(CheckEqual(seqPt2DE, pt2DE.CoordinateSequence));
            Assert.That(CheckEqual(seqPt3D, pt3D.CoordinateSequence));
            Assert.That(CheckEqual(seqPt2DM, pt2DM.CoordinateSequence));
            Assert.That(CheckEqual(seqPt3DM, pt3DM.CoordinateSequence));
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
            var ls2D = (ILineString)this.reader2D.Read("LINESTRING (10 10, 20 20, 30 40)");
            var ls2DE = (ILineString)this.reader2D.Read("LineString EMPTY");
            var ls3D = (ILineString)this.reader3D.Read("LINESTRING Z(10 10 10, 20 20 10, 30 40 10)");
            var ls2DM = (ILineString)this.reader2DM.Read("LINESTRING M(10 10 11, 20 20 11, 30 40 11)");
            var ls3DM = (ILineString)this.reader3DM.Read("LINESTRING ZM(10 10 10 11, 20 20 10 11, 30 40 10 11)");

            // assert
            Assert.That(CheckEqual(seqLs2D, ls2D.CoordinateSequence));
            Assert.That(CheckEqual(seqLs2DE, ls2DE.CoordinateSequence));
            Assert.That(CheckEqual(seqLs3D, ls3D.CoordinateSequence));
            Assert.That(CheckEqual(seqLs2DM, ls2DM.CoordinateSequence));
            Assert.That(CheckEqual(seqLs3DM, ls3DM.CoordinateSequence));
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
            var ls2D = (ILineString)this.reader2D.Read("LINEARRING (10 10, 20 20, 30 40, 10 10)");
            var ls2DE = (ILineString)this.reader2D.Read("LinearRing EMPTY");
            var ls3D = (ILineString)this.reader3D.Read("LINEARRING Z(10 10 10, 20 20 10, 30 40 10, 10 10 10)");
            var ls2DM = (ILineString)this.reader2DM.Read("LINEARRING M(10 10 11, 20 20 11, 30 40 11, 10 10 11)");
            var ls3DM = (ILineString)this.reader3DM.Read("LINEARRING ZM(10 10 10 11, 20 20 10 11, 30 40 10 11, 10 10 10 11)");

            // assert
            Assert.That(CheckEqual(seqLs2D, ls2D.CoordinateSequence));
            Assert.That(CheckEqual(seqLs2DE, ls2DE.CoordinateSequence));
            Assert.That(CheckEqual(seqLs3D, ls3D.CoordinateSequence));
            Assert.That(CheckEqual(seqLs2DM, ls2DM.CoordinateSequence));
            Assert.That(CheckEqual(seqLs3DM, ls3DM.CoordinateSequence));

            Assert.That(() => this.reader2D.Read("LINEARRING (10 10, 20 20, 30 40, 10 99)"), Throws.ArgumentException.With.Message.Contains("must form a closed linestring"));
        }

        [Test]
        public void TestReadPolygon()
        {
            // arrange
            double[] shell = { 10, 10, 10, 20, 20, 20, 20, 15, 10, 10 };
            double[] ring1 = { 11, 11, 12, 11, 12, 12, 12, 11, 11, 11 };
            double[] ring2 = { 11, 19, 11, 18, 12, 18, 12, 19, 11, 19 };

            ICoordinateSequence[] csPoly2D =
            {
                CreateSequence(Ordinates.XY, shell),
                CreateSequence(Ordinates.XY, ring1),
                CreateSequence(Ordinates.XY, ring2),
            };
            var csPoly2DE = CreateSequence(Ordinates.XY, Array.Empty<double>());
            ICoordinateSequence[] csPoly3D =
            {
                CreateSequence(Ordinates.XYZ, shell),
                CreateSequence(Ordinates.XYZ, ring1),
                CreateSequence(Ordinates.XYZ, ring2),
            };
            ICoordinateSequence[] csPoly2DM =
            {
                CreateSequence(Ordinates.XYM, shell),
                CreateSequence(Ordinates.XYM, ring1),
                CreateSequence(Ordinates.XYM, ring2),
            };
            ICoordinateSequence[] csPoly3DM =
            {
                CreateSequence(Ordinates.XYZM, shell),
                CreateSequence(Ordinates.XYZM, ring1),
                CreateSequence(Ordinates.XYZM, ring2),
            };

            // act
            var rdr = this.reader2D;
            IPolygon[] poly2D =
            {
                (IPolygon)rdr.Read("POLYGON ((10 10, 10 20, 20 20, 20 15, 10 10))"),
                (IPolygon)rdr.Read("POLYGON ((10 10, 10 20, 20 20, 20 15, 10 10), (11 11, 12 11, 12 12, 12 11, 11 11))"),
                (IPolygon)rdr.Read("POLYGON ((10 10, 10 20, 20 20, 20 15, 10 10), (11 11, 12 11, 12 12, 12 11, 11 11), (11 19, 11 18, 12 18, 12 19, 11 19))"),
            };
            var poly2DE = (IPolygon)rdr.Read("Polygon EMPTY");
            rdr = this.reader3D;
            IPolygon[] poly3D =
            {
                (IPolygon)rdr.Read("POLYGON Z((10 10 10, 10 20 10, 20 20 10, 20 15 10, 10 10 10))"),
                (IPolygon)rdr.Read("POLYGON Z((10 10 10, 10 20 10, 20 20 10, 20 15 10, 10 10 10), (11 11 10, 12 11 10, 12 12 10, 12 11 10, 11 11 10))"),
                (IPolygon)rdr.Read("POLYGON Z((10 10 10, 10 20 10, 20 20 10, 20 15 10, 10 10 10), (11 11 10, 12 11 10, 12 12 10, 12 11 10, 11 11 10), (11 19 10, 11 18 10, 12 18 10, 12 19 10, 11 19 10))"),
            };
            rdr = this.reader2DM;
            IPolygon[] poly2DM =
            {
                (IPolygon)rdr.Read("POLYGON M((10 10 11, 10 20 11, 20 20 11, 20 15 11, 10 10 11))"),
                (IPolygon)rdr.Read("POLYGON M((10 10 11, 10 20 11, 20 20 11, 20 15 11, 10 10 11), (11 11 11, 12 11 11, 12 12 11, 12 11 11, 11 11 11))"),
                (IPolygon)rdr.Read("POLYGON M((10 10 11, 10 20 11, 20 20 11, 20 15 11, 10 10 11), (11 11 11, 12 11 11, 12 12 11, 12 11 11, 11 11 11), (11 19 11, 11 18 11, 12 18 11, 12 19 11, 11 19 11))"),
            };
            rdr = this.reader3DM;
            IPolygon[] poly3DM =
            {
                (IPolygon)rdr.Read("POLYGON ZM((10 10 10 11, 10 20 10 11, 20 20 10 11, 20 15 10 11, 10 10 10 11))"),
                (IPolygon)rdr.Read("POLYGON ZM((10 10 10 11, 10 20 10 11, 20 20 10 11, 20 15 10 11, 10 10 10 11), (11 11 10 11, 12 11 10 11, 12 12 10 11, 12 11 10 11, 11 11 10 11))"),
                (IPolygon)rdr.Read("POLYGON ZM((10 10 10 11, 10 20 10 11, 20 20 10 11, 20 15 10 11, 10 10 10 11), (11 11 10 11, 12 11 10 11, 12 12 10 11, 12 11 10 11, 11 11 10 11), (11 19 10 11, 11 18 10 11, 12 18 10 11, 12 19 10 11, 11 19 10 11))"),
            };

            // assert
            Assert.That(CheckEqual(csPoly2D[0], poly2D[2].ExteriorRing.CoordinateSequence));
            Assert.That(CheckEqual(csPoly2D[1], poly2D[2].GetInteriorRingN(0).CoordinateSequence));
            Assert.That(CheckEqual(csPoly2D[2], poly2D[2].GetInteriorRingN(1).CoordinateSequence));
            Assert.That(CheckEqual(csPoly2DE, poly2DE.ExteriorRing.CoordinateSequence, 2));

            Assert.That(CheckEqual(csPoly3D[0], poly3D[2].ExteriorRing.CoordinateSequence));
            Assert.That(CheckEqual(csPoly3D[1], poly3D[2].GetInteriorRingN(0).CoordinateSequence));
            Assert.That(CheckEqual(csPoly3D[2], poly3D[2].GetInteriorRingN(1).CoordinateSequence));
            Assert.That(CheckEqual(csPoly2DM[0], poly2DM[2].ExteriorRing.CoordinateSequence));
            Assert.That(CheckEqual(csPoly2DM[1], poly2DM[2].GetInteriorRingN(0).CoordinateSequence));
            Assert.That(CheckEqual(csPoly2DM[2], poly2DM[2].GetInteriorRingN(1).CoordinateSequence));
            Assert.That(CheckEqual(csPoly3DM[0], poly3DM[2].ExteriorRing.CoordinateSequence));
            Assert.That(CheckEqual(csPoly3DM[1], poly3DM[2].GetInteriorRingN(0).CoordinateSequence));
            Assert.That(CheckEqual(csPoly3DM[2], poly3DM[2].GetInteriorRingN(1).CoordinateSequence));
        }

        [Test]
        public void TestReadMultiPoint()
        {
            // arrange
            double[][] coordinates =
            {
                new double[] { 10, 10 },
                new double[] { 20, 20 },
            };
            ICoordinateSequence[] csMP2D =
            {
                CreateSequence(Ordinates.XY, coordinates[0]),
                CreateSequence(Ordinates.XY, coordinates[1]),
            };
            ICoordinateSequence[] csMP3D =
            {
                CreateSequence(Ordinates.XYZ, coordinates[0]),
                CreateSequence(Ordinates.XYZ, coordinates[1]),
            };
            ICoordinateSequence[] csMP2DM =
            {
                CreateSequence(Ordinates.XYM, coordinates[0]),
                CreateSequence(Ordinates.XYM, coordinates[1]),
            };
            ICoordinateSequence[] csMP3DM =
            {
                CreateSequence(Ordinates.XYZM, coordinates[0]),
                CreateSequence(Ordinates.XYZM, coordinates[1]),
            };

            // act
            var rdr = this.reader2D;
            var mP2D = (IMultiPoint)rdr.Read("MULTIPOINT ((10 10), (20 20))");
            var mP2DE = (IMultiPoint)rdr.Read("MultiPoint EMPTY");
            rdr = this.reader3D;
            var mP3D = (IMultiPoint)rdr.Read("MULTIPOINT Z((10 10 10), (20 20 10))");
            rdr = this.reader2DM;
            var mP2DM = (IMultiPoint)rdr.Read("MULTIPOINT M((10 10 11), (20 20 11))");
            rdr = this.reader3DM;
            var mP3DM = (IMultiPoint)rdr.Read("MULTIPOINT ZM((10 10 10 11), (20 20 10 11))");

            // assert
            Assert.That(CheckEqual(csMP2D[0], ((IPoint)mP2D.GetGeometryN(0)).CoordinateSequence));
            Assert.That(CheckEqual(csMP2D[1], ((IPoint)mP2D.GetGeometryN(1)).CoordinateSequence));
            Assert.That(mP2DE.IsEmpty);
            Assert.That(mP2DE.NumGeometries, Is.Zero);
            Assert.That(CheckEqual(csMP3D[0], ((IPoint)mP3D.GetGeometryN(0)).CoordinateSequence));
            Assert.That(CheckEqual(csMP3D[1], ((IPoint)mP3D.GetGeometryN(1)).CoordinateSequence));
            Assert.That(CheckEqual(csMP2DM[0], ((IPoint)mP2DM.GetGeometryN(0)).CoordinateSequence));
            Assert.That(CheckEqual(csMP2DM[1], ((IPoint)mP2DM.GetGeometryN(1)).CoordinateSequence));
            Assert.That(CheckEqual(csMP3DM[0], ((IPoint)mP3DM.GetGeometryN(0)).CoordinateSequence));
            Assert.That(CheckEqual(csMP3DM[1], ((IPoint)mP3DM.GetGeometryN(1)).CoordinateSequence));
        }

        [Test]
        public void TestReadMultiLineString()
        {
            // arrange
            double[][] coordinates =
            {
                new double[] { 10, 10, 20, 20 },
                new double[] { 15, 15, 30, 15 },
            };
            ICoordinateSequence[] csMls2D =
            {
                CreateSequence(Ordinates.XY, coordinates[0]),
                CreateSequence(Ordinates.XY, coordinates[1]),
            };
            ICoordinateSequence[] csMls3D =
            {
                CreateSequence(Ordinates.XYZ, coordinates[0]),
                CreateSequence(Ordinates.XYZ, coordinates[1]),
            };
            ICoordinateSequence[] csMls2DM =
            {
                CreateSequence(Ordinates.XYM, coordinates[0]),
                CreateSequence(Ordinates.XYM, coordinates[1]),
            };
            ICoordinateSequence[] csMls3DM =
            {
                CreateSequence(Ordinates.XYZM, coordinates[0]),
                CreateSequence(Ordinates.XYZM, coordinates[1]),
            };

            // act
            var rdr = this.reader2D;
            var mLs2D = (IMultiLineString)rdr.Read("MULTILINESTRING ((10 10, 20 20), (15 15, 30 15))");
            var mLs2DE = (IMultiLineString)rdr.Read("MultiLineString EMPTY");
            rdr = this.reader3D;
            var mLs3D = (IMultiLineString)rdr.Read("MULTILINESTRING Z((10 10 10, 20 20 10), (15 15 10, 30 15 10))");
            rdr = this.reader2DM;
            var mLs2DM = (IMultiLineString)rdr.Read("MULTILINESTRING M((10 10 11, 20 20 11), (15 15 11, 30 15 11))");
            rdr = this.reader3DM;
            var mLs3DM = (IMultiLineString)rdr.Read("MULTILINESTRING ZM((10 10 10 11, 20 20 10 11), (15 15 10 11, 30 15 10 11))");

            // assert
            Assert.That(CheckEqual(csMls2D[0], ((ILineString)mLs2D.GetGeometryN(0)).CoordinateSequence));
            Assert.That(CheckEqual(csMls2D[1], ((ILineString)mLs2D.GetGeometryN(1)).CoordinateSequence));
            Assert.That(mLs2DE.IsEmpty);
            Assert.That(mLs2DE.NumGeometries, Is.Zero);
            Assert.That(CheckEqual(csMls3D[0], ((ILineString)mLs3D.GetGeometryN(0)).CoordinateSequence));
            Assert.That(CheckEqual(csMls3D[1], ((ILineString)mLs3D.GetGeometryN(1)).CoordinateSequence));
            Assert.That(CheckEqual(csMls2DM[0], ((ILineString)mLs2DM.GetGeometryN(0)).CoordinateSequence));
            Assert.That(CheckEqual(csMls2DM[1], ((ILineString)mLs2DM.GetGeometryN(1)).CoordinateSequence));
            Assert.That(CheckEqual(csMls3DM[0], ((ILineString)mLs3DM.GetGeometryN(0)).CoordinateSequence));
            Assert.That(CheckEqual(csMls3DM[1], ((ILineString)mLs3DM.GetGeometryN(1)).CoordinateSequence));
        }

        [Test]
        public void TestReadMultiPolygon()
        {
            // arrange
            double[] shell1 = new double[] { 10, 10, 10, 20, 20, 20, 20, 15, 10, 10 };
            double[] ring1 = new double[] { 11, 11, 12, 11, 12, 12, 12, 11, 11, 11 };
            double[] shell2 = new double[] { 60, 60, 70, 70, 80, 60, 60, 60 };

            ICoordinateSequence[] csPoly2D =
            {
                CreateSequence(Ordinates.XY, shell1),
                CreateSequence(Ordinates.XY, ring1),
                CreateSequence(Ordinates.XY, shell2),
            };
            ICoordinateSequence[] csPoly3D =
            {
                CreateSequence(Ordinates.XYZ, shell1),
                CreateSequence(Ordinates.XYZ, ring1),
                CreateSequence(Ordinates.XYZ, shell2),
            };
            ICoordinateSequence[] csPoly2DM =
            {
                CreateSequence(Ordinates.XYM, shell1),
                CreateSequence(Ordinates.XYM, ring1),
                CreateSequence(Ordinates.XYM, shell2),
            };
            ICoordinateSequence[] csPoly3DM =
            {
                CreateSequence(Ordinates.XYZM, shell1),
                CreateSequence(Ordinates.XYZM, ring1),
                CreateSequence(Ordinates.XYZM, shell2),
            };

            // act
            var rdr = this.reader2D;
            IMultiPolygon[] poly2D =
            {
                (IMultiPolygon)rdr.Read("MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10)))"),
                (IMultiPolygon)rdr.Read("MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10), (11 11, 12 11, 12 12, 12 11, 11 11)))"),
                (IMultiPolygon)rdr.Read("MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10), (11 11, 12 11, 12 12, 12 11, 11 11)), ((60 60, 70 70, 80 60, 60 60)))"),
            };
            var poly2DE = (IMultiPolygon)rdr.Read("MultiPolygon EMPTY");
            rdr = this.reader3D;
            IMultiPolygon[] poly3D =
            {
                (IMultiPolygon)rdr.Read("MULTIPOLYGON Z(((10 10 10, 10 20 10, 20 20 10, 20 15 10, 10 10 10)))"),
                (IMultiPolygon)rdr.Read("MULTIPOLYGON Z(((10 10 10, 10 20 10, 20 20 10, 20 15 10, 10 10 10), (11 11 10, 12 11 10, 12 12 10, 12 11 10, 11 11 10)))"),
                (IMultiPolygon)rdr.Read("MULTIPOLYGON Z(((10 10 10, 10 20 10, 20 20 10, 20 15 10, 10 10 10), (11 11 10, 12 11 10, 12 12 10, 12 11 10, 11 11 10)), ((60 60 10, 70 70 10, 80 60 10, 60 60 10)))"),
            };
            IMultiPolygon[] poly2DM =
            {
                (IMultiPolygon)rdr.Read("MULTIPOLYGON M(((10 10 11, 10 20 11, 20 20 11, 20 15 11, 10 10 11)))"),
                (IMultiPolygon)rdr.Read("MULTIPOLYGON M(((10 10 11, 10 20 11, 20 20 11, 20 15 11, 10 10 11), (11 11 11, 12 11 11, 12 12 11, 12 11 11, 11 11 11)))"),
                (IMultiPolygon)rdr.Read("MULTIPOLYGON M(((10 10 11, 10 20 11, 20 20 11, 20 15 11, 10 10 11), (11 11 11, 12 11 11, 12 12 11, 12 11 11, 11 11 11)), ((60 60 11, 70 70 11, 80 60 11, 60 60 11)))"),
            };
            rdr = this.reader3DM;
            IMultiPolygon[] poly3DM =
            {
                (IMultiPolygon)rdr.Read("MULTIPOLYGON ZM(((10 10 10 11, 10 20 10 11, 20 20 10 11, 20 15 10 11, 10 10 10 11)))"),
                (IMultiPolygon)rdr.Read("MULTIPOLYGON ZM(((10 10 10 11, 10 20 10 11, 20 20 10 11, 20 15 10 11, 10 10 10 11), (11 11 10 11, 12 11 10 11, 12 12 10 11, 12 11 10 11, 11 11 10 11)))"),
                (IMultiPolygon)rdr.Read("MULTIPOLYGON ZM(((10 10 10 11, 10 20 10 11, 20 20 10 11, 20 15 10 11, 10 10 10 11), (11 11 10 11, 12 11 10 11, 12 12 10 11, 12 11 10 11, 11 11 10 11)), ((60 60 10 11, 70 70 10 11, 80 60 10 11, 60 60 10 11)))"),
            };

            // assert
            Assert.That(CheckEqual(csPoly2D[0], ((IPolygon)poly2D[2].GetGeometryN(0)).ExteriorRing.CoordinateSequence));
            Assert.That(CheckEqual(csPoly2D[1], ((IPolygon)poly2D[2].GetGeometryN(0)).GetInteriorRingN(0).CoordinateSequence));
            Assert.That(CheckEqual(csPoly2D[2], ((IPolygon)poly2D[2].GetGeometryN(1)).ExteriorRing.CoordinateSequence));
            Assert.That(poly2DE.IsEmpty);
            Assert.That(poly2DE.NumGeometries, Is.Zero);

            Assert.That(CheckEqual(csPoly3D[0], ((IPolygon)poly3D[2].GetGeometryN(0)).ExteriorRing.CoordinateSequence));
            Assert.That(CheckEqual(csPoly3D[1], ((IPolygon)poly3D[2].GetGeometryN(0)).GetInteriorRingN(0).CoordinateSequence));
            Assert.That(CheckEqual(csPoly3D[2], ((IPolygon)poly3D[2].GetGeometryN(1)).ExteriorRing.CoordinateSequence));

            Assert.That(CheckEqual(csPoly2DM[0], ((IPolygon)poly2DM[2].GetGeometryN(0)).ExteriorRing.CoordinateSequence));
            Assert.That(CheckEqual(csPoly2DM[1], ((IPolygon)poly2DM[2].GetGeometryN(0)).GetInteriorRingN(0).CoordinateSequence));
            Assert.That(CheckEqual(csPoly2DM[2], ((IPolygon)poly2DM[2].GetGeometryN(1)).ExteriorRing.CoordinateSequence));

            Assert.That(CheckEqual(csPoly3DM[0], ((IPolygon)poly3DM[2].GetGeometryN(0)).ExteriorRing.CoordinateSequence));
            Assert.That(CheckEqual(csPoly3DM[1], ((IPolygon)poly3DM[2].GetGeometryN(0)).GetInteriorRingN(0).CoordinateSequence));
            Assert.That(CheckEqual(csPoly3DM[2], ((IPolygon)poly3DM[2].GetGeometryN(1)).ExteriorRing.CoordinateSequence));
        }

        [Test]
        public void TestReadGeometryCollection()
        {
            // arrange
            double[][] coordinates =
            {
                new double[] { 10, 10 },
                new double[] { 30, 30 },
                new double[] { 15, 15, 20, 20 },
                Array.Empty<double>(),
                new double[] { 10, 10, 20, 20, 30, 40, 10, 10 },
            };

            ICoordinateSequence[] css =
            {
                CreateSequence(Ordinates.XY, coordinates[0]),
                CreateSequence(Ordinates.XY, coordinates[1]),
                CreateSequence(Ordinates.XY, coordinates[2]),
                CreateSequence(Ordinates.XY, coordinates[3]),
                CreateSequence(Ordinates.XY, coordinates[4]),
            };

            // act
            var rdr = GetWKTReader(Ordinates.XY, 1);
            var gc0 = (IGeometryCollection)rdr.Read("GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))");
            var gc1 = (IGeometryCollection)rdr.Read("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING EMPTY, LINESTRING (15 15, 20 20))");
            var gc2 = (IGeometryCollection)rdr.Read("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING (10 10, 20 20, 30 40, 10 10), LINESTRING (15 15, 20 20))");
            var gc3 = (IGeometryCollection)rdr.Read("GeometryCollection EMPTY");

            // assert
            Assert.That(CheckEqual(css[0], ((IPoint)gc0.GetGeometryN(0)).CoordinateSequence));
            Assert.That(CheckEqual(css[1], ((IPoint)gc0.GetGeometryN(1)).CoordinateSequence));
            Assert.That(CheckEqual(css[2], ((ILineString)gc0.GetGeometryN(2)).CoordinateSequence));
            Assert.That(CheckEqual(css[0], ((IPoint)gc1.GetGeometryN(0)).CoordinateSequence));
            Assert.That(CheckEqual(css[3], ((ILinearRing)gc1.GetGeometryN(1)).CoordinateSequence));
            Assert.That(CheckEqual(css[2], ((ILineString)gc1.GetGeometryN(2)).CoordinateSequence));
            Assert.That(CheckEqual(css[0], ((IPoint)gc2.GetGeometryN(0)).CoordinateSequence));
            Assert.That(CheckEqual(css[4], ((ILinearRing)gc2.GetGeometryN(1)).CoordinateSequence));
            Assert.That(CheckEqual(css[2], ((ILineString)gc2.GetGeometryN(2)).CoordinateSequence));
            Assert.That(gc3.IsEmpty);
        }

        [Test]
        public void TestReadLargeNumbers()
        {
            var precisionModel = new PrecisionModel(1E9);
            var geometryFactory = new GeometryFactory(precisionModel, 0);
            var reader = new WKTReader(geometryFactory);
            var point1 = ((IPoint)reader.Read("POINT (123456789.01234567890 10)")).CoordinateSequence;
            var point2 = geometryFactory.CreatePoint(new Coordinate(123456789.01234567890, 10)).CoordinateSequence;
            Assert.That(point1.GetOrdinate(0, Ordinate.X), Is.EqualTo(point2.GetOrdinate(0, Ordinate.X)).Within(1E-7));
            Assert.That(point1.GetOrdinate(0, Ordinate.Y), Is.EqualTo(point2.GetOrdinate(0, Ordinate.Y)).Within(1E-7));
        }

        private static ICoordinateSequence CreateSequence(Ordinates ordinateFlags, double[] xy)
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
                .Create(size, RequiredDimension(ordinateFlags));

            // fill in values
            int k = 0;
            for (int i = 0; i < ordinateValues.Length; i += dimension)
            {
                for (int j = 0; j < dimension; j++)
                {
                    res.SetOrdinate(k, (Ordinate)j, ordinateValues[i + j]);
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
