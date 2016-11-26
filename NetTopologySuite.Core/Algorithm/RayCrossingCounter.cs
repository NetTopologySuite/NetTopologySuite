using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
{
    ///<summary>
    /// Counts the number of segments crossed by a horizontal ray extending to the right
    /// from a given point, in an incremental fashion.
    /// <para>This can be used to determine whether a point lies in a <see cref="IPolygonal"/> geometry.</para>
    /// <para>The class determines the situation where the point lies exactly on a segment.</para>
    /// <para>When being used for Point-In-Polygon determination, this case allows short-circuiting the evaluation.</para>
    /// </summary>
    /// <remarks>
    /// This class handles polygonal geometries with any number of shells and holes.
    /// The orientation of the shell and hole rings is unimportant.
    /// In order to compute a correct location for a given polygonal geometry,
    /// it is essential that <b>all</b> segments are counted which
    /// <list type="Bullet">
    /// <item>touch the ray</item>
    /// <item>lie in in any ring which may contain the point</item>
    /// </list>
    /// <para>
    /// The only exception is when the point-on-segment situation is detected, in which
    /// case no further processing is required.
    /// The implication of the above rule is that segments which can be a priori determined to <i>not</i> touch the ray
    /// (i.e. by a test of their bounding box or Y-extent) do not need to be counted.  This allows for optimization by indexing.
    /// </para>
    /// </remarks>
    /// <author>Martin Davis</author>
    public class RayCrossingCounter
    {
        ///<summary>
        /// Determines the <see cref="GeoAPI.Geometries.Location"/> of a point in a ring.
        /// This method is an exemplar of how to use this class.
        ///</summary>
        /// <param name="p">The point to test</param>
        /// <param name="ring">An array of Coordinates forming a ring</param>
        /// <returns>The location of the point in the ring</returns>
        public static Location LocatePointInRing(Coordinate p, Coordinate[] ring)
        {
            var counter = new RayCrossingCounter(p);

            for (int i = 1; i < ring.Length; i++)
            {
                var p1 = ring[i];
                var p2 = ring[i - 1];
                counter.CountSegment(p1, p2);
                if (counter.IsOnSegment)
                    return counter.Location;
            }
            return counter.Location;
        }

        /// <summary>
        /// Determines the <see cref="Location"/> of a point in a ring.
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <param name="ring">A coordinate sequence forming a ring</param>
        /// <returns>The location of the point in the ring</returns>
        public static Location LocatePointInRing(Coordinate p, ICoordinateSequence ring)
        {
            var counter = new RayCrossingCounter(p);

            var p1 = new Coordinate();
            var p2 = new Coordinate();
            int count = ring.Count;
            for (var i = 1; i < count; i++)
            {
                ring.GetCoordinate(i, p1);
                ring.GetCoordinate(i - 1, p2);
                counter.CountSegment(p1, p2);
                if (counter.IsOnSegment)
                    return counter.Location;
            }
            return counter.Location;
        }

        private readonly Coordinate _p;
        private int _crossingCount;
        // true if the test point lies on an input segment
        private bool _isPointOnSegment;

        public RayCrossingCounter(Coordinate p)
        {
            _p = p;
        }

        ///<summary>
        /// Counts a segment
        ///</summary>
        /// <param name="p1">An endpoint of the segment</param>
        /// <param name="p2">Another endpoint of the segment</param>
        public void CountSegment(Coordinate p1, Coordinate p2)
        {
            /*
             * For each segment, check if it crosses 
             * a horizontal ray running from the test point in the positive x direction.
             */

            // check if the segment is strictly to the left of the test point
            if (p1.X < _p.X && p2.X < _p.X)
                return;

            // check if the point is equal to the current ring vertex
            if (_p.X == p2.X && _p.Y == p2.Y)
            {
                _isPointOnSegment = true;
                return;
            }
            /*
             * For horizontal segments, check if the point is on the segment.
             * Otherwise, horizontal segments are not counted.
             */
            if (p1.Y == _p.Y && p2.Y == _p.Y)
            {
                double minx = p1.X;
                double maxx = p2.X;
                if (minx > maxx)
                {
                    minx = p2.X;
                    maxx = p1.X;
                }
                if (_p.X >= minx && _p.X <= maxx)
                {
                    _isPointOnSegment = true;
                }
                return;
            }
            /*
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
            if (((p1.Y > _p.Y) && (p2.Y <= _p.Y))
                    || ((p2.Y > _p.Y) && (p1.Y <= _p.Y)))
            {
                // translate the segment so that the test point lies on the origin
                double x1 = p1.X - _p.X;
                double y1 = p1.Y - _p.Y;
                double x2 = p2.X - _p.X;
                double y2 = p2.Y - _p.Y;

                /*
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

        ///<summary>
        /// Reports whether the point lies exactly on one of the supplied segments.
        /// <remarks>
        /// This method may be called at any time as segments are processed. If the result of this method is <c>true</c>, 
        /// no further segments need be supplied, since the result will never change again.
        /// </remarks>
        ///</summary>
        public bool IsOnSegment { get { return _isPointOnSegment; } }

        ///<summary>
        /// Gets the <see cref="GeoAPI.Geometries.Location"/> of the point relative to  the ring, polygon
        /// or multipolygon from which the processed segments were provided.
        ///</summary>
        /// <remarks>This property only determines the correct location if <b>all</b> relevant segments have been processed</remarks>
        public Location Location
        {
            get
            {
                if (_isPointOnSegment)
                    return Location.Boundary;

                // The point is in the interior of the ring if the number of X-crossings is
                // odd.
                if ((_crossingCount%2) == 1)
                {
                    return Location.Interior;
                }
                return Location.Exterior;
            }
        }

        ///<summary>
        /// Tests whether the point lies in or on the ring, polygon
        ///</summary>
        ///<remarks>
        /// This property only determines the correct location if <b>all</b> relevant segments have been processed</remarks>
        public bool IsPointInPolygon
        {
            get { return Location != Location.Exterior; } 
        }
    }
}
