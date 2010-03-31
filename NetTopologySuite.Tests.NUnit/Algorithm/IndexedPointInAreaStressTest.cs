using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm.Locate;
using GisSharpBlog.NetTopologySuite.Geometries;
using NUnit.Framework;
using Coord = NetTopologySuite.Coordinates.Simple.Coordinate;
using CoordFac = NetTopologySuite.Coordinates.Simple.CoordinateFactory;
using CoordSeqFac = NetTopologySuite.Coordinates.Simple.CoordinateSequenceFactory;
using PrecisionModel = NetTopologySuite.Coordinates.Simple.PrecisionModel;
namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class IndexedPointInAreaStressTest
    {
        readonly CoordSeqFac _coordSeqFac = new CoordSeqFac(new CoordFac(1.0)); 


        [Test]
        public void TestGrid()
        {
            // Use fixed PM to try and get at least some points hitting the boundary
            IGeometryFactory<Coord> geomFactory = new GeometryFactory<Coord>(_coordSeqFac);
            //		GeometryFactory geomFactory = new GeometryFactory();

            PerturbedGridPolygonBuilder gridBuilder = new PerturbedGridPolygonBuilder(geomFactory);
            gridBuilder.SetNumLines(20);
            gridBuilder.SetLineWidth(10.0);
            gridBuilder.SetSeed(118507219);
            //gridBuilder.SetSeed(1185072199562);
            IGeometry<Coord> area = gridBuilder.Geometry;

            //    PointInAreaLocator pia = new IndexedPointInAreaLocator(area); 
            IPointOnGeometryLocator<Coord> pia = new IndexedPointInAreaLocator<Coord>(area);

            PointInAreaStressTester gridTester = new PointInAreaStressTester(geomFactory, area);
            gridTester.SetNumPoints(100000);
            gridTester.setPIA(pia);

            Boolean isCorrect = gridTester.Run();
            Assert.IsTrue(isCorrect);
        }
    }
}