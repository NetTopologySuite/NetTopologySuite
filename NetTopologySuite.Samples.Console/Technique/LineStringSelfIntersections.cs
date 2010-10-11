using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;

namespace NetTopologySuite.Samples.Technique
{
    /// <summary> 
    /// Shows a technique for identifying the location of self-intersections
    /// in a non-simple LineString.
    /// </summary>		
    public class LineStringSelfIntersections
    {
        [STAThread]
        public static void main(String[] args)
        {
            GeometryFactory<BufferedCoordinate> geoFactory =
                new GeometryFactory<BufferedCoordinate>(
                    new BufferedCoordinateSequenceFactory());

            WktReader<BufferedCoordinate> rdr
                = new WktReader<BufferedCoordinate>(geoFactory, null);

            ILineString line1 = (ILineString) rdr.Read("LINESTRING (0 0, 10 10, 20 20)");
            ShowSelfIntersections(line1);
            ILineString line2 =
                (ILineString) rdr.Read("LINESTRING (0 40, 60 40, 60 0, 20 0, 20 60)");
            ShowSelfIntersections(line2);
        }

        public static void ShowSelfIntersections(ILineString line)
        {
            Console.WriteLine("Line: " + line);
            Console.WriteLine("Self Intersections: " + LineStringSelfIntersectionsOp(line));
        }

        public static IGeometry LineStringSelfIntersectionsOp(ILineString line)
        {
            IGeometry lineEndPts = GetEndPoints(line);
            IGeometry nodedLine = line.Union(lineEndPts);
            IGeometry nodedEndPts = GetEndPoints(nodedLine);
            IGeometry selfIntersections = nodedEndPts.Difference(lineEndPts);
            return selfIntersections;
        }

        public static IGeometry GetEndPoints(IGeometry g)
        {
            List<ICoordinate> endPtList = new List<ICoordinate>();

            if (g is ILineString)
            {
                ILineString line = (ILineString) g;
                endPtList.Add(line.Coordinates.First);
                endPtList.Add(line.Coordinates.Last);
            }
            else if (g is IMultiLineString)
            {
                IMultiLineString mls = (IMultiLineString) g;

                for (Int32 i = 0; i < mls.Count; i++)
                {
                    ILineString line = mls[i];
                    endPtList.Add(line.Coordinates.First);
                    endPtList.Add(line.Coordinates.Last);
                }
            }

            ICoordinate[] endPts = endPtList.ToArray();

            IGeometryFactory<BufferedCoordinate> geoFactory =
                new GeometryFactory<BufferedCoordinate>(
                    new BufferedCoordinateSequenceFactory());

            return geoFactory.CreateMultiPoint(endPts);
        }
    }
}