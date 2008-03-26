using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.LinearReferencing;
using NetTopologySuite.Coordinates;

namespace GisSharpBlog.NetTopologySuite.Samples.LinearReferencing
{
    /// <summary>
    /// Examples of Linear Referencing
    /// </summary>
    public class LinearReferencingExample
    {
        private static IGeometryFactory<BufferedCoordinate2D> fact 
            = GeometryFactory<BufferedCoordinate2D>.CreateFixedPrecision(
                new BufferedCoordinate2DSequenceFactory());

        private static WktReader<BufferedCoordinate2D> rdr
            = new WktReader<BufferedCoordinate2D>(fact, null);
        
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LinearReferencingExample"/> class.
        /// </summary>
        public LinearReferencingExample() { }

        /// <summary>
        /// 
        /// </summary>
        public void Run()      
        {
            RunExtractedLine("LINESTRING (0 0, 10 10, 20 20)", 1, 10);
            RunExtractedLine("MULTILINESTRING ((0 0, 10 10), (20 20, 25 25, 30 40))", 1, 20);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wkt"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void RunExtractedLine(string wkt, double start, double end)    
        {
            Console.WriteLine("=========================");
            IGeometry<BufferedCoordinate2D> g1 = rdr.Read(wkt) as IGeometry<BufferedCoordinate2D>;
            Console.WriteLine("Input Geometry: " + g1);
            Console.WriteLine("Indices to extract: " + start + " " + end);

            LengthIndexedLine<BufferedCoordinate2D> indexedLine 
                = new LengthIndexedLine<BufferedCoordinate2D>(g1);

            IGeometry<BufferedCoordinate2D> subLine = indexedLine.ExtractLine(start, end);
            Console.WriteLine("Extracted Line: " + subLine);

            double[] index = indexedLine.IndicesOf(subLine);
            Console.WriteLine("Indices of extracted line: " + index[0] + " " + index[1]);

            BufferedCoordinate2D midpt = indexedLine.ExtractPoint((index[0] + index[1]) / 2);
            Console.WriteLine("Midpoint of extracted line: " + midpt);
        }
    }
}
