using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Operation.Linemerge;

namespace GisSharpBlog.NetTopologySuite.Samples.Operation.Linemerge
{
	/// <summary> 
    /// Example of using the LineMerger class to sew together a set of fully noded 
	/// linestrings.
	/// </summary>	
	public class LineMergeExample
	{
		private void InitBlock()
		{
			reader = new WKTReader();
		}

		virtual internal IList Data
		{
			get
			{
				IList lines = new ArrayList();
				lines.Add(Read("LINESTRING (220 160, 240 150, 270 150, 290 170)"));
				lines.Add(Read("LINESTRING (60 210, 30 190, 30 160)"));
				lines.Add(Read("LINESTRING (70 430, 100 430, 120 420, 140 400)"));
				lines.Add(Read("LINESTRING (160 310, 160 280, 160 250, 170 230)"));
				lines.Add(Read("LINESTRING (170 230, 180 210, 200 180, 220 160)"));
				lines.Add(Read("LINESTRING (30 160, 40 150, 70 150)"));
				lines.Add(Read("LINESTRING (160 310, 200 330, 220 340, 240 360)"));
				lines.Add(Read("LINESTRING (140 400, 150 370, 160 340, 160 310)"));
				lines.Add(Read("LINESTRING (160 310, 130 300, 100 290, 70 270)"));
				lines.Add(Read("LINESTRING (240 360, 260 390, 260 410, 250 430)"));
				lines.Add(Read("LINESTRING (70 150, 100 180, 100 200)"));
				lines.Add(Read("LINESTRING (70 270, 60 260, 50 240, 50 220, 60 210)"));
				lines.Add(Read("LINESTRING (100 200, 90 210, 60 210)"));				
				return lines;
			}
			
		}		
		private WKTReader reader;
		
		public LineMergeExample()
		{
			InitBlock();
		}
		
		[STAThread]
		public static void main(string[] args)
		{
			LineMergeExample test = new LineMergeExample();
			try
			{
				test.Run();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.StackTrace);
			}
		}
		
		internal virtual void Run()
		{
			IList lineStrings = Data;
			
			LineMerger lineMerger = new LineMerger();
			lineMerger.Add(lineStrings);
		    ICollection mergedLineStrings = lineMerger.GetMergedLineStrings();
			
			Console.WriteLine("Lines formed (" + mergedLineStrings.Count + "):");
            foreach (object obj in mergedLineStrings)
			    Console.WriteLine(obj);
		}
		
		
		internal virtual IGeometry Read(string lineWKT)
		{
			try
			{
				IGeometry geom = reader.Read(lineWKT);				
				return geom;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.StackTrace);
			}			
			return null;
		}
	}
}