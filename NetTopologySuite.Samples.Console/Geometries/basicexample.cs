using System;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

namespace GisSharpBlog.NetTopologySuite.Samples.Geometries
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
			Geometry g1 = new WKTReader().Read("LINESTRING (0 0, 10 10, 20 20)");
			Console.WriteLine("Geometry 1: " + g1);
			
			// create a point by specifying the coordinates directly
			Coordinate[] coordinates = new Coordinate[] { new Coordinate(0, 0), 
                new Coordinate(10, 10), new Coordinate(20, 20) };
			// use the default factory, which gives full double-precision
			Geometry g2 = new GeometryFactory().CreateLineString(coordinates);
			Console.WriteLine("Geometry 2: " + g2);
			
			// compute the intersection of the two geometries
			Geometry g3 = g1.Intersection(g2);
			Console.WriteLine("G1 intersection G2: " + g3);
		}
	}
}