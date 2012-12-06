using System;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    public class CoordinateTest
    {
        [Test]
        public void TestSettingOrdinateValuesViaIndexer()
        {
            var c = new Coordinate();
            Assert.DoesNotThrow(() => c[Ordinate.X] = 1);
            Assert.AreEqual(1, c.X);
            Assert.AreEqual(c.X, c[Ordinate.X]);

            Assert.DoesNotThrow(() => c[Ordinate.Y] = 2);
            Assert.AreEqual(2d, c.Y);
            Assert.AreEqual(c.Y, c[Ordinate.Y]);

            Assert.DoesNotThrow(() => c[Ordinate.Z] = 3);
            Assert.AreEqual(3d, c.Z);
            Assert.AreEqual(c.Z, c[Ordinate.Z]);

            Assert.Throws<ArgumentOutOfRangeException>(() => c[Ordinate.M] = 4);
        }
    }
}