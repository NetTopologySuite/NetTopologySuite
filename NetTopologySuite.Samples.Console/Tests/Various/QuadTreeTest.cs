using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class QuadTreeTest
    {
        [Test]
        public void TestQuadTree()
        {
            var qtree = new Index.Quadtree.Quadtree<IPoint>();
            var ptBuilder = new Shape.Random.RandomPointsInGridBuilder { Extent = new Envelope(-500, 500, -500, 500), NumPoints = 600000, GutterFraction = 0.1d };
            var mp = (IMultiPoint)ptBuilder.GetGeometry();

            foreach (var coord in mp.Coordinates)
            {
                var point = GeometryFactory.Default.CreatePoint(coord);
                qtree.Insert(point.EnvelopeInternal, point);
            }

            var search = new Envelope(4, 6, 4, 6);
            var res = qtree.Query(search);
            Assert.IsTrue(search.MinX == search.MinY && search.MinX == 4d);
            Assert.IsTrue(search.MaxX == search.MaxY && search.MaxX == 6d);

            foreach (var point in res)
                Assert.IsTrue(search.Intersects(point.EnvelopeInternal));
            Console.WriteLine(res.Count);
        }
    }
}