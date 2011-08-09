using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Distance
{
    ///<summary>
    /// Computes the Euclidean distance (L2 metric) from a Point to a Geometry.
    ///</summary>
    /// <remarks>
    /// Also computes two points which are separated by the distance.
    /// </remarks>
    public class DistanceToPoint
    {

        // used for point-line distance calculation
        private static readonly LineSegment TempSegment = new LineSegment();

        public static void ComputeDistance(IGeometry geom, ICoordinate pt, PointPairDistance ptDist)
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
                for (int i = 0; i < gc.NumGeometries; i++)
                {
                    IGeometry g = gc.GetGeometryN(i);
                    ComputeDistance(g, pt, ptDist);
                }
            }
            else
            {
                // assume geom is Point
                ptDist.SetMinimum(geom.Coordinate, pt);
            }
        }

        public static void ComputeDistance(ILineString line, ICoordinate pt, PointPairDistance ptDist)
        {
            ICoordinate[] coords = line.Coordinates;
            for (int i = 0; i < coords.Length - 1; i++)
            {
                TempSegment.SetCoordinates(coords[i], coords[i + 1]);
                // this is somewhat inefficient - could do better
                ICoordinate closestPt = TempSegment.ClosestPoint(pt);
                ptDist.SetMinimum(closestPt, pt);
            }
        }

        public static void ComputeDistance(LineSegment segment, ICoordinate pt, PointPairDistance ptDist)
        {
            ICoordinate closestPt = segment.ClosestPoint(pt);
            ptDist.SetMinimum(closestPt, pt);
        }

        public static void ComputeDistance(IPolygon poly, ICoordinate pt, PointPairDistance ptDist)
        {
            ComputeDistance(poly.ExteriorRing, pt, ptDist);
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                ComputeDistance(poly.GetInteriorRingN(i), pt, ptDist);
            }
        }
    }
}