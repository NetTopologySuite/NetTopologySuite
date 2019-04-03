using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Samples.Geometries
{
    /// <summary>
    /// Shows basic ways of creating and operating on geometries
    /// </summary>
    public class BasicExample
    {
        [STAThread]
        public static void main(string[] args)
        {
            // read a point from a WKT string (using the default point factory)
            var g1 = new WKTReader().Read("LINESTRING (0 0, 10 10, 20 20)");
            Console.WriteLine("Geometry 1: " + g1);

            // create a point by specifying the coordinates directly
            var coordinates = new [] { new Coordinate(0, 0),
                new Coordinate(10, 10), new Coordinate(20, 20) };
            // use the default factory, which gives full double-precision
            IGeometry g2 = new GeometryFactory().CreateLineString(coordinates);
            Console.WriteLine("Geometry 2: " + g2);

            // compute the intersection of the two geometries
            var g3 = g1.Intersection(g2);
            Console.WriteLine("G1 intersection G2: " + g3);

            // create a point
            var point = new GeometryFactory().CreatePoint(new Coordinate(1, 1));
            Console.WriteLine("Point Geometry: " + point);

            // compute whether point is on g1
            Console.WriteLine("Point within g1: " + g1.Contains(point));
        }
    }
}
