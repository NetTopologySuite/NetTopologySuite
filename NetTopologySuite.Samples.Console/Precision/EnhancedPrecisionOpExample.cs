using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Precision;

namespace GisSharpBlog.NetTopologySuite.Samples.Precision
{	
	/// <summary> 
    /// Example of using {EnhancedPrecisionOp} to avoid robustness problems.
	/// </summary>	
	public class EnhancedPrecisionOpExample
	{
		private void  InitBlock()
		{
			reader = new WKTReader();
		}

		[STAThread]
		public static void main(string[] args)
		{
			EnhancedPrecisionOpExample example = new EnhancedPrecisionOpExample();
			try
			{
				example.Run();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
				
		private WKTReader reader;
		
		public EnhancedPrecisionOpExample()
		{
			InitBlock();
		}
		
		internal virtual void Run()
		{
			string wkt1, wkt2;
			// two geometries which cause robustness problems
			wkt1 = "POLYGON ((708653.498611049 2402311.54647056, 708708.895756966 2402203.47250014, 708280.326454234 2402089.6337791, 708247.896591321 2402252.48269854, 708367.379593851 2402324.00761653, 708248.882609455 2402253.07294874, 708249.523621829 2402244.3124463, 708261.854734465 2402182.39086576, 708262.818392579 2402183.35452387, 708653.498611049 2402311.54647056))";
			wkt2 = "POLYGON ((708258.754920656 2402197.91172757, 708257.029447455 2402206.56901508, 708652.961095455 2402312.65463437, 708657.068786251 2402304.6356364, 708258.754920656 2402197.91172757))";
            IGeometry g1 = reader.Read(wkt1);
            IGeometry g2 = reader.Read(wkt2);
			
			Console.WriteLine("This call to intersection will throw a topology exception due to robustness problems:");
			try
			{
                IGeometry result = g1.Intersection(g2);
			}
			catch (TopologyException ex)
			{
                Console.WriteLine(ex.ToString());
			}
			
			Console.WriteLine("Using EnhancedPrecisionOp allows the intersection to be performed with no errors:");
            IGeometry result2 = EnhancedPrecisionOp.Intersection(g1, g2);
			Console.WriteLine(result2);
		}
	}
}