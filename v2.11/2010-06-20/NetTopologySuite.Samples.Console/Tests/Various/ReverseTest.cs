using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.SimpleTests;
using NetTopologySuite.Coordinates;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class ReverseTest : BaseSamples
    {
        [Test]
        public void LineStringReverseTest()
        {
            var coordFactory = GeoFactory.CoordinateFactory;

            ILineString lineString = GeoFactory.CreateLineString(new ICoordinate[]
                                                                     {
                                                                         coordFactory.Create(10, 10),
                                                                         coordFactory.Create(20, 20),
                                                                         coordFactory.Create(20, 30),
                                                                     });

            ILineString reverse = lineString.Reverse();

            Debug.WriteLine(lineString.ToString());
            Debug.WriteLine(reverse.ToString());

            Assert.IsTrue(lineString.Equals(reverse));
            Assert.IsFalse(lineString.EqualsExact(reverse));
            Assert.IsFalse(lineString == reverse);

            Assert.AreEqual(lineString.Coordinates[0], reverse.Coordinates[2]);
            Assert.AreEqual(lineString.Coordinates[1], reverse.Coordinates[1]);
            Assert.AreEqual(lineString.Coordinates[2], reverse.Coordinates[0]);
        }

        [Test]
        public void MultiLineStringReverseTest()
        {
            ILineString lineString1 = GeoFactory.CreateLineString(new ICoordinate[]
                                                                      {
                                                                          CoordFactory.Create(10, 10),
                                                                          CoordFactory.Create(20, 20),
                                                                          CoordFactory.Create(20, 30),
                                                                      });

            ILineString lineString2 = GeoFactory.CreateLineString(new ICoordinate[]
                                                                      {
                                                                          CoordFactory.Create(12, 12),
                                                                          CoordFactory.Create(24, 24),
                                                                          CoordFactory.Create(36, 36),
                                                                      });

            IMultiLineString multiLineString = GeoFactory.CreateMultiLineString(
                new[]
                    {
                        lineString1, lineString2,
                    });

            IMultiLineString reverse = multiLineString.Reverse();

            Debug.WriteLine(multiLineString.ToString());
            Debug.WriteLine(reverse.ToString());

            Assert.IsTrue(multiLineString.Equals(reverse));
            Assert.IsFalse(multiLineString.EqualsExact(reverse));
            Assert.IsFalse(multiLineString == reverse);

            // Shouldn't the coordinates be reversed?
            Assert.IsTrue(multiLineString.Coordinates[0].Equals(reverse.Coordinates[5]));
            Assert.AreEqual(multiLineString.Coordinates[1], reverse.Coordinates[4]);
            Assert.AreEqual(multiLineString.Coordinates[2], reverse.Coordinates[3]);

            Assert.AreEqual(multiLineString.Coordinates[3], reverse.Coordinates[2]);
            Assert.AreEqual(multiLineString.Coordinates[4], reverse.Coordinates[1]);
            Assert.AreEqual(multiLineString.Coordinates[5], reverse.Coordinates[0]);
        }
    }
}