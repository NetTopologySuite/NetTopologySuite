using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class PrecisionModelTest
    {
        [Test]
        public void TestParameterlessConstructor()
        {
            PrecisionModel p = new PrecisionModel();
            //Implicit precision model has scale 0
            Assert.AreEqual(0, p.Scale, 1E-10);
        }

        [Test]
        public void TestGetMaximumSignificantDigits()
        {
            Assert.AreEqual(16, new PrecisionModel(PrecisionModels.Floating).MaximumSignificantDigits);
            Assert.AreEqual(6, new PrecisionModel(PrecisionModels.FloatingSingle).MaximumSignificantDigits);
            Assert.AreEqual(1, new PrecisionModel(PrecisionModels.Fixed).MaximumSignificantDigits);
            Assert.AreEqual(4, new PrecisionModel(1000).MaximumSignificantDigits);
        }


        [Test]
        public void TestMakePrecise()
        {
            PrecisionModel pm_10 = new PrecisionModel(0.1);

            PreciseCoordinateTester(pm_10, 1200.4, 1240.4, 1200, 1240);
            PreciseCoordinateTester(pm_10, 1209.4, 1240.4, 1210, 1240);
        }


        [Test]
        public void TestMakePreciseNegative()
        {
            var pm_1 = new PrecisionModel(1);

            PreciseCoordinateTester(pm_1, -10, -10, -10, -10);
            PreciseCoordinateTester(pm_1, -9.9, -9.9, -10, -10);
            PreciseCoordinateTester(pm_1, -9.5, -9.5, -10, -10);
        }

        private static void PreciseCoordinateTester(PrecisionModel pm,
            double x1, double y1,
            double x2, double y2)
        {
            var p = new Coordinate(x1, y1);
            pm.MakePrecise(p);

            var pPrecise = new Coordinate(x2, y2);
            Assert.IsTrue(p.Equals2D(pPrecise), "Expected {0}, but got {1}", pPrecise, p);
        }
    }
}
