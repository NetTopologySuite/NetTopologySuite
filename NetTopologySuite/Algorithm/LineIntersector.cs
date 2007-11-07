using System;
using System.Text;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// A LineIntersector is an algorithm that can both test whether
    /// two line segments intersect and compute the intersection point
    /// if they do.
    /// The intersection point may be computed in a precise or non-precise manner.
    /// Computing it precisely involves rounding it to an integer.  (This assumes
    /// that the input coordinates have been made precise by scaling them to
    /// an integer grid.)
    /// </summary>
    public abstract class LineIntersector
    {
        public const Int32 DontIntersect = 0;

        public const Int32 DoIntersect = 1;

        public const Int32 Collinear = 2;

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
        /// (E.g. p = (139,9) p1 = (139,10), p2 = (280,1) produces distanct 0.0, which is incorrect.
        /// My hypothesis is that the function is safe to use for points which are the
        /// result of rounding points which lie on the line, but not safe to use for truncated points.
        /// </summary>
        public static Double ComputeEdgeDistance(ICoordinate p, ICoordinate p0, ICoordinate p1)
        {
            Double dx = Math.Abs(p1.X - p0.X);
            Double dy = Math.Abs(p1.Y - p0.Y);

            Double dist = -1.0; // sentinel value
           
            if (p.Equals(p0))
            {
                dist = 0.0;
            }
            else if (p.Equals(p1))
            {
                if (dx > dy)
                {
                    dist = dx;
                }
                else
                {
                    dist = dy;
                }
            }
            else
            {
                Double pdx = Math.Abs(p.X - p0.X);
                Double pdy = Math.Abs(p.Y - p0.Y);
                
                if (dx > dy)
                {
                    dist = pdx;
                }
                else
                {
                    dist = pdy;
                }

                // <FIX>: hack to ensure that non-endpoints always have a non-zero distance
                if (dist == 0.0 && ! p.Equals(p0))
                {
                    dist = Math.Max(pdx, pdy);
                }
            }

            Assert.IsTrue(!(dist == 0.0 && ! p.Equals(p0)), "Bad distance calculation");
            return dist;
        }

        /// <summary>
        /// This function is non-robust, since it may compute the square of large numbers.
        /// Currently not sure how to improve this.
        /// </summary>
        public static Double NonRobustComputeEdgeDistance(ICoordinate p, ICoordinate p1, ICoordinate p2)
        {
            Double dx = p.X - p1.X;
            Double dy = p.Y - p1.Y;
            Double dist = Math.Sqrt(dx*dx + dy*dy); // dummy value
            Assert.IsTrue(! (dist == 0.0 && ! p.Equals(p1)), "Invalid distance calculation");
            return dist;
        }

        protected Int32 result;

        protected ICoordinate[,] inputLines = new ICoordinate[2,2];

        protected ICoordinate[] intPt = new ICoordinate[2];

        /// <summary> 
        /// The indexes of the endpoints of the intersection lines, in order along
        /// the corresponding line
        /// </summary>
        protected Int32[,] intLineIndex;

        protected Boolean isProper;

        protected ICoordinate pa;

        protected ICoordinate pb;

        /// <summary> 
        /// If MakePrecise is true, computed intersection coordinates will be made precise
        /// using <c>Coordinate.MakePrecise</c>.
        /// </summary>
        protected IPrecisionModel precisionModel = null;

        public LineIntersector()
        {
            intPt[0] = new Coordinate();
            intPt[1] = new Coordinate();
            // alias the intersection points for ease of reference
            pa = intPt[0];
            pb = intPt[1];
            result = 0;
        }

        /// <summary>
        /// Force computed intersection to be rounded to a given precision model
        /// </summary>        
        [Obsolete("Use PrecisionModel instead")]
        public IPrecisionModel MakePrecise
        {
            set { precisionModel = value; }
        }

        /// <summary> 
        /// Force computed intersection to be rounded to a given precision model.
        /// No getter is provided, because the precision model is not required to be specified.
        /// </summary>
        public IPrecisionModel PrecisionModel
        {
            set { precisionModel = value; }
        }

        /// <summary> 
        /// Compute the intersection of a point p and the line p1-p2.
        /// This function computes the Boolean value of the hasIntersection test.
        /// The actual value of the intersection (if there is one)
        /// is equal to the value of <c>p</c>.
        /// </summary>
        public abstract void ComputeIntersection(ICoordinate p, ICoordinate p1, ICoordinate p2);

        protected Boolean IsCollinear
        {
            get { return result == Collinear; }
        }

        /// <summary>
        /// Computes the intersection of the lines p1-p2 and p3-p4.
        /// This function computes both the Boolean value of the hasIntersection test
        /// and the (approximate) value of the intersection point itself (if there is one).
        /// </summary>
        public void ComputeIntersection(ICoordinate p1, ICoordinate p2, ICoordinate p3, ICoordinate p4)
        {
            inputLines[0, 0] = p1;
            inputLines[0, 1] = p2;
            inputLines[1, 0] = p3;
            inputLines[1, 1] = p4;
            result = ComputeIntersect(p1, p2, p3, p4);
        }

        public abstract Int32 ComputeIntersect(ICoordinate p1, ICoordinate p2, ICoordinate q1, ICoordinate q2);

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(inputLines[0, 0]).Append("-");
            sb.Append(inputLines[0, 1]).Append(" ");
            sb.Append(inputLines[1, 0]).Append("-");
            sb.Append(inputLines[1, 1]).Append(" : ");

            if (IsEndPoint)
            {
                sb.Append(" endpoint");
            }

            if (isProper)
            {
                sb.Append(" proper");
            }

            if (IsCollinear)
            {
                sb.Append(" collinear");
            }

            return sb.ToString();
        }

        protected Boolean IsEndPoint
        {
            get { return HasIntersection && !isProper; }
        }

        /// <summary> 
        /// Tests whether the input geometries intersect.
        /// </summary>
        /// <returns><c>true</c> if the input geometries intersect.</returns>
        public Boolean HasIntersection
        {
            get { return result != DontIntersect; }
        }

        /// <summary>
        /// Returns the number of intersection points found.  This will be either 0, 1 or 2.
        /// </summary>
        public Int32 IntersectionNum
        {
            get { return result; }
        }

        /// <summary> 
        /// Returns the intIndex'th intersection point.
        /// </summary>
        /// <param name="intIndex">is 0 or 1.</param>
        /// <returns>The intIndex'th intersection point.</returns>
        public ICoordinate GetIntersection(Int32 intIndex)
        {
            return intPt[intIndex];
        }

        protected void ComputeIntLineIndex()
        {
            if (intLineIndex == null)
            {
                intLineIndex = new Int32[2,2];
                ComputeIntLineIndex(0);
                ComputeIntLineIndex(1);
            }
        }

        /// <summary> 
        /// Test whether a point is a intersection point of two line segments.
        /// Note that if the intersection is a line segment, this method only tests for
        /// equality with the endpoints of the intersection segment.
        /// It does not return true if the input point is internal to the intersection segment.
        /// </summary>
        /// <returns><c>true</c> if the input point is one of the intersection points.</returns>
        public Boolean IsIntersection(ICoordinate pt)
        {
            for (Int32 i = 0; i < result; i++)
            {
                if (intPt[i].Equals2D(pt))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary> 
        /// Tests whether either intersection point is an interior point of one of the input segments.
        /// </summary>
        /// <returns>
        /// <c>true</c> if either intersection point is in the interior of one of the input segment.
        /// </returns>
        public Boolean IsInteriorIntersection()
        {
            if (IsInteriorIntersection(0))
            {
                return true;
            }
            
            if (IsInteriorIntersection(1))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tests whether either intersection point is an interior point of the specified input segment.
        /// </summary>
        /// <returns> 
        /// <c>true</c> if either intersection point is in the interior of the input segment.
        /// </returns>
        public Boolean IsInteriorIntersection(Int32 inputLineIndex)
        {
            for (Int32 i = 0; i < result; i++)
            {
                if (!(intPt[i].Equals2D(inputLines[inputLineIndex, 0]) ||
                      intPt[i].Equals2D(inputLines[inputLineIndex, 1])))
                {
                    return true;
                }
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
        public Boolean IsProper
        {
            get { return HasIntersection && isProper; }
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
        public ICoordinate GetIntersectionAlongSegment(Int32 segmentIndex, Int32 intIndex)
        {
            // lazily compute Int32 line array
            ComputeIntLineIndex();
            return intPt[intLineIndex[segmentIndex, intIndex]];
        }

        /// <summary>
        /// Computes the index of the intIndex'th intersection point in the direction of
        /// a specified input line segment.
        /// </summary>
        /// <param name="segmentIndex">is 0 or 1.</param>
        /// <param name="intIndex">is 0 or 1.</param>
        /// <returns>
        /// The index of the intersection point along the segment (0 or 1).
        /// </returns>
        public Int32 GetIndexAlongSegment(Int32 segmentIndex, Int32 intIndex)
        {
            ComputeIntLineIndex();
            return intLineIndex[segmentIndex, intIndex];
        }

        protected void ComputeIntLineIndex(Int32 segmentIndex)
        {
            Double dist0 = GetEdgeDistance(segmentIndex, 0);
            Double dist1 = GetEdgeDistance(segmentIndex, 1);

            if (dist0 > dist1)
            {
                intLineIndex[segmentIndex, 0] = 0;
                intLineIndex[segmentIndex, 1] = 1;
            }
            else
            {
                intLineIndex[segmentIndex, 0] = 1;
                intLineIndex[segmentIndex, 1] = 0;
            }
        }

        /// <summary> 
        /// Computes the "edge distance" of an intersection point along the specified input line segment.
        /// </summary>
        /// <param name="segmentIndex">is 0 or 1.</param>
        /// <param name="intIndex">is 0 or 1.</param>
        /// <returns>The edge distance of the intersection point.</returns>
        public Double GetEdgeDistance(Int32 segmentIndex, Int32 intIndex)
        {
            Double dist = ComputeEdgeDistance(intPt[intIndex], inputLines[segmentIndex, 0], inputLines[segmentIndex, 1]);
            return dist;
        }
    }
}