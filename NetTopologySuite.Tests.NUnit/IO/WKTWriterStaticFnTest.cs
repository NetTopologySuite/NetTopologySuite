using System;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    [TestFixture]
    public class WKTWriterStaticFnTest
    {
        private Random _rnd = new Random(13);

        private WKTReader _reader = new WKTReader() { IsOldNtsCoordinateSyntaxAllowed = false };

        [Test]
        public void TestStaticToPoint()
        {
            for (int i = 0; i < 1000; i++)
            {
                var cs = new Coordinate(100 * _rnd.NextDouble(), 100 * _rnd.NextDouble());
                string toPointText = WKTWriter.ToPoint(cs);
                var cd = _reader.Read(toPointText).Coordinate;
                Assert.That(cd, Is.EqualTo(cs));
            }
        }

        [Test]
        public void TestStaticToLineStringFromSequence()
        {
            for (int i = 0; i < 1000; i++)
            {
                int size = 2 + _rnd.Next(10);
                var cs = GeometryTestCase.GetCSFactory(Ordinates.XY).Create(size, 2, 0);
                for (int j = 0; j < cs.Count; j++)
                {
                    cs.SetOrdinate(j, Ordinate.X, 100 * _rnd.NextDouble());
                    cs.SetOrdinate(j, Ordinate.Y, 100 * _rnd.NextDouble());
                }

                string toLineStringText = WKTWriter.ToLineString(cs);
                var cd = ((ILineString)_reader.Read(toLineStringText)).CoordinateSequence;
                Assert.That(cd.Count, Is.EqualTo(cs.Count));
                for (int j = 0; j < cs.Count; j++)
                {
                    Assert.That(cd.GetCoordinate(j), Is.EqualTo(cs.GetCoordinate(j)));
                }
            }
        }

        [Test]
        public void TestStaticToLineStringFromCoordinateArray()
        {
            for (int i = 0; i < 1000; i++)
            {
                int size = 2 + _rnd.Next(10);
                var cs = new Coordinate[size];
                for (int j = 0; j < cs.Length; j++)
                {
                    cs[j] = new Coordinate(100 * _rnd.NextDouble(), 100 * _rnd.NextDouble());
                }

                string toLineStringText = WKTWriter.ToLineString(cs);
                var cd = _reader.Read(toLineStringText).Coordinates;
                Assert.That(cd, Is.EqualTo(cs));
            }
        }

        [Test]
        public void TestStaticToLineStringFromTwoCoords()
        {
            for (int i = 0; i < 1000; i++)
            {
                Coordinate[] cs =
                {
                    new Coordinate(100 * _rnd.NextDouble(), 100 * _rnd.NextDouble()),
                    new Coordinate(100 * _rnd.NextDouble(), 100 * _rnd.NextDouble())
                };
                string toLineStringText = WKTWriter.ToLineString(cs[0], cs[1]);
                var cd = _reader.Read(toLineStringText).Coordinates;
                Assert.That(cd, Is.EqualTo(cs));
            }
        }

        [Test]
        public void TestStaticToLineStringFromTwoCoordsUsingSpecialValues()
        {
            // this value is interesting because .NET Framework (at least up to 4.7.2) and .NET Core
            // (at least up to 2.2) will fail to round-trip val.ToString("R") in 64-bit mode.
            const double RoundTripTestValue1 = 0.84551240822557006;

            // this value is interesting because always doing the same thing that JTS does in the
            // "normal" WKT writing path (i.e., format using "0.################") will fail to
            // round-trip in .NET.  Also, it's a case where NTS does better than JTS...
            const double RoundTripTestValue2 = 0.000073552131687082412;
            Coordinate[] cs =
            {
                new Coordinate(RoundTripTestValue1, RoundTripTestValue2),
                new Coordinate(RoundTripTestValue2, RoundTripTestValue1)
            };
            string toLineStringText = WKTWriter.ToLineString(cs[0], cs[1]);
            var cd = _reader.Read(toLineStringText).Coordinates;
            Assert.That(cd, Is.EqualTo(cs));
        }
    }
}
