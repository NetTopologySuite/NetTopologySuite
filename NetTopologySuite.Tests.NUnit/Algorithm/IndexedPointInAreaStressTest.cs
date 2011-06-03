using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class IndexedPointInAreaStressTest
    {
        [Test]
        public void TestGrid()
        {
            // Use fixed PM to try and get at least some points hitting the boundary
            IGeometryFactory geomFactory = GeometryFactory.Fixed;
            //		GeometryFactory geomFactory = new GeometryFactory();

            var gridBuilder = new PerturbedGridPolygonBuilder(geomFactory)
                                  {
                                      NumLines = 20,
                                      LineWidth = 10.0,
                                      Seed = 118507219
                                  };
            //gridBuilder.SetSeed(1185072199562);
            IGeometry area = gridBuilder.Geometry;

            //    PointInAreaLocator pia = new IndexedPointInAreaLocator(area); 
            IPointOnGeometryLocator pia = new IndexedPointInAreaLocator(area);

            var gridTester = new PointInAreaStressTester(geomFactory, area)
                                 {
                                     NumPoints = 100000,
                                     TestPointInAreaLocator = pia
                                 };

            Boolean isCorrect = gridTester.Run();
            Assert.IsTrue(isCorrect);
        }
    }

}