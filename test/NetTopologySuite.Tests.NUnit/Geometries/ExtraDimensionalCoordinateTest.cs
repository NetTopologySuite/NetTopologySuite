using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    public class ExtraDimensionalCoordinateTest
    {
        [Test]
        public void TestCreate4DM2()
        {
            var edc = new ExtraDimensionalCoordinate(4, 2);
            Assert.That(edc, Is.Not.Null);
            Assert.That(edc.Dimension == 4);
            Assert.That(edc.Measures == 2);
            Assert.That(edc.Z, Is.EqualTo(double.NaN));
        }

        [Test]
        public void TestIncreaseDimension()
        {
            var edc0 = new ExtraDimensionalCoordinate(4, 2) { X = 10, Y = 11 };
            edc0[Ordinate.Measure1] = 20;
            edc0[Ordinate.Measure2] = 21;

            var edc1 = new ExtraDimensionalCoordinate(edc0.Dimension + 1, edc0.Measures) { CoordinateValue = edc0, Z = 1 };
            Assert.That(edc1.Dimension == 5);
            Assert.That(edc1.Measures == 2);
            Assert.That(edc1.X, Is.EqualTo(10d));
            Assert.That(edc1.Y, Is.EqualTo(11d));
            Assert.That(edc1.Z, Is.EqualTo(1d));
            Assert.That(edc1[Ordinate.Measure1], Is.EqualTo(20d));
            Assert.That(edc1[Ordinate.Measure2], Is.EqualTo(21d));
        }

        [TestCase(2, 0, 2, 0)]
        [TestCase(3, 0, 2, 0)]
        [TestCase(3, 1, 2, 0)]
        [TestCase(4, 1, 3, 0)]
        [TestCase(4, 2, 3, 1)]
        [TestCase(4, 2, 3, 2)]
        [TestCase(4, 2, 2, 2)]
        [TestCase(3, 0, 4, 1)]
        [TestCase(3, 1, 4, 2)]
        [TestCase(3, 2, 4, 2)]
        [TestCase(2, 2, 4, 2)]
        public void TestCoordinateValue(int numSpatialFrom, int numMeasuresFrom, int numSpatialTo, int numMeasuresTo)
        {
            var fromCoord = Create(numSpatialFrom, numMeasuresFrom);
            var toCoord = Create(numSpatialTo, numMeasuresTo, false);

            toCoord.CoordinateValue = fromCoord;

            CheckEqual(fromCoord, toCoord, numSpatialFrom == numSpatialTo && numMeasuresFrom == numMeasuresTo);
        }

        private void CheckEqual(ExtraDimensionalCoordinate c0, ExtraDimensionalCoordinate c1, bool everything)
        {
            if (everything)
            {
                Assert.That(c1.Dimension, Is.EqualTo(c0.Dimension));
                Assert.That(c1.Measures, Is.EqualTo(c0.Measures));
            }

            Assert.That(c1.X, Is.EqualTo(c0.X));
            Assert.That(c1.Y, Is.EqualTo(c0.Y));
            Assert.That(c1.X, Is.EqualTo(c0.X));
            Assert.That(c1.Y, Is.EqualTo(c0.Y));

            int numSpatial0 = c0.Dimension - c0.Measures;
            int numSpatial1 = c1.Dimension - c1.Measures;

            if (numSpatial0 > 2 && numSpatial1 > 2)
                Assert.That(c0.Z, Is.EqualTo(c1.Z));
            else if (numSpatial1 == 2)
                Assert.That(c1.Z, Is.EqualTo(double.NaN));

            int numSpatialToTest = System.Math.Min(numSpatial0, numSpatial1);
            int i = 0;
            for (; i < numSpatialToTest; i++)
                Assert.That(c1[i], Is.EqualTo(c0[i]), $"this[{i}] values differ: {c0[i]} != {c1[i]}");

            int numMeasuresToTest = System.Math.Min(c0.Measures, c1.Measures);
            int j0 = numSpatial0;
            int j1 = numSpatial1;
            for (i = 0; i < numMeasuresToTest; i++, j0++, j1++)
            {
                Assert.That(c1[j1], Is.EqualTo(c0[j0]), $"this[{i}] values differ: {c0[j0]} != {c1[j1]}");
            }
        }

        private static ExtraDimensionalCoordinate Create(int numSpatial, int numMeasures, bool initValues = true)
        {
            if (numSpatial < 2 || numSpatial > 16)
                Assert.Ignore($"Number of spatial dimensions ({numSpatial}) is out of range \u2115[2..16]");
            if (numMeasures < 0 || numMeasures > 16)
                Assert.Ignore($"Number of measure dimensions ({numMeasures}) is out of range \u2115[0..16]");

            var res = new ExtraDimensionalCoordinate(numSpatial + numMeasures, numMeasures);
            if (initValues)
            {
                int i = 0;
                for (; i < numSpatial; i++)
                    res[i] = 10 + i;
                for (; i < res.Dimension; i++)
                    res[i] = 50 + i;
            }
            return res;
        }

    }
}
