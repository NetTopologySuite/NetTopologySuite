using System;
using System.Collections;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Operation.Polygonize;

namespace GisSharpBlog.NetTopologySuite.Samples.Operation.Poligonize
{
	/// <summary>  
    /// Example of using Polygonizer class to polygonize a set of fully noded linestrings.
	/// </summary>	
	public class PolygonizeExample
	{
		[STAThread]
		public static void main(string[] args)
		{
			PolygonizeExample test = new PolygonizeExample();
			try
			{
				test.Run();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.StackTrace);
			}
		}
		
		
		public PolygonizeExample() { }
		
		internal virtual void Run()
		{
			WKTReader rdr = new WKTReader();
			IList lines = new ArrayList();
			
			lines.Add(rdr.Read("LINESTRING (0 0 , 10 10)"));            // isolated edge
            lines.Add(rdr.Read("LINESTRING (185 221, 100 100)"));       //dangling edge
            lines.Add(rdr.Read("LINESTRING (185 221, 88 275, 180 316)"));
            lines.Add(rdr.Read("LINESTRING (185 221, 292 281, 180 316)"));
            lines.Add(rdr.Read("LINESTRING (189 98, 83 187, 185 221)"));
            lines.Add(rdr.Read("LINESTRING (189 98, 325 168, 185 221)"));
			
			Polygonizer polygonizer = new Polygonizer();
			polygonizer.Add(lines);
			
			ICollection polys = polygonizer.Polygons;
			
			Console.WriteLine("Polygons formed (" + polys.Count + "):");
            foreach(object obj in polys)
			    Console.WriteLine(obj);
		}
	}
}