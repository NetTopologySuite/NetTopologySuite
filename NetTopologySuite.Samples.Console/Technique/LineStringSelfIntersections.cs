using System;
using System.Collections;
using System.Collections.Generic;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

namespace GisSharpBlog.NetTopologySuite.Samples.Technique
{
	/// <summary> 
    /// Shows a technique for identifying the location of self-intersections
	/// in a non-simple LineString.
	/// </summary>		
	public class LineStringSelfIntersections
	{		
		[STAThread]
		public static void main(string[] args)
		{
			WKTReader rdr = new WKTReader();
			
			LineString line1 = (LineString) (rdr.Read("LINESTRING (0 0, 10 10, 20 20)"));
			ShowSelfIntersections(line1);
			LineString line2 = (LineString) (rdr.Read("LINESTRING (0 40, 60 40, 60 0, 20 0, 20 60)"));
			ShowSelfIntersections(line2);
		}
		
		public static void  ShowSelfIntersections(LineString line)
		{
			Console.WriteLine("Line: " + line);
			Console.WriteLine("Self Intersections: " + LineStringSelfIntersectionsOp(line));
		}
		
		public static Geometry LineStringSelfIntersectionsOp(LineString line)
		{
			Geometry lineEndPts = GetEndPoints(line);
            Geometry nodedLine = (Geometry) line.Union(lineEndPts);
			Geometry nodedEndPts = GetEndPoints(nodedLine);
            Geometry selfIntersections = (Geometry) nodedEndPts.Difference(lineEndPts);
			return selfIntersections;
		}
		
		public static Geometry GetEndPoints(Geometry g)
		{
			List<Coordinate> endPtList = new List<Coordinate>();
			if (g is LineString)
			{
				LineString line = (LineString) g;				
				endPtList.Add(line.GetCoordinateN(0));
				endPtList.Add(line.GetCoordinateN(line.NumPoints - 1));
			}
			else if (g is MultiLineString)
			{
				MultiLineString mls = (MultiLineString) g;
				for (int i = 0; i < mls.NumGeometries; i++)
				{
					LineString line = (LineString) mls.GetGeometryN(i);
					endPtList.Add(line.GetCoordinateN(0));
					endPtList.Add(line.GetCoordinateN(line.NumPoints - 1));
				}
			}
			Coordinate[] endPts = endPtList.ToArray();
			return (new GeometryFactory()).CreateMultiPoint(endPts);
		}
	}
}