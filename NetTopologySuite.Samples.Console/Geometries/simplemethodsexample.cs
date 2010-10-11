using System;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;

namespace NetTopologySuite.Samples.Geometries
{
    /// <summary>
    /// An example showing a simple use of JTS methods for:
    /// WKT reading
    /// intersection
    /// relate
    /// WKT output	
    /// The expected output from this program is:
    /// ----------------------------------------------------------
    /// A = POLYGON ((40 100, 40 20, 120 20, 120 100, 40 100))
    /// B = LINESTRING (20 80, 80 60, 100 140)
    /// A intersection B = LINESTRING (40 73.33333333333334, 80 60, 90 100)
    /// A relate C = 1F20F1102
    /// ----------------------------------------------------------	
    /// </summary>	
    public class SimpleMethodsExample
    {
        [STAThread]
        public static void Main(String[] args)
        {
            SimpleMethodsExample example = new SimpleMethodsExample();

            try
            {
                example.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public virtual void Run()
        {
            GeometryFactory<BufferedCoordinate> geometryFactory
                = new GeometryFactory<BufferedCoordinate>(
                    new BufferedCoordinateSequenceFactory());
            WktReader<BufferedCoordinate> wktRdr
                = new WktReader<BufferedCoordinate>(geometryFactory, null);

            String wktA = "POLYGON((40 100, 40 20, 120 20, 120 100, 40 100))";
            String wktB = "LINESTRING(20 80, 80 60, 100 140)";
            IGeometry a = wktRdr.Read(wktA);
            IGeometry b = wktRdr.Read(wktB);
            IGeometry c = a.Intersection(b);
            Console.WriteLine("A = " + a);
            Console.WriteLine("B = " + b);
            Console.WriteLine("A intersection B = " + c);
            Console.WriteLine("A relate C = " + a.Relate(b));
        }
    }
}