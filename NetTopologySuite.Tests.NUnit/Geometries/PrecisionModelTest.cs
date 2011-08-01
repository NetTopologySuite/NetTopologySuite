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

            preciseCoordinateTester(pm_10, 1200.4, 1240.4, 1200, 1240);
            preciseCoordinateTester(pm_10, 1209.4, 1240.4, 1210, 1240);
        }

        private void preciseCoordinateTester(PrecisionModel pm,
            double x1, double y1,
            double x2, double y2)
        {
            Coordinate p = new Coordinate(x1, y1);

            pm.MakePrecise(p);

            Coordinate pPrecise = new Coordinate(x2, y2);
            Assert.IsTrue(p.Equals2D(pPrecise));
        }
    }
}
