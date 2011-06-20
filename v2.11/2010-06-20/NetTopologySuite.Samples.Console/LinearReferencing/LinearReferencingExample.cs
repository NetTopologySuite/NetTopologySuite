using System;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.LinearReferencing;
using NetTopologySuite.Coordinates;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.LinearReferencing
{
    /// <summary>
    /// Examples of Linear Referencing
    /// </summary>
    [TestFixture]
    public class LinearReferencingExample
    {
        private static readonly IGeometryFactory<BufferedCoordinate> _factory
            = new GeometryFactory<BufferedCoordinate>(
                new BufferedCoordinateSequenceFactory(
                    new BufferedCoordinateFactory(1.0)));

        private static readonly WktReader<BufferedCoordinate> _reader
            = new WktReader<BufferedCoordinate>(_factory);

        [Test]
        public void Run()
        {
            RunExtractedLine("LINESTRING (0 0, 10 10, 20 20)", 1, 10);
            RunExtractedLine("MULTILINESTRING ((0 0, 10 10), (20 20, 25 25, 30 40))", 1, 20);
        }

        public void RunExtractedLine(String wkt, Double start, Double end)
        {
            Console.WriteLine("=========================");
            IGeometry<BufferedCoordinate> g1 = _reader.Read(wkt);
            Console.WriteLine("Input Geometry: " + g1);
            Console.WriteLine("Indices to extract: " + start + " " + end);

            LengthIndexedLine<BufferedCoordinate> indexedLine
                = new LengthIndexedLine<BufferedCoordinate>(g1);

            IGeometry<BufferedCoordinate> subLine = indexedLine.ExtractLine(start, end);
            Console.WriteLine("Extracted Line: " + subLine);

            Double[] index = indexedLine.IndicesOf(subLine);
            Console.WriteLine("Indices of extracted line: " + index[0] + " " + index[1]);

            BufferedCoordinate midpt = indexedLine.ExtractPoint((index[0] + index[1])/2);
            Console.WriteLine("Midpoint of extracted line: " + midpt);
        }
    }
}