using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Buffer.Validate
{
    /// <summary>
    /// Computes the Euclidean distance (L2 metric) from a Point to a Geometry.
    /// Also computes two points which are separated by the distance.
    /// </summary>
    public static class DistanceToPointFinder
    {
        public static void ComputeDistance(Geometry geom, Coordinate pt, PointPairDistance ptDist)
        {
            if (geom is LineString)
            {
                ComputeDistance((LineString)geom, pt, ptDist);
            }
            else if (geom is Polygon)
            {
                ComputeDistance((Polygon)geom, pt, ptDist);
            }
            else if (geom is GeometryCollection)
            {
                var gc = (GeometryCollection)geom;
                for (int i = 0; i < gc.NumGeometries; i++)
                {
                    var g = gc.GetGeometryN(i);
                    ComputeDistance(g, pt, ptDist);
                }
            }
            else
            {
                // assume geom is Point
                ptDist.SetMinimum(geom.Coordinate, pt);
            }
        }

        public static void ComputeDistance(LineString line, Coordinate pt, PointPairDistance ptDist)
        {
            var coords = line.Coordinates;
            var tempSegment = new LineSegment();
            for (int i = 0; i < coords.Length - 1; i++)
            {
                tempSegment.SetCoordinates(coords[i], coords[i + 1]);
                // this is somewhat inefficient - could do better
                var closestPt = tempSegment.ClosestPoint(pt);
                ptDist.SetMinimum(closestPt, pt);
            }
        }

        public static void ComputeDistance(LineSegment segment, Coordinate pt, PointPairDistance ptDist)
        {
            var closestPt = segment.ClosestPoint(pt);
            ptDist.SetMinimum(closestPt, pt);
        }

        public static void ComputeDistance(Polygon poly, Coordinate pt, PointPairDistance ptDist)
        {
            ComputeDistance(poly.ExteriorRing, pt, ptDist);
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                ComputeDistance(poly.GetInteriorRingN(i), pt, ptDist);
            }
        }
    }
}