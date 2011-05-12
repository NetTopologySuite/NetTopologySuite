using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using GisSharpBlog.NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Utility
{
    public class AffineTransformationTest
    {
        [Test]
        public void TestRotate1()
        {
            AffineTransformation t = AffineTransformation.RotationInstance(Math.PI / 2);
            CheckTransformation(10, 0, t, 0, 10);
            CheckTransformation(0, 10, t, -10, 0);
            CheckTransformation(-10, -10, t, 10, -10);
        }

        //  [Test]
        //public void testRotateAroundPoint1()
        //{
        //  AffineTransformation t = AffineTransformation.RotationInstance(Math.PI/2, 1, 1);
        //  checkTransformation(1, 1, t, 1, 1);
        //  checkTransformation(10, 0, t, 2, 10);
        //  checkTransformation(0, 10, t, -8, 0);
        //  checkTransformation(-10, -10, t, 12, -10);
        //}
        [Test]

        public void TestReflectXy1()
        {
            AffineTransformation t = AffineTransformation.ReflectionInstance(1, 1);
            CheckTransformation(10, 0, t, 0, 10);
            CheckTransformation(0, 10, t, 10, 0);
            CheckTransformation(-10, -10, t, -10, -10);
            CheckTransformation(-3, -4, t, -4, -3);
        }

        [Test]
        public void TestReflectXy2()
        {
            AffineTransformation t = AffineTransformation.ReflectionInstance(1, -1);
            CheckTransformation(10, 0, t, 0, -10);
            CheckTransformation(0, 10, t, -10, 0);
            CheckTransformation(-10, -10, t, 10, 10);
            CheckTransformation(-3, -4, t, 4, 3);
        }

        [Test]
        public void TestReflectXyxy1()
        {
            AffineTransformation t = AffineTransformation.ReflectionInstance(0, 5, 5, 0);
            CheckTransformation(5, 0, t, 5, 0);
            CheckTransformation(0, 0, t, 5, 5);
            CheckTransformation(-10, -10, t, 15, 15);
        }
        [Test]

        public void TestScale1()
        {
            AffineTransformation t = AffineTransformation.ScaleInstance(2, 3);
            CheckTransformation(10, 0, t, 20, 0);
            CheckTransformation(0, 10, t, 0, 30);
            CheckTransformation(-10, -10, t, -20, -30);
        }
        [Test]

        public void TestShear1()
        {
            AffineTransformation t = AffineTransformation.ShearInstance(2, 3);
            CheckTransformation(10, 0, t, 10, 30);
        }

        [Test]
        public void TestTranslate1()
        {
            AffineTransformation t = AffineTransformation.TranslationInstance(2, 3);
            CheckTransformation(1, 0, t, 3, 3);
            CheckTransformation(0, 0, t, 2, 3);
            CheckTransformation(-10, -5, t, -8, -2);
        }
        [Test]

        public void TestTranslateRotate1()
        {
            AffineTransformation t = AffineTransformation.TranslationInstance(3, 3)
                                            .Rotate(Math.PI / 2);
            CheckTransformation(10, 0, t, -3, 13);
            CheckTransformation(-10, -10, t, 7, -7);
        }
        [Test]

        public void TestCompose1()
        {
            AffineTransformation t0 = AffineTransformation.TranslationInstance(10, 0);
            t0.Rotate(Math.PI / 2);
            t0.Translate(0, -10);

            AffineTransformation t1 = AffineTransformation.TranslationInstance(0, 0);
            t1.Rotate(Math.PI / 2);

            CheckTransformation(t0, t1);
        }

        [Test]
        public void TestCompose2()
        {
            AffineTransformation t0 = AffineTransformation.ReflectionInstance(0, 0, 1, 0);
            t0.Reflect(0, 0, 0, -1);

            AffineTransformation t1 = AffineTransformation.RotationInstance(Math.PI);

            CheckTransformation(t0, t1);
        }

        //[Test]
        //public void TestComposeRotation1()
        //{
        //    AffineTransformation t0 = AffineTransformation.RotationInstance(1, 10, 10);

        //    AffineTransformation t1 = AffineTransformation.TranslationInstance(-10, -10);
        //    t1.Rotate(1);
        //    t1.Translate(10, 10);

        //    checkTransformation(t0, t1);
        //}

        [Test]
        public void TestLineString()
        {
            CheckTransformation("LINESTRING (1 2, 10 20, 100 200)");
        }

        [Test]
        public void TestPolygon()
        {
            CheckTransformation("POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0))");
        }
        [Test]
        public void TestPolygonWithHole()
        {
            CheckTransformation("POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0), (1 1, 1 10, 10 10, 10 1, 1 1) )");
        }
        [Test]
        public void TestMultiPoint()
        {
            CheckTransformation("MULTIPOINT (0 0, 1 4, 100 200)");
        }
        [Test]
        public void TestMultiLineString()
        {
            CheckTransformation("MULTILINESTRING ((0 0, 1 10), (10 10, 20 30), (123 123, 456 789))");
        }
        [Test]
        public void TestMultiPolygon()
        {
            CheckTransformation("MULTIPOLYGON ( ((0 0, 100 0, 100 100, 0 100, 0 0), (1 1, 1 10, 10 10, 10 1, 1 1) ), ((200 200, 200 250, 250 250, 250 200, 200 200)) )");
        }
        [Test]

        public void TestGeometryCollection()
        {
            CheckTransformation("GEOMETRYCOLLECTION ( POINT ( 1 1), LINESTRING (0 0, 10 10), POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0)) )");
        }

        [Test]
        public void TestNestedGeometryCollection()
        {
            CheckTransformation("GEOMETRYCOLLECTION ( POINT (20 20), GEOMETRYCOLLECTION ( POINT ( 1 1), LINESTRING (0 0, 10 10), POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0)) ) )");
        }

        [Test]
        public void TestCompose3()
        {
            AffineTransformation t0 = AffineTransformation.ReflectionInstance(0, 10, 10, 0);
            t0.Translate(-10, -10);

            AffineTransformation t1 = AffineTransformation.ReflectionInstance(0, 0, -1, 1);

            CheckTransformation(t0, t1);
        }

        ///<summary>
        /// Checks that a transformation produces the expected result
        ///</summary>
        /// <param name="x">the input pt x</param>
        /// <param name="y">the input pt y</param>
        /// <param name="trans">the transformation</param>
        /// <param name="xp">the expected output x</param>
        /// <param name="yp">the expected output y</param>
        static void CheckTransformation(double x, double y, AffineTransformation trans, double xp, double yp)
        {
            ICoordinate p = new Coordinate(x, y);
            ICoordinate p2 = new Coordinate();
            trans.Transform(p, p2);
            Assert.AreEqual(xp, p2.X, .00005);
            Assert.AreEqual(yp, p2.Y, .00005);

            // if the transformation is invertible, test the inverse
            try
            {
                AffineTransformation invTrans = trans.getInverse();
                ICoordinate pInv = new Coordinate();
                invTrans.Transform(p2, pInv);
                Assert.AreEqual(x, pInv.X, .00005);
                Assert.AreEqual(y, pInv.Y, .00005);

                double det = trans.getDeterminant();
                double detInv = invTrans.getDeterminant();
                Assert.AreEqual(det, 1.0 / detInv, .00005);

            }
            catch (NoninvertibleTransformationException)
            {
            }
        }

        static readonly WKTReader WktReader = new WKTReader();

        static void CheckTransformation(String geomStr)
        {
            Geometry geom = (Geometry)WktReader.Read(geomStr);
            AffineTransformation trans = AffineTransformation
                .RotationInstance(Math.PI / 2);
            AffineTransformation inv = trans.getInverse();
            Geometry transGeom = (Geometry)geom.Clone();
            transGeom.Apply(trans);
            // System.out.println(transGeom);
            transGeom.Apply(inv);
            // check if transformed geometry is equal to original
            bool isEqual = geom.EqualsExact(transGeom, 0.0005);
            Assert.IsTrue(isEqual);
        }

        static void CheckTransformation(AffineTransformation trans0, AffineTransformation trans1)
        {
            double[] m0 = trans0.getMatrixEntries();
            double[] m1 = trans1.getMatrixEntries();
            for (int i = 0; i < m0.Length; i++)
            {
                Assert.AreEqual(m0[i], m1[i], 0.000005);
            }
        }
    }
}