using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm.Distance;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer.Validate
{
    ///<summary>
    /// Computes the Euclidean distance (L2 metric) from a Point to a Geometry.
    /// Also computes two points which are separated by the distance.
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public static class DistanceToPointFinder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible

    {
        ///<summary>
        ///</summary>
        ///<param name="coordFact"></param>
        ///<param name="geom"></param>
        ///<param name="pt"></param>
        ///<param name="ptDist"></param>
        public static void ComputeDistance(ICoordinateFactory<TCoordinate> coordFact, IGeometry<TCoordinate> geom, TCoordinate pt, PointPairDistance<TCoordinate> ptDist)
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
                foreach (IGeometry<TCoordinate> geometry in (IGeometryCollection<TCoordinate>) geom)
                    ComputeDistance(coordFact, geometry, pt, ptDist);
            }
            else
            {
                // assume geom is Point
                ptDist.SetMinimum(new Pair<TCoordinate>(((IPoint<TCoordinate>)geom).Coordinate, pt));
            }
        }

        ///<summary>
        ///</summary>
        ///<param name="coordFact"></param>
        ///<param name="line"></param>
        ///<param name="pt"></param>
        ///<param name="ptDist"></param>
        public static void ComputeDistance(ICoordinateFactory<TCoordinate> coordFact, ILineString<TCoordinate> line, TCoordinate pt, PointPairDistance<TCoordinate> ptDist)
        {
            ICoordinateFactory<TCoordinate> factory = line.Coordinates.CoordinateFactory;
            foreach (Pair<TCoordinate> pair in Slice.GetOverlappingPairs(line.Coordinates))
            {
                LineSegment<TCoordinate> tempSegment = new LineSegment<TCoordinate>(pair);
                TCoordinate closestPt = tempSegment.ClosestPoint(pt, factory);
                ptDist.SetMinimum(new Pair<TCoordinate>(closestPt, pt));
            }
        }

        ///<summary>
        /// Computes the 
        ///</summary>
        ///<param name="coordFact">factory to create new coordinates</param>
        ///<param name="segment"></param>
        ///<param name="pt"></param>
        ///<param name="ptDist"><see cref="PointPairDistance{TCoordinate}"/> to update</param>
        public static void ComputeDistance(ICoordinateFactory<TCoordinate> coordFact, LineSegment<TCoordinate> segment, TCoordinate pt, PointPairDistance<TCoordinate> ptDist)
        {
            TCoordinate closestPt = segment.ClosestPoint(pt, coordFact);
            ptDist.SetMinimum(new Pair<TCoordinate>(closestPt, pt));
        }

        ///<summary>
        ///</summary>
        ///<param name="coordFact"></param>
        ///<param name="poly"></param>
        ///<param name="pt"></param>
        ///<param name="ptDist"></param>
        public static void ComputeDistance(ICoordinateFactory<TCoordinate> coordFact, IPolygon<TCoordinate> poly, TCoordinate pt, PointPairDistance<TCoordinate> ptDist)
        {
            ComputeDistance(coordFact, poly.ExteriorRing, pt, ptDist);
            foreach (ILineString<TCoordinate> lineString in poly.InteriorRings)
            {
                ComputeDistance(coordFact, lineString, pt, ptDist);
            }
        }
    }
}
