using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
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
			
			ILineString line1 = (ILineString) rdr.Read("LINESTRING (0 0, 10 10, 20 20)");
			ShowSelfIntersections(line1);			
            ILineString line2 = (ILineString) rdr.Read("LINESTRING (0 40, 60 40, 60 0, 20 0, 20 60)");
			ShowSelfIntersections(line2);
		}
		
		public static void  ShowSelfIntersections(ILineString line)
		{
			Console.WriteLine("Line: " + line);
			Console.WriteLine("Self Intersections: " + LineStringSelfIntersectionsOp(line));
		}
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
		public static IGeometry LineStringSelfIntersectionsOp(ILineString line)
		{
			IGeometry lineEndPts = GetEndPoints(line);
            IGeometry nodedLine = line.Union(lineEndPts);
			IGeometry nodedEndPts = GetEndPoints(nodedLine);
            IGeometry selfIntersections = nodedEndPts.Difference(lineEndPts);
			return selfIntersections;
		}
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static IGeometry GetEndPoints(IGeometry g)
		{
			List<ICoordinate> endPtList = new List<ICoordinate>();
			if (g is ILineString)
			{
				ILineString line = (ILineString) g;
                endPtList.Add(line.GetCoordinateN(0));
                endPtList.Add(line.GetCoordinateN(line.NumPoints - 1));
			}
			else if (g is IMultiLineString)
			{
				IMultiLineString mls = (IMultiLineString) g;
				for (int i = 0; i < mls.NumGeometries; i++)
				{
					ILineString line = (ILineString) mls.GetGeometryN(i);
                    endPtList.Add(line.GetCoordinateN(0));
                    endPtList.Add(line.GetCoordinateN(line.NumPoints - 1));
				}
			}
			ICoordinate[] endPts = endPtList.ToArray();
			return GeometryFactory.Default.CreateMultiPoint(endPts);
		}
	}
}