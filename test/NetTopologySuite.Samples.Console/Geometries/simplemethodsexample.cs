using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

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
        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        [STAThread]
        public static void  main(string[] args)
        {
            var example = new SimpleMethodsExample();
            try
            {
                example.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public SimpleMethodsExample() { }

        /// <summary>
        ///
        /// </summary>
        public virtual void Run()
        {
            var fact = new GeometryFactory();
            var wktRdr = new WKTReader(fact);

            string wktA = "POLYGON((40 100, 40 20, 120 20, 120 100, 40 100))";
            string wktB = "LINESTRING(20 80, 80 60, 100 140)";
            var A = wktRdr.Read(wktA);
            var B = wktRdr.Read(wktB);
            var C = A.Intersection(B);
            Console.WriteLine("A = " + A);
            Console.WriteLine("B = " + B);
            Console.WriteLine("A intersection B = " + C);
            Console.WriteLine("A relate C = " + A.Relate(B));
        }
    }
}