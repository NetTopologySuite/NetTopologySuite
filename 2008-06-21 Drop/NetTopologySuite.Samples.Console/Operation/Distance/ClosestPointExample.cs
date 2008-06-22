using System;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Operation.Distance;
using NetTopologySuite.Coordinates;

namespace GisSharpBlog.NetTopologySuite.Samples.Operation.Distance
{
    /// <summary> 
    /// Example of computing distance and closest points between geometries
    /// using the <see cref="DistanceOp{TCoordinate}"/> class.
    /// </summary>	
    public class ClosestPointExample
    {
        internal static GeometryFactory<BufferedCoordinate2D> fact;
        internal static IWktGeometryReader wktRdr;

        [STAThread]
        public static void Main(String[] args)
        {
            ClosestPointExample example = new ClosestPointExample();
            example.Run();
        }

        public virtual void Run()
        {
            String polygon = "POLYGON ((200 180, 60 140, 60 260, 200 180))";
            FindClosestPoint(polygon, "POINT (140 280)");
            FindClosestPoint(polygon, "MULTIPOINT (140 280, 140 320)");

            String lineString = "LINESTRING (100 100, 200 100, 200 200, 100 200, 100 100)";
            FindClosestPoint(lineString, "POINT (10 10)");

            lineString = "LINESTRING (100 100, 200 200)";
            FindClosestPoint(lineString, "LINESTRING (100 200, 200 100)");
            FindClosestPoint(lineString, "LINESTRING (150 121, 200 0)");

            polygon =
                "POLYGON (( 76 185, 125 283, 331 276, 324 122, 177 70, 184 155, 69 123, 76 185 ), " +
                "( 267 237, 148 248, 135 185, 223 189, 251 151, 286 183, 267 237 ))";
            FindClosestPoint(polygon,
                             "LINESTRING ( 153 204, 185 224, 209 207, 238 222, 254 186 )");
            FindClosestPoint(polygon,
                             "LINESTRING ( 120 215, 185 224, 209 207, 238 222, 254 186 )");
        }

        public virtual void FindClosestPoint(String wktA, String wktB)
        {
            Console.WriteLine("-------------------------------------");

            try
            {
                IGeometry<BufferedCoordinate2D> a =
                    wktRdr.Read(wktA) as IGeometry<BufferedCoordinate2D>;
                IGeometry<BufferedCoordinate2D> b =
                    wktRdr.Read(wktB) as IGeometry<BufferedCoordinate2D>;
                Console.WriteLine("Geometry A: " + a);
                Console.WriteLine("Geometry B: " + b);
                DistanceOp<BufferedCoordinate2D> distOp = new DistanceOp<BufferedCoordinate2D>(a, b);

                Double distance = distOp.Distance;
                Console.WriteLine("Distance = " + distance);

                Pair<BufferedCoordinate2D>? closestPt = distOp.ClosestPoints();
                ILineString closestPtLine = fact.CreateLineString(closestPt);
                Console.WriteLine("Closest points: " + closestPtLine + " (distance = " +
                                  closestPtLine.Length + ")");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        static ClosestPointExample()
        {
            fact = new GeometryFactory<BufferedCoordinate2D>(
                new BufferedCoordinate2DSequenceFactory());
            wktRdr = new WktReader<BufferedCoordinate2D>(fact, null);
        }
    }
}