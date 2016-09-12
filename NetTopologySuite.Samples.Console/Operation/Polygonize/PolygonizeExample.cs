using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Polygonize;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Operation.Poligonize
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
		
        [Test]
        public void Test()
        {
            Run();
        }

		internal virtual void Run()
		{
			WKTReader rdr = new WKTReader();
			IList<IGeometry> lines = new List<IGeometry>
			                             {
			                                 rdr.Read("LINESTRING (0 0 , 10 10)"),
			                                 rdr.Read("LINESTRING (185 221, 100 100)"),
			                                 rdr.Read("LINESTRING (185 221, 88 275, 180 316)"),
			                                 rdr.Read("LINESTRING (185 221, 292 281, 180 316)"),
			                                 rdr.Read("LINESTRING (189 98, 83 187, 185 221)"),
			                                 rdr.Read("LINESTRING (189 98, 325 168, 185 221)")
			                             };

		    Polygonizer polygonizer = new Polygonizer();
			polygonizer.Add(lines);
			
			var polys = polygonizer.GetPolygons();
			
			Console.WriteLine("Polygons formed (" + polys.Count + "):");
            foreach(var obj in polys)
			    Console.WriteLine(obj);
		}
	}
}