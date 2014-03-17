using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Buffer
{
    public class Test
    {
        [TestAttribute]
        public void Buffer()
        {
            var geom =
                new Polygon(
                    new LinearRing(new Coordinate[]
                                       {
                                           new Coordinate(0, 0), new Coordinate(0, 10), new Coordinate(10, 10),
                                           new Coordinate(10, 0), new Coordinate(0, 0)
                                       }));
            Console.WriteLine(geom.AsText());
            var geom2 = geom.Buffer(2d);
            Console.WriteLine(geom2);
            var geom3 = geom2.Buffer(-2);
            geom3.Normalize();
            Console.WriteLine(geom3);
        }
    }
}