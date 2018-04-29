using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Valid;
using NUnit.Framework;
namespace NetTopologySuite.Tests.NUnit.Operation.Valid
{
    /// <summary>
    /// Tests validating geometries with
    /// non-closed rings.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <version>1.7</version
    [TestFixtureAttribute]
    public class ValidClosedRingTest
    {
        private static WKTReader rdr = new WKTReader();
        [TestAttribute]
        public void TestBadLinearRing()
        {
            var ring = (LinearRing) FromWKT("LINEARRING (0 0, 0 10, 10 10, 10 0, 0 0)");
            UpdateNonClosedRing(ring);
            CheckIsValid(ring, false);
        }
        [TestAttribute]
        public void TestGoodLinearRing()
        {
            var ring = (LinearRing) FromWKT("LINEARRING (0 0, 0 10, 10 10, 10 0, 0 0)");
            CheckIsValid(ring, true);
        }
        [TestAttribute]
        public void TestBadPolygonShell()
        {
            var poly = (Polygon) FromWKT("POLYGON ((0 0, 0 10, 10 10, 10 0, 0 0))");
            UpdateNonClosedRing((LinearRing) poly.ExteriorRing);
            CheckIsValid(poly, false);
        }
        [TestAttribute]
        public void TestBadPolygonHole()
        {
            var poly = (Polygon) FromWKT("POLYGON ((0 0, 0 10, 10 10, 10 0, 0 0), (1 1, 2 1, 2 2, 1 2, 1 1) ))");
            UpdateNonClosedRing((LinearRing) poly.GetInteriorRingN(0));
            CheckIsValid(poly, false);
        }
        [TestAttribute]
        public void TestGoodPolygon()
        {
            var poly = (Polygon) FromWKT("POLYGON ((0 0, 0 10, 10 10, 10 0, 0 0))");
            CheckIsValid(poly, true);
        }
        [TestAttribute]
        public void TestBadGeometryCollection()
        {
            var gc = (GeometryCollection) FromWKT("GEOMETRYCOLLECTION ( POLYGON ((0 0, 0 10, 10 10, 10 0, 0 0), (1 1, 2 1, 2 2, 1 2, 1 1) )), POINT(0 0) )");
            var poly = (Polygon) gc.GetGeometryN(0);
            UpdateNonClosedRing((LinearRing) poly.GetInteriorRingN(0));
            CheckIsValid(poly, false);
        }
        private void CheckIsValid(Geometry geom, bool expected)
        {
            var validator = new IsValidOp(geom);
            var isValid = validator.IsValid;
            Assert.IsTrue(isValid == expected);
        }
        IGeometry FromWKT(String wkt)
        {
            IGeometry geom = null;
            try
            {
                geom = rdr.Read(wkt);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            return geom;
        }
        private void UpdateNonClosedRing(LinearRing ring)
        {
            var pts = ring.Coordinates;
            pts[0].X += 0.0001;
        }
    }
}
