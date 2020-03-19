using System;
using System.Collections;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.HPRtree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index.HPRtree
{
    public class HPRtreeTest
    {
        [Test]
        public void TestEmptyTreeUsingListQuery()
        {
            var tree = new HPRtree<object>();
            var list = tree.Query(new Envelope(0, 0, 1, 1));
            Assert.That(list.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestEmptyTreeUsingItemVisitorQuery()
        {
            var tree = new HPRtree<object>(0);
            tree.Query(new Envelope(0, 0, 1, 1), new ShouldNeverReachHereItemVisitor());
        }

        private class ShouldNeverReachHereItemVisitor : IItemVisitor<object>
        {
            public void VisitItem(object item)
            {
                Assert.Fail("Should never reach here");
            }
        }

        [Test]
        public void TestSpatialIndex()
        {
            var tester = new SpatialIndexTester {SpatialIndex = new HPRtree<object>()};
            tester.Init();
            tester.Run();
            Assert.That(tester.IsSuccess);
        }

        [Test]
        public void TestDisallowedInserts()
        {
            var t = new HPRtree<object>(3);
            t.Insert(new Envelope(0, 0, 0, 0), new object());
            t.Insert(new Envelope(0, 0, 0, 0), new object());
            t.Query(new Envelope());
            try
            {
                t.Insert(new Envelope(0, 0, 0, 0), new object());
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.That(true);
            }
        }

        [Test]
        public void TestQuery()
        {
            var factory = GeometryFactory.Default;
            var geometries = new List<Geometry>
            {
                factory.CreateLineString(new[] {new Coordinate(0, 0), new Coordinate(10, 10)}),
                factory.CreateLineString(new[] {new Coordinate(20, 20), new Coordinate(30, 30)}),
                factory.CreateLineString(new[] {new Coordinate(20, 20), new Coordinate(30, 30)})
            };

            var t = new HPRtree<object>(3);
            foreach (var g in geometries)
            {
                t.Insert(g.EnvelopeInternal, new object());
            }

            t.Query(new Envelope(5, 6, 5, 6));
            try
            {
                Assert.That(t.Query(new Envelope(5, 6, 5, 6)).Count, Is.EqualTo(1));
                Assert.That(t.Query(new Envelope(20, 30, 0, 10)).Count, Is.EqualTo(0));
                Assert.That(t.Query(new Envelope(25, 26, 25, 26)).Count, Is.EqualTo(2));
                Assert.That(t.Query(new Envelope(0, 100, 0, 100)).Count, Is.EqualTo(3));
            }
            catch (Exception x)
            {
                //STRtreeDemo.printSourceData(geometries, System.out);
                //STRtreeDemo.printLevels(t, System.out);
                throw x;
            }
        }

        [Test]
        public void TestQuery3()
        {
            var t = new HPRtree<object>();
            for (int i = 0; i < 3; i++)
            {
                t.Insert(new Envelope(i, i + 1, i, i + 1), i);
            }

            t.Query(new Envelope(0, 1, 0, 1));
            Assert.That(t.Query(new Envelope(1, 2, 1, 2)).Count, Is.EqualTo(3));
            Assert.That(t.Query(new Envelope(9, 10, 9, 10)).Count, Is.EqualTo(0));
        }

        [Test]
        public void TestQuery10()
        {
            var t = new HPRtree<object>();
            for (int i = 0; i < 10; i++)
            {
                t.Insert(new Envelope(i, i + 1, i, i + 1), i);
            }

            t.Query(new Envelope(0, 1, 0, 1));
            Assert.That(t.Query(new Envelope(5, 6, 5, 6)).Count, Is.EqualTo(3));
            Assert.That(t.Query(new Envelope(9, 10, 9, 10)).Count, Is.EqualTo(2));
            Assert.That(t.Query(new Envelope(25, 26, 25, 26)).Count, Is.EqualTo(0));
            Assert.That(t.Query(new Envelope(0, 10, 0, 10)).Count, Is.EqualTo(10));
        }

        [Test]
        public void TestQuery100()
        {
            QueryGrid(100, new HPRtree<object>());
        }

        [Test]
        public void TestQuery100cap8()
        {
            QueryGrid(100, new HPRtree<object>(8));
        }

        [Test]
        public void TestQuery100cap2()
        {
            QueryGrid(100, new HPRtree<object>(2));
        }

        private static void QueryGrid(int size, HPRtree<object> t)
        {
            for (int i = 0; i < size; i++)
            {
                t.Insert(new Envelope(i, i + 1, i, i + 1), i);
            }

            t.Query(new Envelope(0, 1, 0, 1));
            Assert.That(t.Query(new Envelope(5, 6, 5, 6)).Count, Is.EqualTo(3));
            Assert.That(t.Query(new Envelope(9, 10, 9, 10)).Count, Is.EqualTo(3));
            Assert.That(t.Query(new Envelope(25, 26, 25, 26)).Count, Is.EqualTo(3));
            Assert.That(t.Query(new Envelope(0, 10, 0, 10)).Count, Is.EqualTo(11));
        }
    }
}
