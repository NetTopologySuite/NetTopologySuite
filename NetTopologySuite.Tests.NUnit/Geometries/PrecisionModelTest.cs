using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixtureAttribute]
    public class PrecisionModelTest
    {
        [TestAttribute]
        public void TestParameterlessConstructor()
        {
            var p = new PrecisionModel();
            //Implicit precision model has scale 0
            Assert.AreEqual(0, p.Scale, 1E-10);
        }

        [TestAttribute]
        public void TestGetMaximumSignificantDigits()
        {
            Assert.AreEqual(16, new PrecisionModel(PrecisionModels.Floating).MaximumSignificantDigits);
            Assert.AreEqual(6, new PrecisionModel(PrecisionModels.FloatingSingle).MaximumSignificantDigits);
            Assert.AreEqual(1, new PrecisionModel(PrecisionModels.Fixed).MaximumSignificantDigits);
            Assert.AreEqual(4, new PrecisionModel(1000).MaximumSignificantDigits);
        }


        [TestAttribute]
        public void TestMakePrecise()
        {
            var pm10 = new PrecisionModel(0.1);

            PreciseCoordinateTester(pm10, 1200.4, 1240.4, 1200, 1240);
            PreciseCoordinateTester(pm10, 1209.4, 1240.4, 1210, 1240);
        }


        [TestAttribute]
        public void TestMakePreciseNegative()
        {
            var pm1 = new PrecisionModel(1);

            PreciseCoordinateTester(pm1, -10, -10, -10, -10);
            PreciseCoordinateTester(pm1, -9.9, -9.9, -10, -10);
            
            //We use "Asymmetric Arithmetic Rounding", that's is why this is true:
            PreciseCoordinateTester(pm1, -9.5, -9.5, -9, -9);
        }

        private static void PreciseCoordinateTester(IPrecisionModel pm,
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
