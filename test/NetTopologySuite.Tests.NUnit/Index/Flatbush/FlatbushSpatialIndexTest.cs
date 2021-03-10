using System;
using System.Collections.Generic;

using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Flatbush;

using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index.Flatbush
{
    public sealed class FlatbushSpatialIndexTest
    {
        [Test]
        public void TestSpatialIndex()
        {
            var tester = new SpatialIndexTester();
            tester.SpatialIndex = new FlatbushSpatialIndex<object>(tester.ItemCount);
            tester.Init();
            tester.Run();
            Assert.That(tester.IsSuccess);
        }

        [Test]
        public void TestDisallowedInserts()
        {
            var t = new FlatbushSpatialIndex<object>(2);
            t.Insert(new Envelope(0, 0, 0, 0), new object());
            t.Insert(new Envelope(0, 0, 0, 0), new object());
            t.Query(new Envelope());
            Assert.That(() => t.Insert(new Envelope(0, 0, 0, 0), new object()), Throws.InstanceOf<InvalidOperationException>());
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

            var t = new FlatbushSpatialIndex<object>(3);
            foreach (var g in geometries)
            {
                t.Insert(g.EnvelopeInternal, new object());
            }

            t.Build();
            Assert.That(t.Query(new Envelope(5, 6, 5, 6)).Count, Is.EqualTo(1));
            Assert.That(t.Query(new Envelope(20, 30, 0, 10)).Count, Is.EqualTo(0));
            Assert.That(t.Query(new Envelope(25, 26, 25, 26)).Count, Is.EqualTo(2));
            Assert.That(t.Query(new Envelope(0, 100, 0, 100)).Count, Is.EqualTo(3));
        }

        [Test]
        public void TestQuery3()
        {
            var t = new FlatbushSpatialIndex<object>(3);
            for (int i = 0; i < 3; i++)
            {
                t.Insert(new Envelope(i, i + 1, i, i + 1), i);
            }

            t.Build();
            Assert.That(t.Query(new Envelope(1, 2, 1, 2)).Count, Is.EqualTo(3));
            Assert.That(t.Query(new Envelope(9, 10, 9, 10)).Count, Is.EqualTo(0));
        }

        [Test]
        public void TestQuery10()
        {
            var t = new FlatbushSpatialIndex<object>(10);
            for (int i = 0; i < 10; i++)
            {
                t.Insert(new Envelope(i, i + 1, i, i + 1), i);
            }

            t.Build();
            Assert.That(t.Query(new Envelope(5, 6, 5, 6)).Count, Is.EqualTo(3));
            Assert.That(t.Query(new Envelope(9, 10, 9, 10)).Count, Is.EqualTo(2));
            Assert.That(t.Query(new Envelope(25, 26, 25, 26)).Count, Is.EqualTo(0));
            Assert.That(t.Query(new Envelope(0, 10, 0, 10)).Count, Is.EqualTo(10));
        }

        [TestCase(2)]
        [TestCase(100)]
        [TestCase(1000)]
        public void TestQuery100(int nodeSize)
        {
            var t = new FlatbushSpatialIndex<object>(100, nodeSize);
            for (int i = 0; i < 100; i++)
            {
                t.Insert(new Envelope(i, i + 1, i, i + 1), i);
            }

            t.Build();
            Assert.That(t.Query(new Envelope(5, 6, 5, 6)).Count, Is.EqualTo(3));
            Assert.That(t.Query(new Envelope(9, 10, 9, 10)).Count, Is.EqualTo(3));
            Assert.That(t.Query(new Envelope(25, 26, 25, 26)).Count, Is.EqualTo(3));
            Assert.That(t.Query(new Envelope(0, 10, 0, 10)).Count, Is.EqualTo(11));
        }

        [Test]
        public void TestQueryWithVisitor()
        {
            var t = new FlatbushSpatialIndex<object>(100);
            for (int i = 0; i < 100; i++)
            {
                t.Insert(new Envelope(i, i + 1, i, i + 1), i);
            }

            t.Build();
            Assert.That(ResultCount(new Envelope(5, 6, 5, 6)), Is.EqualTo(3));
            Assert.That(ResultCount(new Envelope(9, 10, 9, 10)), Is.EqualTo(3));
            Assert.That(ResultCount(new Envelope(25, 26, 25, 26)), Is.EqualTo(3));
            Assert.That(ResultCount(new Envelope(0, 10, 0, 10)), Is.EqualTo(11));

            int ResultCount(Envelope env)
            {
                int cnt = 0;
                var vis = new AnonymousSpatialIndexVisitor<object>(_ => ++cnt);
                t.Query(env, vis);
                return cnt;
            }
        }

        private sealed class AnonymousSpatialIndexVisitor<T> : IItemVisitor<T>
        {
            private readonly Action<T> _callback;

            public AnonymousSpatialIndexVisitor(Action<T> callback)
            {
                _callback = callback;
            }

            public void VisitItem(T item)
            {
                _callback(item);
            }
        }
    }
}
