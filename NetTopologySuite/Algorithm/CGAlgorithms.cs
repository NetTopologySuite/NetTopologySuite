using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Specifies and implements various fundamental Computational Geometric algorithms.
    /// The algorithms supplied in this class are robust for double-precision floating point.
    /// </summary>
    [Obsolete("Functionality has been split into Area, Length, Orientation, PointLocation and DistanceComputer classes")]
    public static class CGAlgorithms
    {
        /// <summary>
        /// A value that indicates an orientation of clockwise, or a right turn.
        /// </summary>
        public const int Clockwise          = -1;
        /// <summary>
        /// A value that indicates an orientation of clockwise, or a right turn.
        /// </summary>
        public const int Right              = Clockwise;

        /// <summary>
        /// A value that indicates an orientation of counterclockwise, or a left turn.
        /// </summary>
        public const int CounterClockwise   = 1;
        /// <summary>
        /// A value that indicates an orientation of counterclockwise, or a left turn.
        /// </summary>
        public const int Left               = CounterClockwise;

        /// <summary>
        /// A value that indicates an orientation of collinear, or no turn (straight).
        /// </summary>
        public const int Collinear          = 0;
        /// <summary>
        /// A value that indicates an orientation of collinear, or no turn (straight).
        /// </summary>
        public const int Straight           = Collinear;

        /// <summary>
        /// Returns the index of the direction of the point <c>q</c>
        /// relative to a vector specified by <c>p1-p2</c>.
        /// </summary>
        /// <param name="p1">The origin point of the vector.</param>
        /// <param name="p2">The final point of the vector.</param>
        /// <param name="q">The point to compute the direction to.</param>
        /// <returns>
        /// 1 if q is counter-clockwise (left) from p1-p2,
        /// -1 if q is clockwise (right) from p1-p2,
        /// 0 if q is collinear with p1-p2.
        /// </returns>
        [Obsolete("Use Orientation.Index")]
        public static int OrientationIndex(Coordinate p1, Coordinate p2, Coordinate q)
        {
            /**
             * MD - 9 Aug 2010
             * It seems that the basic algorithm is slightly orientation dependent,
             * when computing the orientation of a point very close to a line.
             * This is possibly due to the arithmetic in the translation to the origin.
             *
             * For instance, the following situation produces identical results
             * in spite of the inverse orientation of the line segment:
             *
             * Coordinate p0 = new Coordinate(219.3649559090992, 140.84159161824724);
             * Coordinate p1 = new Coordinate(168.9018919682399, -5.713787599646864);
             *
             * Coordinate p = new Coordinate(186.80814046338352, 46.28973405831556);
             * int orient = orientationIndex(p0, p1, p);
             * int orientInv = orientationIndex(p1, p0, p);

             * A way to force consistent results is to normalize the orientation of the vector
             * using the following code.
             * However, this may make the results of orientationIndex inconsistent
             * through the triangle of points, so it's not clear this is
             * an appropriate patch.
             *
             */
            return (int)Orientation.Index(p1, p2, q);

            /*
            //Testing only
            return ShewchuksDeterminant.OrientationIndex(p1, p2, q);
             */

            /*
            //previous implementation - not quite fully robust
            return RobustDeterminant.OrientationIndex(p1, p2, q);
             */
        }

        /// <summary>
        /// Tests whether a point lies inside or on a ring.
        /// </summary>
        /// <remarks>
        /// <para>The ring may be oriented in either direction.</para>
        /// <para>A point lying exactly on the ring boundary is considered to be inside the ring.</para>
        /// <para>This method does <i>not</i> first check the point against the envelope
        /// of the ring.</para>
        /// </remarks>
        /// <param name="p">Point to check for ring inclusion.</param>
        /// <param name="ring">An array of <see cref="Coordinate"/>s representing the ring (which must have first point identical to last point)</param>
        /// <returns>true if p is inside ring.</returns>
        [Obsolete("Use PointLocation.IsInRing")]
        public static bool IsPointInRing(Coordinate p, Coordinate[] ring)
        {
            return PointLocation.IsInRing(p, ring);
        }

        /// <summary>
        /// Tests whether a point lies inside or on a ring.
        /// </summary>
        /// <remarks>
        /// <para>The ring may be oriented in either direction.</para>
        /// <para>A point lying exactly on the ring boundary is considered to be inside the ring.</para>
        /// <para>This method does <i>not</i> first check the point against the envelope
        /// of the ring.</para>
        /// </remarks>
        /// <param name="p">Point to check for ring inclusion.</param>
        /// <param name="ring">A sequence of <see cref="Coordinate"/>s representing the ring (which must have first point identical to last point)</param>
        /// <returns>true if p is inside ring.</returns>
        [Obsolete("Use PointLocation.IsInRing")]
        public static bool IsPointInRing(Coordinate p, ICoordinateSequence ring)
        {
            return PointLocation.IsInRing(p, ring);
        }

        ///<summary>
        /// Determines whether a point lies in the interior, on the boundary, or in the exterior of a ring.
        ///</summary>
        /// <remarks>
        /// <para>The ring may be oriented in either direction.</para>
        /// <para>This method does <i>not</i> first check the point against the envelope of the ring.</para>
        /// </remarks>
        /// <param name="p">Point to check for ring inclusion</param>
        /// <param name="ring">An array of coordinates representing the ring (which must have first point identical to last point)</param>
        /// <returns>The <see cref="Location"/> of p relative to the ring</returns>
        [Obsolete("Use PointLocation.LocateInRing")]
        public static Location LocatePointInRing(Coordinate p, Coordinate[] ring)
        {
            return PointLocation.LocateInRing(p, ring);
        }

        ///<summary>
        /// Determines whether a point lies in the interior, on the boundary, or in the exterior of a ring.
        ///</summary>
        /// <remarks>
        /// <para>The ring may be oriented in either direction.</para>
        /// <para>This method does <i>not</i> first check the point against the envelope of the ring.</para>
        /// </remarks>
        /// <param name="p">Point to check for ring inclusion</param>
        /// <param name="ring">A sequence of coordinates representing the ring (which must have first point identical to last point)</param>
        /// <returns>The <see cref="Location"/> of p relative to the ring</returns>
        [Obsolete("Use PointLocation.LocateInRing")]
        public static Location LocatePointInRing(Coordinate p, ICoordinateSequence ring)
        {
            return PointLocation.LocateInRing(p, ring);
        }

        /// <summary>
        /// Tests whether a point lies on the line segments defined by a
        /// list of coordinates.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="pt"></param>
        /// <returns>true if the point is a vertex of the line
        /// or lies in the interior of a line segment in the <c>LineString</c>
        /// </returns>
        [Obsolete("Use PointLocation.IsOnLine")]
        public static bool IsOnLine(Coordinate p, Coordinate[] pt)
        {
            return PointLocation.IsOnLine(p, pt);
        }

        /// <summary>
        /// Computes whether a ring defined by an array of <see cref="Coordinate" />s is oriented counter-clockwise.
        /// </summary>>
        /// <remarks>
        /// <list type="Bullet">
        /// <item>The list of points is assumed to have the first and last points equal.</item>
        /// <item>This will handle coordinate lists which contain repeated points.</item>
        /// </list>
        /// <para>This algorithm is only guaranteed to work with valid rings. If the ring is invalid (e.g. self-crosses or touches), the computed result may not be correct.</para>
        /// </remarks>
        /// <param name="ring">An array of <see cref="Coordinate"/>s forming a ring</param>
        /// <returns>true if the ring is oriented <see cref="Algorithm.OrientationIndex.CounterClockwise"/></returns>
        /// <exception cref="ArgumentException">If there are too few points to determine orientation (&lt;4)</exception>
        [Obsolete("Use Orientation.IsCCW")]
        public static bool IsCCW(Coordinate[] ring)
        {
            return Orientation.IsCCW(ring);
        }

        /// <summary>
        /// Computes whether a ring defined by a coordinate sequence is oriented counter-clockwise.
        /// </summary>>
        /// <remarks>
        /// <list type="Bullet">
        /// <item>The list of points is assumed to have the first and last points equal.</item>
        /// <item>This will handle coordinate lists which contain repeated points.</item>
        /// </list>
        /// <para>This algorithm is only guaranteed to work with valid rings. If the ring is invalid (e.g. self-crosses or touches), the computed result may not be correct.</para>
        /// </remarks>
        /// <param name="ring">A coordinate sequence forming a ring</param>
        /// <returns>true if the ring is oriented <see cref="Algorithm.OrientationIndex.CounterClockwise"/></returns>
        /// <exception cref="ArgumentException">If there are too few points to determine orientation (&lt;4)</exception>
        [Obsolete("Use Orientation.IsCCW")]
        public static bool IsCCW(ICoordinateSequence ring)
        {
            return Orientation.IsCCW(ring);
        }

        /// <summary>
        /// Computes the orientation of a point q to the directed line segment p1-p2.
        /// The orientation of a point relative to a directed line segment indicates
        /// which way you turn to get to q after traveling from p1 to p2.
        /// </summary>
        /// <param name="p1">The first vertex of the line segment</param>
        /// <param name="p2">The second vertex of the line segment</param>
        /// <param name="q">The point to compute the relative orientation of</param>
        /// <returns>
        /// 1 if q is counter-clockwise from p1-p2,
        /// or -1 if q is clockwise from p1-p2,
        /// or 0 if q is collinear with p1-p2
        /// </returns>
        [Obsolete("Use Orientation.Index")]
        public static int ComputeOrientation(Coordinate p1, Coordinate p2, Coordinate q)
        {
            return (int)Orientation.Index(p1, p2, q);
        }

        /// <summary>
        /// Computes the distance from a point p to a line segment AB.
        /// Note: NON-ROBUST!
        /// </summary>
        /// <param name="p">The point to compute the distance for.</param>
        /// <param name="A">One point of the line.</param>
        /// <param name="B">Another point of the line (must be different to A).</param>
        /// <returns> The distance from p to line segment AB.</returns>
        [Obsolete("Use DistanceComputer.PointToSegment")]
        public static double DistancePointLine(Coordinate p, Coordinate A, Coordinate B)
        {
            return DistanceComputer.PointToSegment(p, A, B);
        }

        /// <summary>
        /// Computes the perpendicular distance from a point p
        /// to the (infinite) line containing the points AB
        /// </summary>
        /// <param name="p">The point to compute the distance for.</param>
        /// <param name="A">One point of the line.</param>
        /// <param name="B">Another point of the line (must be different to A).</param>
        /// <returns>The perpendicular distance from p to line AB.</returns>
        [Obsolete("Use DistanceComputer.PointToLinePerpendicular")]
        public static double DistancePointLinePerpendicular(Coordinate p, Coordinate A, Coordinate B)
        {
            return DistanceComputer.PointToLinePerpendicular(p, A, B);
        }

        /// <summary>
        /// Computes the distance from a point to a sequence of line segments.
        /// </summary>
        /// <param name="p">A point</param>
        /// <param name="line">A sequence of contiguous line segments defined by their vertices</param>
        /// <returns>The minimum distance between the point and the line segments</returns>
        /// <exception cref="ArgumentException">If there are too few points to make up a line (at least one?)</exception>
        [Obsolete("Use DistanceComputer.PointToSegmentString")]
        public static double DistancePointLine(Coordinate p, Coordinate[] line)
        {
            return DistanceComputer.PointToSegmentString(p, line);
        }

        /// <summary>
        /// Computes the distance from a line segment AB to a line segment CD.
        /// Note: NON-ROBUST!
        /// </summary>
        /// <param name="A">A point of one line.</param>
        /// <param name="B">The second point of the line (must be different to A).</param>
        /// <param name="C">One point of the line.</param>
        /// <param name="D">Another point of the line (must be different to A).</param>
        /// <returns>The distance from line segment AB to line segment CD.</returns>
        [Obsolete("Use DistanceComputer.SegmentToSegment")]
        public static double DistanceLineLine(Coordinate A, Coordinate B, Coordinate C, Coordinate D)
        {
            return DistanceComputer.SegmentToSegment(A, B, C, D);
        }

        /// <summary>
        /// Computes the signed area for a ring.
        /// <remarks>
        /// <para>
        /// The signed area is
        /// </para>
        /// <list type="Table">
        /// <item>positive</item><description>if the ring is oriented CW</description>
        /// <item>negative</item><description>if the ring is oriented CCW</description>
        /// <item>zero</item><description>if the ring is degenerate or flat</description>
        /// </list>
        /// </remarks>
        /// </summary>
        /// <param name="ring">The coordinates of the ring</param>
        /// <returns>The signed area of the ring</returns>
        [Obsolete("Use Area.OfRingSigned")]
        public static double SignedArea(Coordinate[] ring)
        {
            return Area.OfRingSigned(ring);
        }

        /// <summary>
        /// Computes the signed area for a ring.
        /// <remarks>
        /// <para>
        /// The signed area is
        /// </para>
        /// <list type="Table">
        /// <item>positive</item><description>if the ring is oriented CW</description>
        /// <item>negative</item><description>if the ring is oriented CCW</description>
        /// <item>zero</item><description>if the ring is degenerate or flat</description>
        /// </list>
        /// </remarks>
        /// </summary>
        /// <param name="ring">The coordinates forming the ring</param>
        /// <returns>The signed area of the ring</returns>
        [Obsolete("Use Area.OfRingSigned")]
        public static double SignedArea(ICoordinateSequence ring)
        {
            return Area.OfRingSigned(ring);
        }

        /// <summary>
        /// Computes the length of a <c>LineString</c> specified by a sequence of points.
        /// </summary>
        /// <param name="pts">The points specifying the <c>LineString</c></param>
        /// <returns>The length of the <c>LineString</c></returns>
        [Obsolete("Use Length.OfLine")]
        public static double Length(ICoordinateSequence pts)
        {
            return Algorithm.Length.OfLine(pts);
        }
    }
}
