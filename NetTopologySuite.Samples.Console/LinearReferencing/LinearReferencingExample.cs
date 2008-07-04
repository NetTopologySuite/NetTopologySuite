using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.LinearReferencing;

namespace GisSharpBlog.NetTopologySuite.Samples.LinearReferencing
{
    /// <summary>
    /// Examples of Linear Referencing
    /// </summary>
    public class LinearReferencingExample
    {
        private static IGeometryFactory fact = GeometryFactory.Fixed;
        private static WKTReader rdr = new WKTReader(fact);
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LinearReferencingExample"/> class.
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
            IGeometry g1 = rdr.Read(wkt);
            Console.WriteLine("Input Geometry: " + g1);
            Console.WriteLine("Indices to extract: " + start + " " + end);
            
            LengthIndexedLine indexedLine = new LengthIndexedLine(g1);

            IGeometry subLine = indexedLine.ExtractLine(start, end);
            Console.WriteLine("Extracted Line: " + subLine);

            double[] index = indexedLine.IndicesOf(subLine);
            Console.WriteLine("Indices of extracted line: " + index[0] + " " + index[1]);

            ICoordinate midpt = indexedLine.ExtractPoint((index[0] + index[1]) / 2);
            Console.WriteLine("Midpoint of extracted line: " + midpt);
        }
    }
}
