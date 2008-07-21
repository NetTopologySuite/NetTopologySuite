using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class GraphTest2
    {
        private IGeometryFactory factory;
        private ILineString a, b, c, d;
        
        [TestFixtureSetUp]
        public void FixtureSetup()
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
        }       
        
        [Test]
        public void TestGraphBuilder2()
        {            
            GraphBuilder2 builder = new GraphBuilder2();
            builder.Add(a);
            builder.Add(b, c);
            builder.Add(d);

            bool algorithm = builder.PrepareAlgorithm();
            Assert.IsTrue(algorithm);

            int src = builder.EdgeAtLocation(new Coordinate(0, 0));
            Assert.Greater(src, -1);
            int dst = builder.EdgeAtLocation(new Coordinate(150, 300));            
            Assert.Greater(dst, -1);

            ILineString path = builder.perform(src, dst); 
            Assert.IsNotNull(path);
        }
    }
}
