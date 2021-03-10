namespace NetTopologySuite.Algorithm
{
    using System;
    using System.Text;
    using NetTopologySuite.Geometries;
    using Utilities;

    /// <summary>
    /// A <c>LineIntersector</c> is an algorithm that can both test whether
    /// two line segments intersect and compute the intersection point(s)
    /// if they do.
    /// <para>
    /// There are three possible outcomes when determining whether two line segments intersect:
    /// <list type="bullet">
    /// <item><description><see cref="NoIntersection"/> - the segments do not intersect</description></item>
    /// <item><description><see cref="PointIntersection"/> - the segments intersect in a single point</description></item>
    /// <item><description><see cref="CollinearIntersection"/> - the segments are collinear and they intersect in a line segment</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// For segments which intersect in a single point, the point may be either an endpoint
    /// or in the interior of each segment.
    /// If the point lies in the interior of both segments,
    /// this is termed a <i>proper intersection</i>.
    /// The property <see cref="IsProper"/> test for this situation.
    /// </para><para>
    /// The intersection point(s) may be computed in a precise or non-precise manner.
    /// Computing an intersection point precisely involves rounding it
    /// via a supplied <see cref="PrecisionModel"/>.
    /// </para><para>
    /// LineIntersectors do not perform an initial envelope intersection test
    /// to determine if the segments are disjoint.
    /// This is because this class is likely to be used in a context where
    /// envelope overlap is already known to occur (or be likely).
    /// </para>
    /// </summary>
    public abstract class LineIntersector
    {
        /// <summary>
        /// Indicates that line segments do not intersect
        /// </summary>
        public const int NoIntersection = 0;

        /// <summary>
        /// Indicates that line segments intersect in a single point
        /// </summary>
        public const int PointIntersection = 1;

        /// <summary>
        /// Indicates that line segments intersect in a line segment
        /// </summary>
        public const int CollinearIntersection = 2;

        /// <summary>
        /// Computes the "edge distance" of an intersection point p along a segment.
        /// The edge distance is a metric of the point along the edge.
        /// The metric used is a robust and easy to compute metric function.
        /// It is not equivalent to the usual Euclidean metric.
        /// It relies on the fact that either the x or the y ordinates of the
        /// points in the edge are unique, depending on whether the edge is longer in
        /// the horizontal or vertical direction.
        /// NOTE: This function may produce incorrect distances
        /// for inputs where p is not precisely on p1-p2
        /// (E.g. p = (139,9) p1 = (139,10), p2 = (280,1) produces distance 0.0, which is incorrect.
        /// My hypothesis is that the function is safe to use for points which are the
        /// result of rounding points which lie on the line, but not safe to use for truncated points.
        /// </summary>
        public static double ComputeEdgeDistance(Coordinate p, Coordinate p0, Coordinate p1)
        {
            double dx = Math.Abs(p1.X - p0.X);
            double dy = Math.Abs(p1.Y - p0.Y);

            double dist = -1.0;   // sentinel value
            if (p.Equals(p0))
                dist = 0.0;
            else if (p.Equals(p1))
            {
                dist = dx > dy ? dx : dy;
            }
            else
            {
                double pdx = Math.Abs(p.X - p0.X);
                double pdy = Math.Abs(p.Y - p0.Y);
                dist = dx > dy ? pdx : pdy;

                // <FIX>: hack to ensure that non-endpoints always have a non-zero distance
                if (dist == 0.0 && ! p.Equals2D(p0))
                    dist = Math.Max(pdx, pdy);

            }
            Assert.IsTrue(!(dist == 0.0 && ! p.Equals(p0)), "Bad distance calculation");
            return dist;
        }

        /// <summary>
        /// This function is non-robust, since it may compute the square of large numbers.
        /// Currently not sure how to improve this.
        /// </summary>
        public static double NonRobustComputeEdgeDistance(Coordinate p, Coordinate p1, Coordinate p2)
        {
            double dx = p.X - p1.X;
            double dy = p.Y - p1.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);   // dummy value
            Assert.IsTrue(! (dist == 0.0 && ! p.Equals(p1)), "Invalid distance calculation");
            return dist;
        }

        /// <summary>
        /// A value indicating the intersection result
        /// <para/>
        /// Possible values are:
        /// <list type="bullet">
        /// <item><description><see cref="NoIntersection"/></description></item>
        /// <item><description><see cref="PointIntersection"/></description></item>
        /// <item><description><see cref="CollinearIntersection"/></description></item>
        /// </list>
        /// </summary>
        protected int Result;

        /// <summary>
        /// Array of coordinate arrays forming the lines
        /// </summary>
        protected Coordinate[][] InputLines;

        /// <summary>
        /// Array of
        /// </summary>
        protected Coordinate[] IntersectionPoint = new Coordinate[2];

        /// <summary>
        /// The indexes of the endpoints of the intersection lines, in order along
        /// the corresponding line
        /// </summary>
        protected int[] IntersectionLineIndex;

        private bool _isProper;

        /// <summary>
        /// Alias the <see cref="IntersectionPoint"/>[0] for ease of reference
        /// </summary>
        protected Coordinate Pa => IntersectionPoint[0];

        /// <summary>
        /// Alias the <see cref="IntersectionPoint"/>[1] for ease of reference
        /// </summary>
        protected Coordinate Pb => IntersectionPoint[1];

        /// <summary>
        /// If MakePrecise is true, computed intersection coordinates will be made precise
        /// using <c>Coordinate.MakePrecise</c>.
        /// </summary>
        private PrecisionModel _precisionModel;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        protected LineIntersector()
        {
            InputLines = new Coordinate[2][];
            InputLines[0] = new Coordinate[2];
            InputLines[1] = new Coordinate[2];

            IntersectionPoint[0] = new Coordinate();
            IntersectionPoint[1] = new Coordinate();
            //// alias the intersection points for ease of reference
            //Pa = this.IntersectionPoint[0];
            //Pb = this.IntersectionPoint[1];
            Result = 0;
        }

        /// <summary>
        /// Force computed intersection to be rounded to a given precision model.
        /// No getter is provided, because the precision model is not required to be specified.
        /// </summary>
        public PrecisionModel PrecisionModel
        {
            get => _precisionModel;
            set => _precisionModel = value;
        }

        /// <summary>
        /// Gets an endpoint of an input segment.
        /// </summary>
        /// <param name="segmentIndex">the index of the input segment (0 or 1)</param>
        /// <param name="ptIndex">the index of the endpoint (0 or 1)</param>
        /// <returns>The specified endpoint</returns>
        public Coordinate GetEndpoint(int segmentIndex, int ptIndex)
        {
            return InputLines[segmentIndex][ptIndex];
        }

        /// <summary>
        /// Compute the intersection of a point p and the line p1-p2.
        /// This function computes the bool value of the hasIntersection test.
        /// The actual value of the intersection (if there is one)
        /// is equal to the value of <c>p</c>.
        /// </summary>
        public abstract void ComputeIntersection(Coordinate p, Coordinate p1, Coordinate p2);

        /// <summary>
        /// Gets a value indicating if the computed intersection is collinear
        /// </summary>
        protected bool IsCollinear => Result == CollinearIntersection;

        /// <summary>
        /// Computes the intersection of the lines p1-p2 and p3-p4.
        /// This function computes both the bool value of the hasIntersection test
        /// and the (approximate) value of the intersection point itself (if there is one).
        /// </summary>
        /// <param name="p1">The 1st point of the 1st segment</param>
        /// <param name="p2">The 2nd point of the 1st segment</param>
        /// <param name="p3">The 1st point of the 2nd segment</param>
        /// <param name="p4">The 2nd point of the 2nd segment</param>
        public void ComputeIntersection(Coordinate p1, Coordinate p2, Coordinate p3, Coordinate p4)
        {
            //this.inputLines = new[] { p1, p2, p3, p4 };
            InputLines[0][0] = p1;
            InputLines[0][1] = p2;
            InputLines[1][0] = p3;
            InputLines[1][1] = p4;

            Result = ComputeIntersect(p1, p2, p3, p4);
        }

        /// <summary>
        /// Computes the intersection of two line segments, one defined by <paramref name="p1"/> and <paramref name="p2"/>,
        /// the other by <paramref name="q1"/> and <paramref name="q2"/>.
        /// </summary>
        /// <param name="p1">The 1st point of the 1st segment</param>
        /// <param name="p2">The 2nd point of the 1st segment</param>
        /// <param name="q1">The 1st point of the 2nd segment</param>
        /// <param name="q2">The 2nd point of the 2nd segment</param>
        /// <returns></returns>
        /// <remarks>
        /// Don't use this function directly, it is not meant for public use.
        /// Please call <see cref="ComputeIntersection(Coordinate,Coordinate,Coordinate,Coordinate)"/>
        /// and test <see cref="HasIntersection"/> or <see cref="IsCollinear"/> along with <see cref="IsProper"/> and
        /// <see cref="IsEndPoint"/>.
        /// </remarks>
        // TODO Change to protected!
        public abstract int ComputeIntersect(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2);

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(InputLines[0]).Append("-");
            sb.Append(InputLines[1]).Append(" ");
            sb.Append(InputLines[2]).Append("-");
            sb.Append(InputLines[3]).Append(" : ");

            if (IsEndPoint)  sb.Append(" endpoint");
            if (_isProper)   sb.Append(" proper");
            if (IsCollinear) sb.Append(" collinear");

            return sb.ToString();
        }

        /// <summary>
        /// Gets a value indicating if the intersection is an end-point intersection
        /// </summary>
        protected bool IsEndPoint => HasIntersection && !_isProper;

        /// <summary>
        /// Tests whether the input geometries intersect.
        /// </summary>
        /// <returns><c>true</c> if the input geometries intersect.</returns>
        public bool HasIntersection => Result != NoIntersection;

        /// <summary>
        /// Returns the number of intersection points found.  This will be either 0, 1 or 2.
        /// </summary>
        /// <returns>The number of intersection points found (0, 1, or 2)</returns>
        public int IntersectionNum => Result;

        /// <summary>
        /// Returns the intIndex'th intersection point.
        /// </summary>
        /// <param name="intIndex">is 0 or 1.</param>
        /// <returns>The intIndex'th intersection point.</returns>
        public Coordinate GetIntersection(int intIndex)
        {
            return IntersectionPoint[intIndex];
        }

        /// <summary>
        /// Computes the <see cref="IntersectionLineIndex"/> values.
        /// </summary>
        protected void ComputeIntLineIndex()
        {
            if (IntersectionLineIndex != null)
                return;

            IntersectionLineIndex = new int[4];
            ComputeIntLineIndex(0);
            ComputeIntLineIndex(1);
        }

        /// <summary>
        /// Test whether a point is a intersection point of two line segments.
        /// Note that if the intersection is a line segment, this method only tests for
        /// equality with the endpoints of the intersection segment.
        /// It does not return true if the input point is internal to the intersection segment.
        /// </summary>
        /// <returns><c>true</c> if the input point is one of the intersection points.</returns>
        public bool IsIntersection(Coordinate pt)
        {
            for (int i = 0; i < Result; i++)
                if (IntersectionPoint[i].Equals2D(pt))
                    return true;
            return false;
        }

        /// <summary>
        /// Tests whether either intersection point is an interior point of one of the input segments.
        /// </summary>
        /// <returns>
        /// <c>true</c> if either intersection point is in the interior of one of the input segment.
        /// </returns>
        public bool IsInteriorIntersection()
        {
            if (IsInteriorIntersection(0))
                return true;
            if (IsInteriorIntersection(1))
                return true;
            return false;
        }

        /// <summary>
        /// Tests whether either intersection point is an interior point of the specified input segment.
        /// </summary>
        /// <returns>
        /// <c>true</c> if either intersection point is in the interior of the input segment.
        /// </returns>
        public bool IsInteriorIntersection(int inputLineIndex)
        {
            for (int i = 0; i < Result; i++)
            {
                if (!(IntersectionPoint[i].Equals2D(InputLines[inputLineIndex][0]) ||
                      IntersectionPoint[i].Equals2D(InputLines[inputLineIndex][1])))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Tests whether an intersection is proper.
        /// The intersection between two line segments is considered proper if
        /// they intersect in a single point in the interior of both segments
        /// (e.g. the intersection is a single point and is not equal to any of the endpoints).
        /// The intersection between a point and a line segment is considered proper
        /// if the point lies in the interior of the segment (e.g. is not equal to either of the endpoints).
        /// </summary>
        /// <returns><c>true</c>  if the intersection is proper.</returns>
        public bool IsProper
        {
            get => HasIntersection && _isProper;
            protected set => _isProper = value;
        }

        /// <summary>
        /// Computes the intIndex'th intersection point in the direction of
        /// a specified input line segment.
        /// </summary>
        /// <param name="segmentIndex">is 0 or 1.</param>
        /// <param name="intIndex">is 0 or 1.</param>
        /// <returns>
        /// The intIndex'th intersection point in the direction of the specified input line segment.
        /// </returns>
        public Coordinate GetIntersectionAlongSegment(int segmentIndex, int intIndex)
        {
            // lazily compute int line array
            ComputeIntLineIndex();
            int index = segmentIndex == 0 ? 0 : 2;
            return IntersectionPoint[IntersectionLineIndex[index + intIndex]];
        }

        /// <summary>
        /// Computes the index (order) of the intIndex'th intersection point in the direction of
        /// a specified input line segment.
        /// </summary>
        /// <param name="segmentIndex">is 0 or 1.</param>
        /// <param name="intIndex">is 0 or 1.</param>
        /// <returns>
        /// The index of the intersection point along the segment (0 or 1).
        /// </returns>
        public int GetIndexAlongSegment(int segmentIndex, int intIndex)
        {
            ComputeIntLineIndex();
            int index = segmentIndex == 0 ? 0 : 2;
            return IntersectionLineIndex[index + intIndex];
        }

        /// <summary>
        /// Computes the intersection line index
        /// </summary>
        /// <param name="segmentIndex">The segment index</param>
        protected void ComputeIntLineIndex(int segmentIndex)
        {
            double dist0 = GetEdgeDistance(segmentIndex, 0);
            double dist1 = GetEdgeDistance(segmentIndex, 1);
            if (dist0 > dist1)
            {
                int index = segmentIndex == 0 ? 0 : 2;
                IntersectionLineIndex[index] = 0;
                IntersectionLineIndex[index + 1] = 1;
            }
            else
            {
                int index = segmentIndex == 0 ? 0 : 2;
                IntersectionLineIndex[index] = 1;
                IntersectionLineIndex[index + 1] = 0;
            }
        }

        /// <summary>
        /// Computes the "edge distance" of an intersection point along the specified input line segment.
        /// </summary>
        /// <param name="segmentIndex">is 0 or 1.</param>
        /// <param name="intIndex">is 0 or 1.</param>
        /// <returns>The edge distance of the intersection point.</returns>
        public double GetEdgeDistance(int segmentIndex, int intIndex)
        {
            double dist = ComputeEdgeDistance(IntersectionPoint[intIndex], InputLines[segmentIndex][0],
                InputLines[segmentIndex][1]);
            return dist;
        }
    }
}
