using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class IsCCWTest
    {
        private WKTReader reader = new WKTReader();

        [Test]
        public void TestCCW()
        {
            var pts = GetCoordinates("POLYGON ((60 180, 140 240, 140 240, 140 240, 200 180, 120 120, 60 180))");
            Assert.IsFalse(Orientation.IsCCW(pts));
            var seq =
                GetCoordinateSequence("POLYGON ((60 180, 140 240, 140 240, 140 240, 200 180, 120 120, 60 180))");
            Assert.IsFalse(Orientation.IsCCW(seq));

            var pts2 = GetCoordinates("POLYGON ((60 180, 140 120, 100 180, 140 240, 60 180))");
            Assert.IsTrue(Orientation.IsCCW(pts2));
            var seq2 =
                GetCoordinateSequence("POLYGON ((60 180, 140 120, 100 180, 140 240, 60 180))");
            Assert.IsTrue(Orientation.IsCCW(seq2));

            // same pts list with duplicate top point - check that isCCW still works
            var pts2x = GetCoordinates("POLYGON ((60 180, 140 120, 100 180, 140 240, 140 240, 60 180))");
            Assert.IsTrue(Orientation.IsCCW(pts2x));
            var seq2x =
                GetCoordinateSequence("POLYGON ((60 180, 140 120, 100 180, 140 240, 140 240, 60 180))");
            Assert.IsTrue(Orientation.IsCCW(seq2x));
        }

        private Coordinate[] GetCoordinates(string wkt)
        {
            var geom = reader.Read(wkt);
            return geom.Coordinates;
        }

        private ICoordinateSequence GetCoordinateSequence(string wkt)
        {
            var geom = reader.Read(wkt);
            if (!(geom is Polygon))
                throw new ArgumentException($"{nameof(wkt)} must be of Polygon");

            var poly = (Polygon) geom;
            return poly.ExteriorRing.CoordinateSequence;
        }
    }
}
