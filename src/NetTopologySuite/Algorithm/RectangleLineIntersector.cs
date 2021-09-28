using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{

    /// <summary>
    /// Computes whether a rectangle intersects line segments.
    /// </summary>
    /// <remarks>
    /// Rectangles contain a large amount of inherent symmetry
    /// (or to put it another way, although they contain four
    /// coordinates they only actually contain 4 ordinates
    /// worth of information).
    /// The algorithm used takes advantage of the symmetry of
    /// the geometric situation
    /// to optimize performance by minimizing the number
    /// of line intersection tests.
    /// </remarks>
    /// <author>Martin Davis</author>
    public class RectangleLineIntersector
    {
        // for intersection testing, don't need to set precision model
        private readonly LineIntersector _li = new RobustLineIntersector();

        private readonly Envelope _rectEnv;

        private readonly Coordinate _diagUp0;
        private readonly Coordinate _diagUp1;
        private readonly Coordinate _diagDown0;
        private readonly Coordinate _diagDown1;

        /// <summary>
        /// Creates a new intersector for the given query rectangle,
        /// specified as an <see cref="Envelope"/>.
        /// </summary>
        /// <param name="rectEnv">The query rectangle, specified as an Envelope</param>
        public RectangleLineIntersector(Envelope rectEnv)
        {
            _rectEnv = rectEnv;
            /*
             * Up and Down are the diagonal orientations
             * relative to the Left side of the rectangle.
             * Index 0 is the left side, 1 is the right side.
             */
            _diagUp0 = new Coordinate(rectEnv.MinX, rectEnv.MinY);
            _diagUp1 = new Coordinate(rectEnv.MaxX, rectEnv.MaxY);
            _diagDown0 = new Coordinate(rectEnv.MinX, rectEnv.MaxY);
            _diagDown1 = new Coordinate(rectEnv.MaxX, rectEnv.MinY);
        }

        /// <summary>
        /// Tests whether the query rectangle intersects a given line segment.
        /// </summary>
        /// <param name="p0">The first endpoint of the segment</param>
        /// <param name="p1">The second endpoint of the segment</param>
        /// <returns><c>true</c> if the rectangle intersects the segment</returns>
        public bool Intersects(Coordinate p0, Coordinate p1)
        {
            // TODO: confirm that checking envelopes first is faster

            /*
             * If the segment envelope is disjoint from the
             * rectangle envelope, there is no intersection
             */
            var segEnv = new Envelope(p0, p1);
            if (!_rectEnv.Intersects(segEnv))
                return false;

            /*
             * If either segment endpoint lies in the rectangle,
             * there is an intersection.
             */
            if (_rectEnv.Intersects(p0)) return true;
            if (_rectEnv.Intersects(p1)) return true;

            /*
             * Normalize segment.
             * This makes p0 less than p1,
             * so that the segment runs to the right,
             * or vertically upwards.
             */
            if (p0.CompareTo(p1) > 0)
            {
                var tmp = p0;
                p0 = p1;
                p1 = tmp;
            }
            /*
             * Compute angle of segment.
             * Since the segment is normalized to run left to right,
             * it is sufficient to simply test the Y ordinate.
             * "Upwards" means relative to the left end of the segment.
             */
            bool isSegUpwards = p1.Y > p0.Y;

            /*
             * Since we now know that neither segment endpoint
             * lies in the rectangle, there are two possible
             * situations:
             * 1) the segment is disjoint to the rectangle
             * 2) the segment crosses the rectangle completely.
             *
             * In the case of a crossing, the segment must intersect
             * a diagonal of the rectangle.
             *
             * To distinguish these two cases, it is sufficient
             * to test intersection with
             * a single diagonal of the rectangle,
             * namely the one with slope "opposite" to the slope
             * of the segment.
             * (Note that if the segment is axis-parallel,
             * it must intersect both diagonals, so this is
             * still sufficient.)
             */
            if (isSegUpwards)
            {
                _li.ComputeIntersection(p0, p1, _diagDown0, _diagDown1);
            }
            else
            {
                _li.ComputeIntersection(p0, p1, _diagUp0, _diagUp1);
            }
            if (_li.HasIntersection)
                return true;
            return false;

        }
    }
}
