using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NetTopologySuite.Coordinates;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class CoversTest : BaseSamples
    {
        private readonly IPolygon polygon1;
        private readonly IPolygon polygon2;


        public CoversTest()
        {
            IGeometryFactory geoFactory = new GeometryFactory<BufferedCoordinate2D>(
                new BufferedCoordinate2DSequenceFactory());

            ICoordinate[] array1 = null;
            ICoordinate[] array2 = null;

            createCoordinates(ref array1, ref array2);

            polygon1 = geoFactory.CreatePolygon(array1);
            polygon2 = geoFactory.CreatePolygon(array2);
        }

        [Test]
        public void CoversTestTest()
        {
            Assert.IsTrue(polygon2.Within(polygon1));
            Assert.IsTrue(polygon1.Covers(polygon2));
            Assert.IsFalse(polygon1.CoveredBy(polygon2));
        }

        private static void createCoordinates(ref ICoordinate[] array1, ref ICoordinate[] array2)
        {
            BufferedCoordinate2DFactory coordFactory = new BufferedCoordinate2DFactory();

            array1 = new ICoordinate[]
                     {
                         coordFactory.Create(10, 10),
                         coordFactory.Create(50, 10),
                         coordFactory.Create(50, 50),
                         coordFactory.Create(10, 50),
                         coordFactory.Create(10, 10),
                     };

            array2 = new ICoordinate[]
                     {
                         coordFactory.Create(11, 11),
                         coordFactory.Create(20, 11),
                         coordFactory.Create(20, 20),
                         coordFactory.Create(11, 20),
                         coordFactory.Create(11, 11),
                     };
        }
    }
}