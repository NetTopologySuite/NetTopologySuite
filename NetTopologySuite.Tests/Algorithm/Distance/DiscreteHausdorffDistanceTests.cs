using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm.Distance;
using GisSharpBlog.NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;
using NetTopologySuite.Coordinates.Simple;
using Xunit;

namespace NetTopologySuite.Tests.Algorithm.Distance
{
    public class DiscreteHausdorffDistanceTests
    {
        private static IGeometryFactory<BufferedCoordinate> _coordFact =
            new GeometryFactory<BufferedCoordinate>(new BufferedCoordinateSequenceFactory());

        //Polygon - Polygon
        [Fact]
        public void HausdorffDistancePolyPoly()
        {
            IGeometry<BufferedCoordinate> geom0 = _coordFact.WktReader.Read("POLYGON((0 0, 0 2, 1 2, 2 2, 2 0, 0 0))");
            IGeometry<BufferedCoordinate> geom1 = _coordFact.WktReader.Read("POLYGON((0.5 0.5, 0.5 2.5, 1.5 2.5, 2.5 2.5, 2.5 0.5, 0.5 0.5))");

            Double dist = DiscreteHausdorffDistance<BufferedCoordinate>.Distance(geom0, geom1);
            Console.WriteLine(string.Format("HausdorffDistancePolyPoly: {0}", dist));
            Assert.True(Math.Abs(dist - 0.707106781186548d) < 1e-7d);
        }

        //linestring and linestring 
        [Fact]
        public void HausdorffDistanceLineStringLineString1()
        {
            IGeometry<BufferedCoordinate> geom0 = _coordFact.WktReader.Read("LINESTRING (0 0, 2 1)");
            IGeometry<BufferedCoordinate> geom1 = _coordFact.WktReader.Read("LINESTRING (0 0, 2 0)");

            Double dist = DiscreteHausdorffDistance<BufferedCoordinate>.Distance(geom0, geom1);
            Console.WriteLine(string.Format("HausdorffDistanceLineStringLineString1: {0}", dist));
            Assert.True(Math.Abs(dist - 1d) < 1e-7d);
        }

        //linestring and other linestring 
        [Fact]
        public void HausdorffDistanceLineStringLineString2()
        {
            IGeometry<BufferedCoordinate> geom0 = _coordFact.WktReader.Read("LINESTRING (0 0, 2 0)");
            IGeometry<BufferedCoordinate> geom1 = _coordFact.WktReader.Read("LINESTRING (0 1, 1 2, 2 1)");

            Double dist = DiscreteHausdorffDistance<BufferedCoordinate>.Distance(geom0, geom1);
            Console.WriteLine(string.Format("HausdorffDistanceLineStringLineString2: {0}", dist));
            Assert.True(Math.Abs(dist - 2d) < 1e-7d);
        }

        //linestring and multipoint
        [Fact]
        public void HausdorffDistanceLineStringMultiPoint()
        {
            IGeometry<BufferedCoordinate> geom0 = _coordFact.WktReader.Read("LINESTRING (0 0, 2 1)");
            IGeometry<BufferedCoordinate> geom1 = _coordFact.WktReader.Read("MULTIPOINT (0 1, 1 0, 2 1)");

            Double dist = DiscreteHausdorffDistance<BufferedCoordinate>.Distance(geom0, geom1);
            Console.WriteLine(string.Format("HausdorffDistanceLineStringMultiPoint: {0}", dist));
            Assert.True(Math.Abs(dist - 1d) < 1e-7d);
        }

        //another linestring and linestring 
        [Fact]
        public void HausdorffDistanceLineStringLineString3()
        {
            IGeometry<BufferedCoordinate> geom0 = _coordFact.WktReader.Read("LINESTRING (130 0, 0 0, 0 150)");
            IGeometry<BufferedCoordinate> geom1 = _coordFact.WktReader.Read("LINESTRING (10 10, 10 150, 130 10)");

            Double dist = DiscreteHausdorffDistance<BufferedCoordinate>.Distance(geom0, geom1);
            Console.WriteLine(string.Format("HausdorffDistanceLineStringLineString3: {0}", dist));
            Assert.True(Math.Abs(dist - 14.142135623730951d) < 1e-7d);
        }

        //hausdorf with densification 
        [Fact]
        public void HausdorffDistanceLineStringLineStringWithDensityFraction()
        {
            IGeometry<BufferedCoordinate> geom0 = _coordFact.WktReader.Read("LINESTRING (130 0, 0 0, 0 150)");
            IGeometry<BufferedCoordinate> geom1 = _coordFact.WktReader.Read("LINESTRING (10 10, 10 150, 130 10)");

            Double dist = DiscreteHausdorffDistance<BufferedCoordinate>.Distance(geom0, geom1, 0.5d);
            Console.WriteLine(string.Format("HausdorffDistanceLineStringLineStringWithDensityFraction: {0}", dist));
            Assert.True(Math.Abs(dist - 70) < 1e-7d);
        }

    }

    public class DiscreteHausdorffDistanceTestsSimple
    {
        private static IGeometryFactory<Coordinate> _coordFact =
            new GeometryFactory<Coordinate>(new CoordinateSequenceFactory());

        //Polygon - Polygon
        [Fact]
        public void HausdorffDistancePolyPoly()
        {
            IGeometry<Coordinate> geom0 = _coordFact.WktReader.Read("POLYGON((0 0, 0 2, 1 2, 2 2, 2 0, 0 0))");
            IGeometry<Coordinate> geom1 = _coordFact.WktReader.Read("POLYGON((0.5 0.5, 0.5 2.5, 1.5 2.5, 2.5 2.5, 2.5 0.5, 0.5 0.5))");

            Double dist = DiscreteHausdorffDistance<Coordinate>.Distance(geom0, geom1);
            Console.WriteLine(string.Format("HausdorffDistancePolyPoly: {0}", dist));
            Assert.True(Math.Abs(dist - 0.707106781186548d) < 1e-7d);
        }

        //linestring and linestring 
        [Fact]
        public void HausdorffDistanceLineStringLineString1()
        {
            IGeometry<Coordinate> geom0 = _coordFact.WktReader.Read("LINESTRING (0 0, 2 1)");
            IGeometry<Coordinate> geom1 = _coordFact.WktReader.Read("LINESTRING (0 0, 2 0)");

            Double dist = DiscreteHausdorffDistance<Coordinate>.Distance(geom0, geom1);
            Console.WriteLine(string.Format("HausdorffDistanceLineStringLineString1: {0}", dist));
            Assert.True(Math.Abs(dist - 1d) < 1e-7d);
        }

        //linestring and other linestring 
        [Fact]
        public void HausdorffDistanceLineStringLineString2()
        {
            IGeometry<Coordinate> geom0 = _coordFact.WktReader.Read("LINESTRING (0 0, 2 0)");
            IGeometry<Coordinate> geom1 = _coordFact.WktReader.Read("LINESTRING (0 1, 1 2, 2 1)");

            Double dist = DiscreteHausdorffDistance<Coordinate>.Distance(geom0, geom1);
            Console.WriteLine(string.Format("HausdorffDistanceLineStringLineString2: {0}", dist));
            Assert.True(Math.Abs(dist - 2d) < 1e-7d);
        }

        //linestring and multipoint
        [Fact]
        public void HausdorffDistanceLineStringMultiPoint()
        {
            IGeometry<Coordinate> geom0 = _coordFact.WktReader.Read("LINESTRING (0 0, 2 1)");
            IGeometry<Coordinate> geom1 = _coordFact.WktReader.Read("MULTIPOINT (0 1, 1 0, 2 1)");

            Double dist = DiscreteHausdorffDistance<Coordinate>.Distance(geom0, geom1);
            Console.WriteLine(string.Format("HausdorffDistanceLineStringMultiPoint: {0}", dist));
            Assert.True(Math.Abs(dist - 1d) < 1e-7d);
        }

        //another linestring and linestring 
        [Fact]
        public void HausdorffDistanceLineStringLineString3()
        {
            IGeometry<Coordinate> geom0 = _coordFact.WktReader.Read("LINESTRING (130 0, 0 0, 0 150)");
            IGeometry<Coordinate> geom1 = _coordFact.WktReader.Read("LINESTRING (10 10, 10 150, 130 10)");

            Double dist = DiscreteHausdorffDistance<Coordinate>.Distance(geom0, geom1);
            Console.WriteLine(string.Format("HausdorffDistanceLineStringLineString3: {0}", dist));
            Assert.True(Math.Abs(dist - 14.142135623730951d) < 1e-7d);
        }

        //hausdorf with densification 
        [Fact]
        public void HausdorffDistanceLineStringLineStringWithDensityFraction()
        {
            IGeometry<Coordinate> geom0 = _coordFact.WktReader.Read("LINESTRING (130 0, 0 0, 0 150)");
            IGeometry<Coordinate> geom1 = _coordFact.WktReader.Read("LINESTRING (10 10, 10 150, 130 10)");

            Double dist = DiscreteHausdorffDistance<Coordinate>.Distance(geom0, geom1, 0.5d);
            Console.WriteLine(string.Format("HausdorffDistanceLineStringLineStringWithDensityFraction: {0}", dist));
            Assert.True(Math.Abs(dist - 70) < 1e-7d);
        }

    }

}
