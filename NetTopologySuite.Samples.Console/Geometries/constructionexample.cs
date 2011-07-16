using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;

namespace NetTopologySuite.Samples.Geometries
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
        public static void Main(String[] args)
        {
            BufferedCoordinateFactory coordFactory = new BufferedCoordinateFactory();
            // create a factory using default values (e.g. floating precision)
            IGeometryFactory<BufferedCoordinate> fact
                = new GeometryFactory<BufferedCoordinate>(
                    new BufferedCoordinateSequenceFactory(coordFactory));

            IPoint p1 = fact.CreatePoint(coordFactory.Create(0, 0));
            Console.WriteLine(p1);

            IPoint p2 = fact.CreatePoint(coordFactory.Create(1, 1));
            Console.WriteLine(p1);

            ICoordinate[] coords = new ICoordinate[]
                                       {
                                           coordFactory.Create(0, 0),
                                           coordFactory.Create(1, 1),
                                       };

            IMultiPoint mpt = fact.CreateMultiPoint(coords);
            Console.WriteLine(mpt);
        }
    }
}