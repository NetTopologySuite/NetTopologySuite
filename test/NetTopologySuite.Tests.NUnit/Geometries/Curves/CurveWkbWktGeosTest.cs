// SPDX-License-Identifier: BSD-3-Clause
// GEOS-aligned WKT/WKB curve I/O tests. Assisted-by: xAI Grok

using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Curves;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Curves
{
    /// <summary>
    /// WKT/WKB import dovetailed with GEOS SQL/MM curve types (8–12).
    /// </summary>
    public class CurveWkbWktGeosTest
    {
        [TestCase("CIRCULARSTRING (0 0, 1 1, 2 0)")]
        [TestCase("CIRCULARSTRING EMPTY")]
        [TestCase("COMPOUNDCURVE ((0 0, 1 0), CIRCULARSTRING (1 0, 2 1, 3 0))")]
        [TestCase("COMPOUNDCURVE (LINESTRING (0 0, 1 0), CIRCULARSTRING (1 0, 2 1, 3 0))")]
        [TestCase("CURVEPOLYGON (CIRCULARSTRING (0 0, 2 2, 4 0, 2 -2, 0 0))")]
        [TestCase("MULTICURVE (CIRCULARSTRING (0 0, 1 1, 2 0), (3 0, 4 0))")]
        [TestCase("MULTICURVE EMPTY")]
        [TestCase("MULTISURFACE (CURVEPOLYGON (CIRCULARSTRING (0 0, 2 2, 4 0, 2 -2, 0 0)), POLYGON ((10 10, 20 10, 20 20, 10 20, 10 10)))")]
        [TestCase("MULTISURFACE EMPTY")]
        public void WktRoundTrip(string wkt)
        {
            var g = new WKTReader().Read(wkt);
            string written = g.AsText();
            var again = new WKTReader().Read(written);
            Assert.That(again.EqualsExact(g), Is.True, written);
        }

        [Test]
        public void GeosStyleLineStringTagInCompoundCurve()
        {
            var g = new WKTReader().Read(
                "COMPOUNDCURVE (LINESTRING (0 0, 1 0), CIRCULARSTRING (1 0, 2 1, 3 0))");
            var cc = (CompoundCurve)g;
            Assert.That(cc.Curves[0], Is.InstanceOf<LineString>());
            Assert.That(cc.Curves[1], Is.InstanceOf<CircularString>());
        }

        [TestCase("CIRCULARSTRING (0 0, 1 1, 2 0)")]
        [TestCase("CIRCULARSTRING EMPTY")]
        [TestCase("COMPOUNDCURVE ((0 0, 1 0), CIRCULARSTRING (1 0, 2 1, 3 0))")]
        [TestCase("CURVEPOLYGON (CIRCULARSTRING (0 0, 2 2, 4 0, 2 -2, 0 0))")]
        [TestCase("CURVEPOLYGON EMPTY")]
        [TestCase("MULTICURVE (CIRCULARSTRING (0 0, 1 1, 2 0), (3 0, 4 0))")]
        [TestCase("MULTICURVE EMPTY")]
        [TestCase("MULTISURFACE (CURVEPOLYGON (CIRCULARSTRING (0 0, 2 2, 4 0, 2 -2, 0 0)))")]
        [TestCase("MULTISURFACE EMPTY")]
        public void WkbRoundTrip(string wkt)
        {
            var g = new WKTReader().Read(wkt);
            var writer = new WKBWriter();
            byte[] bytes = writer.Write(g);
            var reader = new WKBReader();
            var again = reader.Read(bytes);
            Assert.That(again.GetType(), Is.EqualTo(g.GetType()), g.GeometryType);
            Assert.That(again.EqualsExact(g), Is.True, WKBWriter.ToHex(bytes));
        }

        [Test]
        public void WkbTypeCodesMatchGeosIso()
        {
            Assert.That((int)WKBGeometryTypes.WKBCircularString, Is.EqualTo(8));
            Assert.That((int)WKBGeometryTypes.WKBCompoundCurve, Is.EqualTo(9));
            Assert.That((int)WKBGeometryTypes.WKBCurvePolygon, Is.EqualTo(10));
            Assert.That((int)WKBGeometryTypes.WKBMultiCurve, Is.EqualTo(11));
            Assert.That((int)WKBGeometryTypes.WKBMultiSurface, Is.EqualTo(12));
        }

        [Test]
        public void MultiSurfacePreservesMemberSubtypes()
        {
            var g = (MultiSurface)new WKTReader().Read(
                "MULTISURFACE (CURVEPOLYGON (CIRCULARSTRING (0 0, 2 2, 4 0, 2 -2, 0 0)), POLYGON ((10 10, 20 10, 20 20, 10 20, 10 10)))");
            Assert.That(g.GetGeometryN(0), Is.InstanceOf<CurvePolygon>());
            Assert.That(g.GetGeometryN(1), Is.InstanceOf<Polygon>());
        }
    }
}
