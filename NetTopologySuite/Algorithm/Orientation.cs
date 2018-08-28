using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Functions to compute the orientation of basic geometric structures
    /// including point triplets(triangles) and rings.
    /// Orientation is a fundamental property of planar geometries
    /// (and more generally geometry on two-dimensional manifolds).
    /// <para/>
    /// Orientation is notoriously subject to numerical precision errors
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
        /// <list type="Table">
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
            /**
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
        /// Computes whether a ring defined by an array of <see cref="Coordinate"/>s is
        /// oriented counter-clockwise.
        /// <list type="Bullet">
        /// <item>The list of points is assumed to have the first and last points equal.</item>
        /// <item>This will handle coordinate lists which contain repeated points.</item>
        /// </list>
        /// This algorithm is <b>only</b> guaranteed to work with valid rings.If the
        /// ring is invalid(e.g.self-crosses or touches), the computed result may not
        /// be correct.
        /// </summary>
        /// <param name="ring">An array of <c>Coordinate</c>s forming a ring.</param>
        /// <returns><c>true</c> if the ring is oriented counter-clockwise.</returns>
        /// <exception cref="ArgumentException">Thrown if there are too few points to determine orientation (&lt; 4)</exception>
        public static bool IsCCW(Coordinate[] ring)
        {
            // # of points without closing endpoint
            int nPts = ring.Length - 1;
            // sanity check
            if (nPts < 3)
                throw new ArgumentException(
                    "Ring has fewer than 4 points, so orientation cannot be determined",
                    nameof(ring));

            // find highest point
            var hiPt = ring[0];
            int hiIndex = 0;
            for (int i = 1; i <= nPts; i++)
            {
                var p = ring[i];
                if (p.Y > hiPt.Y)
                {
                    hiPt = p;
                    hiIndex = i;
                }
            }

            // find distinct point before highest point
            int iPrev = hiIndex;
            do
            {
                iPrev = iPrev - 1;
                if (iPrev < 0)
                    iPrev = nPts;
            } while (ring[iPrev].Equals2D(hiPt) && iPrev != hiIndex);

            // find distinct point after highest point
            int iNext = hiIndex;
            do
            {
                iNext = (iNext + 1) % nPts;
            } while (ring[iNext].Equals2D(hiPt) && iNext != hiIndex);

            var prev = ring[iPrev];
            var next = ring[iNext];

            /**
             * This check catches cases where the ring contains an A-B-A configuration
             * of points. This can happen if the ring does not contain 3 distinct points
             * (including the case where the input array has fewer than 4 elements), or
             * it contains coincident line segments.
             */
            if (prev.Equals2D(hiPt) || next.Equals2D(hiPt) || prev.Equals2D(next))
                return false;

            var disc = Index(prev, hiPt, next);

            /**
             * If disc is exactly 0, lines are collinear. There are two possible cases:
             * (1) the lines lie along the x axis in opposite directions (2) the lines
             * lie on top of one another
             *
             * (1) is handled by checking if next is left of prev ==> CCW (2) will never
             * happen if the ring is valid, so don't check for it (Might want to assert
             * this)
             */
            bool isCCW;
            if (disc == OrientationIndex.Collinear)
            {
                // poly is CCW if prev x is right of next x
                isCCW = (prev.X > next.X);
            }
            else
            {
                // if area is positive, points are ordered CCW
                isCCW = (disc > 0);
            }
            return isCCW;
        }

        /// <summary>
        /// Computes whether a ring defined by an <see cref="ICoordinateSequence"/> is
        /// oriented counter-clockwise.
        /// <list type="Bullet">
        /// <item>The list of points is assumed to have the first and last points equal.</item>
        /// <item>This will handle coordinate lists which contain repeated points.</item>
        /// </list>
        /// This algorithm is <b>only</b> guaranteed to work with valid rings.If the
        /// ring is invalid(e.g.self-crosses or touches), the computed result may not
        /// be correct.
        /// </summary>
        /// <param name="ring">A <c>CoordinateSequence</c>s forming a ring.</param>
        /// <returns><c>true</c> if the ring is oriented counter-clockwise.</returns>
        /// <exception cref="ArgumentException">Thrown if there are too few points to determine orientation (&lt; 4)</exception>
        public static bool IsCCW(ICoordinateSequence ring)
        {
            // # of points without closing endpoint
            int nPts = ring.Count - 1;
            // sanity check
            if (nPts < 3)
                throw new ArgumentException(
                    "Ring has fewer than 4 points, so orientation cannot be determined",
                    nameof(ring));

            // find highest point
            var hiPt = ring.GetCoordinate(0);
            int hiIndex = 0;
            for (int i = 1; i <= nPts; i++)
            {
                var p = ring.GetCoordinate(i);
                if (p.Y > hiPt.Y)
                {
                    hiPt = ring.GetCoordinate(i);
                    hiIndex = i;
                }
            }

            // find distinct point before highest point
            Coordinate prev;
            int iPrev = hiIndex;
            do
            {
                iPrev = iPrev - 1;
                if (iPrev < 0)
                    iPrev = nPts;
                prev = ring.GetCoordinate(iPrev);
            } while (prev.Equals2D(hiPt) && iPrev != hiIndex);

            // find distinct point after highest point
            Coordinate next;
            int iNext = hiIndex;
            do
            {
                iNext = (iNext + 1) % nPts;
                next = ring.GetCoordinate(iNext);
            } while (next.Equals2D(hiPt) && iNext != hiIndex);

            /**
             * This check catches cases where the ring contains an A-B-A configuration
             * of points. This can happen if the ring does not contain 3 distinct points
             * (including the case where the input array has fewer than 4 elements), or
             * it contains coincident line segments.
             */
            if (prev.Equals2D(hiPt) || next.Equals2D(hiPt) || prev.Equals2D(next))
                return false;

            var disc = Index(prev, hiPt, next);

            /**
             * If disc is exactly 0, lines are collinear. There are two possible cases:
             * (1) the lines lie along the x axis in opposite directions (2) the lines
             * lie on top of one another
             *
             * (1) is handled by checking if next is left of prev ==> CCW (2) will never
             * happen if the ring is valid, so don't check for it (Might want to assert
             * this)
             */
            bool isCCW;
            if (disc == OrientationIndex.Collinear)
            {
                // polygon is CCW if previous x is right of next x
                isCCW = (prev.X > next.X);
            }
            else
            {
                // if area is positive, points are ordered CCW
                isCCW = (disc > 0);
            }
            return isCCW;
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
