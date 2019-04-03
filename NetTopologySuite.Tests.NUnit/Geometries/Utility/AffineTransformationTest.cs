using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Utility
{
    public class AffineTransformationTest
    {
        [TestAttribute]
        public void TestRotate1()
        {
            var t = AffineTransformation.RotationInstance(Math.PI / 2);
            CheckTransformation(10, 0, t, 0, 10);
            CheckTransformation(0, 10, t, -10, 0);
            CheckTransformation(-10, -10, t, 10, -10);
        }

        [Test]
        public void TestRotate2()
        {
            var t = AffineTransformation.RotationInstance(1, 0);
            CheckTransformation(10, 0, t, 0, 10);
            CheckTransformation(0, 10, t, -10, 0);
            CheckTransformation(-10, -10, t, 10, -10);
        }

        [Test]
        public void TestRotateAroundPoint1()
        {
          var t = AffineTransformation.RotationInstance(Math.PI/2, 1, 1);
          CheckTransformation(1, 1, t, 1, 1);
          CheckTransformation(10, 0, t, 2, 10);
          CheckTransformation(0, 10, t, -8, 0);
          CheckTransformation(-10, -10, t, 12, -10);
        }

        [Test]
        public void TestRotateAroundPoint2()
        {
            var t = AffineTransformation.RotationInstance(1, 0, 1, 1);
            CheckTransformation(1, 1, t, 1, 1);
            CheckTransformation(10, 0, t, 2, 10);
            CheckTransformation(0, 10, t, -8, 0);
            CheckTransformation(-10, -10, t, 12, -10);
        }

        [TestAttribute]

        public void TestReflectXy1()
        {
            var t = AffineTransformation.ReflectionInstance(1, 1);
            CheckTransformation(10, 0, t, 0, 10);
            CheckTransformation(0, 10, t, 10, 0);
            CheckTransformation(-10, -10, t, -10, -10);
            CheckTransformation(-3, -4, t, -4, -3);
        }

        [TestAttribute]
        public void TestReflectXy2()
        {
            var t = AffineTransformation.ReflectionInstance(1, -1);
            CheckTransformation(10, 0, t, 0, -10);
            CheckTransformation(0, 10, t, -10, 0);
            CheckTransformation(-10, -10, t, 10, 10);
            CheckTransformation(-3, -4, t, 4, 3);
        }

        [TestAttribute]
        public void TestReflectXyxy1()
        {
            var t = AffineTransformation.ReflectionInstance(0, 5, 5, 0);
            CheckTransformation(5, 0, t, 5, 0);
            CheckTransformation(0, 0, t, 5, 5);
            CheckTransformation(-10, -10, t, 15, 15);
        }
        [TestAttribute]

        public void TestScale1()
        {
            var t = AffineTransformation.ScaleInstance(2, 3);
            CheckTransformation(10, 0, t, 20, 0);
            CheckTransformation(0, 10, t, 0, 30);
            CheckTransformation(-10, -10, t, -20, -30);
        }
        [TestAttribute]

        public void TestShear1()
        {
            var t = AffineTransformation.ShearInstance(2, 3);
            CheckTransformation(10, 0, t, 10, 30);
        }

        [TestAttribute]
        public void TestTranslate1()
        {
            var t = AffineTransformation.TranslationInstance(2, 3);
            CheckTransformation(1, 0, t, 3, 3);
            CheckTransformation(0, 0, t, 2, 3);
            CheckTransformation(-10, -5, t, -8, -2);
        }
        [TestAttribute]

        public void TestTranslateRotate1()
        {
            var t = AffineTransformation.TranslationInstance(3, 3)
                                            .Rotate(Math.PI / 2);
            CheckTransformation(10, 0, t, -3, 13);
            CheckTransformation(-10, -10, t, 7, -7);
        }
        [TestAttribute]

        public void TestCompose1()
        {
            var t0 = AffineTransformation.TranslationInstance(10, 0);
            t0.Rotate(Math.PI / 2);
            t0.Translate(0, -10);

            var t1 = AffineTransformation.TranslationInstance(0, 0);
            t1.Rotate(Math.PI / 2);

            CheckTransformation(t0, t1);
        }

        [TestAttribute]
        public void TestCompose2()
        {
            var t0 = AffineTransformation.ReflectionInstance(0, 0, 1, 0);
            t0.Reflect(0, 0, 0, -1);

            var t1 = AffineTransformation.RotationInstance(Math.PI);

            CheckTransformation(t0, t1);
        }

        //[TestAttribute]
        //public void TestComposeRotation1()
        //{
        //    AffineTransformation t0 = AffineTransformation.RotationInstance(1, 10, 10);

        //    AffineTransformation t1 = AffineTransformation.TranslationInstance(-10, -10);
        //    t1.Rotate(1);
        //    t1.Translate(10, 10);

        //    checkTransformation(t0, t1);
        //}

        [TestAttribute]
        public void TestLineString()
        {
            CheckTransformation("LINESTRING (1 2, 10 20, 100 200)");
        }

        [TestAttribute]
        public void TestPolygon()
        {
            CheckTransformation("POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0))");
        }
        [TestAttribute]
        public void TestPolygonWithHole()
        {
            CheckTransformation("POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0), (1 1, 1 10, 10 10, 10 1, 1 1) )");
        }
        [TestAttribute]
        public void TestMultiPoint()
        {
            CheckTransformation("MULTIPOINT (0 0, 1 4, 100 200)");
        }
        [TestAttribute]
        public void TestMultiLineString()
        {
            CheckTransformation("MULTILINESTRING ((0 0, 1 10), (10 10, 20 30), (123 123, 456 789))");
        }
        [TestAttribute]
        public void TestMultiPolygon()
        {
            CheckTransformation("MULTIPOLYGON ( ((0 0, 100 0, 100 100, 0 100, 0 0), (1 1, 1 10, 10 10, 10 1, 1 1) ), ((200 200, 200 250, 250 250, 250 200, 200 200)) )");
        }
        [TestAttribute]

        public void TestGeometryCollection()
        {
            CheckTransformation("GEOMETRYCOLLECTION ( POINT ( 1 1), LINESTRING (0 0, 10 10), POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0)) )");
        }

        [TestAttribute]
        public void TestNestedGeometryCollection()
        {
            CheckTransformation("GEOMETRYCOLLECTION ( POINT (20 20), GEOMETRYCOLLECTION ( POINT ( 1 1), LINESTRING (0 0, 10 10), POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0)) ) )");
        }

        [TestAttribute]
        public void TestCompose3()
        {
            var t0 = AffineTransformation.ReflectionInstance(0, 10, 10, 0);
            t0.Translate(-10, -10);

            var t1 = AffineTransformation.ReflectionInstance(0, 0, -1, 1);

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
            var p = new Coordinate(x, y);
            var p2 = new Coordinate();
            trans.Transform(p, p2);
            Assert.AreEqual(xp, p2.X, .00005);
            Assert.AreEqual(yp, p2.Y, .00005);

            // if the transformation is invertible, test the inverse
            try
            {
                var invTrans = trans.GetInverse();
                var pInv = new Coordinate();
                invTrans.Transform(p2, pInv);
                Assert.AreEqual(x, pInv.X, .00005);
                Assert.AreEqual(y, pInv.Y, .00005);

                double det = trans.Determinant;
                double detInv = invTrans.Determinant;
                Assert.AreEqual(det, 1.0 / detInv, .00005);

            }
            catch (NoninvertibleTransformationException)
            {
            }
        }

        static readonly WKTReader WktReader = new WKTReader();

        static void CheckTransformation(string geomStr)
        {
            var geom = (Geometry)WktReader.Read(geomStr);
            var trans = AffineTransformation
                .RotationInstance(Math.PI / 2);
            var inv = trans.GetInverse();
            var transGeom = (Geometry)geom.Copy();
            transGeom.Apply(trans);
            // System.out.println(transGeom);
            transGeom.Apply(inv);
            // check if transformed geometry is equal to original
            bool isEqual = geom.EqualsExact(transGeom, 0.0005);
            Assert.IsTrue(isEqual);
        }

        static void CheckTransformation(AffineTransformation trans0, AffineTransformation trans1)
        {
            double[] m0 = trans0.MatrixEntries;
            double[] m1 = trans1.MatrixEntries;
            for (int i = 0; i < m0.Length; i++)
            {
                Assert.AreEqual(m0[i], m1[i], 0.000005);
            }
        }
    }
}
