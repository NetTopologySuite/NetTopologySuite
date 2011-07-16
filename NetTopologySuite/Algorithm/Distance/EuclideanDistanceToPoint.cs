using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Algorithm.Distance
{
    public class EuclideanDistanceToPoint<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        public static void ComputeDistance(ICoordinateFactory<TCoordinate> coordFact, IGeometry<TCoordinate> geom,
                                           TCoordinate pt, PointPairDistance<TCoordinate> ptDist)
        {
            if (geom is ILineString<TCoordinate>)
            {
                ComputeDistance(coordFact, (ILineString<TCoordinate>) geom, pt, ptDist);
            }
            else if (geom is IPolygon<TCoordinate>)
            {
                ComputeDistance(coordFact, (IPolygon<TCoordinate>) geom, pt, ptDist);
            }
            else if (geom is IGeometryCollection<TCoordinate>)
            {
                IGeometryCollection<TCoordinate> gc = (IGeometryCollection<TCoordinate>) geom;
                foreach (IGeometry<TCoordinate> geometry in gc)
                    ComputeDistance(coordFact, geometry, pt, ptDist);
            }
            else
            {
                // assume geom is Point
                TCoordinate[] tmp = {geom.Coordinates[0], pt};

                ptDist.SetMinimum(new Pair<TCoordinate>(geom.Coordinates[0], pt));
            }
        }

        public static void ComputeDistance(ICoordinateFactory<TCoordinate> coordFact, ILineString<TCoordinate> line,
                                           TCoordinate pt, PointPairDistance<TCoordinate> ptDist)
        {
            foreach (Pair<TCoordinate> pair in Slice.GetOverlappingPairs(line.Coordinates))
            {
                LineSegment<TCoordinate> tmpSeg = new LineSegment<TCoordinate>(pair);
                TCoordinate closestPoint = tmpSeg.ClosestPoint(pt, coordFact);
                ptDist.SetMinimum(new Pair<TCoordinate>(closestPoint, pt));
            }
        }

        public static void ComputeDistance(ICoordinateFactory<TCoordinate> coordFact, LineSegment<TCoordinate> segment,
                                           TCoordinate pt, PointPairDistance<TCoordinate> ptDist)
        {
            TCoordinate closestPt = segment.ClosestPoint(pt, coordFact);
            ptDist.SetMinimum(new Pair<TCoordinate>(closestPt, pt));
        }

        public static void ComputeDistance(ICoordinateFactory<TCoordinate> coordFact, IPolygon<TCoordinate> poly,
                                           TCoordinate pt, PointPairDistance<TCoordinate> ptDist)
        {
            ComputeDistance(coordFact, poly.ExteriorRing, pt, ptDist);
            foreach (ILineString<TCoordinate> ring in poly.InteriorRings)
                ComputeDistance(coordFact, ring, pt, ptDist);
        }
    }
}