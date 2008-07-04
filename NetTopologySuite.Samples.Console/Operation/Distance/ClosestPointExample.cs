using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Operation.Distance;

namespace GisSharpBlog.NetTopologySuite.Samples.Operation.Distance
{
	/// <summary> 
    /// Example of computing distance and closest points between geometries
	/// using the DistanceOp class.
	/// </summary>	
	public class ClosestPointExample
	{		
		internal static GeometryFactory fact;	
		internal static WKTReader wktRdr;
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
		[STAThread]
		public static void main(string[] args)
		{
			ClosestPointExample example = new ClosestPointExample();
			example.Run();
		}
		
        /// <summary>
        /// 
        /// </summary>
		public ClosestPointExample() { }
		
        /// <summary>
        /// 
        /// </summary>
		public virtual void  Run()
		{
			FindClosestPoint("POLYGON ((200 180, 60 140, 60 260, 200 180))", "POINT (140 280)");
			FindClosestPoint("POLYGON ((200 180, 60 140, 60 260, 200 180))", "MULTIPOINT (140 280, 140 320)");
			FindClosestPoint("LINESTRING (100 100, 200 100, 200 200, 100 200, 100 100)", "POINT (10 10)");
			FindClosestPoint("LINESTRING (100 100, 200 200)", "LINESTRING (100 200, 200 100)");
			FindClosestPoint("LINESTRING (100 100, 200 200)", "LINESTRING (150 121, 200 0)");
			FindClosestPoint("POLYGON (( 76 185, 125 283, 331 276, 324 122, 177 70, 184 155, 69 123, 76 185 ), ( 267 237, 148 248, 135 185, 223 189, 251 151, 286 183, 267 237 ))", "LINESTRING ( 153 204, 185 224, 209 207, 238 222, 254 186 )");
			FindClosestPoint("POLYGON (( 76 185, 125 283, 331 276, 324 122, 177 70, 184 155, 69 123, 76 185 ), ( 267 237, 148 248, 135 185, 223 189, 251 151, 286 183, 267 237 ))", "LINESTRING ( 120 215, 185 224, 209 207, 238 222, 254 186 )");
		}
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wktA"></param>
        /// <param name="wktB"></param>
		public virtual void  FindClosestPoint(string wktA, string wktB)
		{
			Console.WriteLine("-------------------------------------");
			try
			{
				IGeometry A = wktRdr.Read(wktA);
                IGeometry B = wktRdr.Read(wktB);
				Console.WriteLine("Geometry A: " + A);
				Console.WriteLine("Geometry B: " + B);
				DistanceOp distOp = new DistanceOp(A, B);
				
				double distance = distOp.Distance();
				Console.WriteLine("Distance = " + distance);
				
				ICoordinate[] closestPt = distOp.ClosestPoints();
				ILineString closestPtLine = fact.CreateLineString(closestPt);
				Console.WriteLine("Closest points: " + closestPtLine + " (distance = " + closestPtLine.Length + ")");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.StackTrace);
			}
		}

        /// <summary>
        /// 
        /// </summary>
		static ClosestPointExample()
		{
			fact = new GeometryFactory();
			wktRdr = new WKTReader(fact);
		}
	}
}