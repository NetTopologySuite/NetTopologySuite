using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Samples.Technique
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
            var rdr = new WKTReader();

            var line1 = (ILineString) rdr.Read("LINESTRING (0 0, 10 10, 20 20)");
            ShowSelfIntersections(line1);
            var line2 = (ILineString) rdr.Read("LINESTRING (0 40, 60 40, 60 0, 20 0, 20 60)");
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
            var lineEndPts = GetEndPoints(line);
            var nodedLine = line.Union(lineEndPts);
            var nodedEndPts = GetEndPoints(nodedLine);
            var selfIntersections = nodedEndPts.Difference(lineEndPts);
            return selfIntersections;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static IGeometry GetEndPoints(IGeometry g)
        {
            var endPtList = new List<Coordinate>();
            if (g is ILineString)
            {
                var line = (ILineString) g;
                endPtList.Add(line.GetCoordinateN(0));
                endPtList.Add(line.GetCoordinateN(line.NumPoints - 1));
            }
            else if (g is IMultiLineString)
            {
                var mls = (IMultiLineString) g;
                for (int i = 0; i < mls.NumGeometries; i++)
                {
                    var line = (ILineString) mls.GetGeometryN(i);
                    endPtList.Add(line.GetCoordinateN(0));
                    endPtList.Add(line.GetCoordinateN(line.NumPoints - 1));
                }
            }
            var endPts = endPtList.ToArray();
            return GeometryFactory.Default.CreateMultiPointFromCoords(endPts);
        }
    }
}
