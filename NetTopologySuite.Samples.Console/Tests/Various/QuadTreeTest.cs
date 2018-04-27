using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class QuadTreeTest
    {
        [Test, Ignore("It is a known limitation to Quadtree implementation that more items are being returned than the ones actually intersecting.")]
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

            Console.WriteLine(string.Format("Query returned: {0}", res.Count));

            var reallyIntersecting = 0;
            foreach (var point in res)
            {
                if (search.Intersects(point.EnvelopeInternal))
                    reallyIntersecting++;
                //Assert.IsTrue(search.Intersects(point.EnvelopeInternal));
            }
            Console.WriteLine(string.Format("Really intersecting: {0}", reallyIntersecting));
            Console.WriteLine(string.Format("Ratio: {0}", (double)reallyIntersecting / res.Count));
        }
    }
}