using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Functions to compute the orientation of basic geometric structures
    /// including point triplets(triangles) and rings.
    /// Orientation is a fundamental property of planar geometries
    /// (and more generally geometry on two-dimensional manifolds).
    /// <para/>
    /// Determining triangle orientation
    /// is notoriously subject to numerical precision errors
    /// in the case of collinear or nearly collinear points.
    /// NTS uses extended-precision arithmetic to increase
    /// the robustness of the computation.
    /// </summary>
    /// <author>
    /// Martin Davis
    /// </author>
    public static class Orientation
    {
        /// <summary>
        /// Returns the orientation index of the direction of the point <paramref name="q"/> relative to
        /// a directed infinite line specified by <c>p1-&gt;p2</c>.
        /// The index indicates whether the point lies to the <see cref="OrientationIndex.Left"/>
        /// or <see cref="OrientationIndex.Right"/> of the line, or lies on it <see cref="OrientationIndex.Collinear"/>.
        /// The index also indicates the orientation of the triangle formed by the three points
        /// (<see cref="OrientationIndex.CounterClockwise"/>, <see cref="OrientationIndex.Clockwise"/>, or
        /// <see cref="OrientationIndex.Straight"/> )
        /// </summary>
        /// <param name="p1">The origin point of the line vector</param>
        /// <param name="p2">The final point of the line vector</param>
        /// <param name="q">The point to compute the direction to</param>
        /// <returns>
        /// The <see cref="OrientationIndex"/> of q in regard to the vector <c>p1-&gt;p2</c>
        /// <list type="table">
        /// <listheader>
        /// <term>Value</term><description>Description</description>
        /// </listheader>
        /// <item>
        /// <term><see cref="OrientationIndex.Collinear"/>, <see cref="OrientationIndex.Straight"/></term>
        /// <description><paramref name="q"/> is collinear with <c>p1-&gt;p2</c></description>
        /// </item>
        /// <item>
        /// <term><see cref="OrientationIndex.Clockwise"/>, <see cref="OrientationIndex.Right"/></term>
        /// <description><paramref name="q"/> is clockwise (right) from <c>p1-&gt;p2</c></description>
        /// </item>
        /// <item>
        /// <term><see cref="OrientationIndex.CounterClockwise"/>, <see cref="OrientationIndex.Left"/></term>
        /// <description><paramref name="q"/> is counter-clockwise (left) from <c>p1-&gt;p2</c></description>
        /// </item>
        /// </list>
        /// </returns>
        public static OrientationIndex Index(Coordinate p1, Coordinate p2, Coordinate q)
        {
            /*
             * MD - 9 Aug 2010 It seems that the basic algorithm is slightly orientation
             * dependent, when computing the orientation of a point very close to a
             * line. This is possibly due to the arithmetic in the translation to the
             * origin.
             *
             * For instance, the following situation produces identical results in spite
             * of the inverse orientation of the line segment:
             *
             * Coordinate p0 = new Coordinate(219.3649559090992, 140.84159161824724);
             * Coordinate p1 = new Coordinate(168.9018919682399, -5.713787599646864);
             *
             * Coordinate p = new Coordinate(186.80814046338352, 46.28973405831556); int
             * orient = orientationIndex(p0, p1, p); int orientInv =
             * orientationIndex(p1, p0, p);
             *
             * A way to force consistent results is to normalize the orientation of the
             * vector using the following code. However, this may make the results of
             * orientationIndex inconsistent through the triangle of points, so it's not
             * clear this is an appropriate patch.
             *
             */
            var res = (OrientationIndex)CGAlgorithmsDD.OrientationIndex(p1, p2, q);
            return res; //(Orientation)CGAlgorithmsDD.OrientationIndex(p1, p2, q);
            // testing only
            //return ShewchuksDeterminant.orientationIndex(p1, p2, q);
            // previous implementation - not quite fully robust
            //return RobustDeterminant.orientationIndex(p1, p2, q);
        }

        /// <summary>
        /// Tests if a ring defined by an array of <see cref="Coordinate"/>s is
        /// oriented counter-clockwise.
        /// <list type="bullet">
        /// <item><description>The list of points is assumed to have the first and last points equal.</description></item>
        /// <item><description>This handles coordinate lists which contain repeated points.</description></item>
        /// <item><description>This handles rings which contain collapsed segments (in particular, along the top of the ring).</description></item>
        /// </list>
        /// This algorithm is guaranteed to work with valid rings.
        /// It also works with "mildly invalid" rings
        /// which contain collapsed(coincident) flat segments along the top of the ring.
        /// If the ring is "more" invalid (e.g.self-crosses or touches),
        /// the computed result may not be correct.
        /// </summary>
        /// <param name="ring">An array of <c>Coordinate</c>s forming a ring (with first and last point identical)</param>
        /// <returns><c>true</c> if the ring is oriented counter-clockwise.</returns>
        public static bool IsCCW(Coordinate[] ring)
        {
            // wrap with an XY CoordinateSequence
            return IsCCW(new CoordinateArraySequence(ring, 2, 0));
        }

        /// <summary>
        /// Tests if a ring defined by a <see cref="CoordinateSequence"/> is
        /// oriented counter-clockwise.
        /// <list type="bullet">
        /// <item><description>The list of points is assumed to have the first and last points equal.</description></item>
        /// <item><description>This handles coordinate lists which contain repeated points.</description></item>
        /// <item><description>This handles rings which contain collapsed segments (in particular, along the top of the ring).</description></item>
        /// </list>
        /// This algorithm is guaranteed to work with valid rings.
        /// It also works with "mildly invalid" rings
        /// which contain collapsed(coincident) flat segments along the top of the ring.
        /// If the ring is "more" invalid (e.g.self-crosses or touches),
        /// the computed result may not be correct.
        /// </summary>
        /// <param name="ring">A <c>CoordinateSequence</c>s forming a ring (with first and last point identical).</param>
        /// <returns><c>true</c> if the ring is oriented counter-clockwise.</returns>
        public static bool IsCCW(CoordinateSequence ring)
        {
            // # of points without closing endpoint
            int nPts = ring.Count - 1;
            // return default value if ring is flat
            if (nPts < 3) return false;

            /*
             * Find first highest point after a lower point, if one exists
             * (e.g. a rising segment)
             * If one does not exist, hiIndex will remain 0
             * and the ring must be flat.
             * Note this relies on the convention that
             * rings have the same start and end point. 
             */
            var upHiPt = ring.GetCoordinate(0);
            double prevY = upHiPt.Y;
            Coordinate upLowPt = null;
            int iUpHi = 0;
            for (int i = 1; i <= nPts; i++)
            {
                double py = ring.GetOrdinate(i, Ordinate.Y);
                /*
                 * If segment is upwards and endpoint is higher, record it
                 */
                if (py > prevY && py >= upHiPt.Y)
                {
                    upHiPt = ring.GetCoordinate(i);
                    iUpHi = i;
                    upLowPt = ring.GetCoordinate(i - 1);
                }
                prevY = py;
            }
            /*
             * Check if ring is flat and return default value if so
             */
            if (iUpHi == 0) return false;

            /*
             * Find the next lower point after the high point
             * (e.g. a falling segment).
             * This must exist since ring is not flat.
             */
            int iDownLow = iUpHi;
            do
            {
                iDownLow = (iDownLow + 1) % nPts;
            } while (iDownLow != iUpHi && ring.GetOrdinate(iDownLow, Ordinate.Y) == upHiPt.Y);

            var downLowPt = ring.GetCoordinate(iDownLow);
            int iDownHi = iDownLow > 0 ? iDownLow - 1 : nPts - 1;
            var downHiPt = ring.GetCoordinate(iDownHi);

            /*
             * Two cases can occur:
             * 1) the hiPt and the downPrevPt are the same.  
             *    This is the general position case of a "pointed cap".
             *    The ring orientation is determined by the orientation of the cap
             * 2) The hiPt and the downPrevPt are different.
             *    In this case the top of the cap is flat.
             *    The ring orientation is given by the direction of the flat segment
             */
            if (upHiPt.Equals2D(downHiPt))
            {
                /*
                 * Check for the case where the cap has configuration A-B-A. 
                 * This can happen if the ring does not contain 3 distinct points
                 * (including the case where the input array has fewer than 4 elements), or
                 * it contains coincident line segments.
                 */
                if (upLowPt.Equals2D(upHiPt) || downLowPt.Equals2D(upHiPt) || upLowPt.Equals2D(downLowPt))
                    return false;

                /*
                 * It can happen that the top segments are coincident.
                 * This is an invalid ring, which cannot be computed correctly.
                 * In this case the orientation is 0, and the result is false.
                 */
                var index = Index(upLowPt, upHiPt, downLowPt);
                return index == OrientationIndex.CounterClockwise;
            }
            else
            {
                /*
                 * Flat cap - direction of flat top determines orientation
                 */
                double delX = downHiPt.X - upHiPt.X;
                return delX < 0;
            }
        }

        /// <summary>
        /// Tests if a ring defined by an array of <see cref="Coordinate"/>s is
        /// oriented counter-clockwise, using the signed area of the ring.
        /// <list type="bullet">
        /// <item><description>The list of points is assumed to have the first and last points equal.</description></item>
        /// <item><description>This handles coordinate lists which contain repeated points.</description></item>
        /// <item><description>This handles rings which contain collapsed segments 
        ///    (in particular, along the top of the ring).</description></item>
        /// <item><description>This handles rings which are invalid due to self-intersection</description></item>
        /// </list>
        /// This algorithm is guaranteed to work with valid rings.
        /// For invalid rings (containing self-intersections),
        /// the algorithm determines the orientation of
        /// the largest enclosed area (including overlaps).
        /// This provides a more useful result in some situations, such as buffering.
        /// <para/>
        /// However, this approach may be less accurate in the case of
        /// rings with almost zero area.
        /// (Note that the orientation of rings with zero area is essentially
        /// undefined, and hence non-deterministic.)
        /// </summary>
        /// <param name="ring">An array of Coordinates forming a ring (with first and last point identical)</param>
        /// <returns><c>true</c> if the ring is oriented counter-clockwise.</returns>
        public static bool IsCCWArea(Coordinate[] ring)
        {
            return Area.OfRingSigned(ring) < 0;
        }

        /// <summary>
        /// Tests if a ring defined by a <see cref="CoordinateSequence"/> is
        /// oriented counter-clockwise, using the signed area of the ring.
        /// <list type="bullet">
        /// <item><description>The list of points is assumed to have the first and last points equal.</description></item>
        /// <item><description>This handles coordinate lists which contain repeated points.</description></item>
        /// <item><description>This handles rings which contain collapsed segments 
        ///    (in particular, along the top of the ring).</description></item>
        /// <item><description>This handles rings which are invalid due to self-intersection</description></item>
        /// </list>
        /// This algorithm is guaranteed to work with valid rings.
        /// For invalid rings (containing self-intersections),
        /// the algorithm determines the orientation of
        /// the largest enclosed area (including overlaps).
        /// This provides a more useful result in some situations, such as buffering.
        /// <para/>
        /// However, this approach may be less accurate in the case of
        /// rings with almost zero area.
        /// (Note that the orientation of rings with zero area is essentially
        /// undefined, and hence non-deterministic.)
        /// </summary>
        /// <param name="ring">An array of Coordinates forming a ring (with first and last point identical)</param>
        /// <returns><c>true</c> if the ring is oriented counter-clockwise.</returns>
        public static bool IsCCWArea(CoordinateSequence ring)
        {
            return Area.OfRingSigned(ring) < 0;
        }
        /// <summary>
        /// Re-orients an orientation.
        /// </summary>
        /// <param name="orientation">The orientation</param>
        /// <returns></returns>
        public static OrientationIndex ReOrient(OrientationIndex orientation)
        {
            return (OrientationIndex)(-(int) orientation);
        }

    }
}
