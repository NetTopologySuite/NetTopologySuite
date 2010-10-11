using System;
using GeoAPI.Coordinates;
using NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes an approximate intersection of two line segments
    /// by taking the most central of the endpoints of the segments.
    /// </summary>
    /// <remarks>
    /// This is effective in cases where the segments are nearly parallel
    /// and should intersect at an endpoint.
    /// It is also a reasonable strategy for cases where the 
    /// endpoint of one segment lies on or almost on the interior of another one.
    /// Taking the most central endpoint ensures that the computed intersection
    /// point lies in the envelope of the segments.
    /// Also, by always returning one of the input points, this should result 
    /// in reducing segment fragmentation.
    /// Intended to be used as a last resort for 
    /// computing ill-conditioned intersection situations which 
    /// cause other methods to fail.
    /// </remarks>
    public class CentralEndpointIntersector<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private TCoordinate _intPt;
        private LineSegment<TCoordinate> _line0;
        private LineSegment<TCoordinate> _line1;

        public CentralEndpointIntersector(ICoordinateFactory<TCoordinate> coordinateFactory,
                                          TCoordinate p00, TCoordinate p01,
                                          TCoordinate p10, TCoordinate p11)
            : this(coordinateFactory,
                   new LineSegment<TCoordinate>(p00, p01),
                   new LineSegment<TCoordinate>(p10, p11))
        {
        }

        public CentralEndpointIntersector(ICoordinateFactory<TCoordinate> coordinateFactory,
                                          LineSegment<TCoordinate> line0,
                                          LineSegment<TCoordinate> line1)
        {
            _line0 = line0;
            _line1 = line1;
            compute(coordinateFactory);
        }

        public static TCoordinate GetIntersection(ICoordinateFactory<TCoordinate> coordinateFactory,
                                                  TCoordinate p00, TCoordinate p01,
                                                  TCoordinate p10, TCoordinate p11)
        {
            CentralEndpointIntersector<TCoordinate> intersector
                = new CentralEndpointIntersector<TCoordinate>(coordinateFactory,
                                                              p00, p01, p10, p11);
            return intersector.GetIntersectionPoint();
        }

        private void compute(ICoordinateFactory<TCoordinate> coordinateFactory)
        {
            TCoordinate centroid = average(coordinateFactory, _line0.P0, _line0.P1, _line1.P0, _line1.P1);
            _intPt = findNearestPoint(centroid, _line0.P0, _line0.P1, _line1.P0, _line1.P1);
        }

        public TCoordinate GetIntersectionPoint()
        {
            return _intPt;
        }

        private static TCoordinate average(ICoordinateFactory<TCoordinate> coordinateFactory,
                                           params TCoordinate[] pts)
        {
            if (pts.Length == 0)
            {
                return default(TCoordinate);
            }

            TCoordinate first = pts[0];

            if (pts.Length == 1)
            {
                return first;
            }

            Int32 componentCount = first.ComponentCount;

            Double[] avg = new Double[componentCount];

            Int32 n = pts.Length;

            for (Int32 i = 0; i < pts.Length; i++)
            {
                for (Int32 componentIndex = 0; componentIndex < componentCount; componentIndex++)
                {
                    avg[componentIndex] += (Double) pts[i][componentIndex];
                }
            }

            for (Int32 componentIndex = 0; componentIndex < componentCount; componentIndex++)
            {
                avg[componentIndex] /= n;
            }

            return coordinateFactory.Create(avg);
        }

        // Determines a point closest to the given point from a set of points.
        private static TCoordinate findNearestPoint(TCoordinate p, params TCoordinate[] pts)
        {
            Double minDist = Double.MaxValue;

            TCoordinate result = default(TCoordinate);

            for (Int32 i = 0; i < pts.Length; i++)
            {
                Double dist = p.Distance(pts[i]);

                if (dist < minDist)
                {
                    minDist = dist;
                    result = pts[i];
                }
            }

            return result;
        }
    }
}