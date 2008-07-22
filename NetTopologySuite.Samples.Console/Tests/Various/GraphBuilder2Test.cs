using System;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Samples.Tests.Various;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class GraphBuilder2Test
    {
        private IGeometryFactory factory;
        private ILineString a, b, c, d, e;
        private ILineString result, revresult;
        private IPoint start, end;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            Environment.CurrentDirectory = Path.Combine(
               AppDomain.CurrentDomain.BaseDirectory,
               @"../../../NetTopologySuite.Samples.Shapefiles");
        }

        [SetUp]
        public void Setup()
        {
            factory = GeometryFactory.Fixed;

            a = factory.CreateLineString(new ICoordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(100, 0),
                new Coordinate(200, 100),
                new Coordinate(200, 200), 
            });
            b = factory.CreateLineString(new ICoordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(100, 100),
                new Coordinate(200, 200),
            });
            c = factory.CreateLineString(new ICoordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(0, 100),
                new Coordinate(100, 200),
                new Coordinate(200, 200),
            });
            d = factory.CreateLineString(new ICoordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(300, 0),
                new Coordinate(300, 200),
                new Coordinate(150, 200),
                new Coordinate(150, 300),
            });
            e = factory.CreateLineString(new ICoordinate[]
            {
                new Coordinate(100, 300),
                new Coordinate(150, 300),
                new Coordinate(200, 300),
            });

            result = factory.CreateLineString(new ICoordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(300, 0),
                new Coordinate(300, 200),
                new Coordinate(150, 200),
                new Coordinate(150, 300),
            });
            revresult = result.Reverse();

            start = a.StartPoint;
            end = d.EndPoint;
        }
        
        [Test]
        public void TestGraphBuilder2WithSampleGeometries()
        {
            GraphBuilder2 builder = new GraphBuilder2();
            builder.Add(a);
            builder.Add(b, c);
            builder.Add(d);
            builder.Add(e);
            builder.Initialize();

            int src = builder.EdgeAtLocation(start);
            Assert.Greater(src, -1);
            int dst = builder.EdgeAtLocation(end);
            Assert.Greater(dst, -1);

            ILineString path = builder.perform(src, dst);
            Assert.IsNotNull(path);
            Assert.AreEqual(result, path);
        }

        [Test]
        public void TestBidirectionalGraphBuilder2WithSampleGeometries()
        {
            GraphBuilder2 builder = new GraphBuilder2(true);
            builder.Add(a);
            builder.Add(b, c);
            builder.Add(d);
            builder.Add(e);
            builder.Initialize();

            int src = builder.EdgeAtLocation(start);
            Assert.Greater(src, -1);
            int dst = builder.EdgeAtLocation(end);
            Assert.Greater(dst, -1);

            ILineString path = builder.perform(dst, src);
            Assert.IsNotNull(path);
            Assert.AreEqual(revresult, path);
        }

        [Test]
        [ExpectedException(typeof(TopologyException))]
        public void CheckGraphBuilder2ExceptionUsingNoGeometries()
        {
            GraphBuilder2 builder = new GraphBuilder2();
            builder.Initialize();
        }

        [Test]
        [ExpectedException(typeof(TopologyException))]
        public void CheckGraphBuilder2ExceptionUsingOneGeometry()
        {
            GraphBuilder2 builder = new GraphBuilder2();
            Assert.IsTrue(builder.Add(a));
            builder.Initialize();
        }

        [Test]
        [ExpectedException(typeof(TopologyException))]
        public void CheckGraphBuilder2ExceptionUsingARepeatedGeometry()
        {
            GraphBuilder2 builder = new GraphBuilder2();
            Assert.IsTrue(builder.Add(a));
            Assert.IsFalse(builder.Add(a));
            Assert.IsFalse(builder.Add(a, a));
            builder.Initialize();
        }

        [Test]
        [ExpectedException(typeof(TopologyException))]
        public void CheckGraphBuilder2ExceptionUsingDifferentFactories()
        {
            GraphBuilder2 builder = new GraphBuilder2();
            Assert.IsTrue(builder.Add(a));
            Assert.IsTrue(builder.Add(b, c));
            Assert.IsTrue(builder.Add(d));
            builder.Add(GeometryFactory.Default.CreateLineString(new ICoordinate[]
            {
                new Coordinate(0 ,0),
                new Coordinate(50 , 50),
            }));
        }
    }
}
