using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;

namespace GisSharpBlog.NetTopologySuite.Samples.Geometries
{
    /// <summary> 
    /// An example showing the results of using different precision models
    /// in computations involving geometric constructions.
    /// A simple intersection computation is carried out in three different
    /// precision models (Floating, FloatingSingle and Fixed with 0 decimal places).
    /// The input is the same in all cases (since it is precise in all three models),
    /// The output shows the effects of rounding in the single-precision and fixed-precision
    /// models.
    /// </summary>	
    public class PrecisionModelExample
    {
        [STAThread]
        public static void Main(String[] args)
        {
            PrecisionModelExample example = new PrecisionModelExample();

            try
            {
                example.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private readonly ICoordinateFactory<BufferedCoordinate> _coordinateFactory =
            new BufferedCoordinateFactory();

        public virtual void Run()
        {
            Example1();
            Example2();
        }

        public virtual void Example1()
        {
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine(
                "Example 1 shows roundoff from computing in different precision models");
            String wktA = "POLYGON ((60 180, 160 260, 240 80, 60 180))";
            String wktB = "POLYGON ((200 260, 280 160, 80 100, 200 260))";
            Console.WriteLine("A = " + wktA);
            Console.WriteLine("B = " + wktB);

            Intersection(wktA, wktB, new PrecisionModel<BufferedCoordinate>(_coordinateFactory));
            Intersection(wktA,
                         wktB,
                         new PrecisionModel<BufferedCoordinate>(_coordinateFactory,
                                                                  PrecisionModelType.SingleFloating));
            Intersection(wktA, wktB, new PrecisionModel<BufferedCoordinate>(_coordinateFactory, 1));
        }

        public virtual void Example2()
        {
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine(
                "Example 2 shows that roundoff can change the topology of geometry computed in different precision models");
            String wktA = "POLYGON ((0 0, 160 0, 160 1, 0 0))";
            String wktB = "POLYGON ((40 60, 40 -20, 140 -20, 140 60, 40 60))";
            Console.WriteLine("A = " + wktA);
            Console.WriteLine("B = " + wktB);

            Difference(wktA, wktB, new PrecisionModel<BufferedCoordinate>(_coordinateFactory));
            Difference(wktA, wktB, new PrecisionModel<BufferedCoordinate>(_coordinateFactory, 1));
        }


        public virtual void Intersection(String wktA, 
                                         String wktB,
                                         PrecisionModel<BufferedCoordinate> pm)
        {
            Console.WriteLine("Running example using Precision Model = " + pm);
            GeometryFactory<BufferedCoordinate> fact
                = new GeometryFactory<BufferedCoordinate>(pm);
            WktReader<BufferedCoordinate> wktRdr = new WktReader<BufferedCoordinate>(fact, null);

            IGeometry a = wktRdr.Read(wktA);
            IGeometry b = wktRdr.Read(wktB);
            IGeometry c = a.Intersection(b);

            Console.WriteLine("A intersection B = " + c);
        }

        public virtual void Difference(String wktA, 
                                       String wktB,
                                       PrecisionModel<BufferedCoordinate> pm)
        {
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("Running example using Precision Model = " + pm);
            GeometryFactory<BufferedCoordinate> fact
                = new GeometryFactory<BufferedCoordinate>(pm);
            WktReader<BufferedCoordinate> wktRdr = new WktReader<BufferedCoordinate>(fact, null);

            IGeometry a = wktRdr.Read(wktA);
            IGeometry b = wktRdr.Read(wktB);
            IGeometry c = a.Difference(b);

            Console.WriteLine("A intersection B = " + c);
        }
    }
}