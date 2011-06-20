#define buffered
using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Index.KdTree;
#if buffered
using NetTopologySuite.Tests.OperationTests.Union;
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
using coordFac = NetTopologySuite.Coordinates.BufferedCoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.BufferedCoordinateSequenceFactory;
#else
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;
using coordFac = NetTopologySuite.Coordinates.Simple.CoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.Simple.CoordinateSequenceFactory;
#endif
using NUnit.Framework;

namespace NetTopologySuite.Tests.Index
{
    [TestFixture]
    public class KdTreeTests
    {
        private readonly ICoordinateFactory<coord> _factory;
        private readonly IGeometryFactory<coord> _geoFactory;

        public KdTreeTests()
        {
            _factory = new coordFac();
            _geoFactory = new GeometryFactory<coord>(new coordSeqFac((coordFac)_factory));
        }

        [Test]
        public void Constructors()
        {
            KdTree<coord> tree = new KdTree<coord>();
            Assert.IsNotNull(tree);
            Assert.AreEqual(0.0d, tree.Tolerance);
            Assert.AreEqual(0, tree.Count);

            tree = new KdTree<coord>(1.25);
            Assert.IsNotNull(tree);
            Assert.AreEqual(1.25d, tree.Tolerance);
            Assert.AreEqual(0, tree.Count);
            Console.WriteLine("Constructors passed.");

        }
        [Test]
        public void InsertZeroTolerance()
        {
            KdTree<coord> tree = new KdTree<coord>();
            tree.Insert(_factory.Create(0, 0));
            tree.Insert(_factory.Create(1, 0));
            KdNode<coord> n1 = tree.Insert(_factory.Create(2, 0));
            KdNode<coord> n2 = tree.Insert(_factory.Create(3, 0));
            KdNode<coord> n3 = tree.Insert(_factory.Create(4, 0));
            tree.Insert(_factory.Create(5, 0));
            Assert.AreEqual(6, tree.Count);
            List<KdNode<coord>> query =
                new List<KdNode<coord>>(
                    tree.Query(_geoFactory.CreateExtents(_factory.Create(2, -1), _factory.Create(4, 1))));
            Assert.AreEqual(3, query.Count);
            Assert.IsTrue(query.Contains(n1));
            Assert.IsTrue(query.Contains(n2));
            Assert.IsTrue(query.Contains(n3));
            Console.WriteLine("InsertZeroTolerance passed.");
        }

        [Test]
        public void InsertSmallTolerance()
        {
            KdTree<coord> tree = new KdTree<coord>(0.01);
            tree.Insert(_factory.Create(0, 0));
            tree.Insert(_factory.Create(1, 0));
            tree.Insert(_factory.Create(2, 0));
            tree.Insert(_factory.Create(3, 0));
            tree.Insert(_factory.Create(3, 0));
            tree.Insert(_factory.Create(4, 0));
            tree.Insert(_factory.Create(5, 0));
            Assert.AreEqual(6, tree.Count);
            List<KdNode<coord>> query =
                new List<KdNode<coord>>(
                    tree.Query(_geoFactory.CreateExtents(_factory.Create(2, -1), _factory.Create(4, 1))));
            Assert.AreEqual(3, query.Count);
            Console.WriteLine("InsertSmallTolerance passed.");
        }

        [Test]
        public void InsertBigTolerance()
        {
            KdTree<coord> tree = new KdTree<coord>(2.00);
            tree.Insert(_factory.Create(0, 0));
            tree.Insert(_factory.Create(1, 0));
            KdNode<coord> n2 = tree.Insert(_factory.Create(3, 0));
            KdNode<coord> n1 = tree.Insert(_factory.Create(2, 0));
            KdNode<coord> n3 = tree.Insert(_factory.Create(4, 0));
            tree.Insert(_factory.Create(5, 0));
            Assert.AreEqual(4, tree.Count);
            List<KdNode<coord>> query =
                new List<KdNode<coord>>(
                    tree.Query(_geoFactory.CreateExtents(_factory.Create(2, -1), _factory.Create(4, 1))));
            Assert.AreEqual(1, query.Count);
            Assert.IsFalse(query.Contains(n1));
            Assert.IsTrue(query.Contains(n2));
            Assert.IsTrue(query.Contains(n3));
            Assert.IsTrue(n2.Coordinate.Equals(n3.Coordinate));
            Console.WriteLine("InsertBigTolerance passed.");
        }
        [Test]
        public void InsertManyPoints()
        {
            KdTree<coord> tree = new KdTree<coord>();
            IExtents<coord> extents = _geoFactory.CreateExtents();
            foreach (IGeometry<coord> geom in IOTool<coord>.ReadGeometries(_geoFactory, "sh.txt"))
            {
                IEnumerable<coord> coords = geom.GetVertexes();
                tree.InsertRange(coords);
                extents.ExpandToInclude(coords);
            }
            extents.Scale(1.0005d);
            List<KdNode<coord>> query = new List<KdNode<coord>>(tree.Query(extents));
            Assert.Less(query.Count, tree.Count);
            Console.WriteLine("InsertManyPoints passed.");
        }

    }
}
