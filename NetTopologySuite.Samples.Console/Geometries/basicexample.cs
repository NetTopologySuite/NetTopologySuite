using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;

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
            BufferedCoordinate2DFactory coordFactory = new BufferedCoordinate2DFactory();
			
            // use the default factory, which gives full double-precision
            IGeometryFactory<BufferedCoordinate2D> geoFactory
                = new GeometryFactory<BufferedCoordinate2D>(
                    new BufferedCoordinate2DSequenceFactory(coordFactory));

			// read a point from a WKT string (using the default point factory)
            IGeometry g1 = new WktReader<BufferedCoordinate2D>(geoFactory, null).Read(
                "LINESTRING (0 0, 10 10, 20 20)");

			Console.WriteLine("Geometry 1: " + g1);

			// create a point by specifying the coordinates directly
            ICoordinate[] coordinates = new ICoordinate[] { coordFactory.Create(0, 0), 
                coordFactory.Create(10, 10), coordFactory.Create(20, 20) };

            IGeometry g2 = geoFactory.CreateLineString(coordinates);
			Console.WriteLine("Geometry 2: " + g2);
			
			// compute the intersection of the two geometries
			IGeometry g3 = g1.Intersection(g2);
			Console.WriteLine("G1 intersection G2: " + g3);
		}
	}
}