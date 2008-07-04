using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

namespace GisSharpBlog.NetTopologySuite.Samples.Geometries
{		
	/// <summary> 
    /// An example showing the results of using different precision models
	/// in computations involving geometric constructions.
	/// A simple intersection computation is carried out in three different
	/// precision models (Floating, FloatingSingle and Fixed with 0 decimal places).
	/// The input is the same in all cases (since it is precise in all three models),
	/// The output shows the effects of rounding in the single-precision and fixed-precision
	/// models.
	/// </summary>	
	public class PrecisionModelExample
	{
		[STAThread]
		public static void main(string[] args)
		{
			PrecisionModelExample example = new PrecisionModelExample();
			try
			{
				example.Run();
			}
			catch (Exception ex)
			{
                Console.WriteLine(ex.StackTrace);
			}
		}
		
		public PrecisionModelExample() { }
		
		public virtual void  Run()
		{
			Example1();
			Example2();
		}
		
		public virtual void  Example1()
		{
			Console.WriteLine("-------------------------------------------");
			Console.WriteLine("Example 1 shows roundoff from computing in different precision models");
			string wktA = "POLYGON ((60 180, 160 260, 240 80, 60 180))";
			string wktB = "POLYGON ((200 260, 280 160, 80 100, 200 260))";
			Console.WriteLine("A = " + wktA);
			Console.WriteLine("B = " + wktB);
			
			Intersection(wktA, wktB, new PrecisionModel());
			Intersection(wktA, wktB, new PrecisionModel(PrecisionModels.FloatingSingle));
			Intersection(wktA, wktB, new PrecisionModel(1));
		}
		
		public virtual void  Example2()
		{
			Console.WriteLine("-------------------------------------------");
			Console.WriteLine("Example 2 shows that roundoff can change the topology of geometry computed in different precision models");
			string wktA = "POLYGON ((0 0, 160 0, 160 1, 0 0))";
			string wktB = "POLYGON ((40 60, 40 -20, 140 -20, 140 60, 40 60))";
			Console.WriteLine("A = " + wktA);
			Console.WriteLine("B = " + wktB);
			
			Difference(wktA, wktB, new PrecisionModel());
			Difference(wktA, wktB, new PrecisionModel(1));
		}
		
		
		public virtual void  Intersection(string wktA, string wktB, PrecisionModel pm)
		{
			Console.WriteLine("Running example using Precision Model = " + pm);
			GeometryFactory fact = new GeometryFactory(pm);
			WKTReader wktRdr = new WKTReader(fact);
			
			IGeometry A = wktRdr.Read(wktA);
			IGeometry B = wktRdr.Read(wktB);
			IGeometry C = A.Intersection(B);
			
			Console.WriteLine("A intersection B = " + C);
		}
		
		public virtual void  Difference(string wktA, string wktB, PrecisionModel pm)
		{
			Console.WriteLine("-------------------------------------------");
			Console.WriteLine("Running example using Precision Model = " + pm);
			GeometryFactory fact = new GeometryFactory(pm);
			WKTReader wktRdr = new WKTReader(fact);

            IGeometry A = wktRdr.Read(wktA);
            IGeometry B = wktRdr.Read(wktB);
            IGeometry C = A.Difference(B);
			
			Console.WriteLine("A intersection B = " + C);
		}
	}
}