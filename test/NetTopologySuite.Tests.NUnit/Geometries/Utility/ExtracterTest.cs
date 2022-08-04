using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Tests.NUnit.Geometries.Utility
{
    internal class ExtracterTest : GeometryTestCase
    {
        const string PointWkt = "POINT (10 10)";
        const string MultiPointWkt = "MULTIPOINT ((10 10), (20 20))";
        const string LineStringWkt = "LINESTRING (10 10, 20 20)";
        const string MultiLineStringWkt = "MULTILINESTRING ((10 10, 20 20), (10 11, 20 21))";
        const string PolygonWkt = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";
        const string MultiPolygonWkt = "MULTIPOLYGON(((10 10, 10 20, 20 20, 20 10, 10 10)), ((30 10, 30 20, 40 20, 40 10, 30 10)))";
        const string GeometryCollectionWkt = "GEOMETRYCOLLECTION(" + PointWkt + ", " + LineStringWkt + ", " + PolygonWkt + ")";

        [Test]
        public void TestWithPolygon()
        {
            var geom = Read(PolygonWkt);
            var points = Extracter.GetPoints(geom);
            Check(points, Array.Empty<Point>());
            var lines = Extracter.GetLines(geom);
            Check(lines, Array.Empty<LineString>());
            var polygons = Extracter.GetPolygons(geom);
            Check(polygons, geom);
        }

        [Test]
        public void TestWithMultiPolygon()
        {
            var geom = Read(MultiPolygonWkt);
            var points = Extracter.GetPoints(geom);
            Check(points, Array.Empty<Point>());
            var lines = Extracter.GetLines(geom);
            Check(lines, Array.Empty<LineString>());
            var polygons = Extracter.GetPolygons(geom);
            Check(polygons, geom);
        }

        [Test]
        public void TestWithLineString()
        {
            var geom = Read(LineStringWkt);
            var points = Extracter.GetPoints(geom);
            Check(points, Array.Empty<Point>());
            var lines = Extracter.GetLines(geom);
            Check(lines, geom);
            var polygons = Extracter.GetPolygons(geom);
            Check(polygons, Array.Empty<Polygon>());
        }

        [Test]
        public void TestWithMultiLineString()
        {
            var geom = Read(MultiLineStringWkt);
            var points = Extracter.GetPoints(geom);
            Check(points, Array.Empty<Point>());
            var lines = Extracter.GetLines(geom);
            Check(lines, geom);
            var polygons = Extracter.GetPolygons(geom);
            Check(polygons, Array.Empty<Polygon>());
        }

        [Test]
        public void TestWithPoint()
        {
            var geom = Read(PointWkt);
            var points = Extracter.GetPoints(geom);
            Check(points, geom);
            var lines = Extracter.GetLines(geom);
            Check(lines, Array.Empty<LineString>());
            var polygons = Extracter.GetPolygons(geom);
            Check(polygons, Array.Empty<Polygon>());
        }

        [Test]
        public void TestWithMultiPoint()
        {
            var geom = Read(MultiPointWkt);
            var points = Extracter.GetPoints(geom);
            Check(points, geom);
            var lines = Extracter.GetLines(geom);
            Check(lines, Array.Empty<LineString>());
            var polygons = Extracter.GetPolygons(geom);
            Check(polygons, Array.Empty<Polygon>());
        }

        [Test]
        public void TestWithGeometryCollection()
        {
            var geom = Read(GeometryCollectionWkt);
            var points = Extracter.GetPoints(geom);
            Check(points, geom.GetGeometryN(0));
            var lines = Extracter.GetLines(geom);
            Check(lines, geom.GetGeometryN(1));
            var polygons = Extracter.GetPolygons(geom);
            Check(polygons, geom.GetGeometryN(2));
        }

        private void Check<T>(IList<T> geoms, params T[] expected) where T: Geometry
        {
            Assert.That(geoms.Count, Is.EqualTo(expected.Length));
            for (int i = 0; i < expected.Length; i++)
                Assert.That(geoms[i], Is.SameAs(expected[i]));
        }
        private void Check<T>(IList<T> geoms, Geometry geometry) where T : Geometry
        {
            Assert.That(geoms.Count, Is.EqualTo(geometry.NumGeometries));
            for (int i = 0; i < geometry.NumGeometries; i++)
                Assert.That(geoms[i], Is.SameAs((T)geometry.GetGeometryN(i)));
        }
    }
}
