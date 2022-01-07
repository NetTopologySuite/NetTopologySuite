using Trngl = NetTopologySuite.Triangulate.Tri.Tri;

using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate.Tri;
using NUnit.Framework;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Tests.NUnit.Triangulate.Tri
{
    public class TriTest : GeometryTestCase
    {
        private Trngl triCentre;
        private Trngl tri0;
        private Trngl tri1;
        private Trngl tri2;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            triCentre = CreateSimpleTriangulation(out tri0, out tri1, out tri2);
        }

        [Test]
        public void TestAdjacent()
        {
            Assert.That(tri0 == triCentre.GetAdjacent(0));
            Assert.That(tri1 == triCentre.GetAdjacent(1));
            Assert.That(tri2 == triCentre.GetAdjacent(2));
        }

        [Test]
        public void TestMidpoint()
        {
            var tri = Tri(0, 0, 0, 10, 10, 0);
            CheckEqualXY(new Coordinate(0, 5), tri.MidPoint(0));
            CheckEqualXY(new Coordinate(5, 5), tri.MidPoint(1));
            CheckEqualXY(new Coordinate(5, 0), tri.MidPoint(2));
        }

        [Test]
        public void TestCoordinateIndex()
        {
            var tri = Tri(0, 0, 0, 10, 10, 0);
            Assert.That(tri.GetIndex(new Coordinate(0, 0)), Is.EqualTo(0));
            Assert.That(tri.GetIndex(new Coordinate(0, 10)), Is.EqualTo(1));
            Assert.That(tri.GetIndex(new Coordinate(10, 0)), Is.EqualTo(2));
        }

        private static Trngl Tri(double x0, double y0, double x1, double y1, double x2, double y2)
        {
            var tri = Trngl.Create(
                new Coordinate(x0, y0),
                new Coordinate(x1, y1),
                new Coordinate(x2, y2));
            Assert.That(Orientation.Index(
                tri.GetCoordinate(0), tri.GetCoordinate(1), tri.GetCoordinate(2)), Is.EqualTo(OrientationIndex.Clockwise));
            return tri;
        }

        private static Trngl CreateSimpleTriangulation(out Trngl tri0, out Trngl tri1, out Trngl tri2)
        {
            var tri = Tri(10, 10, 10, 20, 20, 10);
            tri0 = Tri(10, 20, 10, 10, 0, 10);
            tri1 = Tri(20, 10, 10, 20, 20, 20);
            tri2 = Tri(10, 10, 20, 10, 10, 0);
            Build(tri, tri0, tri1, tri2);
            return tri;
        }

        private static void Build(params Trngl[] tri)
        {
            var triList = new List<Trngl>();
            for (int i = 0; i < tri.Length; i++)
            {
                triList.Add(tri[i]);
            }
            TriangulationBuilder.Build(triList);
        }
    }

}
