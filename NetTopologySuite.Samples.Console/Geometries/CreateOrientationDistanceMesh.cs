using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Geometries
{
    public class CreateOrientationDistanceMesh
    {

        [Test]
        public void BuildGeometries()
        {
            Coordinate coord = new Coordinate(3412805, 5320858);

            var orientation =
                new List<String>(new[]
                                     {
                                         "N", "NOzN", "NO", "NOzO",
                                         "O", "SOzO", "SO", "SOzS", 
                                         "S", "SWzS", "SW", "SWzW",
                                         "W", "NWzW", "NW", "NWzN"
                                     });

            var slices = BuildSlices(coord);
            int distance = 0;
            foreach (IGeometry geometry in BuildConcentricBuffers(coord))
            {
                distance += 5;
                var orIt = orientation.GetEnumerator();
                foreach (IGeometry slice in slices)
                {
                    orIt.MoveNext();
                    var geom = geometry.Intersection(slice);
                    Console.WriteLine("INSERT INTO \"PIF2988\".\"Gitter\" VALUES('"+orIt.Current+"', "+distance+", SetSRID(ST_GeomFromText('"+geom.AsText()+"'), 31467));");
                }
            }

        }

        public IEnumerable<IGeometry> BuildConcentricBuffers(Coordinate coord)
        {
            IPoint center = GeometryFactory.Floating.CreatePoint(coord);
            IPolygon lastPolygon = null;
            var distance = 0;
            while (distance <= 100000)
            {
                distance += 5000;
                var polygon = (IPolygon) center.Buffer(distance);
                yield return polygon.Difference(lastPolygon);
                lastPolygon = polygon;
            }
        }


        public IList<IGeometry> BuildSlices(Coordinate coord)
        {
            const double start = 101.25d;
            const double range = -22.5d;

            var slices = new List<IGeometry>(16);
            
            for (double angle = start; angle > 101.25d - 360d; angle += range)
            {
                var coordinates = new[]
                                      {
                                          coord,
                                          GetNextCoordinate(coord, angle, 200000),
                                          GetNextCoordinate(coord, angle + range, 200000),
                                          coord,
                                      };
                var shell = GeometryFactory.Floating.CreateLinearRing(coordinates);
                slices.Add(GeometryFactory.Floating.CreatePolygon(shell, null));
            }
            return slices;
        }

        static Coordinate GetNextCoordinate(Coordinate start, double angle, double distance)
        {
            const double toRadians = Math.PI/180d;
            double angleInRadians = angle*toRadians;
            double dx = Math.Cos(angleInRadians) * distance;
            double dy = Math.Sin(angleInRadians) * distance;

            return new Coordinate(start.X + dx, start.Y + dy);
        }
    }
}