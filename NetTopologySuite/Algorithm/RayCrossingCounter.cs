using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    public class RayCrossingCounter<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        public static Locations LocatePointInRing(TCoordinate p, ILinearRing<TCoordinate> ring)
        {
            if ( ring == null )
                return Locations.None;

            RayCrossingCounter<TCoordinate> counter = new RayCrossingCounter<TCoordinate>(p);
            TCoordinate p1 = default(TCoordinate);
            foreach (var p2 in ring.Coordinates)
            {
                counter.CountSegment(p1, p2);
                if (counter.IsOnSegment)
                    return counter.Location;
            }

            return counter.Location;
        }

        private TCoordinate _point;
        private int _crossingCount = 0;
        // true if the test point lies on an input segment
        private Boolean _isPointOnSegment = false;

        public RayCrossingCounter(TCoordinate point)
        {
            _point = point;
        }

        public void CountSegment(TCoordinate p1, TCoordinate p2)
        {
            /**
             * For each segment, check if it crosses 
             * a horizontal ray running from the test point in the positive x direction.
             */

            // check if the segment is strictly to the left of the test point
            if (p1[Ordinates.X] < _point[Ordinates.X] && p2[Ordinates.X] < _point[Ordinates.X])
                return;

            // check if the point is equal to the current ring vertex
            if (_point[Ordinates.X] == p2[Ordinates.X] && _point[Ordinates.Y] == p2[Ordinates.Y])
            {
                _isPointOnSegment = true;
                return;
            }
            /**
             * For horizontal segments, check if the point is on the segment.
             * Otherwise, horizontal segments are not counted.
             */
            if (p1[Ordinates.Y] == _point[Ordinates.Y] && p2[Ordinates.Y] == _point[Ordinates.Y])
            {
                double minx = p1[Ordinates.X];
                double maxx = p2[Ordinates.X];
                if (minx > maxx)
                {
                    minx = p2[Ordinates.X];
                    maxx = p1[Ordinates.X];
                }
                if (_point[Ordinates.X] >= minx && _point[Ordinates.X] <= maxx)
                {
                    _isPointOnSegment = true;
                }
                return;
            }
            /**
             * Evaluate all non-horizontal segments which cross a horizontal ray to the
             * right of the test pt. To avoid double-counting shared vertices, we use the
             * convention that
             * <ul>
             * <li>an upward edge includes its starting endpoint, and excludes its
             * final endpoint
             * <li>a downward edge excludes its starting endpoint, and includes its
             * final endpoint
             * </ul>
             */
            if (((p1[Ordinates.Y] > _point[Ordinates.Y]) && (p2[Ordinates.Y] <= _point[Ordinates.Y]))
                    || ((p2[Ordinates.Y] > _point[Ordinates.Y]) && (p1[Ordinates.Y] <= _point[Ordinates.Y])))
            {
                // translate the segment so that the test point lies on the origin
                double x1 = p1[Ordinates.X] - _point[Ordinates.X];
                double y1 = p1[Ordinates.Y] - _point[Ordinates.Y];
                double x2 = p2[Ordinates.X] - _point[Ordinates.X];
                double y2 = p2[Ordinates.Y] - _point[Ordinates.Y];

                /**
                 * The translated segment straddles the x-axis. Compute the sign of the
                 * ordinate of intersection with the x-axis. (y2 != y1, so denominator
                 * will never be 0.0)
                 */
                // double xIntSign = RobustDeterminant.signOfDet2x2(x1, y1, x2, y2) / (y2
                // - y1);
                // MD - faster & more robust computation?
                double xIntSign = RobustDeterminant.SignOfDet2x2(x1, y1, x2, y2);
                if (xIntSign == 0.0)
                {
                    _isPointOnSegment = true;
                    return;
                }
                if (y2 < y1)
                    xIntSign = -xIntSign;
                // xsave = xInt;

                // The segment crosses the ray if the sign is strictly positive.
                if (xIntSign > 0.0)
                {
                    _crossingCount++;
                }
            }
        }

        /**
         * Reports whether the point lies exactly on one of the supplied segments.
         * This method may be called at any time as segments are processed.
         * If the result of this method is <tt>true</tt>, 
         * no further segments need be supplied, since the result
         * will never change again.
         * 
         * @return true if the point lies exactly on a segment
         */
        public Boolean IsOnSegment { get { return _isPointOnSegment; } }

        /**
         * Gets the {@link Location} of the point relative to 
         * the ring, polygon
         * or multipolygon from which the processed segments were provided.
         * <p>
         * This method only determines the correct location 
         * if <b>all</b> relevant segments must have been processed. 
         * 
         * @return the Location of the point
         */
        public Locations Location
        {
            get
            {
                if (_isPointOnSegment)
                    return Locations.Boundary;

                // The point is in the interior of the ring if the number of X-crossings is
                // odd.
                if ((_crossingCount % 2) == 1)
                {
                    return Locations.Interior;
                }
                return Locations.Exterior;
            }
        }

        /**
         * Tests whether the point lies in or on 
         * the ring, polygon
         * or multipolygon from which the processed segments were provided.
         * <p>
         * This method only determines the correct location 
         * if <b>all</b> relevant segments must have been processed. 
         * 
         * @return true if the point lies in or on the supplied polygon
         */
        ///<summary>
        ///</summary>
        public Boolean IsPointInPolygon
        {
            get { return Location != Locations.Exterior; }
        }

    }
}
