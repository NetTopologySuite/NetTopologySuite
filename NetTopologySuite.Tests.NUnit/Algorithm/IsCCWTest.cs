using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixtureAttribute]
    public class IsCCWTest
    {
        private WKTReader reader = new WKTReader();

        [TestAttribute]
        public void TestCCW()
        {
            Coordinate[] pts = GetCoordinates("POLYGON ((60 180, 140 240, 140 240, 140 240, 200 180, 120 120, 60 180))");
            Assert.IsFalse(Orientation.IsCCW(pts));
            ICoordinateSequence seq =
                GetCoordinateSequence("POLYGON ((60 180, 140 240, 140 240, 140 240, 200 180, 120 120, 60 180))");
            Assert.IsFalse(Orientation.IsCCW(seq));

            Coordinate[] pts2 = GetCoordinates("POLYGON ((60 180, 140 120, 100 180, 140 240, 60 180))");
            Assert.IsTrue(Orientation.IsCCW(pts2));
            ICoordinateSequence seq2 =
                GetCoordinateSequence("POLYGON ((60 180, 140 120, 100 180, 140 240, 60 180))");
            Assert.IsTrue(Orientation.IsCCW(seq2));

            // same pts list with duplicate top point - check that isCCW still works
            Coordinate[] pts2x = GetCoordinates("POLYGON ((60 180, 140 120, 100 180, 140 240, 140 240, 60 180))");
            Assert.IsTrue(Orientation.IsCCW(pts2x));
            ICoordinateSequence seq2x =
                GetCoordinateSequence("POLYGON ((60 180, 140 120, 100 180, 140 240, 140 240, 60 180))");
            Assert.IsTrue(Orientation.IsCCW(seq2x));
        }

        private Coordinate[] GetCoordinates(String wkt)
        {
            IGeometry geom = reader.Read(wkt);
            return geom.Coordinates;
        }

        private ICoordinateSequence GetCoordinateSequence(string wkt)
        {
            var geom = reader.Read(wkt);
            if (geom is IGeometryCollection)
                throw new ArgumentException($"{nameof(wkt)} must be of LineString or Polygon");

            var poly = (IPolygon) geom;
            return poly.ExteriorRing.CoordinateSequence;
        }
    }
}
