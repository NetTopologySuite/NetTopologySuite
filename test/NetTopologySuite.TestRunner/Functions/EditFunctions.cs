using System;
using NetTopologySuite.Geometries;

namespace Open.Topology.TestRunner.Functions
{
    public class EditFunctions
    {
        public static Geometry AddHole(Geometry polyGeom, Geometry hole)
        {
            var factory = polyGeom.Factory;

            // input checks
            if (!(polyGeom is Polygon polygon))
                throw new ArgumentException("A is not a polygon");
            if (!(hole is Polygon || hole is LineString))
             throw new ArgumentException("B must be a polygon or line");

            var holePts = ExtractLine(hole);
            if (!CoordinateArrays.IsRing(holePts))
            {
                throw new ArgumentException("B is not a valid ring");
            }

            var shell = (LinearRing)polygon.ExteriorRing.Copy();

            var holes = new LinearRing[polygon.NumInteriorRings + 1];
            for (int i = 0; i < polygon.NumInteriorRings; i++)
            {
                holes[i] = (LinearRing)polygon.GetInteriorRingN(i).Copy();
            }
            holes[holes.Length - 1] = factory.CreateLinearRing(holePts);
            return factory.CreatePolygon(shell, holes);
        }

        private static Coordinate[] ExtractLine(Geometry hole)
        {
            if (hole is Polygon phole) {
                return phole.ExteriorRing.Coordinates;
            }
            return hole.Coordinates;
        }
    }
}
