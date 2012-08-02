using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Polygonize;

namespace NetTopologySuite.Samples.Operation.Poligonize
{

    /*
     * based on
     * http://blog.opengeo.org/2012/06/21/splitpolygon-wps-process-p1/
     * http://blog.opengeo.org/2012/07/24/splitpolygon-wps-process-p2/
     * 
     * and
     * https://github.com/mdavisog/wps-splitpoly
     * 
     * and of course
     * http://sourceforge.net/mailarchive/forum.php?thread_name=CAK2ens3FY3qMT915_LoRiz6uqyww156swONSWRRaXc0anrxREg%40mail.gmail.com&forum_name=jts-topo-suite-user
     */
    public class SplitPolygonExample
    {
	    internal static IGeometry Polygonize(IGeometry geometry)
	    {
	        var lines = LineStringExtracter.GetLines(geometry);
            var polygonizer = new Polygonizer();
            polygonizer.Add(lines);
            var polys = polygonizer.GetPolygons();
            var polyArray = GeometryFactory.ToGeometryArray(polys);
            return geometry.Factory.CreateGeometryCollection(polyArray);
	    }        
        
        internal static IGeometry SplitPolygon(IGeometry polygon, IGeometry line)
        {
            var nodedLinework = polygon.Boundary.Union(line);
            var polygons = Polygonize(nodedLinework);

            // only keep polygons which are inside the input
            var output = new List<IGeometry>();
            for (var i = 0; i < polygons.NumGeometries; i++)
            {
                var candpoly = (Polygon) polygons.GetGeometryN(i);
                if (polygon.Contains(candpoly.InteriorPoint))
                    output.Add(candpoly);
            }
            /*
            return polygon.Factory.CreateGeometryCollection(
                GeometryFactory.ToGeometryArray(output));
             */
            return polygon.Factory.BuildGeometry(output);
        }

		[STAThread]
		public static void Main(string[] args)
		{
			var test = new PolygonizeExample();
			try
			{
				test.Run();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.StackTrace);
			}
		}
		
        [Test]
        public void Test()
        {
            Run();
        }

        internal void Run()
        {
            var reader = new WKTReader();
            var polygon = reader.Read("POLYGON((0 0, 0 100, 100 100, 100 0, 0 0), (10 10, 90 10, 90 90, 10 90, 10 10))");

            var lineWkts = new[] {
                "LINESTRING(50 -10, 50 110)", 
                "LINESTRING(5 -10, 5 110)",
                "LINESTRING(5 -10, 5 95, 110 95)", 
                "LINESTRING(5 -10, 5 110, 110 50)" };

            Console.WriteLine(string.Format("Splitting\n{0}", polygon));
            foreach (var lineWkt in lineWkts)
            {
                var line = reader.Read(lineWkt);
                Console.WriteLine(string.Format("\nwith\n{0}", lineWkt));
                var splitPolygons = SplitPolygon(polygon, line);
                Console.WriteLine(string.Format("results in:\n{0}", splitPolygons));
            }
        }

        /*
	public static Geometry splitPolygon(Geometry poly, Geometry line)
    {		
        Geometry nodedLinework = poly.getBoundary().union(line);
        Geometry polys = polygonize(nodedLinework);
        
        // only keep polygons which are inside the input
        List output = new ArrayList();
        for (int i = 0; i < polys.getNumGeometries(); i++)
        {
            Polygon candpoly = (Polygon) polys.getGeometryN(i);
            if (poly.contains(candpoly.getInteriorPoint()))
            {
                output.add(candpoly);
            }
        }	
        return poly.getFactory().createGeometryCollection(GeometryFactory.toGeometryArray(output));	} */
    }
}