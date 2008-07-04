using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Samples.Geometries
{	
	/// <summary> 
    /// Examples of constructing Geometries programmatically.
	/// The Best Practice moral here is:
	/// Use the GeometryFactory to construct Geometries whenever possible.
	/// This has several advantages:
	///     Simplifies your code.
	///     Allows you to take advantage of convenience methods provided by GeometryFactory.
	///     Insulates your code from changes in the signature of JTS constructors	
	/// </summary>	
	public class ConstructionExample
	{		
		[STAThread]
		public static void main(string[] args)
		{
			// create a factory using default values (e.g. floating precision)
			GeometryFactory fact = new GeometryFactory();
			
			IPoint p1 = fact.CreatePoint(new Coordinate(0, 0));
			Console.WriteLine(p1);
			
			IPoint p2 = fact.CreatePoint(new Coordinate(1, 1));
			Console.WriteLine(p1);
			
			IMultiPoint mpt = fact.CreateMultiPoint(new ICoordinate[]{ new Coordinate(0, 0), new Coordinate(1, 1), });
			Console.WriteLine(mpt);
		}
	}
}