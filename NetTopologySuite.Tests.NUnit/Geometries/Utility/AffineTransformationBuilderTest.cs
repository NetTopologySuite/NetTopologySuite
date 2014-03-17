using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Utility
{
    /// <summary>
    /// Tests <see cref="AffineTransformationBuilder"/>
    /// </summary>  
    /// <author>Martin Davis</author>
    public class AffineTransformationBuilderTest
    {
        [TestAttribute]
        public void TestRotate1()
        {
            Run(0, 0, 1, 0, 0, 1,
                0, 0, 0, 1, -1, 0);
        }

        [TestAttribute]
        public void TestRotate2()
        {
            Run(0, 0, 1, 0, 0, 1,
                0, 0, 1, 1, -1, 1);
        }

        [TestAttribute]
        public void TestScale1()
        {
            Run(0, 0, 1, 0, 0, 1,
                0, 0, 2, 0, 0, 2);
        }

        [TestAttribute]
        public void TestTranslate1()
        {
            Run(0, 0, 1, 0, 0, 1,
                5, 6, 6, 6, 5, 7);
        }

        [TestAttribute]
        public void TestLinear1()
        {
            Run(0, 0, 1, 0, 0, 1,
                0, 0, 0, 0, 5, 7);
        }

        [TestAttribute]
        public void TestSingular2()
        {
            // points on a line mapping to collinear points - not uniquely specified
            RunSingular(0, 0, 1, 1, 2, 2,
                        0, 0, 10, 10, 30, 30);
        }

        [TestAttribute]
        public void TestSingular3()
        {
            // points on a line mapping to collinear points - not uniquely specified
            RunSingular(0, 0, 1, 1, 2, 2,
                        0, 0, 10, 10, 20, 20);
        }

        [TestAttribute]
        public void TestSingular1()
        {
            // points on a line mapping to non-collinear points - no solution
            RunSingular(0, 0, 1, 1, 2, 2,
                        0, 0, 1, 2, 1, 3);
        }

        [TestAttribute]
        public void TestSingleControl1()
        {
            Run(0, 0,
                5, 6);
        }

        [TestAttribute]
        public void TestDualControlTranslation()
        {
            Run(0, 0, 1, 1,
                    5, 5, 6, 6);
        }

        [TestAttribute]
        public void TestDualControlGeneral()
        {
            Run(0, 0, 1, 1,
                    5, 5, 6, 9);
        }

        static void Run(double p0x, double p0y,
            double p1x, double p1y,
            double p2x, double p2y,
            double pp0x, double pp0y,
            double pp1x, double pp1y,
            double pp2x, double pp2y
            )
        {
            Coordinate p0 = new Coordinate(p0x, p0y);
            Coordinate p1 = new Coordinate(p1x, p1y);
            Coordinate p2 = new Coordinate(p2x, p2y);

            Coordinate pp0 = new Coordinate(pp0x, pp0y);
            Coordinate pp1 = new Coordinate(pp1x, pp1y);
            Coordinate pp2 = new Coordinate(pp2x, pp2y);

            AffineTransformationBuilder atb = new AffineTransformationBuilder(
                p0, p1, p2,
                pp0, pp1, pp2);
            AffineTransformation trans = atb.GetTransformation();

            Coordinate dest = new Coordinate();
            AssertEqualPoint(pp0, trans.Transform(p0, dest));
            AssertEqualPoint(pp1, trans.Transform(p1, dest));
            AssertEqualPoint(pp2, trans.Transform(p2, dest));
        }

        void Run(double p0x, double p0y,
            double p1x, double p1y,
            double pp0x, double pp0y,
            double pp1x, double pp1y
            )
        {
            Coordinate p0 = new Coordinate(p0x, p0y);
            Coordinate p1 = new Coordinate(p1x, p1y);

            Coordinate pp0 = new Coordinate(pp0x, pp0y);
            Coordinate pp1 = new Coordinate(pp1x, pp1y);

            AffineTransformation trans = AffineTransformationFactory.CreateFromControlVectors(
                p0, p1,
                pp0, pp1);

            Coordinate dest = new Coordinate();
            AssertEqualPoint(pp0, trans.Transform(p0, dest));
            AssertEqualPoint(pp1, trans.Transform(p1, dest));
        }

        void Run(double p0x, double p0y,
            double pp0x, double pp0y
            )
        {
            Coordinate p0 = new Coordinate(p0x, p0y);

            Coordinate pp0 = new Coordinate(pp0x, pp0y);

            AffineTransformation trans = AffineTransformationFactory.CreateFromControlVectors(
                p0, pp0);

            Coordinate dest = new Coordinate();
            AssertEqualPoint(pp0, trans.Transform(p0, dest));
        }


        static void RunSingular(double p0x, double p0y,
            double p1x, double p1y,
            double p2x, double p2y,
            double pp0x, double pp0y,
            double pp1x, double pp1y,
            double pp2x, double pp2y
            )
        {
            Coordinate p0 = new Coordinate(p0x, p0y);
            Coordinate p1 = new Coordinate(p1x, p1y);
            Coordinate p2 = new Coordinate(p2x, p2y);

            Coordinate pp0 = new Coordinate(pp0x, pp0y);
            Coordinate pp1 = new Coordinate(pp1x, pp1y);
            Coordinate pp2 = new Coordinate(pp2x, pp2y);

            AffineTransformationBuilder atb = new AffineTransformationBuilder(
                p0, p1, p2,
                pp0, pp1, pp2);
            AffineTransformation trans = atb.GetTransformation();
            Assert.IsNull(trans);
        }

        private Coordinate ctl0 = new Coordinate(-10, -10);
        private Coordinate ctl1 = new Coordinate(10, 20);
        private Coordinate ctl2 = new Coordinate(10, -20);

        [TestAttribute]
        public void TestTransform1()
        {
            AffineTransformation trans = new AffineTransformation();
            trans.Rotate(1);
            trans.Translate(10, 10);
            trans.Scale(2, 2);
            RunTransform(trans, ctl0, ctl1, ctl2);
        }

        [TestAttribute]
        public void TestTransform2()
        {
            AffineTransformation trans = new AffineTransformation();
            trans.Rotate(3);
            trans.Translate(10, 10);
            trans.Scale(2, 10);
            trans.Shear(5, 2);
            trans.Reflect(5, 8, 10, 2);
            RunTransform(trans, ctl0, ctl1, ctl2);
        }

        private static void RunTransform(AffineTransformation trans,
            Coordinate p0,
            Coordinate p1,
            Coordinate p2)
        {
            Coordinate pp0 = trans.Transform(p0, new Coordinate());
            Coordinate pp1 = trans.Transform(p1, new Coordinate());
            Coordinate pp2 = trans.Transform(p2, new Coordinate());

            AffineTransformationBuilder atb = new AffineTransformationBuilder(
                p0, p1, p2,
                pp0, pp1, pp2);
            AffineTransformation atbTrans = atb.GetTransformation();

            Coordinate dest = new Coordinate();
            AssertEqualPoint(pp0, atbTrans.Transform(p0, dest));
            AssertEqualPoint(pp1, atbTrans.Transform(p1, dest));
            AssertEqualPoint(pp2, atbTrans.Transform(p2, dest));
        }


        private static void AssertEqualPoint(Coordinate p, Coordinate q)
        {
            Assert.AreEqual(p.X, q.X, 0.00005);
            Assert.AreEqual(p.Y, q.Y, 0.00005);
        }

    }
}