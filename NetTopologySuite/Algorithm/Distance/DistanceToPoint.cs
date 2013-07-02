using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Distance
{
    ///<summary>
    /// Computes the Euclidean distance (L2 metric) from a <see cref="Coordinate"/> to a <see cref="IGeometry"/>.
    ///</summary>
    /// <remarks>
    /// Also computes two points on the geometry which are separated by the distance found.
    /// </remarks>
    public static class DistanceToPoint
    {
        public static void ComputeDistance(IGeometry geom, Coordinate pt, PointPairDistance ptDist)
        {
            if (geom is ILineString)
            {
                ComputeDistance((ILineString) geom, pt, ptDist);
            }
            else if (geom is IPolygon)
            {
                ComputeDistance((IPolygon) geom, pt, ptDist);
            }
            else if (geom is IGeometryCollection)
            {
                var gc = (IGeometryCollection) geom;
                for (var i = 0; i < gc.NumGeometries; i++)
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

        public static void ComputeDistance(ILineString line, Coordinate pt, PointPairDistance ptDist)
        {
            var coords = line.Coordinates;
            var tempSegment = new LineSegment();
            for (var i = 0; i < coords.Length - 1; i++)
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

        public static void ComputeDistance(IPolygon poly, Coordinate pt, PointPairDistance ptDist)
        {
            ComputeDistance(poly.ExteriorRing, pt, ptDist);
            for (var i = 0; i < poly.NumInteriorRings; i++)
            {
                ComputeDistance(poly.GetInteriorRingN(i), pt, ptDist);
            }
        }
    }
}