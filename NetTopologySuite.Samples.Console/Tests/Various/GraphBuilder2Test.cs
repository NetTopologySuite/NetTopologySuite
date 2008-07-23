using System;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
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

        /// <summary>
        /// Loads the shapefile as a graph allowing SP analysis to be carried out
        /// </summary>
        /// <param name="fileName">The name of the shape file we want to load</param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public ILineString TestGraphBuilder2WithSampleGeometries(string fileName, int src, int dst)
        {
            ShapefileReader reader = new ShapefileReader(fileName);
            IGeometryCollection edges = reader.ReadAll();
            return TestGraphBuilder2WithSampleGeometries(edges, src, dst);
        }

        /// <summary>
        /// Uses the passed geometry collection to generate a QuickGraph.
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public ILineString TestGraphBuilder2WithSampleGeometries(IGeometryCollection edges, int src, int dst)
        {
            GraphBuilder2 builder = new GraphBuilder2(true);            
            foreach (IMultiLineString edge in edges.Geometries)
                foreach (ILineString line in edge.Geometries)                                
                    builder.Add(line);            
            builder.Initialize();

            return builder.perform(src, dst);
        }

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

            int src = builder.VertexAtLocation(start);
            Assert.Greater(src, -1);
            int dst = builder.VertexAtLocation(end);
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

            int src = builder.VertexAtLocation(start);
            Assert.Greater(src, -1);
            int dst = builder.VertexAtLocation(end);
            Assert.Greater(dst, -1);


            ILineString path = builder.perform(src, dst);
            Assert.IsNotNull(path);
            Assert.AreEqual(result, path);

            ILineString revpath = builder.perform(dst, src);
            Assert.IsNotNull(revpath);
            Assert.AreEqual(revresult, revpath);
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

        [Test]
        [ExpectedException(typeof(ApplicationException))]
        public void CheckGraphBuilder2ExceptionUsingDoubleInitialization()
        {
            GraphBuilder2 builder = new GraphBuilder2();
            builder.Add(a);
            builder.Add(b, c);
            builder.Add(d);
            builder.Add(e);
            builder.Initialize();
            builder.Initialize();
        }

        [Test]
        public void BuildGraphFromMinimalGraphShapefile()
        {
            string shapepath = "minimalgraph.shp";
            int count = 15;

            Assert.IsTrue(File.Exists(shapepath));
            ShapefileReader reader = new ShapefileReader(shapepath);
            IGeometryCollection edges = reader.ReadAll();
            Assert.IsNotNull(edges);
            Assert.IsInstanceOfType(typeof(GeometryCollection), edges);
            Assert.AreEqual(count, edges.NumGeometries);

            ILineString startls = edges.GetGeometryN(0).GetGeometryN(0) as ILineString;
            Assert.IsNotNull(startls);
            ILineString endls = edges.GetGeometryN(5).GetGeometryN(0) as ILineString; ;
            Assert.IsNotNull(endls);

            GraphBuilder2 builder = new GraphBuilder2(true);
            foreach (IMultiLineString mlstr in edges.Geometries)
            {
                Assert.AreEqual(1, mlstr.NumGeometries);
                ILineString str = mlstr.GetGeometryN(0) as ILineString;
                Assert.IsNotNull(str);
                Assert.IsTrue(builder.Add(str));
            }
            builder.Initialize();

            int src = builder.VertexAtLocation(startls.StartPoint);
            int dst = builder.VertexAtLocation(endls.EndPoint);
            ILineString path = builder.perform(src, dst);
            Assert.IsNotNull(path);
        }
    }
}
